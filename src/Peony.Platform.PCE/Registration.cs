namespace Peony.Platform.PCE;

using Peony.Core;

/// <summary>
/// Registers PC Engine platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(PceProfile.Instance, "pce", "pc engine", "turbografx", "turbografx-16", "tg16");
	}
}
