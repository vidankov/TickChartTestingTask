using System;

namespace UTS.AvaloniaUI.ComponentTask1.Utilities;

/// <summary>
/// Сообщение о сгенерированном тике для тестового бенчмарка
/// </summary>
public class TickGeneratedMessage
{
    public decimal Price { get; }
    public DateTime Timestamp { get; }

    public TickGeneratedMessage(decimal price)
    {
        Price = price;
        Timestamp = DateTime.Now;
    }
}