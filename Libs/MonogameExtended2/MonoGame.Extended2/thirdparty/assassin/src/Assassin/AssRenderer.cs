using System;
using System.Diagnostics;
using Assassin.Native;

namespace Assassin
{
    public class AssRenderer : IDisposableEx, INativeResource<AssRendererHandle>
    {

        internal AssRenderer(AssRendererHandle safeHandle)
        {
            SafeHandle = safeHandle;
        }

        ~AssRenderer()
        {
            Dispose(false);
        }

        public void SetFrameSize(int width, int height)
        {
            this.EnsureNotDisposed();

            NativeMethods.ass_set_frame_size(SafeHandle.DangerousGetHandle(), width, height);

            _width = width;
            _height = height;
        }

        public void SetFonts(string familyName)
        {
            this.EnsureNotDisposed();

            NativeMethods.ass_set_fonts(SafeHandle.DangerousGetHandle(), null, familyName, DefaultFontProvider.AutoDetect, null, true);
        }

        public AssImage RenderFrame(AssTrack track, long timestamp)
        {
            return RenderFrame(track, timestamp, out _);
        }

        public AssImage RenderFrame(AssTrack track, long timestamp, out FrameChange frameChange)
        {
            this.EnsureNotDisposed();

            if (_width <= 0 || _height <= 0)
            {
                throw new AssException("Cannot render frame: frame dimensions are not initialized");
            }

            var framePointer = NativeMethods.ass_render_frame(SafeHandle.DangerousGetHandle(), track.SafeHandle.DangerousGetHandle(), timestamp, out frameChange);

            // framePointer == nullptr => the image is blank
            return new AssImage(Width, Height, framePointer);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool IsDisposed { get; private set; }

        public int Width
        {
            [DebuggerStepThrough]
            get => _width;
        }

        public int Height
        {
            [DebuggerStepThrough]
            get => _height;
        }

        public AssRendererHandle SafeHandle { get; }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            if (disposing)
            {
                SafeHandle.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private int _width;
        private int _height;

    }
}
