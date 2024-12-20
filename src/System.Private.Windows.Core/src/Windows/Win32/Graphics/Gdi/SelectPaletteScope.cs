﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Windows.Win32.Graphics.GdiPlus;

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  Helper to scope palette selection.
/// </summary>
/// <remarks>
///  <para>
///   Use in a <see langword="using" /> statement. If you must pass this around, always pass
///   by <see langword="ref" /> to avoid duplicating the handle and risking a double palette reset.
///  </para>
/// </remarks>
#if DEBUG
internal class SelectPaletteScope : DisposalTracking.Tracker, IDisposable
#else
internal readonly ref struct SelectPaletteScope
#endif
{
    public HDC HDC { get; }
    public HPALETTE HPALETTE { get; }

    private static HPALETTE s_halftonePalette;

    public SelectPaletteScope(HDC hdc, HPALETTE hpalette, bool forceBackground, bool realizePalette)
    {
        HDC = hdc;
        HPALETTE = PInvokeCore.SelectPalette(hdc, hpalette, forceBackground);
        if (!HPALETTE.IsNull && realizePalette)
        {
            PInvokeCore.RealizePalette(hdc);
        }
    }

    /// <summary>
    ///  Uses the GDI+ halftone palette for the given <paramref name="hdc"/> if the color depth is 8 bpp or less.
    /// </summary>
    public static SelectPaletteScope HalftonePalette(HDC hdc, bool forceBackground, bool realizePalette)
    {
        if (PInvokeCore.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.BITSPIXEL) > 8)
        {
            // https://docs.microsoft.com/windows/win32/api/Gdiplusgraphics/nf-gdiplusgraphics-graphics-gethalftonepalette
            // The purpose of the Graphics::GetHalftonePalette method is to enable GDI+ to produce a better
            // quality halftone when the display uses 8 bits per pixel. This method allocates a palette of
            // 256 entries (each of which are 4 bytes a piece).
            //
            // Doing this is a bit pointless when the color depth is much higher (the normal scenario). As such
            // we'll skip doing this unless we see 8bpp or less.
#if DEBUG
            return new SelectPaletteScope();
#else
            return default;
#endif
        }

        if (s_halftonePalette.IsNull)
        {
            GdiPlusInitialization.EnsureInitialized();
            s_halftonePalette = PInvokeCore.GdipCreateHalftonePalette();
        }

        return new SelectPaletteScope(
            hdc,
            s_halftonePalette,
            forceBackground,
            realizePalette);
    }

    public static implicit operator HPALETTE(in SelectPaletteScope paletteScope) => paletteScope.HPALETTE;

    public void Dispose()
    {
        if (!HPALETTE.IsNull)
        {
            PInvokeCore.SelectPalette(HDC, HPALETTE, bForceBkgd: false);
        }

        DisposalTracking.SuppressFinalize(this);
    }

#if DEBUG
    public SelectPaletteScope() { }
#endif
}
