namespace Peony.Platform.GBA;

using Peony.Core;

/// <summary>
/// Registers GBA platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(GbaProfile.Instance, "gba", "game boy advance", "gameboy advance", "advance");
	}
}
