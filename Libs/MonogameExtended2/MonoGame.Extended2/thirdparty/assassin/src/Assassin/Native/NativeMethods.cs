using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Assassin.Native;

internal static partial class NativeMethods
{

    public static int ass_library_version()
    {
        if (Is64Bit)
        {
            return X64.ass_library_version();
        }
        else
        {
            return X86.ass_library_version();
        }
    }

    public static IntPtr ass_library_init()
    {
        if (Is64Bit)
        {
            return X64.ass_library_init();
        }
        else
        {
            return X86.ass_library_init();
        }
    }

    public static void ass_library_done(IntPtr library)
    {
        if (Is64Bit)
        {
            X64.ass_library_done(library);
        }
        else
        {
            X86.ass_library_done(library);
        }
    }

    public static void ass_set_fonts_dir(IntPtr library, string? fontsDir)
    {
        if (Is64Bit)
        {
            X64.ass_set_fonts_dir(library, fontsDir);
        }
        else
        {
            X86.ass_set_fonts_dir(library, fontsDir);
        }
    }

    public static void ass_set_extract_fonts(IntPtr library, bool extract)
    {
        if (Is64Bit)
        {
            X64.ass_set_extract_fonts(library, extract);
        }
        else
        {
            X86.ass_set_extract_fonts(library, extract);
        }
    }

    public static void ass_set_style_overrides(IntPtr library, IntPtr list)
    {
        if (Is64Bit)
        {
            X64.ass_set_style_overrides(library, list);
        }
        else
        {
            X86.ass_set_style_overrides(library, list);
        }
    }

    public static void ass_process_force_style(IntPtr track)
    {
        if (Is64Bit)
        {
            X64.ass_process_force_style(track);
        }
        else
        {
            X86.ass_process_force_style(track);
        }
    }

    public static void ass_set_message_cb(IntPtr library, IntPtr callback, IntPtr userData)
    {
        if (Is64Bit)
        {
            X64.ass_set_message_cb(library, callback, userData);
        }
        else
        {
            X86.ass_set_message_cb(library, callback, userData);
        }
    }

    public static void ass_set_message_cb(IntPtr library, MessageCallback? callback, IntPtr userData)
    {
        if (Is64Bit)
        {
            X64.ass_set_message_cb(library, callback, userData);
        }
        else
        {
            X86.ass_set_message_cb(library, callback, userData);
        }
    }

    public static IntPtr ass_renderer_init(IntPtr library)
    {
        if (Is64Bit)
        {
            return X64.ass_renderer_init(library);
        }
        else
        {
            return X86.ass_renderer_init(library);
        }
    }

    public static void ass_renderer_done(IntPtr renderer)
    {
        if (Is64Bit)
        {
            X64.ass_renderer_done(renderer);
        }
        else
        {
            X86.ass_renderer_done(renderer);
        }
    }

    public static void ass_set_frame_size(IntPtr renderer, int width, int height)
    {
        if (Is64Bit)
        {
            X64.ass_set_frame_size(renderer, width, height);
        }
        else
        {
            X86.ass_set_frame_size(renderer, width, height);
        }
    }

    public static void ass_set_storage_size(IntPtr renderer, int width, int height)
    {
        if (Is64Bit)
        {
            X64.ass_set_storage_size(renderer, width, height);
        }
        else
        {
            X86.ass_set_storage_size(renderer, width, height);
        }
    }

    public static void ass_set_shaper(IntPtr renderer, ShapingLevel level)
    {
        if (Is64Bit)
        {
            X64.ass_set_shaper(renderer, level);
        }
        else
        {
            X86.ass_set_shaper(renderer, level);
        }
    }

    public static void ass_set_margins(IntPtr renderer, int top, int bottom, int left, int right)
    {
        if (Is64Bit)
        {
            X64.ass_set_margins(renderer, top, bottom, left, right);
        }
        else
        {
            X86.ass_set_margins(renderer, top, bottom, left, right);
        }
    }

    public static void ass_set_use_margins(IntPtr renderer, bool use)
    {
        if (Is64Bit)
        {
            X64.ass_set_use_margins(renderer, use);
        }
        else
        {
            X86.ass_set_use_margins(renderer, use);
        }
    }

