namespace Peony.Core;

/// <summary>
/// Central registry for platform profiles.
/// Platforms register at startup via explicit calls. No reflection.
/// </summary>
public static class PlatformResolver {
	private static readonly Dictionary<PlatformId, IPlatformProfile> _profiles = [];
	private static readonly Dictionary<string, IPlatformProfile> _nameMap = new(StringComparer.OrdinalIgnoreCase);
	private static readonly Dictionary<string, IPlatformProfile> _extensionMap = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>Register a platform profile with optional name aliases</summary>
	public static void Register(IPlatformProfile profile, params string[] aliases) {
		_profiles[profile.Platform] = profile;
		_nameMap[profile.Platform.ToString()] = profile;
		_nameMap[profile.DisplayName] = profile;

		foreach (var alias in aliases) {
			_nameMap[alias] = profile;
		}

		foreach (var ext in profile.RomExtensions) {
			_extensionMap[ext] = profile;
		}
	}

	/// <summary>Resolve a profile by name (case-insensitive). Supports platform ID names and display names.</summary>
	public static IPlatformProfile? Resolve(string name) {
		return _nameMap.GetValueOrDefault(name);
	}

	/// <summary>Resolve a profile by ROM file extension (e.g., ".sfc")</summary>
	public static IPlatformProfile? ResolveByExtension(string extension) {
		return _extensionMap.GetValueOrDefault(extension);
	}

	/// <summary>Get a profile by platform ID. Throws if not registered.</summary>
	public static IPlatformProfile GetProfile(PlatformId platform) {
		return _profiles.TryGetValue(platform, out var profile)
			? profile
			: throw new InvalidOperationException($"Platform '{platform}' is not registered. Call Registration.RegisterAll() first.");
	}

	/// <summary>Try to get a profile by platform ID</summary>
	public static bool TryGetProfile(PlatformId platform, out IPlatformProfile? profile) {
		return _profiles.TryGetValue(platform, out profile);
	}

	/// <summary>Get all registered profiles</summary>
	public static IReadOnlyList<IPlatformProfile> GetAll() {
		return _profiles.Values.ToList();
	}

	/// <summary>Check if a platform is registered</summary>
	public static bool IsRegistered(PlatformId platform) {
		return _profiles.ContainsKey(platform);
	}

	/// <summary>Clear all registrations (for testing)</summary>
	public static void Clear() {
		_profiles.Clear();
		_nameMap.Clear();
		_extensionMap.Clear();
	}
}
