using System.Collections.Generic;
using System.Threading;
using ReactiveUI.SourceGenerators;

namespace UTS.AvaloniaUI.ComponentTask1.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    private readonly System.Threading.Timer _timer;
    private bool _isRunning;
    private decimal _currentPrice = 100;

    private readonly IPriceGenerator _priceGenerator;
    private readonly ITickChart _tickChart;
    
    [Reactive] private string _selectedTheme = "Dark";
    [Reactive] private int _maxVisibleTicks = 500;
    [Reactive] private bool _isTestBenchEnabled;
    [Reactive] private string _buttonText = "Start";

    public List<string> AvailableThemes { get; } = new() { "Dark", "Light" };
    public ITickChart TickChartControl => _tickChart;

    public MainWindowViewModel(IPriceGenerator priceGenerator, ITickChart tickChart)
    {
        _tickChart = tickChart;
        _tickChart.MaxVisibleTicks = MaxVisibleTicks;

        _priceGenerator = priceGenerator;
        _priceGenerator.Reset();

        // Таймер для генерации данных каждые 100мс (10 тиков в секунду)
        _timer = new System.Threading.Timer(_ => GenerateTick(), null, Timeout.Infinite, 10);

        this.WhenAnyValue(viewModel => viewModel.MaxVisibleTicks)
            .BindTo(_tickChart, model => model.MaxVisibleTicks);

        this.WhenAnyValue(viewModel => viewModel.SelectedTheme)
            .BindTo(_tickChart, model => model.ChartTheme);
    }

    [ReactiveCommand]
    private void StartStop()
    {
        if (_isRunning)
        {
            _timer.Change(Timeout.Infinite, 10);
        }
        else
        {
            _timer.Change(0, 10);
        }

        _isRunning = !_isRunning;
        ButtonText = _isRunning ? "Stop" : "Start";
    }

    private void GenerateTick()
    {
        _currentPrice = _priceGenerator.GenerateNextPriceValue();

        //System.Diagnostics.Debug.WriteLine($"Generated tick: {_currentPrice}, Chart is null: {Chart == null}");

        if (IsTestBenchEnabled)
        {
            //System.Diagnostics.Debug.WriteLine($"ViewModel: Sending tick to MessageBus - {_currentPrice}");
            PerformanceMessageBus.PublishTick(_currentPrice);
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            try
            {
                _tickChart.AddTick(_currentPrice);
                //System.Diagnostics.Debug.WriteLine($"SUCCESS: Tick {_currentPrice} added to chart!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR adding tick: {ex.Message}");
            }
        });
    }
}