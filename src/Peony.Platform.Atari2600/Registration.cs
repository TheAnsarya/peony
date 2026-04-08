namespace Peony.Platform.Atari2600;

using Peony.Core;

/// <summary>
/// Registers Atari 2600 platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(Atari2600Profile.Instance, "atari2600", "atari 2600", "2600");
	}
}
