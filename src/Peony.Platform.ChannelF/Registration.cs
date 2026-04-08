namespace Peony.Platform.ChannelF;

using Peony.Core;

/// <summary>
/// Registers Channel F platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(ChannelFProfile.Instance, "channelf", "channel f", "fairchild", "f8");
	}
}
