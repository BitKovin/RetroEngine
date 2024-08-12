using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Assassin.Native;

public static unsafe class Color32ImageBlending
{

    public static void Blend(int width, int height, Color32Rgba* srcBitmap, float srcAlpha, Color32Rgba* dstBitmap, float dstAlpha, Color32Rgba* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void Blend(int width, int height, Color32Rgba* srcBitmap, float srcAlpha, Color32Abgr* dstBitmap, float dstAlpha, Color32Rgba* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void Blend(int width, int height, Color32Abgr* srcBitmap, float srcAlpha, Color32Rgba* dstBitmap, float dstAlpha, Color32Rgba* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void Blend(int width, int height, Color32Abgr* srcBitmap, float srcAlpha, Color32Abgr* dstBitmap, float dstAlpha, Color32Rgba* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void Blend(int width, int height, Color32Rgba* srcBitmap, float srcAlpha, Color32Rgba* dstBitmap, float dstAlpha, Color32Abgr* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void Blend(int width, int height, Color32Rgba* srcBitmap, float srcAlpha, Color32Abgr* dstBitmap, float dstAlpha, Color32Abgr* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void Blend(int width, int height, Color32Abgr* srcBitmap, float srcAlpha, Color32Rgba* dstBitmap, float dstAlpha, Color32Abgr* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void Blend(int width, int height, Color32Abgr* srcBitmap, float srcAlpha, Color32Abgr* dstBitmap, float dstAlpha, Color32Abgr* resultBitmap)
    {
        for (var j = 0; j < height; j++)
        {
            var lineOffset = width * j;
            var srcLine = srcBitmap + lineOffset;
            var dstLine = dstBitmap + lineOffset;
            var resLine = resultBitmap + lineOffset;

            for (var i = 0; i < width; i++)
            {
                var srcPixel = srcLine + i;
                var dstPixel = dstLine + i;
                var resPixel = resLine + i;

                BlendPixel(srcPixel, srcAlpha, dstPixel, dstAlpha, resPixel);
            }
        }
    }

    public static void BlendPixel(Color32Rgba* src, float srcAlpha, Color32Rgba* dst, float dstAlpha, Color32Rgba* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    public static void BlendPixel(Color32Abgr* src, float srcAlpha, Color32Rgba* dst, float dstAlpha, Color32Rgba* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    public static void BlendPixel(Color32Rgba* src, float srcAlpha, Color32Abgr* dst, float dstAlpha, Color32Rgba* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    public static void BlendPixel(Color32Abgr* src, float srcAlpha, Color32Abgr* dst, float dstAlpha, Color32Rgba* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    public static void BlendPixel(Color32Rgba* src, float srcAlpha, Color32Rgba* dst, float dstAlpha, Color32Abgr* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    public static void BlendPixel(Color32Abgr* src, float srcAlpha, Color32Rgba* dst, float dstAlpha, Color32Abgr* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    public static void BlendPixel(Color32Rgba* src, float srcAlpha, Color32Abgr* dst, float dstAlpha, Color32Abgr* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    public static void BlendPixel(Color32Abgr* src, float srcAlpha, Color32Abgr* dst, float dstAlpha, Color32Abgr* result)
    {
        var sv = new Vector4(src->R, src->G, src->B, src->A);
        var dv = new Vector4(dst->R, dst->G, dst->B, dst->A);
        var rv = BlendColorVector(ref sv, srcAlpha, ref dv, dstAlpha);
        WriteFinalColorVector(in rv, result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector4 BlendColorVector(ref Vector4 src, float srcAlpha, ref Vector4 dst, float dstAlpha)
    {
        src /= byte.MaxValue;
        dst /= byte.MaxValue;

        var res = src * srcAlpha + dst * dstAlpha;
        res *= byte.MaxValue;

        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteFinalColorVector(in Vector4 v, Color32Rgba* result)
    {
        result->R = (byte)Math.Clamp((int)v.X, byte.MinValue, byte.MaxValue);
        result->G = (byte)Math.Clamp((int)v.Y, byte.MinValue, byte.MaxValue);
        result->B = (byte)Math.Clamp((int)v.Z, byte.MinValue, byte.MaxValue);
        result->A = (byte)Math.Clamp((int)v.W, byte.MinValue, byte.MaxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteFinalColorVector(in Vector4 v, Color32Abgr* result)
    {
        result->R = (byte)Math.Clamp((int)v.X, byte.MinValue, byte.MaxValue);
        result->G = (byte)Math.Clamp((int)v.Y, byte.MinValue, byte.MaxValue);
        result->B = (byte)Math.Clamp((int)v.Z, byte.MinValue, byte.MaxValue);
        result->A = (byte)Math.Clamp((int)v.W, byte.MinValue, byte.MaxValue);
    }

}
