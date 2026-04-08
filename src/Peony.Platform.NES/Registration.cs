namespace Peony.Platform.NES;

using Peony.Core;

/// <summary>
/// Registers NES platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(NesProfile.Instance, "nes");
	}
}
