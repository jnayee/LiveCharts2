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

using System.Collections.Generic;
using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Kernel;

/// <summary>
/// Defines a schedule to be drawn by an <see cref="Paint"/> instance.
/// </summary>
public class PaintSchedule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaintSchedule"/> class.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="geometries">The geometries.</param>
    public PaintSchedule(Paint task, HashSet<Animatable> geometries)
    {
        PaintTask = task;
        Geometries = geometries;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaintSchedule"/> class.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="geometries">The geometries.</param>
    public PaintSchedule(Paint task, params Animatable[] geometries)
    {
        PaintTask = task;
        Geometries = new HashSet<Animatable>(geometries);
    }

    /// <summary>
    /// Gets or sets the paint task.
    /// </summary>
    /// <value>
    /// The drawable task.
    /// </value>
    public Paint PaintTask { get; set; }

    /// <summary>
    /// Gets or sets the geometries.
    /// </summary>
    /// <value>
    /// The geometries.
    /// </value>
    public HashSet<Animatable> Geometries { get; set; }
}
