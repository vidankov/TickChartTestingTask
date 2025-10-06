using System.Collections.Generic;
using System.Threading;

namespace UTS.AvaloniaUI.ComponentTask1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly System.Threading.Timer _timer;
    private readonly Random _random = new();
    private bool _isRunning;
    private int _maxVisibleTicks = 500;
    private string _selectedTheme = "Dark";
    private readonly IPriceGenerator _priceGenerator;
    private readonly ITickChart _tickChart;
    private bool _isTestBenchEnabled;
    private decimal _currentPrice = 100;

    public bool IsTestBenchEnabled
    {
        get => _isTestBenchEnabled;
        set => this.RaiseAndSetIfChanged(ref _isTestBenchEnabled, value);
    }

    public List<string> AvailableThemes { get; } = new() { "Dark", "Light" };

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> StartStopCommand { get; }

    public ITickChart TickChartControl => _tickChart;

    public int MaxVisibleTicks
    {
        get => _maxVisibleTicks;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxVisibleTicks, value);
            _tickChart.MaxVisibleTicks = value;
        }
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            _tickChart.ChartTheme = value;
        }
    }

    public string ButtonText => _isRunning ? "Stop" : "Start";

    public MainWindowViewModel(IPriceGenerator priceGenerator, ITickChart tickChart)
    {
        _tickChart = tickChart;
        _tickChart.ChartTheme = SelectedTheme;
        _tickChart.MaxVisibleTicks = MaxVisibleTicks;

        _priceGenerator = priceGenerator;
        _priceGenerator.Reset();

        // Таймер для генерации данных каждые 100мс (10 тиков в секунду)
        _timer = new System.Threading.Timer(_ => GenerateTick(), null, Timeout.Infinite, 10);

        // Команда для кнопки Start/Stop
        StartStopCommand = ReactiveCommand.Create(StartStop);
    }

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
        this.RaisePropertyChanged(nameof(ButtonText));
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