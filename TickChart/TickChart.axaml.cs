using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ScottPlot.Avalonia;
using TickChartControl.Models;

namespace UTS.AvalonaiUI.ComponentTask1.TickChartControl;

public class TickChart : TemplatedControl
{
    private readonly List<TickData> _ticks = new();
    private AvaPlot? _plot;

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

    public TickChart()
    {
        this.TemplateApplied += OnTemplateApplied;
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
            while (_ticks.Count > MaxVisibleTicks)
            {
                _ticks.RemoveAt(0);
            }
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

        ApplyTheme();
        _plot.Refresh();
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

        while (_ticks.Count > MaxVisibleTicks)
        {
            _ticks.RemoveAt(0);
        }

        UpdatePlot();
    }

    private void UpdatePlot()
    {
        if (_plot?.Plot == null || _ticks.Count == 0) return;

        _plot.Plot.Clear();

        double[] times = new double[_ticks.Count];
        double[] prices = new double[_ticks.Count];

        for (int i = 0; i < _ticks.Count; i++)
        {
            var tick = _ticks[i];
            times[i] = tick.Time.ToOADate();
            prices[i] = (double)tick.Price;
        }

        var scatter = _plot.Plot.Add.Scatter(times, prices);
        scatter.Color = ChartTheme == "Dark"
            ? ScottPlot.Color.FromHex("#00FF00")
            : ScottPlot.Color.FromHex("#007ACC");
        scatter.LineWidth = 2;
        scatter.MarkerSize = 0;

        _plot.Plot.Axes.AutoScale();
        _plot.Refresh();
    }
}