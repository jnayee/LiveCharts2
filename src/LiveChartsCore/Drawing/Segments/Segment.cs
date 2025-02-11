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

using LiveChartsCore.Motion;

namespace LiveChartsCore.Drawing.Segments;

/// <summary>
/// Defines a path segment that is part of a sequence.
/// </summary>
public class Segment : Animatable
{
    private readonly FloatMotionProperty _xiProperty;
    private readonly FloatMotionProperty _yiProperty;
    private readonly FloatMotionProperty _xjProperty;
    private readonly FloatMotionProperty _yjProperty;

    /// <summary>
    /// Initializes a new insance of the <see cref="Segment"/> class.
    /// </summary>
    public Segment()
    {
        _xiProperty = RegisterMotionProperty(new FloatMotionProperty(nameof(Xi), 0f));
        _yiProperty = RegisterMotionProperty(new FloatMotionProperty(nameof(Yi), 0f));
        _xjProperty = RegisterMotionProperty(new FloatMotionProperty(nameof(Xj), 0f));
        _yjProperty = RegisterMotionProperty(new FloatMotionProperty(nameof(Yj), 0f));
    }

    /// <summary>
    /// Gets or sets the segment id, a unique and consecutive integer.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the start point in the X axis.
    /// </summary>
    public float Xi
    {
        get => _xiProperty.GetMovement(this);
        set => _xiProperty.SetMovement(value, this);
    }

    /// <summary>
    /// Gets the start point in the X axis.
    /// </summary>
    public float Yi
    {
        get => _yiProperty.GetMovement(this);
        set => _yiProperty.SetMovement(value, this);
    }

    /// <summary>
    /// Gets the end point in the X axis.
    /// </summary>
    public float Xj
    {
        get => _xjProperty.GetMovement(this);
        set => _xjProperty.SetMovement(value, this);
    }

    /// <summary>
    /// Gets the end point in the Y axis.
    /// </summary>
    public float Yj
    {
        get => _yjProperty.GetMovement(this);
        set => _yjProperty.SetMovement(value, this);
    }

    /// <summary>
    /// Follows the specified segment.
    /// </summary>
    /// <param name="segment"></param>
    public virtual void Follows(Segment segment)
    {
        IsValid = segment.IsValid;
        CurrentTime = segment.CurrentTime;
        RemoveOnCompleted = segment.RemoveOnCompleted;

        var xProp = segment.MotionProperties[nameof(Xj)];
        var yProp = segment.MotionProperties[nameof(Yj)];

        MotionProperties[nameof(Xi)].CopyFrom(xProp);
        MotionProperties[nameof(Xj)].CopyFrom(xProp);
        MotionProperties[nameof(Yi)].CopyFrom(yProp);
        MotionProperties[nameof(Yj)].CopyFrom(yProp);
    }
}
