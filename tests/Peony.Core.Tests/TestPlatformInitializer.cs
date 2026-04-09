using System.Runtime.CompilerServices;

namespace Peony.Core.Tests;

/// <summary>
/// Module initializer that registers all platform profiles before any tests run.
/// Ensures PlatformResolver is available for all test classes that depend on it.
/// </summary>
public static class TestPlatformInitializer {
	[ModuleInitializer]
	public static void Initialize() {
		Peony.Platform.NES.Registration.RegisterAll();
		Peony.Platform.SNES.Registration.RegisterAll();
		Peony.Platform.GameBoy.Registration.RegisterAll();
		Peony.Platform.GBA.Registration.RegisterAll();
		Peony.Platform.Atari2600.Registration.RegisterAll();
		Peony.Platform.Lynx.Registration.RegisterAll();
		Peony.Platform.SMS.Registration.RegisterAll();
		Peony.Platform.PCE.Registration.RegisterAll();
		Peony.Platform.Genesis.Registration.RegisterAll();
		Peony.Platform.WonderSwan.Registration.RegisterAll();
		Peony.Platform.ChannelF.Registration.RegisterAll();
	}
}
