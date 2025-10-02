using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Timers;
using UTS.AvalonaiUI.ComponentTask1.TickChartControl;
using UTS.AvaloniaUI.ComponentTask1.Utilities;

namespace UTS.AvaloniaUI.ComponentTask1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Timer _timer;
    private readonly Random _random = new();
    private bool _isRunning;
    private int _maxVisibleTicks = 500;
    private string _selectedTheme = "Dark";
    private TickChart? _chart;
    private bool _isTestBenchEnabled;
    private decimal _currentPrice = 100;

    public bool IsTestBenchEnabled
    {
        get => _isTestBenchEnabled;
        set => this.RaiseAndSetIfChanged(ref _isTestBenchEnabled, value);
    }

    public List<string> AvailableThemes { get; } = new() { "Dark", "Light" };

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> StartStopCommand { get; }

    public int MaxVisibleTicks
    {
        get => _maxVisibleTicks;
        set => this.RaiseAndSetIfChanged(ref _maxVisibleTicks, value);
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
    }

    public TickChart? Chart
    {
        get => _chart;
        set => this.RaiseAndSetIfChanged(ref _chart, value);
    }

    public string ButtonText => _isRunning ? "Stop" : "Start";

    public MainWindowViewModel()
    {
        // Таймер для генерации данных каждые 100мс (10 тиков в секунду)
        _timer = new Timer(100);
        _timer.Elapsed += (s, e) => GenerateTick();

        // Команда для кнопки Start/Stop
        StartStopCommand = ReactiveCommand.Create(StartStop);
    }

    private void StartStop()
    {
        if (_isRunning)
        {
            _timer.Stop();
        }
        else
        {
            _timer.Start();
        }

        _isRunning = !_isRunning;
        this.RaisePropertyChanged(nameof(ButtonText));
    }

    private void GenerateTick()
    {
        _currentPrice += _random.Next(0, 8) * 2;
        var variation = (decimal)((_random.NextDouble() - 0.5) * 1.0);
        _currentPrice += variation;

        //System.Diagnostics.Debug.WriteLine($"Generated tick: {_currentPrice}, Chart is null: {Chart == null}");

        if (IsTestBenchEnabled)
        {
            System.Diagnostics.Debug.WriteLine($"ViewModel: Sending tick to MessageBus - {_currentPrice}");
            PerformanceMessageBus.PublishTick(_currentPrice);
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (Chart != null)
            {
                try
                {
                    Chart.AddTick(_currentPrice);
                    //System.Diagnostics.Debug.WriteLine($"SUCCESS: Tick {_currentPrice} added to chart!");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR adding tick: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Chart is null - cannot add tick!");
            }
        });
    }
}