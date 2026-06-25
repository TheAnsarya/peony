namespace Peony.Platform.M68000;

using Peony.Core;

/// <summary>
/// Registers the flat M68000 platform profile.
/// </summary>
public static class Registration {
    public static void RegisterAll() {
        PlatformResolver.Register(M68000Profile.Instance,
            "m68000", "m68k", "68000", "68k", "flat68k", "arcade68k");
    }
}
