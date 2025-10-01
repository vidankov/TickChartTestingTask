using ReactiveUI;
using System;
using System.Timers;
using UTS.AvalonaiUI.ComponentTask1.TickChartControl;

namespace UTS.AvaloniaUI.ComponentTask1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Timer _timer;
    private readonly Random _random = new();
    private bool _isRunning;
    private int _maxVisibleTicks = 500;
    private string _selectedTheme = "Dark";
    private TickChart? _chart;

    public MainWindowViewModel()
    {
        // Таймер для генерации данных каждые 100мс (10 тиков в секунду)
        _timer = new Timer(100);
        _timer.Elapsed += (s, e) => GenerateTick();

        // Команда для кнопки Start/Stop
        StartStopCommand = ReactiveCommand.Create(StartStop);
    }

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
        // Генерируем тестовые данные как на картинке из ТЗ
        // Базовые уровни: 100, 102, 104, 106, 108, 110, 112, 114
        var basePrice = 100 + (_random.Next(0, 8) * 2);

        // Добавляем небольшую случайную вариацию (±0.5)
        var variation = (decimal)((_random.NextDouble() - 0.5) * 1.0);
        var price = (decimal)basePrice + variation;

        // Вызываем в UI потоке (Avalonia требует этого)
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Chart?.AddTick(price);
        });
    }
}