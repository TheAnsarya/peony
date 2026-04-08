namespace Peony.Platform.Lynx;

using Peony.Core;

/// <summary>
/// Registers Atari Lynx platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(LynxProfile.Instance, "lynx", "atari lynx");
	}
}
