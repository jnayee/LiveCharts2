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

namespace LiveChartsCore.Drawing;

/// <summary>
/// Defiens an animable bezier segment.
/// </summary>
/// <seealso cref="IAnimatable" />
public interface IAnimatableBezierSegment : IAnimatable
{
    /// <summary>
    /// Gets or sets the x0.
    /// </summary>
    /// <value>
    /// The x0.
    /// </value>
    float X0 { get; set; }

    /// <summary>
    /// Gets or sets the y0.
    /// </summary>
    /// <value>
    /// The y0.
    /// </value>
    float Y0 { get; set; }

    /// <summary>
    /// Gets or sets the x1.
    /// </summary>
    /// <value>
    /// The x1.
    /// </value>
    float X1 { get; set; }

    /// <summary>
    /// Gets or sets the y1.
    /// </summary>
    /// <value>
    /// The y1.
    /// </value>
    float Y1 { get; set; }

    /// <summary>
    /// Gets or sets the x2.
    /// </summary>
    /// <value>
    /// The x2.
    /// </value>
    float X2 { get; set; }

    /// <summary>
    /// Gets or sets the y2.
    /// </summary>
    /// <value>
    /// The y2.
    /// </value>
    float Y2 { get; set; }
}
