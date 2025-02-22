// The MIT License(MIT)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.SkiaSharpView.Avalonia;

/// <inheritdoc cref="IPolarChartView" />
public class PolarChart : UserControl, IPolarChartView
{
    #region fields

    /// <summary>
    /// The legend
    /// </summary>
    protected IChartLegend? legend;

    /// <summary>
    /// The tool tip
    /// </summary>
    protected IChartTooltip? tooltip;

    private MotionCanvas? _avaloniaCanvas;
    private Chart? _core;
    private readonly CollectionDeepObserver<ISeries> _seriesObserver;
    private readonly CollectionDeepObserver<IPolarAxis> _angleObserver;
    private readonly CollectionDeepObserver<IPolarAxis> _radiusObserver;
    private readonly CollectionDeepObserver<ChartElement> _visualsObserver;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PolarChart"/> class.
    /// </summary>
    /// <exception cref="Exception">Default colors are not valid</exception>
    public PolarChart()
    {
        InitializeComponent();

        // workaround to detect mouse events.
        // Avalonia do not seem to detect pointer events if background is not set.
        ((IChartView)this).BackColor = LvcColor.FromArgb(0, 0, 0, 0);

        LiveCharts.Configure(config => config.UseDefaults());

        InitializeCore();

        AttachedToVisualTree += OnAttachedToVisualTree;

        _seriesObserver = new CollectionDeepObserver<ISeries>(OnDeepCollectionChanged, OnDeepCollectionPropertyChanged, true);
        _angleObserver = new CollectionDeepObserver<IPolarAxis>(OnDeepCollectionChanged, OnDeepCollectionPropertyChanged, true);
        _radiusObserver = new CollectionDeepObserver<IPolarAxis>(OnDeepCollectionChanged, OnDeepCollectionPropertyChanged, true);
        _visualsObserver = new CollectionDeepObserver<ChartElement>(
           OnDeepCollectionChanged, OnDeepCollectionPropertyChanged, true);

        AngleAxes =
            [
                LiveCharts.DefaultSettings.GetProvider().GetDefaultPolarAxis()
            ];
        RadiusAxes =
            [
                LiveCharts.DefaultSettings.GetProvider().GetDefaultPolarAxis()
            ];
        Series = new ObservableCollection<ISeries>();
        VisualElements = new ObservableCollection<ChartElement>();

        PointerWheelChanged += PolarChart_PointerWheelChanged;
        PointerPressed += PolarChart_PointerPressed;
        PointerMoved += PolarChart_PointerMoved;

        PointerExited += PolarChart_PointerLeave;
        DetachedFromVisualTree += PolarChart_DetachedFromVisualTree;
    }

    #region avalonia/dependency properties

    /// <summary>
    /// The sync context property.
    /// </summary>
    public static readonly AvaloniaProperty<object> SyncContextProperty =
       AvaloniaProperty.Register<PolarChart, object>(nameof(SyncContext), new object(), inherits: true);

    /// <summary>
    /// The title property.
    /// </summary>
    public static readonly AvaloniaProperty<VisualElement?> TitleProperty =
       AvaloniaProperty.Register<PolarChart, VisualElement?>(nameof(Title), null, inherits: true);

    /// <summary>
    /// The series property.
    /// </summary>
    public static readonly AvaloniaProperty<IEnumerable<ISeries>> SeriesProperty =
       AvaloniaProperty.Register<PolarChart, IEnumerable<ISeries>>(nameof(Series), [], inherits: true);

    /// <summary>
    /// The visual elements property
    /// </summary>
    public static readonly AvaloniaProperty<IEnumerable<ChartElement>> VisualElementsProperty =
        AvaloniaProperty.Register<PolarChart, IEnumerable<ChartElement>>(
            nameof(VisualElements), [], inherits: true);

    /// <summary>
    /// The fit to bounds property.
    /// </summary>
    public static readonly AvaloniaProperty<bool> FitToBoundsProperty =
        AvaloniaProperty.Register<PolarChart, bool>(nameof(FitToBounds), false, inherits: true);

    /// <summary>
    /// The total angle property.
    /// </summary>
    public static readonly AvaloniaProperty<double> TotalAngleProperty =
        AvaloniaProperty.Register<PolarChart, double>(nameof(TotalAngle), 360d, inherits: true);

