using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Collection definition for tests that depend on PlatformResolver global state.
/// Forces sequential execution so PlatformResolverTests.Clear() doesn't
/// interfere with other tests needing registered platforms.
/// </summary>
[CollectionDefinition("PlatformResolver")]
public class PlatformResolverCollection : ICollectionFixture<PlatformResolverFixture> {
}

/// <summary>
/// Shared fixture that ensures all platforms are registered.
/// </summary>
public class PlatformResolverFixture {
	public PlatformResolverFixture() {
		RegisterAllPlatforms();
	}

	public static void RegisterAllPlatforms() {
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
