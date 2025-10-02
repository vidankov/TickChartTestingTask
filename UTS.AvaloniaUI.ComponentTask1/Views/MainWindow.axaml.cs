using Avalonia;
using Avalonia.Controls;
using System;
using UTS.AvalonaiUI.ComponentTask1.TickChartControl;
using UTS.AvaloniaUI.ComponentTask1.Behaviors;
using UTS.AvaloniaUI.ComponentTask1.Utilities;

namespace UTS.AvaloniaUI.ComponentTask1.Views;

public partial class MainWindow : Window
{
    private DateTime _lastSecondTime = DateTime.Now;
    private int _ticksThisSecond;

    public MainWindow()
    {
        InitializeComponent();

        PerformanceMessageBus.SubscribeToTicks(OnTickGenerated);

        this.Loaded += OnWindowLoaded;
        this.Closed += OnWindowClosed;
    }

    private void OnWindowLoaded(object? sender, EventArgs e)
    {
        LinkChart(sender, e);
        NumericUpDownDiagnostic();
    }
    private void LinkChart(object? sender, EventArgs e)
    {
        var chartControl = this.FindControl<TickChart>("ChartControl");
        if (chartControl == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: ChartControl not found!");
            return;
        }
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            viewModel.Chart = chartControl;
            System.Diagnostics.Debug.WriteLine("SUCCESS: Chart linked to ViewModel!");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ERROR: DataContext is {DataContext?.GetType().Name ?? "null"}");
        }
    }
    private void NumericUpDownDiagnostic()
    {
        var numericUpDown = this.FindControl<Avalonia.Controls.NumericUpDown>("MaxTicksControl");
        if (numericUpDown != null)
        {
            System.Diagnostics.Debug.WriteLine("=== NUMERICUPDOWN DIAGNOSTICS ===");
            System.Diagnostics.Debug.WriteLine($"Value: {numericUpDown.Value}");
            System.Diagnostics.Debug.WriteLine($"Text: '{numericUpDown.Text}'");
            System.Diagnostics.Debug.WriteLine($"IsEnabled: {numericUpDown.IsEnabled}");
            System.Diagnostics.Debug.WriteLine($"IsVisible: {numericUpDown.IsVisible}");
            System.Diagnostics.Debug.WriteLine($"Opacity: {numericUpDown.Opacity}");
            System.Diagnostics.Debug.WriteLine($"Foreground: {numericUpDown.Foreground}");
            System.Diagnostics.Debug.WriteLine($"Background: {numericUpDown.Background}");

            // Проверим внутренние элементы
            numericUpDown.TemplateApplied += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("NumericUpDown template applied");
            };

            // Подпишемся на изменения
            numericUpDown.GetObservable(Avalonia.Controls.NumericUpDown.ValueProperty)
                .Subscribe(value => System.Diagnostics.Debug.WriteLine($"NumericUpDown value changed to: {value}"));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ERROR: MaxTicksControl not found!");
        }
    }
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        PerformanceMessageBus.UnsubscribeFromTicks(OnTickGenerated);
    }
    private void OnTickGenerated(TickGeneratedMessage message)
    {
        System.Diagnostics.Debug.WriteLine($"Tick generated: {message.Price}, Benchmark enabled: {TestBenchBehavior.GetIsEnabled(this)}");

        TestBenchBehavior.RecordTick();

        var tps = TestBenchBehavior.GetTicksPerSecond(this);
        var total = TestBenchBehavior.GetTotalTicks(this);
        System.Diagnostics.Debug.WriteLine($"After RecordTick - TPS: {tps}, Total: {total}");

        UpdateBenchmarkUI();
    }
    private void UpdateBenchmarkUI()
    {
        // Обновляем UI в главном потоке
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // Получаем статистику из TestBenchBehavior для этого окна
            var tps = TestBenchBehavior.GetTicksPerSecond(this);
            var total = TestBenchBehavior.GetTotalTicks(this);

            // Обновляем TextBlock'и
            if (this.FindControl<TextBlock>("TpsText") is { } tpsText)
                tpsText.Text = tps.ToString();

            if (this.FindControl<TextBlock>("TotalTicksText") is { } totalText)
                totalText.Text = total.ToString();
        });
    }
}