    /// <summary>
    /// The inner radius property.
    /// </summary>
    public static readonly AvaloniaProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<PolarChart, double>(nameof(InnerRadius), 0d, inherits: true);

    /// <summary>
    /// The initial rotation property.
    /// </summary>
    public static readonly AvaloniaProperty<double> InitialRotationProperty =
        AvaloniaProperty.Register<PolarChart, double>(
            nameof(InitialRotation), LiveCharts.DefaultSettings.PolarInitialRotation, inherits: true);

    /// <summary>
    /// The x axes property.
    /// </summary>
    public static readonly AvaloniaProperty<IEnumerable<IPolarAxis>> AngleAxesProperty =
        AvaloniaProperty.Register<PolarChart, IEnumerable<IPolarAxis>>(nameof(AngleAxes), [], inherits: true);

    /// <summary>
    /// The y axes property.
    /// </summary>
    public static readonly AvaloniaProperty<IEnumerable<IPolarAxis>> RadiusAxesProperty =
        AvaloniaProperty.Register<PolarChart, IEnumerable<IPolarAxis>>(nameof(RadiusAxes), [], inherits: true);

    /// <summary>
    /// The animations speed property.
    /// </summary>
    public static readonly AvaloniaProperty<TimeSpan> AnimationsSpeedProperty =
        AvaloniaProperty.Register<PolarChart, TimeSpan>(
            nameof(AnimationsSpeed), LiveCharts.DefaultSettings.AnimationsSpeed, inherits: true);

    /// <summary>
    /// The easing function property.
    /// </summary>
    public static readonly AvaloniaProperty<Func<float, float>> EasingFunctionProperty =
        AvaloniaProperty.Register<PolarChart, Func<float, float>>(
            nameof(AnimationsSpeed), LiveCharts.DefaultSettings.EasingFunction, inherits: true);

    /// <summary>
    /// The tool tip position property.
    /// </summary>
    public static readonly AvaloniaProperty<TooltipPosition> TooltipPositionProperty =
        AvaloniaProperty.Register<PolarChart, TooltipPosition>(
            nameof(TooltipPosition), LiveCharts.DefaultSettings.TooltipPosition, inherits: true);

    /// <summary>
    /// The tool tip finding strategy property.
    /// </summary>
    public static readonly AvaloniaProperty<FindingStrategy> TooltipFindingStrategyProperty =
        AvaloniaProperty.Register<PolarChart, FindingStrategy>(
            nameof(LegendPosition), LiveCharts.DefaultSettings.FindingStrategy, inherits: true);

    /// <summary>
    /// The tooltip background paint property
    /// </summary>
    public static readonly AvaloniaProperty<Paint?> TooltipBackgroundPaintProperty =
        AvaloniaProperty.Register<PolarChart, Paint?>(
            nameof(TooltipBackgroundPaint), (Paint?)LiveCharts.DefaultSettings.TooltipBackgroundPaint, inherits: true);

    /// <summary>
    /// The tooltip text paint property
    /// </summary>
    public static readonly AvaloniaProperty<Paint?> TooltipTextPaintProperty =
        AvaloniaProperty.Register<PolarChart, Paint?>(
            nameof(TooltipTextPaint), (Paint?)LiveCharts.DefaultSettings.TooltipTextPaint, inherits: true);

    /// <summary>
    /// The tooltip text size property
    /// </summary>
    public static readonly AvaloniaProperty<double> TooltipTextSizeProperty =
        AvaloniaProperty.Register<PolarChart, double>(
            nameof(TooltipTextSize), LiveCharts.DefaultSettings.TooltipTextSize, inherits: true);

    /// <summary>
    /// The legend position property.
    /// </summary>
    public static readonly AvaloniaProperty<LegendPosition> LegendPositionProperty =
        AvaloniaProperty.Register<PolarChart, LegendPosition>(
            nameof(LegendPosition), LiveCharts.DefaultSettings.LegendPosition, inherits: true);

    /// <summary>
    /// The legend background paint property
    /// </summary>
    public static readonly AvaloniaProperty<Paint?> LegendBackgroundPaintProperty =
        AvaloniaProperty.Register<PolarChart, Paint?>(
            nameof(LegendBackgroundPaint), (Paint?)LiveCharts.DefaultSettings.LegendBackgroundPaint, inherits: true);

