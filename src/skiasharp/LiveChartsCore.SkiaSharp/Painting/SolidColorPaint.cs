﻿// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Painting;

/// <summary>
/// Defines a set of geometries that will be painted using a solid color.
/// </summary>
/// <seealso cref="Paint" />
public class SolidColorPaint : SkiaPaint
{
    private SkiaSharpDrawingContext? _drawingContext;
    internal SKPaint? _skiaPaint;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorPaint"/> class.
    /// </summary>
    public SolidColorPaint()
        : base()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorPaint"/> class.
    /// </summary>
    /// <param name="color">The color.</param>
    public SolidColorPaint(SKColor color)
        : base()
    {
        Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorPaint"/> class.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="strokeWidth">Width of the stroke.</param>
    public SolidColorPaint(SKColor color, float strokeWidth)
        : base(strokeWidth)
    {
        Color = color;
    }

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    /// <value>
    /// The color.
    /// </value>
    public SKColor Color { get; set; }

    /// <inheritdoc cref="Paint.CloneTask" />
    public override Paint CloneTask()
    {
        var clone = new SolidColorPaint
        {
            PaintStyle = PaintStyle,
            Color = Color,
            IsAntialias = IsAntialias,
            StrokeThickness = StrokeThickness,
            StrokeCap = StrokeCap,
            StrokeJoin = StrokeJoin,
            StrokeMiter = StrokeMiter,
            FontFamily = FontFamily,
            SKFontStyle = SKFontStyle,
            SKTypeface = SKTypeface,
            PathEffect = PathEffect?.Clone(),
            ImageFilter = ImageFilter?.Clone()
        };

        return clone;
    }

    /// <inheritdoc cref="Paint.InitializeTask(DrawingContext)" />

    public override void InitializeTask(DrawingContext drawingContext)
    {
        var skiaContext = (SkiaSharpDrawingContext)drawingContext;
        _skiaPaint ??= new SKPaint();

        _skiaPaint.Color = Color;
        _skiaPaint.IsAntialias = IsAntialias;
        _skiaPaint.StrokeCap = StrokeCap;
        _skiaPaint.StrokeJoin = StrokeJoin;
        _skiaPaint.StrokeMiter = StrokeMiter;
        _skiaPaint.StrokeWidth = StrokeThickness;
        _skiaPaint.Style = PaintStyle.HasFlag(PaintStyle.Stroke) ? SKPaintStyle.Stroke : SKPaintStyle.Fill;

        if (HasCustomFont) _skiaPaint.Typeface = GetSKTypeface();

        if (PathEffect is not null)
        {
            PathEffect.CreateEffect(skiaContext);
            _skiaPaint.PathEffect = PathEffect.SKPathEffect;
        }

        if (ImageFilter is not null)
        {
            ImageFilter.CreateFilter(skiaContext);
            _skiaPaint.ImageFilter = ImageFilter.SKImageFilter;
        }

        var clip = GetClipRectangle(skiaContext.MotionCanvas);
        if (clip != LvcRectangle.Empty)
        {
            _ = skiaContext.Canvas.Save();
            skiaContext.Canvas.ClipRect(new SKRect(clip.X, clip.Y, clip.X + clip.Width, clip.Y + clip.Height));
            _drawingContext = skiaContext;
        }

        skiaContext.ActiveSkiaPaint = _skiaPaint;
    }

    /// <inheritdoc cref="Paint.ApplyOpacityMask(DrawingContext, float)" />
    public override void ApplyOpacityMask(DrawingContext context, float opacity)
    {
        var skiaContext = (SkiaSharpDrawingContext)context;
        var baseColor = Color;
        skiaContext.ActiveSkiaPaint.Color =
            new SKColor(
                baseColor.Red,
                baseColor.Green,
                baseColor.Blue,
                (byte)(baseColor.Alpha * opacity));
    }

    /// <inheritdoc cref="Paint.RestoreOpacityMask(DrawingContext, float)" />
    public override void RestoreOpacityMask(DrawingContext context, float opacity)
    {
        var skiaContext = (SkiaSharpDrawingContext)context;
        skiaContext.ActiveSkiaPaint.Color = Color;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        // Note #301222
        // Disposing typefaces could cause render issues.
        // Does this causes memory leaks?
        // Should the user dispose typefaces manually?
        //if (HasCustomFont && _skiaPaint != null) _skiaPaint.Typeface.Dispose();
        PathEffect?.Dispose();
        ImageFilter?.Dispose();

        if (_drawingContext is not null && GetClipRectangle(_drawingContext.MotionCanvas) != LvcRectangle.Empty)
        {
            _drawingContext.Canvas.Restore();
            _drawingContext = null;
        }

        _skiaPaint?.Dispose();
        _skiaPaint = null;

        GC.SuppressFinalize(this);
    }
}
