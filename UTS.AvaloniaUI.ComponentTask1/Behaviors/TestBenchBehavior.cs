using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace UTS.AvaloniaUI.ComponentTask1.Behaviors;

/// <summary>
/// Behavior для тестового бенчмарка - полностью самодостаточный
/// </summary>
public static class TestBenchBehavior
{
    #region Attached Properties

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsEnabled",
            typeof(TestBenchBehavior),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly AttachedProperty<int> TicksPerSecondProperty =
        AvaloniaProperty.RegisterAttached<Control, int>("TicksPerSecond", typeof(TestBenchBehavior));

    public static readonly AttachedProperty<int> TotalTicksProperty =
        AvaloniaProperty.RegisterAttached<Control, int>("TotalTicks", typeof(TestBenchBehavior));

    #endregion

    #region Private Fields

    private static int _totalTicks;
    private static int _ticksPerSecond;
    private static int _ticksThisSecond;
    private static DateTime _lastSecondTime = DateTime.Now;
    private static Control? _activeControl;

    #endregion

    static TestBenchBehavior()
    {
        IsEnabledProperty.Changed.Subscribe(OnIsEnabledChanged);
    }

    #region Public Methods

    public static bool GetIsEnabled(Control control) => control.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Control control, bool value) => control.SetValue(IsEnabledProperty, value);

    public static int GetTicksPerSecond(Control control) => control.GetValue(TicksPerSecondProperty);
    private static void SetTicksPerSecond(Control control, int value) => control.SetValue(TicksPerSecondProperty, value);

    public static int GetTotalTicks(Control control) => control.GetValue(TotalTicksProperty);
    private static void SetTotalTicks(Control control, int value) => control.SetValue(TotalTicksProperty, value);

    /// <summary>
    /// Регистрирует тик для активного контрола
    /// </summary>
    public static void RecordTick()
    {
        if (_activeControl == null)
        {
            Debug.WriteLine("TestBenchBehavior: No active control for recording tick");
            return;
        }

        _totalTicks++;
        _ticksThisSecond++;

        var now = DateTime.Now;
        if ((now - _lastSecondTime).TotalSeconds >= 1.0)
        {
            _ticksPerSecond = _ticksThisSecond;
            _ticksThisSecond = 0;
            _lastSecondTime = now;

            var memoryMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
            Debug.WriteLine($"TestBench - TPS: {_ticksPerSecond}, Total: {_totalTicks}, Memory: {memoryMB}MB");

            // Обновляем UI в главном потоке
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                SetTicksPerSecond(_activeControl, _ticksPerSecond);
                SetTotalTicks(_activeControl, _totalTicks);
            });
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                SetTotalTicks(_activeControl, _totalTicks);
            });
        }
    }

    #endregion

    #region Private Methods

    private static void OnIsEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        var control = args.Sender as Control;
        if (control == null) return;

        if (args.NewValue.Value)
        {
            // Активируем этот контрол
            _activeControl = control;
            Initialize();
            Debug.WriteLine("TestBenchBehavior: Benchmark ENABLED");
        }
        else
        {
            // Деактивируем если это активный контрол
            if (_activeControl == control)
            {
                _activeControl = null;
                Debug.WriteLine("TestBenchBehavior: Benchmark DISABLED");
            }
        }
    }

    private static void Initialize()
    {
        _totalTicks = 0;
        _ticksPerSecond = 0;
        _ticksThisSecond = 0;
        _lastSecondTime = DateTime.Now;

        // Сбрасываем UI
        if (_activeControl != null)
        {
            SetTicksPerSecond(_activeControl, 0);
            SetTotalTicks(_activeControl, 0);
        }
    }

    #endregion
}