    /// <summary>
    /// The legend text paint property
    /// </summary>
    public static readonly AvaloniaProperty<Paint?> LegendTextPaintProperty =
        AvaloniaProperty.Register<PolarChart, Paint?>(
            nameof(LegendTextPaint), (Paint?)LiveCharts.DefaultSettings.LegendTextPaint, inherits: true);

    /// <summary>
    /// The legend text size property
    /// </summary>
    public static readonly AvaloniaProperty<double> LegendTextSizeProperty =
        AvaloniaProperty.Register<PolarChart, double>(
            nameof(LegendTextSize), LiveCharts.DefaultSettings.LegendTextSize, inherits: true);

    /// <summary>
    /// The data pointer down command property
    /// </summary>
    public static readonly AvaloniaProperty<ICommand?> UpdateStartedCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(UpdateStartedCommand), null, inherits: true);

    /// <summary>
    /// The pointer pressed command.
    /// </summary>
    public static readonly AvaloniaProperty<ICommand?> PointerPressedCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(PointerPressedCommand), null, inherits: true);

    /// <summary>
    /// The pointer released command.
    /// </summary>
    public static readonly AvaloniaProperty<ICommand?> PointerReleasedCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(PointerReleasedCommand), null, inherits: true);

    /// <summary>
    /// The pointer move command.
    /// </summary>
    public static readonly AvaloniaProperty<ICommand?> PointerMoveCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(PointerMoveCommand), null, inherits: true);

    /// <summary>
    /// The data pointer down command property
    /// </summary>
    public static readonly AvaloniaProperty<ICommand?> DataPointerDownCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(DataPointerDownCommand), null, inherits: true);

    /// <summary>
    /// The hovered points chaanged command property
    /// </summary>
    public static readonly AvaloniaProperty<ICommand?> HoveredPointsChangedCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(HoveredPointsChangedCommand), null, inherits: true);

    /// <summary>
    /// The chart point pointer down command property
    /// </summary>
    [Obsolete($"Use the {nameof(DataPointerDown)} event instead with a {nameof(FindingStrategy)} that used TakeClosest.")]
    public static readonly AvaloniaProperty<ICommand?> ChartPointPointerDownCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(ChartPointPointerDownCommand), null, inherits: true);

    /// <summary>
    /// The <see cref="VisualElement"/> pointer down command property
    /// </summary>
    public static readonly AvaloniaProperty<ICommand?> VisualElementsPointerDownCommandProperty =
        AvaloniaProperty.Register<PolarChart, ICommand?>(nameof(VisualElementsPointerDownCommand), null, inherits: true);

    #endregion

    #region events

    /// <inheritdoc cref="IChartView.Measuring" />
    public event ChartEventHandler? Measuring;

    /// <inheritdoc cref="IChartView.UpdateStarted" />
    public event ChartEventHandler? UpdateStarted;

    /// <inheritdoc cref="IChartView.UpdateFinished" />
    public event ChartEventHandler? UpdateFinished;

    /// <inheritdoc cref="IChartView.DataPointerDown" />
    public event ChartPointsHandler? DataPointerDown;

    /// <inheritdoc cref="IChartView.HoveredPointsChanged" />
    public event ChartPointHoverHandler? HoveredPointsChanged;

    /// <inheritdoc cref="IChartView.ChartPointPointerDown" />
    [Obsolete($"Use the {nameof(DataPointerDown)} event instead with a {nameof(FindingStrategy)} that used TakeClosest.")]
    public event ChartPointHandler? ChartPointPointerDown;

    /// <inheritdoc cref="IChartView.VisualElementsPointerDown"/>
    public event VisualElementsHandler? VisualElementsPointerDown;

    #endregion

    #region properties

    /// <inheritdoc cref="IChartView.DesignerMode" />
    bool IChartView.DesignerMode => Design.IsDesignMode;

    /// <inheritdoc cref="IChartView.CoreChart" />
    public Chart CoreChart => _core ?? throw new Exception("Core not set yet.");

    /// <inheritdoc cref="IChartView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => _core is null ? throw new Exception("core not found") : _core.Canvas;

    PolarChartEngine IPolarChartView.Core =>
        _core is null ? throw new Exception("core not found") : (PolarChartEngine)_core;

    LvcColor IChartView.BackColor
    {
        get => Background is not ISolidColorBrush b
                ? new LvcColor()
                : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
        set => Background = new SolidColorBrush(new Color(value.R, value.G, value.B, value.A));
    }

    LvcSize IChartView.ControlSize => _avaloniaCanvas is null
        ? new LvcSize()
        : new LvcSize
        {
            Width = (float)_avaloniaCanvas.Bounds.Width,
            Height = (float)_avaloniaCanvas.Bounds.Height
        };

    /// <inheritdoc cref="IChartView.DrawMargin" />
    public Margin? DrawMargin { get => null; set => throw new NotImplementedException(); }

    /// <inheritdoc cref="IChartView.SyncContext" />
    public object SyncContext
    {
        get => GetValue(SyncContextProperty)!;
        set => SetValue(SyncContextProperty, value);
    }

    /// <inheritdoc cref="IPolarChartView.FitToBounds" />
    public bool FitToBounds
    {
        get => (bool)GetValue(FitToBoundsProperty)!;
        set => SetValue(FitToBoundsProperty, value);
    }

    /// <inheritdoc cref="IPolarChartView.TotalAngle" />
    public double TotalAngle
    {
        get => (double)GetValue(TotalAngleProperty)!;
        set => SetValue(TotalAngleProperty, value);
    }

    /// <inheritdoc cref="IPolarChartView.InnerRadius" />
    public double InnerRadius
    {
        get => (double)GetValue(InnerRadiusProperty)!;
        set => SetValue(InnerRadiusProperty, value);
    }

    /// <inheritdoc cref="IPolarChartView.InitialRotation" />
    public double InitialRotation
    {
        get => (double)GetValue(InitialRotationProperty)!;
        set => SetValue(InitialRotationProperty, value);
    }

    /// <inheritdoc cref="IChartView.Title" />
    public VisualElement? Title
    {
        get => (VisualElement?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <inheritdoc cref="IPolarChartView.Series" />
    public IEnumerable<ISeries> Series
    {
        get => (IEnumerable<ISeries>)GetValue(SeriesProperty)!;
        set => SetValue(SeriesProperty, value);
    }

    /// <inheritdoc cref="IPolarChartView.AngleAxes" />
    public IEnumerable<IPolarAxis> AngleAxes
    {
        get => (IEnumerable<IPolarAxis>)GetValue(AngleAxesProperty)!;
        set => SetValue(AngleAxesProperty, value);
    }

    /// <inheritdoc cref="IPolarChartView.RadiusAxes" />
    public IEnumerable<IPolarAxis> RadiusAxes
    {
        get => (IEnumerable<IPolarAxis>)GetValue(RadiusAxesProperty)!;
        set => SetValue(RadiusAxesProperty, value);
    }

    /// <inheritdoc cref="IChartView.VisualElements" />
    public IEnumerable<ChartElement> VisualElements
    {
        get => (IEnumerable<ChartElement>)GetValue(VisualElementsProperty)!;
        set => SetValue(VisualElementsProperty, value);
    }

    /// <inheritdoc cref="IChartView.AnimationsSpeed" />
    public TimeSpan AnimationsSpeed
    {
        get => (TimeSpan)GetValue(AnimationsSpeedProperty)!;
        set => SetValue(AnimationsSpeedProperty, value);
    }

    /// <inheritdoc cref="IChartView.EasingFunction" />
    public Func<float, float>? EasingFunction
    {
        get => (Func<float, float>?)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    /// <inheritdoc cref="IChartView.TooltipPosition" />
    public TooltipPosition TooltipPosition
    {
        get => (TooltipPosition)GetValue(TooltipPositionProperty)!;
        set => SetValue(TooltipPositionProperty, value);
    }

    /// <inheritdoc cref="IChartView.TooltipBackgroundPaint" />
    public Paint? TooltipBackgroundPaint
    {
        get => (Paint?)GetValue(TooltipBackgroundPaintProperty);
        set => SetValue(TooltipBackgroundPaintProperty, value);
    }

    /// <inheritdoc cref="IChartView.TooltipTextPaint" />
    public Paint? TooltipTextPaint
    {
        get => (Paint?)GetValue(TooltipTextPaintProperty);
        set => SetValue(TooltipTextPaintProperty, value);
    }

    /// <inheritdoc cref="IChartView.TooltipTextSize" />
    public double TooltipTextSize
    {
        get => (double?)GetValue(TooltipTextSizeProperty) ?? LiveCharts.DefaultSettings.TooltipTextSize;
        set => SetValue(TooltipTextSizeProperty, value);
    }

    /// <inheritdoc cref="IChartView.Tooltip" />
    public IChartTooltip? Tooltip { get => tooltip; set => tooltip = value; }

    /// <inheritdoc cref="IChartView.LegendPosition" />
    public LegendPosition LegendPosition
    {
        get => (LegendPosition)GetValue(LegendPositionProperty)!;
        set => SetValue(LegendPositionProperty, value);
    }

    /// <inheritdoc cref="IChartView.LegendBackgroundPaint" />
    public Paint? LegendBackgroundPaint
    {
        get => (Paint?)GetValue(LegendBackgroundPaintProperty);
        set => SetValue(LegendBackgroundPaintProperty, value);
    }

    /// <inheritdoc cref="IChartView.LegendTextPaint" />
    public Paint? LegendTextPaint
    {
        get => (Paint?)GetValue(LegendTextPaintProperty);
        set => SetValue(LegendTextPaintProperty, value);
    }

    /// <inheritdoc cref="IChartView.LegendTextSize" />
    public double LegendTextSize
    {
        get => (double?)GetValue(LegendTextSizeProperty) ?? LiveCharts.DefaultSettings.LegendTextSize;
        set => SetValue(LegendTextSizeProperty, value);
    }

    /// <inheritdoc cref="IChartView.Legend" />
    public IChartLegend? Legend { get => legend; set => legend = value; }

    /// <inheritdoc cref="IChartView.AutoUpdateEnabled" />
    public bool AutoUpdateEnabled { get; set; } = true;

    /// <inheritdoc cref="IChartView.UpdaterThrottler" />
    public TimeSpan UpdaterThrottler { get; set; } = LiveCharts.DefaultSettings.UpdateThrottlingTimeout;

    /// <summary>
    /// Gets or sets a command to execute when the chart update started.
    /// </summary>
    public ICommand? UpdateStartedCommand
    {
        get => (ICommand?)GetValue(UpdateStartedCommandProperty);
        set => SetValue(UpdateStartedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a command to execute when the pointer is pressed on the chart.
    /// </summary>
    public ICommand? PointerPressedCommand
    {
        get => (ICommand?)GetValue(PointerPressedCommandProperty);
        set => SetValue(PointerPressedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a command to execute when the pointer is released on the chart.
    /// </summary>
    public ICommand? PointerReleasedCommand
    {
        get => (ICommand?)GetValue(PointerReleasedCommandProperty);
        set => SetValue(PointerReleasedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a command to execute when the pointer moves over the chart.
    /// </summary>
    public ICommand? PointerMoveCommand
    {
        get => (ICommand?)GetValue(PointerMoveCommandProperty);
        set => SetValue(PointerMoveCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a command to execute when the pointer goes down on a data or data points.
    /// </summary>
    public ICommand? DataPointerDownCommand
    {
        get => (ICommand?)GetValue(DataPointerDownCommandProperty);
        set => SetValue(DataPointerDownCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a command to execute when the hovered points change.
    /// </summary>
    public ICommand? HoveredPointsChangedCommand
    {
        get => (ICommand?)GetValue(HoveredPointsChangedCommandProperty);
        set => SetValue(HoveredPointsChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a command to execute when the pointer goes down on a chart point.
    /// </summary>
    [Obsolete($"Use the {nameof(DataPointerDown)} event instead with a {nameof(FindingStrategy)} that used TakeClosest.")]
    public ICommand? ChartPointPointerDownCommand
    {
        get => (ICommand?)GetValue(ChartPointPointerDownCommandProperty);
        set => SetValue(ChartPointPointerDownCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a command to execute when the pointer goes down on a visual element(s).
    /// </summary>
    public ICommand? VisualElementsPointerDownCommand
    {
        get => (ICommand?)GetValue(VisualElementsPointerDownCommandProperty);
        set => SetValue(VisualElementsPointerDownCommandProperty, value);
    }

    #endregion

    /// <inheritdoc cref="IPolarChartView.ScalePixelsToData(LvcPointD, int, int)"/>
    public LvcPointD ScalePixelsToData(LvcPointD point, int angleAxisIndex = 0, int radiusAxisIndex = 0)
    {
        if (_core is not PolarChartEngine cc) throw new Exception("core not found");

        var scaler = new PolarScaler(
            cc.DrawMarginLocation, cc.DrawMarginSize, cc.AngleAxes[angleAxisIndex], cc.RadiusAxes[radiusAxisIndex],
            cc.InnerRadius, cc.InitialRotation, cc.TotalAnge);

        return scaler.ToChartValues(point.X, point.Y);
    }

    /// <inheritdoc cref="IPolarChartView.ScaleDataToPixels(LvcPointD, int, int)"/>
    public LvcPointD ScaleDataToPixels(LvcPointD point, int angleAxisIndex = 0, int radiusAxisIndex = 0)
    {
        if (_core is not PolarChartEngine cc) throw new Exception("core not found");

        var scaler = new PolarScaler(
            cc.DrawMarginLocation, cc.DrawMarginSize, cc.AngleAxes[angleAxisIndex], cc.RadiusAxes[radiusAxisIndex],
            cc.InnerRadius, cc.InitialRotation, cc.TotalAnge);

        var r = scaler.ToPixels(point.X, point.Y);

        return new LvcPointD { X = (float)r.X, Y = (float)r.Y };
    }

    /// <inheritdoc cref="IChartView.GetPointsAt(LvcPointD, FindingStrategy, FindPointFor)"/>
    public IEnumerable<ChartPoint> GetPointsAt(LvcPointD point, FindingStrategy strategy = FindingStrategy.Automatic, FindPointFor findPointFor = FindPointFor.HoverEvent)
    {
        if (_core is not PolarChartEngine cc) throw new Exception("core not found");

        if (strategy == FindingStrategy.Automatic)
            strategy = cc.Series.GetFindingStrategy();

        return cc.Series.SelectMany(series => series.FindHitPoints(cc, new(point), strategy, findPointFor));
    }

    /// <inheritdoc cref="IChartView.GetVisualsAt(LvcPointD)"/>
    public IEnumerable<IChartElement> GetVisualsAt(LvcPointD point)
    {
        return _core is not PolarChartEngine cc
            ? throw new Exception("core not found")
            : cc.VisualElements.SelectMany(visual => ((VisualElement)visual).IsHitBy(cc, new(point)));
    }

    void IChartView.InvokeOnUIThread(Action action) =>
        Dispatcher.UIThread.Post(action);

    /// <summary>
    /// Initializes the core.
    /// </summary>
    /// <returns></returns>
    protected void InitializeCore()
    {
        var canvas = this.FindControl<MotionCanvas>("canvas");
        _avaloniaCanvas = canvas;
        _core = new PolarChartEngine(this, config => config.UseDefaults(), canvas!.CanvasCore);

        _core.Measuring += OnCoreMeasuring;
        _core.UpdateStarted += OnCoreUpdateStarted;
        _core.UpdateFinished += OnCoreUpdateFinished;

        _core.Update();
    }

    /// <inheritdoc cref="OnPropertyChanged(AvaloniaPropertyChangedEventArgs)"/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_core is null || change.Property.Name == nameof(IsPointerOver)) return;

        if (change.Property.Name == nameof(SyncContext))
        {
            CoreCanvas.Sync = change.NewValue ?? new object();
        }

        if (change.Property.Name == nameof(Series))
        {
            _seriesObserver?.Dispose((IEnumerable<ISeries>?)change.OldValue);
            _seriesObserver?.Initialize((IEnumerable<ISeries>?)change.NewValue);
        }

        if (change.Property.Name == nameof(AngleAxes))
        {
            _angleObserver?.Dispose((IEnumerable<IPolarAxis>?)change.OldValue);
            _angleObserver?.Initialize((IEnumerable<IPolarAxis>?)change.NewValue);
        }

        if (change.Property.Name == nameof(RadiusAxes))
        {
            _radiusObserver?.Dispose((IEnumerable<IPolarAxis>?)change.OldValue);
            _radiusObserver?.Initialize((IEnumerable<IPolarAxis>?)change.NewValue);
        }

        if (change.Property.Name == nameof(VisualElements))
        {
            _visualsObserver?.Dispose((IEnumerable<ChartElement>?)change.OldValue);
            _visualsObserver?.Initialize((IEnumerable<ChartElement>?)change.NewValue);
        }

        _core.Update();
    }

    private void InitializeComponent() =>
        AvaloniaXamlLoader.Load(this);

    private void OnDeepCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        _core?.Update();

    private void OnDeepCollectionPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        _core?.Update();

    private void PolarChart_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        //if (core is null) return;

        //var c = (PolarChart<SkiaSharpDrawingContext>)core;
        //var p = e.GetPosition(this);

        //c.Zoom(new PointF((float)p.X, (float)p.Y), e.Delta.Y > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }

    private void PolarChart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        var p = e.GetPosition(this);

        if (PointerPressedCommand is not null)
        {
            var args = new PointerCommandArgs(this, new(p.X, p.Y), e);
            if (PointerPressedCommand.CanExecute(args)) PointerPressedCommand.Execute(args);
        }

        foreach (var w in desktop.Windows) w.PointerReleased += Window_PointerReleased;
        _core?.InvokePointerDown(new LvcPoint((float)p.X, (float)p.Y), false);
    }

    private void PolarChart_PointerMoved(object? sender, PointerEventArgs e)
    {
        var p = e.GetPosition(_avaloniaCanvas);

        if (PointerMoveCommand is not null)
        {
            var args = new PointerCommandArgs(this, new(p.X, p.Y), e);
            if (PointerMoveCommand.CanExecute(args)) PointerMoveCommand.Execute(args);
        }

        _core?.InvokePointerMove(new LvcPoint((float)p.X, (float)p.Y));
    }

    private void Window_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        foreach (var w in desktop.Windows) w.PointerReleased -= Window_PointerReleased;
        var p = e.GetPosition(this);

        if (PointerReleasedCommand is not null)
        {
            var args = new PointerCommandArgs(this, new(p.X, p.Y), e);
            if (PointerReleasedCommand.CanExecute(args)) PointerReleasedCommand.Execute(args);
        }

        _core?.InvokePointerUp(new LvcPoint((float)p.X, (float)p.Y), false);
    }

    private void OnCoreUpdateFinished(IChartView chart) => UpdateFinished?.Invoke(this);

    private void OnCoreUpdateStarted(IChartView chart)
    {
        if (UpdateStartedCommand is not null)
        {
            var args = new ChartCommandArgs(this);
            if (UpdateStartedCommand.CanExecute(args)) UpdateStartedCommand.Execute(args);
        }

        UpdateStarted?.Invoke(this);
    }

    private void OnCoreMeasuring(IChartView chart) => Measuring?.Invoke(this);

    private void PolarChart_PointerLeave(object? sender, PointerEventArgs e) => _core?.InvokePointerLeft();

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) => _core?.Load();

    private void PolarChart_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) => _core?.Unload();

    void IChartView.OnDataPointerDown(IEnumerable<ChartPoint> points, LvcPoint pointer)
    {
        DataPointerDown?.Invoke(this, points);
        if (DataPointerDownCommand is not null && DataPointerDownCommand.CanExecute(points)) DataPointerDownCommand.Execute(points);

        ChartPointPointerDown?.Invoke(this, points.FindClosestTo(pointer));
#pragma warning disable CS0618 // Type or member is obsolete
        ChartPointPointerDownCommand?.Execute(points.FindClosestTo(pointer));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    void IChartView.OnHoveredPointsChanged(IEnumerable<ChartPoint>? newPoints, IEnumerable<ChartPoint>? oldPoints)
    {
        HoveredPointsChanged?.Invoke(this, newPoints, oldPoints);

        var args = new HoverCommandArgs(this, newPoints, oldPoints);
        if (HoveredPointsChangedCommand is not null && HoveredPointsChangedCommand.CanExecute(args)) HoveredPointsChangedCommand.Execute(args);
    }

    void IChartView.OnVisualElementPointerDown(
        IEnumerable<IInteractable> visualElements, LvcPoint pointer)
    {
        var args = new VisualElementsEventArgs(CoreChart, visualElements, pointer);

        VisualElementsPointerDown?.Invoke(this, args);
        if (VisualElementsPointerDownCommand is not null && VisualElementsPointerDownCommand.CanExecute(args))
            VisualElementsPointerDownCommand.Execute(args);
    }

    void IChartView.Invalidate() => CoreCanvas.Invalidate();
}
