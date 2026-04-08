namespace Peony.Platform.SNES;

using Peony.Core;

/// <summary>
/// Registers SNES platform profile with the central resolver.
/// Called explicitly at CLI startup — no reflection.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(SnesProfile.Instance, "snes", "super nintendo", "super nes");
	}
}
