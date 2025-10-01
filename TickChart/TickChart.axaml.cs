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
    }
}