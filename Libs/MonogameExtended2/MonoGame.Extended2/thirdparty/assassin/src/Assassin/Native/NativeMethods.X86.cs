using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Assassin.Native;

partial class NativeMethods
{

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    private static unsafe class X86
    {

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern int ass_library_version();

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern IntPtr ass_library_init();

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_library_done(IntPtr library);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_fonts_dir(IntPtr library, [MarshalAs(UnmanagedType.LPStr)] string? fontsDir);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_extract_fonts(IntPtr library, [MarshalAs(UnmanagedType.Bool)] bool extract);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_style_overrides(IntPtr library, IntPtr list);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_process_force_style(IntPtr track);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_message_cb(IntPtr library, IntPtr callback, IntPtr userData);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_message_cb(
            IntPtr library,
            [MarshalAs(UnmanagedType.FunctionPtr)] MessageCallback? callback,
            IntPtr userData);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern IntPtr ass_renderer_init(IntPtr library);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_renderer_done(IntPtr renderer);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_frame_size(IntPtr renderer, int width, int height);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_storage_size(IntPtr renderer, int width, int height);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_shaper(IntPtr renderer, ShapingLevel level);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_margins(IntPtr renderer, int top, int bottom, int left, int right);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_use_margins(IntPtr renderer, [MarshalAs(UnmanagedType.Bool)] bool use);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_pixel_aspect(IntPtr renderer, double pixelAspectRatio);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_aspect_ratio(IntPtr renderer, double displayAspectRatio, double storageAspectRatio);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_font_scale(IntPtr renderer, double fontScale);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_hinting(IntPtr renderer, Hinting hinting);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_line_spacing(IntPtr renderer, double lineSpacing);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_line_position(IntPtr renderer, double linePosition);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_get_available_font_providers(IntPtr library, out IntPtr providers, out UIntPtr size);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_fonts(
            IntPtr renderer,
            [MarshalAs(UnmanagedType.LPStr)] string? defaultFont,
            [MarshalAs(UnmanagedType.LPStr)] string? defaultFamily,
            DefaultFontProvider fontProvider,
            [MarshalAs(UnmanagedType.LPStr)] string? config,
            [MarshalAs(UnmanagedType.Bool)] bool update);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_selective_style_override_enabled(IntPtr renderer, OverrideBits bits);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_selective_style_override(IntPtr renderer, in AssStyle style);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_fonts_update(IntPtr renderer);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_cache_limits(IntPtr renderer, int glyphMax, int bitmapMaxSize);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern IntPtr ass_render_frame(IntPtr renderer, IntPtr track, long now, out FrameChange detectChange);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern IntPtr ass_new_track(IntPtr library);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_free_track(IntPtr track);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern int ass_alloc_style(IntPtr track);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern int ass_alloc_event(IntPtr track);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_free_style(IntPtr track, int styleId);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_free_event(IntPtr track, int eventId);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_process_data(
            IntPtr track,
            [MarshalAs(UnmanagedType.LPStr)] string data,
            int size);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_process_codec_private(
            IntPtr track,
            [MarshalAs(UnmanagedType.LPStr)] string data,
            int size);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_process_chunk(
            IntPtr track,
            [MarshalAs(UnmanagedType.LPStr)] string data,
            int size,
            long timeCode,
            long duration);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_set_check_readorder(IntPtr track, [MarshalAs(UnmanagedType.Bool)] bool checkReadOrder);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_flush_events(IntPtr track);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern IntPtr ass_read_file(
            IntPtr library,
            [MarshalAs(UnmanagedType.LPStr)] string fileName,
            [MarshalAs(UnmanagedType.LPStr)] string? codePage);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern IntPtr ass_read_memory(
            IntPtr library,
            IntPtr buffer,
            int bufferSize,
            [MarshalAs(UnmanagedType.LPStr)] string? codePage);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern int ass_read_styles(
            IntPtr track,
            [MarshalAs(UnmanagedType.LPStr)] string fileName,
            [MarshalAs(UnmanagedType.LPStr)] string? codePage);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_add_font(
            IntPtr library,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr data,
            int dataSize);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_clear_fonts(IntPtr library);

        [DllImport(LibraryName, CallingConvention = AssConvention, CharSet = AssCharSet)]
        public static extern void ass_step_sub(IntPtr track, long now, int movement);

        private const string LibraryDir = "x86";
        private const string LibraryName = LibraryDir + "/" + LibraryBaseName;

    }

}
