using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ScottPlot;
using ScottPlot.Avalonia;

namespace UTS.AvalonaiUI.ComponentTask1.TickChartControl;

public class TickChart : TemplatedControl, ITickChart
{
    private ITickStorage _ticks;
    private AvaPlot? _plot;
    private DateTime _lastRenderTime = DateTime.Now;

    public static readonly StyledProperty<int> MaxVisibleTicksProperty =
        AvaloniaProperty.Register<TickChart, int>(
            nameof(MaxVisibleTicks), defaultValue: 500);

    public static readonly StyledProperty<string> ChartThemeProperty =
        AvaloniaProperty.Register<TickChart, string>(
            nameof(ChartTheme), defaultValue: "Dark");

    public int MaxVisibleTicks
    {
        get => GetValue(MaxVisibleTicksProperty);
        set => SetValue(MaxVisibleTicksProperty, value);
    }

    public string ChartTheme
    {
        get => GetValue(ChartThemeProperty);
        set => SetValue(ChartThemeProperty, value);
    }

    public TickChart(ITickStorage ticksStorage)
    {
        this.TemplateApplied += OnTemplateApplied;
        _ticks = ticksStorage;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChartThemeProperty)
        {
            ApplyTheme();
            UpdatePlot();
        }
        else if (change.Property == MaxVisibleTicksProperty)
        {
            _ticks = _ticks.Resize(MaxVisibleTicks);
            UpdatePlot();
        }
    }
    private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        _plot = e.NameScope.Find<AvaPlot>("PART_Plot");

        if (_plot != null)
        {
            InitializePlot();
        }
    }
    private void InitializePlot()
    {
        if (_plot?.Plot == null) return;

        _plot.Plot.Title("Tick Chart");
        _plot.Plot.YLabel("Price");
        _plot.Plot.XLabel("Time");

        SetBottomAxesDateTimeFromat();
        ApplyTheme();
        _plot.Refresh();
    }
    private void SetBottomAxesDateTimeFromat()
    {
        _plot.Plot.Axes.DateTimeTicksBottom();
        _plot.Plot.RenderManager.RenderStarting += (s, e) =>
        {
            if (_ticks.Count == 0) return;

            var ticks = _plot.Plot.Axes.Bottom.TickGenerator.Ticks;

            string tickLabelFormat = GetTickLabelFormat();

            for (int i = 0; i < ticks.Length; i++)
            {
                DateTime dateTime = DateTime.FromOADate(ticks[i].Position);
                string label = dateTime.ToString(tickLabelFormat);
                ticks[i] = new Tick(ticks[i].Position, label);
            }
        };
    }
    private string GetTickLabelFormat()
    {
        var head = _ticks.Head;
        var tail = _ticks.Tail;

        if (head == null || tail == null)
        {
            return "HH:mm:ss";
        }

        TimeSpan timeSpan = tail.Time - head.Time;

        return timeSpan.TotalHours <= 1 ? "HH:mm:ss" :
               timeSpan.TotalDays <= 1 ? "HH:mm" :
               "dd.MM.yy\nHH:mm";
    }
    private void ApplyTheme()
    {
        if (_plot?.Plot == null) return;

        if (ChartTheme == "Dark")
        {
            _plot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");
            _plot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#2D2D30");
            _plot.Plot.Axes.Color(ScottPlot.Color.FromHex("#FFFFFF"));
        }
        else
        {
            _plot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            _plot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#F8F8F8");
            _plot.Plot.Axes.Color(ScottPlot.Color.FromHex("#000000"));
        }
    }
    public void AddTick(decimal price)
    {
        var tick = new TickData(DateTime.Now, price);
        _ticks.Add(tick);
        UpdatePlot();
    }
    private void UpdatePlot()
    {
        if (_plot?.Plot == null || _ticks.Count == 0) return;

        if ((DateTime.Now - _lastRenderTime).TotalMilliseconds < 33)
            return;

        lock (_plot.Plot.Sync)
        {
            var (times, prices, _) = _ticks.GetPlotData();

            _plot.Plot.Clear();

            var signalXY = _plot.Plot.Add.SignalXY(times, prices);
            signalXY.Color = ChartTheme == "Dark"
                ? ScottPlot.Color.FromHex("#00FF00")
                : ScottPlot.Color.FromHex("#007ACC");
            signalXY.LineWidth = 2;
            signalXY.MarkerSize = 0;
            _plot.Plot.Axes.AutoScale();
        }
        
        _plot.Refresh();
        _lastRenderTime = DateTime.Now;
    }
}