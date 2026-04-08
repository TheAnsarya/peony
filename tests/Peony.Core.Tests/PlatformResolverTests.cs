using Peony.Core;
using Peony.Platform.NES;
using Peony.Platform.SNES;
using Peony.Platform.GameBoy;
using Peony.Platform.GBA;
using Peony.Platform.Atari2600;
using Peony.Platform.Lynx;
using Peony.Platform.SMS;
using Peony.Platform.PCE;
using Peony.Platform.Genesis;
using Peony.Platform.WonderSwan;
using Peony.Platform.ChannelF;
using Xunit;

namespace Peony.Core.Tests;

/// <summary>
/// Tests for PlatformResolver registration, lookup, and alias resolution
/// </summary>
[Collection("PlatformResolver")]
public class PlatformResolverTests : IDisposable {
	public PlatformResolverTests() {
		PlatformResolver.Clear();
	}

	public void Dispose() {
		// Re-register all platforms after tests so other test classes
		// that depend on PlatformResolver are not affected
		PlatformResolver.Clear();
		RegisterAllPlatforms();
	}

	[Fact]
	public void RegisterAll_RegistersAllPlatforms() {
		RegisterAllPlatforms();

		var all = PlatformResolver.GetAll();
		Assert.Equal(11, all.Count);
	}

