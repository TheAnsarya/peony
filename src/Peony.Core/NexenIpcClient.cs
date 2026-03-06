using System.IO.Pipes;

namespace Peony.Core;

/// <summary>
/// IPC client for communicating with a running Nexen emulator instance
/// via named pipes. Uses the binary envelope protocol defined in the
/// Nexen-Peony IPC specification.
/// </summary>
public sealed class NexenIpcClient : IAsyncDisposable, IDisposable {
	private const string PipePrefix = "nexen-debug-";
	private const ushort ProtocolVersion = 1;
	private const int HeaderSize = 10; // uint32 length + uint32 msgId + uint16 type
	private const int ConnectTimeoutMs = 5000;

	private NamedPipeClientStream? _pipe;
	private BinaryWriter? _writer;
	private uint _nextMsgId;

	/// <summary>Whether the client is currently connected to a Nexen instance.</summary>
	public bool IsConnected => _pipe?.IsConnected == true;

	/// <summary>ROM info from the connected Nexen instance.</summary>
	public NexenRomInfo? RomInfo { get; private set; }

	/// <summary>
	/// Discover running Nexen instances by scanning for named pipes.
	/// </summary>
	public static IReadOnlyList<int> DiscoverInstances() {
		var pids = new List<int>();
		// On Windows, named pipes are listed in \\.\pipe\
		try {
			foreach (var pipeName in Directory.GetFiles(@"\\.\pipe\")) {
				var name = Path.GetFileName(pipeName);
				if (name.StartsWith(PipePrefix, StringComparison.OrdinalIgnoreCase)) {
					var pidStr = name[PipePrefix.Length..];
					if (int.TryParse(pidStr, out int pid))
						pids.Add(pid);
				}
			}
		} catch (IOException) {
			// Pipe enumeration may fail on some systems
		}
		return pids;
	}

	/// <summary>
	/// Connect to a Nexen instance by process ID.
	/// </summary>
	/// <param name="processId">The Nexen process ID, or null to connect to the first available.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <exception cref="NexenConnectionException">Thrown if connection fails.</exception>
	public async Task ConnectAsync(int? processId = null, CancellationToken ct = default) {
		if (IsConnected)
			throw new InvalidOperationException("Already connected. Disconnect first.");

		string pipeName;
		if (processId.HasValue) {
			pipeName = PipePrefix + processId.Value;
		} else {
			var instances = DiscoverInstances();
			if (instances.Count == 0)
				throw new NexenConnectionException("No running Nexen instances found.");
			pipeName = PipePrefix + instances[0];
		}

		_pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

		try {
			await _pipe.ConnectAsync(ConnectTimeoutMs, ct).ConfigureAwait(false);
		} catch (TimeoutException) {
			_pipe.Dispose();
			_pipe = null;
			throw new NexenConnectionException($"Connection to Nexen pipe '{pipeName}' timed out.");
		} catch (OperationCanceledException) {
			_pipe.Dispose();
			_pipe = null;
			throw new NexenConnectionException($"Connection to Nexen pipe '{pipeName}' was cancelled.");
		} catch (IOException ex) {
			_pipe.Dispose();
			_pipe = null;
			throw new NexenConnectionException($"Failed to connect to Nexen: {ex.Message}", ex);
		}

		_writer = new BinaryWriter(_pipe);
		_nextMsgId = 1;

		// Perform handshake
		await HandshakeAsync(ct).ConfigureAwait(false);
	}

	/// <summary>
	/// Read a block of memory from the emulator.
	/// </summary>
	/// <param name="memoryType">The memory region to read (e.g., 0 for system memory, platform-specific).</param>
	/// <param name="startAddress">Start address in the memory region.</param>
	/// <param name="length">Number of bytes to read.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The raw memory bytes.</returns>
	public async Task<byte[]> ReadMemoryAsync(byte memoryType, uint startAddress, uint length, CancellationToken ct = default) {
		EnsureConnected();

		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(memoryType);
		pw.Write(startAddress);
		pw.Write(length);

		var response = await SendRequestAsync(MsgType.GetMemory, payload.ToArray(), ct).ConfigureAwait(false);

		using var reader = new BinaryReader(new MemoryStream(response));
		reader.ReadByte();   // memoryType echo
		reader.ReadUInt32(); // startAddress echo
		var respLength = reader.ReadUInt32();
		return reader.ReadBytes((int)respLength);
	}

	/// <summary>
	/// Read CDL (Code/Data Log) flags from the emulator.
	/// </summary>
	public async Task<byte[]> GetCdlDataAsync(byte memoryType, uint offset, uint length, CancellationToken ct = default) {
		EnsureConnected();

		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(memoryType);
		pw.Write(offset);
		pw.Write(length);

		var response = await SendRequestAsync(MsgType.GetCdlData, payload.ToArray(), ct).ConfigureAwait(false);

		using var reader = new BinaryReader(new MemoryStream(response));
		reader.ReadByte();   // memoryType echo
		reader.ReadUInt32(); // offset echo
		var respLength = reader.ReadUInt32();
		return reader.ReadBytes((int)respLength);
	}

	/// <summary>
	/// Get CDL statistics from the emulator.
	/// </summary>
	public async Task<CdlCoverageStats> GetCdlStatsAsync(byte memoryType, CancellationToken ct = default) {
		EnsureConnected();

		var response = await SendRequestAsync(MsgType.GetCdlStats, [memoryType], ct).ConfigureAwait(false);

		using var reader = new BinaryReader(new MemoryStream(response));
		var totalBytes = reader.ReadInt32();
		var codeBytes = reader.ReadInt32();
		var dataBytes = reader.ReadInt32();
		var drawnBytes = reader.ReadInt32();
		var classified = codeBytes + dataBytes + drawnBytes;
		var unclassified = totalBytes - classified;
		var coverage = totalBytes > 0 ? (double)classified / totalBytes * 100 : 0;
		var romSize = reader.ReadInt32();

		return new CdlCoverageStats(totalBytes, codeBytes, dataBytes, drawnBytes,
			classified, unclassified, coverage, romSize);
	}

	/// <summary>
	/// Push a label to the Nexen debugger.
	/// </summary>
	public async Task SetLabelAsync(uint address, byte memoryType, string label, string comment = "", CancellationToken ct = default) {
		EnsureConnected();

		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(address);
		pw.Write(memoryType);
		WriteNullTerminatedString(pw, label);
		WriteNullTerminatedString(pw, comment);

		await SendRequestAsync(MsgType.SetLabel, payload.ToArray(), ct).ConfigureAwait(false);
	}

	/// <summary>
	/// Push multiple labels to the Nexen debugger.
	/// </summary>
	public async Task SetLabelsAsync(IEnumerable<NexenLabel> labels, CancellationToken ct = default) {
		foreach (var label in labels) {
			await SetLabelAsync(label.Address, label.MemoryType, label.Name, label.Comment, ct).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Get ROM information from the connected Nexen instance.
	/// </summary>
	public async Task<NexenRomInfo> GetRomInfoAsync(CancellationToken ct = default) {
		EnsureConnected();

		var response = await SendRequestAsync(MsgType.GetRomInfo, [], ct).ConfigureAwait(false);

		using var reader = new BinaryReader(new MemoryStream(response));
		var consoleType = reader.ReadByte();
		var romCrc32 = reader.ReadUInt32();
		var romSize = reader.ReadUInt32();
		var romName = ReadNullTerminatedString(reader);
		var romFileName = ReadNullTerminatedString(reader);

		return new NexenRomInfo(consoleType, romCrc32, romSize, romName, romFileName);
	}

	/// <summary>
	/// Translate a CPU address to an absolute (file) address.
	/// </summary>
	public async Task<uint> GetAbsoluteAddressAsync(uint cpuAddress, byte memoryType, CancellationToken ct = default) {
		EnsureConnected();

		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(cpuAddress);
		pw.Write(memoryType);

		var response = await SendRequestAsync(MsgType.GetAbsAddress, payload.ToArray(), ct).ConfigureAwait(false);
		return BitConverter.ToUInt32(response, 0);
	}

	/// <summary>
	/// Graceful disconnect from the Nexen instance.
	/// </summary>
	public async Task DisconnectAsync() {
		if (_pipe is null || !_pipe.IsConnected)
			return;

		try {
			await SendMessageAsync(MsgType.Disconnect, []).ConfigureAwait(false);
		} catch {
			// Best effort disconnect
		}

		Cleanup();
	}

	public async ValueTask DisposeAsync() {
		await DisconnectAsync().ConfigureAwait(false);
	}

	public void Dispose() {
		Cleanup();
	}

	// ========================================================================
	// Private helpers
	// ========================================================================

	private async Task HandshakeAsync(CancellationToken ct) {
		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(ProtocolVersion);
		pw.Write((ushort)1); // Client type: Peony
		pw.Write((uint)0);   // Capabilities (reserved)
		WriteNullTerminatedString(pw, "Peony 1.0");

		var response = await SendRequestAsync(MsgType.Handshake, payload.ToArray(), ct).ConfigureAwait(false);

		using var reader = new BinaryReader(new MemoryStream(response));
		var serverVersion = reader.ReadUInt16();
		if (serverVersion < ProtocolVersion)
			throw new NexenConnectionException($"Nexen protocol version {serverVersion} is too old (need {ProtocolVersion}+).");

		var consoleType = reader.ReadByte();
		var romCrc32 = reader.ReadUInt32();
		var romSize = reader.ReadUInt32();
		var romName = ReadNullTerminatedString(reader);
		var nexenVersion = ReadNullTerminatedString(reader);

		RomInfo = new NexenRomInfo(consoleType, romCrc32, romSize, romName, "");
	}

	private async Task<byte[]> SendRequestAsync(MsgType type, byte[] payload, CancellationToken ct) {
		var msgId = await SendMessageAsync(type, payload).ConfigureAwait(false);
		return await ReadResponseAsync(msgId, ct).ConfigureAwait(false);
	}

	private async Task<uint> SendMessageAsync(MsgType type, byte[] payload) {
		EnsureConnected();
		var msgId = _nextMsgId++;

		var totalLength = (uint)(HeaderSize + payload.Length);
		_writer!.Write(totalLength);
		_writer.Write(msgId);
		_writer.Write((ushort)type);
		if (payload.Length > 0)
			_writer.Write(payload);
		_writer.Flush();
		await _pipe!.FlushAsync().ConfigureAwait(false);

		return msgId;
	}

	private async Task<byte[]> ReadResponseAsync(uint expectedMsgId, CancellationToken ct) {
		var header = new byte[HeaderSize];
		await _pipe!.ReadExactlyAsync(header, ct).ConfigureAwait(false);

		var length = BitConverter.ToUInt32(header, 0);
		var msgId = BitConverter.ToUInt32(header, 4);
		var type = BitConverter.ToUInt16(header, 8);

		if (msgId != expectedMsgId)
			throw new NexenProtocolException($"Response message ID {msgId} does not match request {expectedMsgId}.");

		// Check for error flag
		if ((type & 0x0004) != 0) {
			var payloadLen = (int)(length - HeaderSize);
			var errorPayload = new byte[payloadLen];
			if (payloadLen > 0)
				await _pipe.ReadExactlyAsync(errorPayload, ct).ConfigureAwait(false);
			using var er = new BinaryReader(new MemoryStream(errorPayload));
			var errorCode = er.ReadUInt16();
			var errorMsg = ReadNullTerminatedString(er);
			throw new NexenRemoteException(errorCode, errorMsg);
		}

		var dataLength = (int)(length - HeaderSize);
		if (dataLength <= 0)
			return [];
		var data = new byte[dataLength];
		await _pipe.ReadExactlyAsync(data, ct).ConfigureAwait(false);
		return data;
	}

	private void EnsureConnected() {
		if (!IsConnected)
			throw new InvalidOperationException("Not connected to Nexen. Call ConnectAsync() first.");
	}

	private void Cleanup() {
		_writer?.Dispose();
		_pipe?.Dispose();
		_writer = null;
		_pipe = null;
		RomInfo = null;
	}

	private static void WriteNullTerminatedString(BinaryWriter writer, string value) {
		writer.Write(System.Text.Encoding.UTF8.GetBytes(value));
		writer.Write((byte)0);
	}

	private static string ReadNullTerminatedString(BinaryReader reader) {
		var bytes = new List<byte>();
		byte b;
		while ((b = reader.ReadByte()) != 0)
			bytes.Add(b);
		return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
	}

	private enum MsgType : ushort {
		Handshake = 0x0001,
		HandshakeAck = 0x0002,
		Disconnect = 0x0003,
		Heartbeat = 0x0004,
		GetMemory = 0x0100,
		GetMemoryResp = 0x0101,
		GetMemorySize = 0x0102,
		GetMemorySizeResp = 0x0103,
		GetCpuState = 0x0200,
		GetCpuStateResp = 0x0201,
		GetProgramCounter = 0x0202,
		GetProgramCounterResp = 0x0203,
		GetPpuState = 0x0300,
		GetPpuStateResp = 0x0301,
		GetVram = 0x0302,
		GetVramResp = 0x0303,
		GetCdlData = 0x0400,
		GetCdlDataResp = 0x0401,
		GetCdlStats = 0x0402,
		GetCdlStatsResp = 0x0403,
		GetCdlFunctions = 0x0404,
		GetCdlFunctionsResp = 0x0405,
		SetLabel = 0x0500,
		SetLabelAck = 0x0501,
		GetAbsAddress = 0x0600,
		GetAbsAddressResp = 0x0601,
		GetRelAddress = 0x0602,
		GetRelAddressResp = 0x0603,
		GetExecStatus = 0x0700,
		GetExecStatusResp = 0x0701,
		GetRomInfo = 0x0800,
		GetRomInfoResp = 0x0801,
	}
}

/// <summary>ROM info from a connected Nexen instance.</summary>
public record NexenRomInfo(byte ConsoleType, uint RomCrc32, uint RomSize, string RomName, string RomFileName);

/// <summary>A label to push to the Nexen debugger.</summary>
public record NexenLabel(uint Address, byte MemoryType, string Name, string Comment = "");

/// <summary>Exception for Nexen connection failures.</summary>
public class NexenConnectionException : Exception {
	public NexenConnectionException(string message) : base(message) { }
	public NexenConnectionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Exception for IPC protocol errors.</summary>
public class NexenProtocolException : Exception {
	public NexenProtocolException(string message) : base(message) { }
}

/// <summary>Exception for remote errors reported by the Nexen server.</summary>
public class NexenRemoteException : Exception {
	public ushort ErrorCode { get; }
	public NexenRemoteException(ushort errorCode, string message) : base(message) {
		ErrorCode = errorCode;
	}
}
