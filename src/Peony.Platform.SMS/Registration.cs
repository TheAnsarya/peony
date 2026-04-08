namespace Peony.Platform.SMS;

using Peony.Core;

/// <summary>
/// Registers Sega Master System platform profile with the central resolver.
/// </summary>
public static class Registration {
	public static void RegisterAll() {
		PlatformResolver.Register(SmsProfile.Instance, "sms", "master system", "sega master system", "game gear", "gg");
	}
}