	[Theory]
	[InlineData("NES", PlatformId.NES)]
	[InlineData("nes", PlatformId.NES)]
	[InlineData("SNES", PlatformId.SNES)]
	[InlineData("snes", PlatformId.SNES)]
	[InlineData("super nintendo", PlatformId.SNES)]
	[InlineData("super nes", PlatformId.SNES)]
	[InlineData("GameBoy", PlatformId.GameBoy)]
	[InlineData("gameboy", PlatformId.GameBoy)]
	[InlineData("game boy", PlatformId.GameBoy)]
	[InlineData("gb", PlatformId.GameBoy)]
	[InlineData("GBA", PlatformId.GBA)]
	[InlineData("gba", PlatformId.GBA)]
	[InlineData("game boy advance", PlatformId.GBA)]
	[InlineData("Atari2600", PlatformId.Atari2600)]
	[InlineData("atari 2600", PlatformId.Atari2600)]
	[InlineData("2600", PlatformId.Atari2600)]
	[InlineData("Lynx", PlatformId.Lynx)]
	[InlineData("lynx", PlatformId.Lynx)]
	[InlineData("atari lynx", PlatformId.Lynx)]
	[InlineData("SMS", PlatformId.SMS)]
	[InlineData("sms", PlatformId.SMS)]
	[InlineData("master system", PlatformId.SMS)]
	[InlineData("game gear", PlatformId.SMS)]
	[InlineData("PCE", PlatformId.PCE)]
	[InlineData("pce", PlatformId.PCE)]
	[InlineData("pc engine", PlatformId.PCE)]
	[InlineData("turbografx", PlatformId.PCE)]
	[InlineData("Genesis", PlatformId.Genesis)]
	[InlineData("genesis", PlatformId.Genesis)]
	[InlineData("mega drive", PlatformId.Genesis)]
	[InlineData("WonderSwan", PlatformId.WonderSwan)]
	[InlineData("wonderswan", PlatformId.WonderSwan)]
	[InlineData("ws", PlatformId.WonderSwan)]
	[InlineData("ChannelF", PlatformId.ChannelF)]
	[InlineData("channelf", PlatformId.ChannelF)]
	[InlineData("channel f", PlatformId.ChannelF)]
	[InlineData("fairchild", PlatformId.ChannelF)]
	public void Resolve_ByNameOrAlias_ReturnsCorrectPlatform(string name, PlatformId expected) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.Resolve(name);
		Assert.NotNull(profile);
		Assert.Equal(expected, profile.Platform);
	}

	[Theory]
	[InlineData(".nes", PlatformId.NES)]
	[InlineData(".sfc", PlatformId.SNES)]
	[InlineData(".smc", PlatformId.SNES)]
	[InlineData(".gb", PlatformId.GameBoy)]
	[InlineData(".gbc", PlatformId.GameBoy)]
	[InlineData(".gba", PlatformId.GBA)]
	[InlineData(".a26", PlatformId.Atari2600)]
	[InlineData(".lnx", PlatformId.Lynx)]
	[InlineData(".sms", PlatformId.SMS)]
	[InlineData(".gg", PlatformId.SMS)]
	[InlineData(".pce", PlatformId.PCE)]
	[InlineData(".md", PlatformId.Genesis)]
	[InlineData(".gen", PlatformId.Genesis)]
	[InlineData(".ws", PlatformId.WonderSwan)]
	[InlineData(".wsc", PlatformId.WonderSwan)]
	[InlineData(".chf", PlatformId.ChannelF)]
	public void ResolveByExtension_ReturnsCorrectPlatform(string ext, PlatformId expected) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.ResolveByExtension(ext);
		Assert.NotNull(profile);
		Assert.Equal(expected, profile.Platform);
	}

	[Fact]
	public void Resolve_UnknownName_ReturnsNull() {
		RegisterAllPlatforms();

		Assert.Null(PlatformResolver.Resolve("playstation"));
		Assert.Null(PlatformResolver.Resolve(""));
	}

	[Fact]
	public void ResolveByExtension_UnknownExtension_ReturnsNull() {
		RegisterAllPlatforms();

		Assert.Null(PlatformResolver.ResolveByExtension(".iso"));
	}

	[Fact]
	public void GetProfile_RegisteredPlatform_Returns() {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(PlatformId.SNES);
		Assert.Equal(PlatformId.SNES, profile.Platform);
		Assert.Equal("Super Nintendo", profile.DisplayName);
	}

	[Fact]
	public void GetProfile_UnregisteredPlatform_Throws() {
		PlatformResolver.Clear();

		Assert.Throws<InvalidOperationException>(() => PlatformResolver.GetProfile(PlatformId.SNES));
	}

	[Fact]
	public void TryGetProfile_ReturnsCorrectly() {
		RegisterAllPlatforms();

		Assert.True(PlatformResolver.TryGetProfile(PlatformId.NES, out var nesProfile));
		Assert.NotNull(nesProfile);
		Assert.Equal(PlatformId.NES, nesProfile.Platform);
	}

	[Fact]
	public void IsRegistered_ReturnsCorrectly() {
		Assert.False(PlatformResolver.IsRegistered(PlatformId.NES));

		Peony.Platform.NES.Registration.RegisterAll();

		Assert.True(PlatformResolver.IsRegistered(PlatformId.NES));
		Assert.False(PlatformResolver.IsRegistered(PlatformId.SNES));
	}

	[Fact]
	public void Clear_RemovesAllRegistrations() {
		RegisterAllPlatforms();
		Assert.Equal(11, PlatformResolver.GetAll().Count);

		PlatformResolver.Clear();
		Assert.Empty(PlatformResolver.GetAll());
	}

	[Theory]
	[InlineData(PlatformId.NES)]
	[InlineData(PlatformId.SNES)]
	[InlineData(PlatformId.GameBoy)]
	[InlineData(PlatformId.GBA)]
	[InlineData(PlatformId.Atari2600)]
	[InlineData(PlatformId.Lynx)]
	[InlineData(PlatformId.SMS)]
	[InlineData(PlatformId.PCE)]
	[InlineData(PlatformId.Genesis)]
	[InlineData(PlatformId.WonderSwan)]
	[InlineData(PlatformId.ChannelF)]
	public void Profile_HasRequiredProperties(PlatformId platformId) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(platformId);
		Assert.NotNull(profile.DisplayName);
		Assert.NotEmpty(profile.DisplayName);
		Assert.NotNull(profile.CpuDecoder);
		Assert.NotNull(profile.Analyzer);
		Assert.NotNull(profile.OutputGenerator);
		Assert.NotNull(profile.AssetExtractors);
		Assert.NotNull(profile.RomExtensions);
		Assert.NotEmpty(profile.RomExtensions);
	}

	[Theory]
	[InlineData(PlatformId.NES, (byte)0x01)]
	[InlineData(PlatformId.SNES, (byte)0x02)]
	[InlineData(PlatformId.GameBoy, (byte)0x03)]
	[InlineData(PlatformId.GBA, (byte)0x04)]
	[InlineData(PlatformId.Genesis, (byte)0x05)]
	[InlineData(PlatformId.SMS, (byte)0x06)]
	[InlineData(PlatformId.PCE, (byte)0x07)]
	[InlineData(PlatformId.Atari2600, (byte)0x08)]
	[InlineData(PlatformId.Lynx, (byte)0x09)]
	[InlineData(PlatformId.WonderSwan, (byte)0x0a)]
	public void Profile_HasCorrectPansyPlatformId(PlatformId platformId, byte expectedPansyId) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(platformId);
		Assert.Equal(expectedPansyId, profile.PansyPlatformId);
	}

	[Fact]
	public void ChannelF_HasNullPansyPlatformId() {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(PlatformId.ChannelF);
		Assert.Null(profile.PansyPlatformId);
	}

	[Fact]
	public void SnesProfile_UsesCustomOutputGenerator() {
		Peony.Platform.SNES.Registration.RegisterAll();

		var profile = PlatformResolver.GetProfile(PlatformId.SNES);
		Assert.IsType<SnesOutputGenerator>(profile.OutputGenerator);
	}

	[Theory]
	[InlineData(PlatformId.NES)]
	[InlineData(PlatformId.GameBoy)]
	[InlineData(PlatformId.GBA)]
	[InlineData(PlatformId.Atari2600)]
	[InlineData(PlatformId.Lynx)]
	[InlineData(PlatformId.SMS)]
	[InlineData(PlatformId.PCE)]
	[InlineData(PlatformId.Genesis)]
	[InlineData(PlatformId.WonderSwan)]
	[InlineData(PlatformId.ChannelF)]
	public void NonSnesPlatforms_UseGenericOutputGenerator(PlatformId platformId) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(platformId);
		Assert.IsType<PoppyFormatter>(profile.OutputGenerator);
	}

	[Fact]
	public void SnesProfile_IsSingleton() {
		Assert.Same(SnesProfile.Instance, SnesProfile.Instance);
	}

	[Fact]
	public void NesProfile_IsSingleton() {
		Assert.Same(NesProfile.Instance, NesProfile.Instance);
	}

	[Theory]
	[InlineData(PlatformId.NES)]
	[InlineData(PlatformId.SNES)]
	[InlineData(PlatformId.GameBoy)]
	[InlineData(PlatformId.GBA)]
	public void PlatformsWithExtractors_HaveGraphicsExtractor(PlatformId platformId) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(platformId);
		Assert.NotNull(profile.GraphicsExtractor);
	}

	[Theory]
	[InlineData(PlatformId.NES)]
	[InlineData(PlatformId.SNES)]
	[InlineData(PlatformId.GameBoy)]
	[InlineData(PlatformId.GBA)]
	public void PlatformsWithExtractors_HaveTextExtractor(PlatformId platformId) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(platformId);
		Assert.NotNull(profile.TextExtractor);
	}

	[Theory]
	[InlineData(PlatformId.Atari2600)]
	[InlineData(PlatformId.Lynx)]
	[InlineData(PlatformId.SMS)]
	[InlineData(PlatformId.PCE)]
	[InlineData(PlatformId.Genesis)]
	[InlineData(PlatformId.WonderSwan)]
	[InlineData(PlatformId.ChannelF)]
	public void PlatformsWithoutExtractors_HaveNullExtractors(PlatformId platformId) {
		RegisterAllPlatforms();

		var profile = PlatformResolver.GetProfile(platformId);
		Assert.Null(profile.GraphicsExtractor);
		Assert.Null(profile.TextExtractor);
	}

	private static void RegisterAllPlatforms() {
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