    public static void ass_set_pixel_aspect(IntPtr renderer, double pixelAspectRatio)
    {
        if (Is64Bit)
        {
            X64.ass_set_pixel_aspect(renderer, pixelAspectRatio);
        }
        else
        {
            X86.ass_set_pixel_aspect(renderer, pixelAspectRatio);
        }
    }

    public static void ass_set_aspect_ratio(IntPtr renderer, double displayAspectRatio, double storageAspectRatio)
    {
        if (Is64Bit)
        {
            X64.ass_set_aspect_ratio(renderer, displayAspectRatio, storageAspectRatio);
        }
        else
        {
            X86.ass_set_aspect_ratio(renderer, displayAspectRatio, storageAspectRatio);
        }
    }

    public static void ass_set_font_scale(IntPtr renderer, double fontScale)
    {
        if (Is64Bit)
        {
            X64.ass_set_font_scale(renderer, fontScale);
        }
        else
        {
            X86.ass_set_font_scale(renderer, fontScale);
        }
    }

    public static void ass_set_hinting(IntPtr renderer, Hinting hinting)
    {
        if (Is64Bit)
        {
            X64.ass_set_hinting(renderer, hinting);
        }
        else
        {
            X86.ass_set_hinting(renderer, hinting);
        }
    }

    public static void ass_set_line_spacing(IntPtr renderer, double lineSpacing)
    {
        if (Is64Bit)
        {
            X64.ass_set_line_spacing(renderer, lineSpacing);
        }
        else
        {
            X86.ass_set_line_spacing(renderer, lineSpacing);
        }
    }

    public static void ass_set_line_position(IntPtr renderer, double linePosition)
    {
        if (Is64Bit)
        {
            X64.ass_set_line_position(renderer, linePosition);
        }
        else
        {
            X86.ass_set_line_position(renderer, linePosition);
        }
    }

    public static void ass_get_available_font_providers(IntPtr library, out IntPtr providers, out UIntPtr size)
    {
        if (Is64Bit)
        {
            X64.ass_get_available_font_providers(library, out providers, out size);
        }
        else
        {
            X86.ass_get_available_font_providers(library, out providers, out size);
        }
    }

    public static void ass_set_fonts(IntPtr renderer, string? defaultFont, string? defaultFamily, DefaultFontProvider fontProvider, string? config, bool update)
    {
        if (Is64Bit)
        {
            X64.ass_set_fonts(renderer, defaultFont, defaultFamily, fontProvider, config, update);
        }
        else
        {
            X86.ass_set_fonts(renderer, defaultFont, defaultFamily, fontProvider, config, update);
        }
    }

    public static void ass_set_selective_style_override_enabled(IntPtr renderer, OverrideBits bits)
    {
        if (Is64Bit)
        {
            X64.ass_set_selective_style_override_enabled(renderer, bits);
        }
        else
        {
            X86.ass_set_selective_style_override_enabled(renderer, bits);
        }
    }

    public static void ass_set_selective_style_override(IntPtr renderer, in AssStyle style)
    {
        if (Is64Bit)
        {
            X64.ass_set_selective_style_override(renderer, in style);
        }
        else
        {
            X86.ass_set_selective_style_override(renderer, in style);
        }
    }

    public static void ass_fonts_update(IntPtr renderer)
    {
        if (Is64Bit)
        {
            X64.ass_fonts_update(renderer);
        }
        else
        {
            X86.ass_fonts_update(renderer);
        }
    }

    public static void ass_set_cache_limits(IntPtr renderer, int glyphMax, int bitmapMaxSize)
    {
        if (Is64Bit)
        {
            X64.ass_set_cache_limits(renderer, glyphMax, bitmapMaxSize);
        }
        else
        {
            X86.ass_set_cache_limits(renderer, glyphMax, bitmapMaxSize);
        }
    }

    public static IntPtr ass_render_frame(IntPtr renderer, IntPtr track, long now, out FrameChange detectChange)
    {
        if (Is64Bit)
        {
            return X64.ass_render_frame(renderer, track, now, out detectChange);
        }
        else
        {
            return X86.ass_render_frame(renderer, track, now, out detectChange);
        }
    }

    public static IntPtr ass_new_track(IntPtr library)
    {
        if (Is64Bit)
        {
            return X64.ass_new_track(library);
        }
        else
        {
            return X86.ass_new_track(library);
        }
    }

