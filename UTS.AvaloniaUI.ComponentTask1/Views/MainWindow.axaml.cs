using Avalonia;
using Avalonia.Controls;
using System;
using UTS.AvalonaiUI.ComponentTask1.TickChartControl;

namespace UTS.AvaloniaUI.ComponentTask1.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.Loaded += OnWindowLoaded;
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
}