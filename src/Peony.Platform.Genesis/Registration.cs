namespace Peony.Platform.Genesis;

using Peony.Core;

/// <summary>
/// Registers Sega Genesis platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(GenesisProfile.Instance, "genesis", "mega drive", "megadrive", "sega genesis", "md");
	}
}
