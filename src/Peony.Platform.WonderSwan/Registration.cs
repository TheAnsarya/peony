namespace Peony.Platform.WonderSwan;

using Peony.Core;

/// <summary>
/// Registers WonderSwan platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(WonderSwanProfile.Instance, "ws", "wonderswan", "wonder swan", "wsc");
	}
}
