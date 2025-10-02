using Avalonia.Controls;
using UTS.AvaloniaUI.ComponentTask1.Behaviors;

namespace UTS.AvaloniaUI.ComponentTask1.Extensions;

public static class TickChartExtensions
{
    public static void RecordTickForBenchmark(this Control control)
    {
        TestBenchBehavior.RecordTick();
    }
}