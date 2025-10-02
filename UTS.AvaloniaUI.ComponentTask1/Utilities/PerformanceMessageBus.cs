using Avalonia.Threading;
using System;

namespace UTS.AvaloniaUI.ComponentTask1.Utilities;

public static class PerformanceMessageBus
{
    private static readonly object _lockObject = new object();
    private static event Action<TickGeneratedMessage>? TickGenerated;

    public static void PublishTick(decimal price)
    {
        var message = new TickGeneratedMessage(price);
        System.Diagnostics.Debug.WriteLine($"MessageBus: Publishing tick - {price}");

        Dispatcher.UIThread.Post(() =>
        {
            lock (_lockObject)
            {
                TickGenerated?.Invoke(message);
            }
        }, DispatcherPriority.Background);
    }

    public static void SubscribeToTicks(Action<TickGeneratedMessage> handler)
    {
        lock (_lockObject)
        {
            TickGenerated += handler;
            System.Diagnostics.Debug.WriteLine($"MessageBus: Subscriber added, total subscribers: {GetSubscriberCount()}");
        }
    }

    public static void UnsubscribeFromTicks(Action<TickGeneratedMessage> handler)
    {
        lock (_lockObject)
        {
            TickGenerated -= handler;
            System.Diagnostics.Debug.WriteLine($"MessageBus: Subscriber removed");
        }
    }

    private static int GetSubscriberCount()
    {
        lock (_lockObject)
        {
            if (TickGenerated == null) return 0;
            return TickGenerated.GetInvocationList().Length;
        }
    }
}