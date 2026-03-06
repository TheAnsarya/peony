using System.IO.Pipes;
using Peony.Core;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for NexenIpcClient using mock in-process named pipe servers.
/// All pipe operations use async I/O and Task.Run to avoid sync context deadlocks.
/// </summary>
public class NexenIpcClientTests : IDisposable {
	private const string TestPipeName = "nexen-ipc-test-pipe";
	private const int HeaderSize = 10; // uint32 length + uint32 msgId + uint16 type

	private NamedPipeServerStream? _server;

	public void Dispose() {
		_server?.Dispose();
		GC.SuppressFinalize(this);
	}

	// ========================================================================
	// Connection tests
	// ========================================================================

	[Fact]
	public void IsConnected_WhenNew_ReturnsFalse() {
		using var client = new NexenIpcClient();
		Assert.False(client.IsConnected);
	}

	[Fact]
	public void RomInfo_WhenNew_IsNull() {
		using var client = new NexenIpcClient();
		Assert.Null(client.RomInfo);
	}

	[Fact]
	public async Task ConnectAsync_NoServer_ThrowsConnectionException() {
		using var client = new NexenIpcClient();
		// Use a pipe name that doesn't exist and a nonexistent PID
		await Assert.ThrowsAsync<NexenConnectionException>(
			() => client.ConnectAsync(processId: 99999, ct: new CancellationTokenSource(500).Token));
	}

