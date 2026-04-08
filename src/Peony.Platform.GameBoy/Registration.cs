namespace Peony.Platform.GameBoy;

using Peony.Core;

/// <summary>
/// Registers Game Boy platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(GameBoyProfile.Instance, "gameboy", "game boy", "gb");
	}
}
