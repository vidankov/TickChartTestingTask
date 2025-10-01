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
}