    public static void ass_free_track(IntPtr track)
    {
        if (Is64Bit)
        {
            X64.ass_free_track(track);
        }
        else
        {
            X86.ass_free_track(track);
        }
    }

    public static int ass_alloc_style(IntPtr track)
    {
        if (Is64Bit)
        {
            return X64.ass_alloc_style(track);
        }
        else
        {
            return X86.ass_alloc_style(track);
        }
    }

    public static int ass_alloc_event(IntPtr track)
    {
        if (Is64Bit)
        {
            return X64.ass_alloc_event(track);
        }
        else
        {
            return X86.ass_alloc_event(track);
        }
    }

    public static void ass_free_style(IntPtr track, int styleId)
    {
        if (Is64Bit)
        {
            X64.ass_free_style(track, styleId);
        }
        else
        {
            X86.ass_free_style(track, styleId);
        }
    }

    public static void ass_free_event(IntPtr track, int eventId)
    {
        if (Is64Bit)
        {
            X64.ass_free_event(track, eventId);
        }
        else
        {
            X86.ass_free_event(track, eventId);
        }
    }

    public static void ass_process_data(IntPtr track, string data, int size)
    {
        if (Is64Bit)
        {
            X64.ass_process_data(track, data, size);
        }
        else
        {
            X86.ass_process_data(track, data, size);
        }
    }

    public static void ass_process_codec_private(IntPtr track, string data, int size)
    {
        if (Is64Bit)
        {
            X64.ass_process_codec_private(track, data, size);
        }
        else
        {
            X86.ass_process_codec_private(track, data, size);
        }
    }

    public static void ass_process_chunk(IntPtr track, string data, int size, long timeCode, long duration)
    {
        if (Is64Bit)
        {
            X64.ass_process_chunk(track, data, size, timeCode, duration);
        }
        else
        {
            X86.ass_process_chunk(track, data, size, timeCode, duration);
        }
    }

    public static void ass_set_check_readorder(IntPtr track, bool checkReadOrder)
    {
        if (Is64Bit)
        {
            X64.ass_set_check_readorder(track, checkReadOrder);
        }
        else
        {
            X86.ass_set_check_readorder(track, checkReadOrder);
        }
    }

    public static void ass_flush_events(IntPtr track)
    {
        if (Is64Bit)
        {
            X64.ass_flush_events(track);
        }
        else
        {
            X86.ass_flush_events(track);
        }
    }

    public static IntPtr ass_read_file(IntPtr library, string fileName, string? codePage)
    {
        if (Is64Bit)
        {
            return X64.ass_read_file(library, fileName, codePage);
        }
        else
        {
            return X86.ass_read_file(library, fileName, codePage);
        }
    }

    public static IntPtr ass_read_memory(IntPtr library, IntPtr buffer, int bufferSize, string? codePage)
    {
        if (Is64Bit)
        {
            return X64.ass_read_memory(library, buffer, bufferSize, codePage);
        }
        else
        {
            return X86.ass_read_memory(library, buffer, bufferSize, codePage);
        }
    }

    public static int ass_read_styles(IntPtr track, string fileName, string? codePage)
    {
        if (Is64Bit)
        {
            return X64.ass_read_styles(track, fileName, codePage);
        }
        else
        {
            return X86.ass_read_styles(track, fileName, codePage);
        }
    }

    public static void ass_add_font(IntPtr library, string name, IntPtr data, int dataSize)
    {
        if (Is64Bit)
        {
            X64.ass_add_font(library, name, data, dataSize);
        }
        else
        {
            X86.ass_add_font(library, name, data, dataSize);
        }
    }

    public static void ass_clear_fonts(IntPtr library)
    {
        if (Is64Bit)
        {
            X64.ass_clear_fonts(library);
        }
        else
        {
            X86.ass_clear_fonts(library);
        }
    }

    public static void ass_step_sub(IntPtr track, long now, int movement)
    {
        if (Is64Bit)
        {
            X64.ass_step_sub(track, now, movement);
        }
        else
        {
            X86.ass_step_sub(track, now, movement);
        }
    }

    private static bool Is64Bit
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Environment.Is64BitProcess;
    }

    private const string LibraryBaseName = "ass.dll";
    private const CallingConvention AssConvention = CallingConvention.Cdecl;
    private const CharSet AssCharSet = CharSet.Ansi;

}