	[Fact]
	public async Task ConnectAsync_AlreadyConnected_ThrowsInvalidOperation() {
		var (client, _) = await ConnectWithMockHandshakeAsync();

		try {
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => client.ConnectAsync(processId: 1));
		} finally {
			client.Dispose();
		}
	}

	// ========================================================================
	// Handshake tests
	// ========================================================================

	[Fact]
	public async Task ConnectAsync_PerformsHandshake_SetsRomInfo() {
		var (client, _) = await ConnectWithMockHandshakeAsync(
			consoleType: 0x01, romCrc32: 0xdeadbeef, romSize: 0x40000, romName: "TestRom");

		Assert.True(client.IsConnected);
		Assert.NotNull(client.RomInfo);
		Assert.Equal((byte)0x01, client.RomInfo.ConsoleType);
		Assert.Equal(0xdeadbeef, client.RomInfo.RomCrc32);
		Assert.Equal(0x40000u, client.RomInfo.RomSize);
		Assert.Equal("TestRom", client.RomInfo.RomName);

		client.Dispose();
	}

	// ========================================================================
	// ReadMemoryAsync tests
	// ========================================================================

	[Fact]
	public async Task ReadMemoryAsync_ReturnsCorrectData() {
		var (client, server) = await ConnectWithMockHandshakeAsync();
		byte[] expectedData = [0x4c, 0x00, 0x80, 0xea, 0xea, 0xea, 0xea, 0xea];

		var readTask = Task.Run(() => client.ReadMemoryAsync(0, 0x8000, 8));

		var (msgId, _, _) = await ReadMessageFromPipeAsync(server);
		await SendMemoryResponseAsync(server, msgId, 0, 0x8000, expectedData);

		var result = await readTask;

		Assert.Equal(expectedData, result);
		client.Dispose();
	}

	[Fact]
	public async Task ReadMemoryAsync_NotConnected_ThrowsInvalidOperation() {
		using var client = new NexenIpcClient();
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => client.ReadMemoryAsync(0, 0, 16));
	}

	// ========================================================================
	// GetCdlDataAsync tests
	// ========================================================================

	[Fact]
	public async Task GetCdlDataAsync_ReturnsCorrectFlags() {
		var (client, server) = await ConnectWithMockHandshakeAsync();
		byte[] expectedCdl = [0x01, 0x11, 0x02, 0x01, 0x11, 0x01, 0x11, 0x02];

		var cdlTask = Task.Run(() => client.GetCdlDataAsync(0, 0x0000, 8));

		var (msgId, _, _) = await ReadMessageFromPipeAsync(server);
		await SendCdlResponseAsync(server, msgId, 0, 0x0000, expectedCdl);

		var result = await cdlTask;

		Assert.Equal(expectedCdl, result);
		client.Dispose();
	}

	// ========================================================================
	// GetCdlStatsAsync tests
	// ========================================================================

	[Fact]
	public async Task GetCdlStatsAsync_ReturnsStats() {
		var (client, server) = await ConnectWithMockHandshakeAsync();

		var statsTask = Task.Run(() => client.GetCdlStatsAsync(0));

		var (msgId, _, _) = await ReadMessageFromPipeAsync(server);
		await SendCdlStatsResponseAsync(server, msgId, totalBytes: 32768, codeBytes: 20000,
			dataBytes: 5000, drawnBytes: 1000, romSize: 32768);

		var stats = await statsTask;

		Assert.Equal(32768, stats.TotalBytes);
		Assert.Equal(20000, stats.CodeBytes);
		Assert.Equal(5000, stats.DataBytes);
		Assert.Equal(1000, stats.DrawnBytes);
		Assert.Equal(26000, stats.ClassifiedBytes);
		Assert.Equal(6768, stats.UnclassifiedBytes);
		Assert.Equal(32768, stats.RomSize);
		client.Dispose();
	}

	// ========================================================================
	// SetLabelAsync tests
	// ========================================================================

	[Fact]
	public async Task SetLabelAsync_SendsCorrectPayload() {
		var (client, server) = await ConnectWithMockHandshakeAsync();

		var labelTask = Task.Run(() => client.SetLabelAsync(0x8000, 0, "Reset", "Entry point"));

		var (msgId, type, payload) = await ReadMessageFromPipeAsync(server);
		Assert.Equal(0x0500, type); // SetLabel

		using var reader = new BinaryReader(new MemoryStream(payload));
		Assert.Equal(0x8000u, reader.ReadUInt32());
		Assert.Equal((byte)0, reader.ReadByte());
		Assert.Equal("Reset", ReadNullTerminated(reader));
		Assert.Equal("Entry point", ReadNullTerminated(reader));

		// Send ack
		await SendAckAsync(server, msgId, 0x0501);

		await labelTask;
		client.Dispose();
	}

	[Fact]
	public async Task SetLabelsAsync_SendsMultipleLabels() {
		var (client, server) = await ConnectWithMockHandshakeAsync();
		var labels = new NexenLabel[] {
			new(0x8000, 0, "Reset"),
			new(0xfffa, 0, "NMI_Vector"),
		};

		var labelsTask = Task.Run(() => client.SetLabelsAsync(labels));

		// Handle two label requests
		for (int i = 0; i < 2; i++) {
			var (msgId, _, _) = await ReadMessageFromPipeAsync(server);
			await SendAckAsync(server, msgId, 0x0501);
		}

		await labelsTask;
		client.Dispose();
	}

	// ========================================================================
	// GetAbsoluteAddressAsync tests
	// ========================================================================

	[Fact]
	public async Task GetAbsoluteAddressAsync_ReturnsAddress() {
		var (client, server) = await ConnectWithMockHandshakeAsync();

		var addrTask = Task.Run(() => client.GetAbsoluteAddressAsync(0xc000, 0));

		var (msgId, _, _) = await ReadMessageFromPipeAsync(server);
		await SendRawResponseAsync(server, msgId, 0x0601, BitConverter.GetBytes(0x00004000u));

		var result = await addrTask;
		Assert.Equal(0x00004000u, result);
		client.Dispose();
	}

	// ========================================================================
	// Error handling tests
	// ========================================================================

	[Fact]
	public async Task RemoteError_ThrowsNexenRemoteException() {
		var (client, server) = await ConnectWithMockHandshakeAsync();

		var readTask = Task.Run(() => client.ReadMemoryAsync(0, 0x0000, 16));

		var (msgId, _, _) = await ReadMessageFromPipeAsync(server);
		await SendErrorResponseAsync(server, msgId, 0x0002, "Memory type not available");

		var ex = await Assert.ThrowsAsync<NexenRemoteException>(() => readTask);
		Assert.Equal((ushort)0x0002, ex.ErrorCode);
		Assert.Contains("Memory type not available", ex.Message);
		client.Dispose();
	}

	// ========================================================================
	// Disconnect tests
	// ========================================================================

	[Fact]
	public async Task DisconnectAsync_CleansUpState() {
		var (client, _) = await ConnectWithMockHandshakeAsync();
		Assert.True(client.IsConnected);

		// DisconnectAsync sends a disconnect message; we just dispose
		client.Dispose();

		Assert.False(client.IsConnected);
		Assert.Null(client.RomInfo);
	}

	// ========================================================================
	// NexenRomInfo record tests
	// ========================================================================

	[Fact]
	public void NexenRomInfo_RecordEquality() {
		var a = new NexenRomInfo(0x01, 0xdeadbeef, 0x40000, "Test", "test.nes");
		var b = new NexenRomInfo(0x01, 0xdeadbeef, 0x40000, "Test", "test.nes");
		Assert.Equal(a, b);
	}

	[Fact]
	public void NexenLabel_RecordEquality() {
		var a = new NexenLabel(0x8000, 0, "Reset", "");
		var b = new NexenLabel(0x8000, 0, "Reset", "");
		Assert.Equal(a, b);
	}

	[Fact]
	public void NexenLabel_DefaultComment_IsEmpty() {
		var label = new NexenLabel(0x8000, 0, "Reset");
		Assert.Equal("", label.Comment);
	}

	// ========================================================================
	// Exception type tests
	// ========================================================================

	[Fact]
	public void NexenConnectionException_HasMessage() {
		var ex = new NexenConnectionException("test error");
		Assert.Equal("test error", ex.Message);
	}

	[Fact]
	public void NexenConnectionException_HasInnerException() {
		var inner = new IOException("pipe broken");
		var ex = new NexenConnectionException("test", inner);
		Assert.Same(inner, ex.InnerException);
	}

	[Fact]
	public void NexenProtocolException_HasMessage() {
		var ex = new NexenProtocolException("bad protocol");
		Assert.Equal("bad protocol", ex.Message);
	}

	[Fact]
	public void NexenRemoteException_HasErrorCode() {
		var ex = new NexenRemoteException(0x0002, "not found");
		Assert.Equal((ushort)0x0002, ex.ErrorCode);
		Assert.Equal("not found", ex.Message);
	}

	// ========================================================================
	// DiscoverInstances tests
	// ========================================================================

	[Fact]
	public void DiscoverInstances_ReturnsListOfInts() {
		// Just validate it doesn't throw — actual pipe discovery is OS-dependent
		var result = NexenIpcClient.DiscoverInstances();
		Assert.NotNull(result);
	}

	// ========================================================================
	// Mock pipe server helpers
	// ========================================================================

	/// <summary>
	/// Create a mock named pipe server and connect a NexenIpcClient to it,
	/// completing the handshake automatically.
	/// </summary>
	private async Task<(NexenIpcClient Client, NamedPipeServerStream Server)> ConnectWithMockHandshakeAsync(
		byte consoleType = 0x01,
		uint romCrc32 = 0xdeadbeef,
		uint romSize = 0x40000,
		string romName = "TestRom") {
		var pipeName = $"{TestPipeName}-{Guid.NewGuid():N}";

		var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
		_server = server;

		var client = new NexenIpcClient();

		// Start connect (will block waiting for handshake)
		var connectTask = ConnectClientToPipeAsync(client, pipeName);

		// Accept connection on server side
		await server.WaitForConnectionAsync().ConfigureAwait(false);

		// Read handshake request from client
		var (msgId, type, _) = await ReadMessageFromPipeAsync(server);
		Assert.Equal(0x0001, type); // Handshake

		// Send handshake response
		await SendHandshakeResponseAsync(server, msgId, consoleType, romCrc32, romSize, romName, "Nexen-Test");

		// Wait for client connection to complete
		await connectTask;

		return (client, server);
	}

	/// <summary>
	/// Connect the client using reflection to set the pipe name directly,
	/// bypassing the PID-based discovery.
	/// </summary>
	private static async Task ConnectClientToPipeAsync(NexenIpcClient client, string pipeName) {
		// Access private fields to inject our pipe directly
		var pipeField = typeof(NexenIpcClient).GetField("_pipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		var writerField = typeof(NexenIpcClient).GetField("_writer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		var msgIdField = typeof(NexenIpcClient).GetField("_nextMsgId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

		var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
		await pipe.ConnectAsync(5000).ConfigureAwait(false);

		pipeField.SetValue(client, pipe);
		writerField.SetValue(client, new BinaryWriter(pipe));
		msgIdField.SetValue(client, (uint)1);

		// Perform handshake via private method
		var handshakeMethod = typeof(NexenIpcClient).GetMethod("HandshakeAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		await ((Task)handshakeMethod.Invoke(client, [CancellationToken.None])!).ConfigureAwait(false);
	}

	// ========================================================================
	// Async pipe helper methods (avoid BinaryReader sync blocking)
	// ========================================================================

	private static async Task<(uint MsgId, ushort Type, byte[] Payload)> ReadMessageFromPipeAsync(Stream pipe) {
		var header = new byte[HeaderSize];
		await pipe.ReadExactlyAsync(header).ConfigureAwait(false);

		var length = BitConverter.ToUInt32(header, 0);
		var msgId = BitConverter.ToUInt32(header, 4);
		var type = BitConverter.ToUInt16(header, 8);
		var payloadLen = (int)(length - HeaderSize);
		var payload = Array.Empty<byte>();
		if (payloadLen > 0) {
			payload = new byte[payloadLen];
			await pipe.ReadExactlyAsync(payload).ConfigureAwait(false);
		}
		return (msgId, type, payload);
	}

	private static async Task SendPipeMessageAsync(Stream pipe, uint msgId, ushort type, byte[] payload) {
		using var msg = new MemoryStream();
		using var writer = new BinaryWriter(msg);
		var totalLength = (uint)(HeaderSize + payload.Length);
		writer.Write(totalLength);
		writer.Write(msgId);
		writer.Write(type);
		if (payload.Length > 0)
			writer.Write(payload);
		writer.Flush();

		await pipe.WriteAsync(msg.ToArray()).ConfigureAwait(false);
		await pipe.FlushAsync().ConfigureAwait(false);
	}

	private static async Task SendHandshakeResponseAsync(Stream pipe, uint msgId,
		byte consoleType, uint romCrc32, uint romSize, string romName, string nexenVersion) {
		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write((ushort)1);  // protocol version
		pw.Write(consoleType);
		pw.Write(romCrc32);
		pw.Write(romSize);
		WriteNullTerminated(pw, romName);
		WriteNullTerminated(pw, nexenVersion);

		await SendPipeMessageAsync(pipe, msgId, 0x0002, payload.ToArray()); // HandshakeAck
	}

	private static async Task SendMemoryResponseAsync(Stream pipe, uint msgId,
		byte memoryType, uint startAddress, byte[] data) {
		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(memoryType);
		pw.Write(startAddress);
		pw.Write((uint)data.Length);
		pw.Write(data);

		await SendPipeMessageAsync(pipe, msgId, 0x0101, payload.ToArray()); // GetMemoryResp
	}

	private static async Task SendCdlResponseAsync(Stream pipe, uint msgId,
		byte memoryType, uint offset, byte[] cdlData) {
		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(memoryType);
		pw.Write(offset);
		pw.Write((uint)cdlData.Length);
		pw.Write(cdlData);

		await SendPipeMessageAsync(pipe, msgId, 0x0401, payload.ToArray()); // GetCdlDataResp
	}

	private static async Task SendCdlStatsResponseAsync(Stream pipe, uint msgId,
		int totalBytes, int codeBytes, int dataBytes, int drawnBytes, int romSize) {
		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(totalBytes);
		pw.Write(codeBytes);
		pw.Write(dataBytes);
		pw.Write(drawnBytes);
		pw.Write(romSize);

		await SendPipeMessageAsync(pipe, msgId, 0x0403, payload.ToArray()); // GetCdlStatsResp
	}

	private static Task SendAckAsync(Stream pipe, uint msgId, ushort ackType) {
		return SendPipeMessageAsync(pipe, msgId, ackType, []);
	}

	private static Task SendRawResponseAsync(Stream pipe, uint msgId, ushort type, byte[] data) {
		return SendPipeMessageAsync(pipe, msgId, type, data);
	}

	private static async Task SendErrorResponseAsync(Stream pipe, uint msgId, ushort errorCode, string errorMsg) {
		using var payload = new MemoryStream();
		using var pw = new BinaryWriter(payload);
		pw.Write(errorCode);
		WriteNullTerminated(pw, errorMsg);

		// Error flag (0x0004) added to the type field
		await SendPipeMessageAsync(pipe, msgId, 0x0004, payload.ToArray());
	}

	private static void WriteNullTerminated(BinaryWriter writer, string value) {
		writer.Write(System.Text.Encoding.UTF8.GetBytes(value));
		writer.Write((byte)0);
	}

	private static string ReadNullTerminated(BinaryReader reader) {
		var bytes = new List<byte>();
		byte b;
		while ((b = reader.ReadByte()) != 0)
			bytes.Add(b);
		return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
	}
}
