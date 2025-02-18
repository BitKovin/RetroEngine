using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Text.Extensions;
using SharpFont;

namespace MonoGame.Extended.Text
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a dynamic sprite font.
    /// This class cannot be inherited.
    /// </summary>
    public sealed class DynamicSpriteFont : DisposableBase
    {
        private const char CrChar = '\r';
        private const char LfChar = '\n';
        private static readonly char[] LineSeparators = { LfChar };

        private readonly Dictionary<char, Texture2D>? _texturesDict;
        private readonly Dictionary<char, GlyphMetrics> _metricsDict;
        private readonly GraphicsDevice? _graphics;
        private readonly Font? _fallbackFont;

        /// <summary>
        /// Creates a new <see cref="DynamicSpriteFont"/> instance.
        /// This created instance can only be used for measuring strings.
        /// </summary>
        /// <param name="font">The primary <see cref="Font"/> to use.</param>
        /// <param name="fallbackFont">
        /// An optional fallback <see cref="Font"/> that is used if the primary font doesn’t contain a symbol
        /// or its glyph is empty.
        /// </param>
        public DynamicSpriteFont(Font font, Font? fallbackFont = null)
            : this(null, font, fallbackFont)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DynamicSpriteFont"/> instance.
        /// </summary>
        /// <param name="graphics">
        /// The <see cref="GraphicsDevice"/> to manage underlying textures. If <see langword="null"/>,
        /// this instance cannot be used for drawing.
        /// </param>
        /// <param name="font">The primary <see cref="Font"/> to use.</param>
        /// <param name="fallbackFont">
        /// An optional fallback <see cref="Font"/> that is used if the primary font doesn’t contain a symbol
        /// or its glyph is empty.
        /// </param>
        public DynamicSpriteFont(GraphicsDevice? graphics, Font font, Font? fallbackFont = null)
        {
            if (graphics is not null)
            {
                _texturesDict = new Dictionary<char, Texture2D>();
            }

            _metricsDict = new Dictionary<char, GlyphMetrics>();
            _graphics = graphics;

            Font = font;
            _fallbackFont = fallbackFont;
        }

        /// <summary>
        /// The associated primary <see cref="Font"/> object.
        /// </summary>
        public Font Font { get; }

        /// <summary>
        /// The fallback <see cref="Font"/> to use when the primary font doesn’t include a symbol or returns an empty glyph.
        /// </summary>
        public Font? FallbackFont => _fallbackFont;

        /// <summary>
        /// Gets whether this <see cref="DynamicSpriteFont"/> can be used for drawing.
        /// </summary>
        public bool CanDraw => _graphics is not null;

        /// <summary>
        /// Measures a string and returns the size when it is drawn with a fixed line height.
        /// </summary>
        /// <param name="str">The text to measure.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="characterSpacing">The character spacing.</param>
        /// <param name="lineHeight">Line height.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public Vector2 MeasureString(string str, Vector2 maxBounds, Vector2 scale, float characterSpacing, float lineHeight)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (string.IsNullOrEmpty(str))
            {
                return Vector2.Zero;
            }

            return DrawOrMeasure(null, str, Vector2.Zero, maxBounds, scale, characterSpacing, lineHeight, Color.White);
        }

        /// <summary>
        /// Measures a string using default parameters and returns the size when it is drawn.
        /// </summary>
        /// <param name="str">The text to measure.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public Vector2 MeasureString(string str)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            Vector2 maxBounds = new Vector2(100000, 100000);
            Vector2 scale = Vector2.One;
            float characterSpacing = 1.0f;
            float lineHeight = 1.0f;

            if (string.IsNullOrEmpty(str))
            {
                return Vector2.Zero;
            }

            return DrawOrMeasure(null, str, Vector2.Zero, maxBounds, scale, characterSpacing, lineHeight, Color.White);
        }

        /// <summary>
        /// Measures a string and returns the size when it is drawn with variable line heights.
        /// </summary>
        /// <param name="str">The text to measure.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="spacing">
        /// The spacing factor. Its X component is used for character spacing, and Y component for line spacing.
        /// </param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public Vector2 MeasureString(string str, Vector2 maxBounds, Vector2 scale, Vector2 spacing)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (string.IsNullOrEmpty(str))
            {
                return Vector2.Zero;
            }

            return DrawOrMeasure(null, str, Vector2.Zero, maxBounds, scale, spacing, Color.White);
        }

        /// <summary>
        /// Draws or measures a string with a fixed line height.
        /// </summary>
        /// <param name="spriteBatch">
        /// The <see cref="SpriteBatch"/> to use. If <see langword="null"/>, only measuring is performed.
        /// </param>
        /// <param name="str">The text to draw or measure.</param>
        /// <param name="location">The top left location of the drawn string.</param>
        /// <param name="maxBounds">
        /// Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.
        /// </param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="characterSpacing">The character spacing.</param>
        /// <param name="lineHeight">The fixed line height.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        internal Vector2 DrawOrMeasure(SpriteBatch? spriteBatch, string str, Vector2 location, Vector2 maxBounds, Vector2 scale, float characterSpacing, float lineHeight, Color color)
        {
            if (maxBounds.X.Equals(0))
            {
                maxBounds.X = float.MaxValue;
            }

            if (maxBounds.Y.Equals(0))
            {
                maxBounds.Y = float.MaxValue;
            }

            if (lineHeight.Equals(0))
            {
                lineHeight = Font.Size;
            }

            AddCharInfo(LfChar, null);
            AddStringInfo(str);

            float actualBoundsX = 0;
            float? lastYMin = null;
            float? firstBaseLineY = null;

            var lines = str.Split(LineSeparators, StringSplitOptions.None);
            var currentLineIndex = 0;
            var canDraw = CanDraw;

            foreach (var line in lines)
            {
                var outOfBounds = false;
                var nextCharIndex = 0;

                do
                {
                    var currentLineWidth = 0.0f;
                    var yMax = float.MinValue;
                    var yMin = float.MaxValue;
                    var iterationBegin = nextCharIndex;

                    for (var j = iterationBegin; j < line.Length; ++j)
                    {
                        var ch = line[j];
                        nextCharIndex = j + 1;
                        var metrics = _metricsDict[ch];

                        if (ch != CrChar)
                        {
                            float charWidth = metrics.Width.ToSingle() > 0
                                ? metrics.Width.ToSingle()
                                : metrics.HorizontalAdvance.ToSingle();

                            var expectedLineWidth = currentLineWidth + charWidth * scale.X;

                            if (j > 0)
                            {
                                expectedLineWidth += characterSpacing * scale.X;
                            }

                            if (expectedLineWidth > maxBounds.X)
                            {
                                nextCharIndex = j;
                                break;
                            }

                            currentLineWidth = expectedLineWidth;
                        }

                        var yTop = metrics.HorizontalBearingY.ToSingle();
                        var yBottom = yTop - metrics.Height.ToSingle();
                        yMax = Math.Max(yMax, yTop);
                        yMin = Math.Min(yMin, yBottom);
                    }

                    if (nextCharIndex == iterationBegin)
                    {
                        outOfBounds = true;
                        break;
                    }

                    if (firstBaseLineY is null)
                    {
                        firstBaseLineY = yMax;
                    }

                    if ((firstBaseLineY.Value + lineHeight * currentLineIndex - yMin) * scale.Y > maxBounds.Y)
                    {
                        outOfBounds = true;
                    }

                    if (outOfBounds)
                    {
                        if (currentLineIndex is 0)
                        {
                            firstBaseLineY = null;
                        }

                        break;
                    }

                    if (canDraw && _texturesDict is not null && spriteBatch is not null)
                    {
                        var currentX = location.X;
                        var currentY = location.Y + (firstBaseLineY.Value + lineHeight * currentLineIndex) * scale.Y;

                        for (var j = iterationBegin; j < nextCharIndex; ++j)
                        {
                            var ch = line[j];

                            if (ch is CrChar)
                            {
                                continue;
                            }

                            var metrics = _metricsDict[ch];

                            if (_texturesDict.ContainsKey(ch))
                            {
                                var texture = _texturesDict[ch];
                                var charX = currentX;
                                var charY = currentY - metrics.HorizontalBearingY.ToSingle() * scale.Y;
                                spriteBatch.Draw(texture, new Vector2(charX, charY), null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                            }

                            if (metrics.Width.ToSingle() > 0)
                            {
                                currentX += (metrics.Width.ToSingle() + characterSpacing) * scale.X;
                            }
                            else
                            {
                                currentX += (metrics.HorizontalAdvance.ToSingle() + characterSpacing) * scale.X;
                            }
                        }
                    }

                    lastYMin = yMin;
                    actualBoundsX = Math.Max(actualBoundsX, currentLineWidth);
                    ++currentLineIndex;
                }
                while (nextCharIndex < line.Length);

                if (outOfBounds)
                {
                    break;
                }
            }

            var actualBoundsY = Math.Max(currentLineIndex - 1, 0) * lineHeight * scale.Y;

            if (firstBaseLineY is not null)
            {
                actualBoundsY += firstBaseLineY.Value * scale.Y;
            }

            if (lastYMin is not null)
            {
                actualBoundsY -= lastYMin.Value * scale.Y;
            }

            return new Vector2(actualBoundsX, actualBoundsY);
        }

        /// <summary>
        /// Draws or measures a string with variable line heights.
        /// </summary>
        /// <param name="spriteBatch">
        /// The <see cref="SpriteBatch"/> to use. If <see langword="null"/>, only measuring is performed.
        /// </param>
        /// <param name="str">The text to draw or measure.</param>
        /// <param name="location">The top left location of the drawn string.</param>
        /// <param name="maxBounds">
        /// Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.
        /// </param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="spacing">
        /// The spacing factor. Its X component is used for character spacing, and Y component for line spacing.
        /// </param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        internal Vector2 DrawOrMeasure(SpriteBatch? spriteBatch, string str, Vector2 location, Vector2 maxBounds, Vector2 scale, Vector2 spacing, Color color)
        {
            if (maxBounds.X.Equals(0))
            {
                maxBounds.X = float.MaxValue;
            }

            if (maxBounds.Y.Equals(0))
            {
                maxBounds.Y = float.MaxValue;
            }

            AddCharInfo(LfChar, null);
            AddStringInfo(str);

            float actualBoundsX = 0;
            float actualBoundsY = 0;
            var newLineCharMetrics = _metricsDict[LfChar];

            var lines = str.Split(LineSeparators, StringSplitOptions.None);
            var currentLineIndex = 0;
            var canDraw = CanDraw;

            foreach (var line in lines)
            {
                var outOfBounds = false;
                var nextCharIndex = 0;

                do
                {
                    var currentLineHeight = newLineCharMetrics.Height.ToSingle();
                    var currentLineWidth = 0.0f;
                    var yMax = float.MinValue;
                    var yMin = float.MaxValue;
                    var iterationBegin = nextCharIndex;

                    for (var j = iterationBegin; j < line.Length; ++j)
                    {
                        var ch = line[j];
                        nextCharIndex = j + 1;
                        var metrics = _metricsDict[ch];

                        if (ch is not CrChar)
                        {
                            float charWidth = metrics.Width.ToSingle() > 0
                                ? metrics.Width.ToSingle()
                                : metrics.HorizontalAdvance.ToSingle();

                            var expectedLineWidth = currentLineWidth + charWidth * scale.X;

                            if (j > 0)
                            {
                                expectedLineWidth += spacing.X * scale.X;
                            }

                            if (expectedLineWidth > maxBounds.X)
                            {
                                nextCharIndex = j;
                                break;
                            }

                            currentLineWidth = expectedLineWidth;
                        }

                        var yTop = metrics.HorizontalBearingY.ToSingle();
                        var yBottom = yTop - metrics.Height.ToSingle();
                        yMax = Math.Max(yMax, yTop);
                        yMin = Math.Min(yMin, yBottom);
                    }

                    if (nextCharIndex == iterationBegin)
                    {
                        outOfBounds = true;
                        break;
                    }

                    if (line.Length > 0)
                    {
                        currentLineHeight = (yMax - yMin) * scale.Y;
                    }

                    if (actualBoundsY + currentLineHeight > maxBounds.Y)
                    {
                        outOfBounds = true;
                    }

                    if (outOfBounds)
                    {
                        break;
                    }

                    if (canDraw && _texturesDict is not null && spriteBatch is not null)
                    {
                        var currentX = location.X;
                        var currentY = location.Y + actualBoundsY * scale.Y;

                        if (currentLineIndex > 0)
                        {
                            currentY += spacing.Y * scale.Y;
                        }

                        for (var j = iterationBegin; j < nextCharIndex; ++j)
                        {
                            var ch = line[j];

                            if (ch is CrChar)
                            {
                                continue;
                            }

                            var metrics = _metricsDict[ch];

                            if (_texturesDict.ContainsKey(ch))
                            {
                                var texture = _texturesDict[ch];
                                var charX = currentX;
                                var charY = currentY + (yMax - metrics.HorizontalBearingY.ToSingle()) * scale.Y;
                                spriteBatch.Draw(texture, new Vector2(charX, charY), null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                            }

                            if (metrics.Width.ToSingle() > 0)
                            {
                                currentX += (metrics.Width.ToSingle() + spacing.X) * scale.X;
                            }
                            else
                            {
                                currentX += (metrics.HorizontalAdvance.ToSingle() + spacing.X) * scale.X;
                            }
                        }
                    }

                    actualBoundsX = Math.Max(actualBoundsX, currentLineWidth);
                    actualBoundsY += currentLineHeight + spacing.Y * scale.Y;
                    ++currentLineIndex;
                }
                while (nextCharIndex < line.Length);

                if (outOfBounds)
                {
                    break;
                }
            }

            if (actualBoundsY > 0)
            {
                actualBoundsY -= spacing.Y * scale.Y;
            }

            return new Vector2(actualBoundsX, actualBoundsY);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_texturesDict is not null)
            {
                foreach (var value in _texturesDict.Values)
                {
                    value.Dispose();
                }

                _texturesDict.Clear();
            }

            _metricsDict.Clear();
        }

        /// <summary>
        /// Adds information of characters in a string to the cache.
        /// </summary>
        /// <param name="str">The string containing characters to add.</param>
        private void AddStringInfo(string str)
        {
            foreach (var ch in str)
            {
                if (ch is LfChar)
                {
                    // This character is already cached.
                    continue;
                }

                AddCharInfo(ch, null);
            }
        }

        /// <summary>
        /// Adds the information of a character to the cache.
        /// </summary>
        /// <param name="char">The character to add.</param>
        /// <param name="nextChar">
        /// The next character. When set to <see langword="null"/>, it means ignoring the next character.
        /// </param>
        private void AddCharInfo(char @char, char? nextChar)
        {
            if (_metricsDict.ContainsKey(@char))
            {
                return;
            }

            var canDraw = CanDraw;
            // Start with the primary font.
            var fontFace = Font.FontFace;
            var glyphIndex = fontFace.GetCharIndex(@char);
            Font usedFont = Font;

            // If the primary font doesn’t contain the glyph (index 0) and a fallback is provided, switch.
            if (glyphIndex == 0 && _fallbackFont is not null)
            {
                fontFace = _fallbackFont.FontFace;
                glyphIndex = fontFace.GetCharIndex(@char);
                usedFont = _fallbackFont;
            }

            // Load the glyph with rendering flags if drawing is supported.
            var loadFlags = canDraw ? LoadFlags.Render : LoadFlags.Default;
            fontFace.LoadGlyph(glyphIndex, loadFlags, LoadTarget.Normal);
            var glyph = fontFace.Glyph;

            // If the glyph bitmap is empty and we haven’t yet tried the fallback, try it.
            if (glyph.Bitmap.Buffer == IntPtr.Zero && usedFont == Font && _fallbackFont is not null)
            {
                fontFace = _fallbackFont.FontFace;
                glyphIndex = fontFace.GetCharIndex(@char);
                usedFont = _fallbackFont;
                fontFace.LoadGlyph(glyphIndex, loadFlags, LoadTarget.Normal);
                glyph = fontFace.Glyph;
            }

            _metricsDict.Add(@char, glyph.Metrics);

            if (canDraw && _texturesDict is not null)
            {
                if (glyph.Bitmap.Buffer == IntPtr.Zero)
                {
                    return;
                }

                // Some fonts might have size.X or size.Y equal to 0, so we add a one-pixel redundancy.
                var charSize = fontFace.GetCharSize(glyphIndex, glyph.Metrics, nextChar, 1, 1);
                var texture = new Texture2D(_graphics, (int)Math.Round(charSize.X), (int)Math.Round(charSize.Y), false, SurfaceFormat.Color);
                glyph.Bitmap.RenderToTexture(texture);
                _texturesDict.Add(@char, texture);
            }
        }
    }
}
