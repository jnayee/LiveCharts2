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

namespace LiveChartsCore.Kernel.Events;

/// <summary>
/// Defines event extensions.
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Executes the given action when a point is measured, this metod just subscribes to the
    /// <see cref="Series{TModel, TVisual, TLabel}.PointMeasured"/> event, but with a simpler syntax.
    /// </summary>
    /// <typeparam name="TModel">TThe type of the model.</typeparam>
    /// <typeparam name="TVisual">The type of the visual.</typeparam>
    /// <typeparam name="TLabel">The type of the label.</typeparam>
    /// <param name="series">The target series.</param>
    /// <param name="predicate">The action to execute.</param>
    /// <returns>The series.</returns>
    /// <remarks>
    /// The action is subscribed to the <see cref="Series{TModel, TVisual, TLabel}.PointMeasured"/> event.
    /// </remarks>
    public static Series<TModel, TVisual, TLabel> OnPointMeasured<TModel, TVisual, TLabel>(
        this Series<TModel, TVisual, TLabel> series, Action<ChartPoint<TModel, TVisual, TLabel>> predicate)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        series.PointMeasured += predicate;
        return series;
    }

    /// <summary>
    /// Executes the given action when a point is measured, this metod just subscribes to the
    /// <see cref="Series{TModel, TVisual, TLabel}.PointCreated"/> event, but with a simpler syntax.
    /// </summary>
    /// <typeparam name="TModel">TThe type of the model.</typeparam>
    /// <typeparam name="TVisual">The type of the visual.</typeparam>
    /// <typeparam name="TLabel">The type of the label.</typeparam>
    /// <param name="series">The target series.</param>
    /// <param name="predicate">The action to execute.</param>
    /// <returns>The series.</returns>
    /// <remarks>
    /// The action is subscribed to the <see cref="Series{TModel, TVisual, TLabel}.PointCreated"/> event.
    /// </remarks>
    public static Series<TModel, TVisual, TLabel> OnPointCreated<TModel, TVisual, TLabel>(
        this Series<TModel, TVisual, TLabel> series, Action<ChartPoint<TModel, TVisual, TLabel>> predicate)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        series.PointCreated += predicate;
        return series;
    }

    /// <summary>
    /// Executes the given action when a point is measured, this metod just subscribes to the
    /// <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerDown"/> event, but with a simpler syntax.
    /// </summary>
    /// <typeparam name="TModel">TThe type of the model.</typeparam>
    /// <typeparam name="TVisual">The type of the visual.</typeparam>
    /// <typeparam name="TLabel">The type of the label.</typeparam>
    /// <param name="series">The target series.</param>
    /// <param name="predicate">The action to execute.</param>
    /// <returns>The series.</returns>
    /// <remarks>
    /// The action is subscribed to the <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerDown"/> event.
    /// </remarks>
    public static Series<TModel, TVisual, TLabel> OnPointDown<TModel, TVisual, TLabel>(
        this Series<TModel, TVisual, TLabel> series, ChartPointHandler<TModel, TVisual, TLabel> predicate)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        series.ChartPointPointerDown += predicate;
        return series;
    }

    /// <summary>
    /// Executes the given action when a point is measured, this metod just subscribes to the
    /// <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerHover"/> event, but with a simpler syntax.
    /// </summary>
    /// <typeparam name="TModel">TThe type of the model.</typeparam>
    /// <typeparam name="TVisual">The type of the visual.</typeparam>
    /// <typeparam name="TLabel">The type of the label.</typeparam>
    /// <param name="series">The target series.</param>
    /// <param name="predicate">The action to execute.</param>
    /// <returns>The series.</returns>
    /// <remarks>
    /// The action is subscribed to the <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerHover"/> event.
    /// </remarks>
    public static Series<TModel, TVisual, TLabel> OnPointHover<TModel, TVisual, TLabel>(
        this Series<TModel, TVisual, TLabel> series, ChartPointHandler<TModel, TVisual, TLabel> predicate)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        series.ChartPointPointerHover += predicate;
        return series;
    }

    /// <summary>
    /// Executes the given action when a point is measured, this metod just subscribes to the
    /// <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerHoverLost"/> event, but with a simpler syntax.
    /// </summary>
    /// <typeparam name="TModel">TThe type of the model.</typeparam>
    /// <typeparam name="TVisual">The type of the visual.</typeparam>
    /// <typeparam name="TLabel">The type of the label.</typeparam>
    /// <param name="series">The target series.</param>
    /// <param name="predicate">The action to execute.</param>
    /// <returns>The series.</returns>
    /// <remarks>
    /// The action is subscribed to the <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerHoverLost"/> event.
    /// </remarks>
    public static Series<TModel, TVisual, TLabel> OnPointHoverLost<TModel, TVisual, TLabel>(
        this Series<TModel, TVisual, TLabel> series, ChartPointHandler<TModel, TVisual, TLabel> predicate)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        series.ChartPointPointerHoverLost += predicate;
        return series;
    }
}
