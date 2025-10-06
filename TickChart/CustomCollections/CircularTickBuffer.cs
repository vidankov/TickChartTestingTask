namespace TickChartControl.Models;

/// <summary>
/// Циклический буфер для хранения тиковых данных для графика ScottPlot.
/// Оптимизирован для ситуаций, когда частота генерации тиков выше частоты рендера.
/// - Добавление тика: O(1), данные не упорядочиваются;
/// - Получение данных из метода GetPlotData: O(n), данные упорядочиваются;
/// </summary>
public class CircularTickBuffer : ITickStorage
{
    private readonly TickData[] _buffer;
    private readonly double[] _timesForPlot;
    private readonly double[] _pricesForPlot;
    private int _start = 0;
    private int _count = 0;
    private bool _isFull = false;

    /// <summary>
    /// Максимальная вместимость буфера
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Текущее количество элементов в буфере
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Флаг, указывающий что буфер полностью заполнен
    /// </summary>
    public bool IsFull => _isFull;

    /// <summary>
    /// Получает самый старый (первый) элемент в буфере
    /// </summary>
    public TickData? Head
    {
        get
        {
            if (_count == 0) return null;
            return _buffer[_start];
        }
    }

    /// <summary>
    /// Получает самый новый (последний) элемент в буфере  
    /// </summary>
    public TickData? Tail
    {
        get
        {
            if (_count == 0) return null;
            int tailIndex = (_start + _count - 1) % Capacity;
            return _buffer[tailIndex];
        }
    }

    /// <summary>
    /// Создает новый экземпляр циклического буфера с указанной вместимостью
    /// </summary>
    /// <param name="capacity">Максимальное количество элементов</param>
    public CircularTickBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));
        }

        Capacity = capacity;
        _buffer = new TickData[capacity];
        _timesForPlot = new double[capacity];
        _pricesForPlot = new double[capacity];
    }

    /// <summary>
    /// Добавляет новый тик в буфер
    /// </summary>
    /// <param name="tick">Данные тика</param>
    public void Add(TickData tick)
    {
        // Вычисляем позицию для нового элемента
        int position = (_start + _count) % Capacity;

        // Сохраняем данные во всех массивах
        _buffer[position] = tick;
        _timesForPlot[position] = tick.Time.ToOADate();
        _pricesForPlot[position] = (double)tick.Price;

        if (_isFull)
        {
            // Буфер полный - сдвигаем начальную позицию
            _start = (_start + 1) % Capacity;
        }
        else
        {
            // Буфер не полный - увеличиваем счетчик
            _count++;
            _isFull = _count == Capacity;
        }
    }

    /// <summary>
    /// Очищает буфер
    /// </summary>
    public void Clear()
    {
        _start = 0;
        _count = 0;
        _isFull = false;
    }

    /// <summary>
    /// Получает данные для построения графика в правильном порядке
    /// </summary>
    /// <returns>
    /// Кортеж содержащий:
    /// - times: массив временных меток в правильном порядке
    /// - prices: массив цен в правильном порядке  
    /// - count: количество актуальных элементов
    /// </returns>
    public (double[] times, double[] prices, int count) GetPlotData()
    {
        if (_count == 0)
        {
            return (Array.Empty<double>(), Array.Empty<double>(), 0);
        }

        // Создаем новые массивы правильного размера
        double[] orderedTimes = new double[_count];
        double[] orderedPrices = new double[_count];

        if (_isFull)
        {
            // Буфер полный - данные хранятся в циклическом порядке
            // Нужно собрать их в правильной последовательности
            for (int i = 0; i < _count; i++)
            {
                int sourceIndex = (_start + i) % Capacity;
                orderedTimes[i] = _timesForPlot[sourceIndex];
                orderedPrices[i] = _pricesForPlot[sourceIndex];
            }
        }
        else
        {
            // Буфер не полный - данные уже в правильном порядке (с индекса 0)
            Array.Copy(_timesForPlot, orderedTimes, _count);
            Array.Copy(_pricesForPlot, orderedPrices, _count);
        }

        return (orderedTimes, orderedPrices, _count);
    }

    /// <summary>
    /// Изменяет размер буфера с сохранением существующих данных
    /// </summary>
    /// <param name="newCapacity">Новая вместимость</param>
    /// <returns>Новый буфер с измененным размером</returns>
    public ITickStorage Resize(int newCapacity)
    {
        if (newCapacity <= 0)
            throw new ArgumentException("New capacity must be greater than 0", nameof(newCapacity));

        // Создаем новый буфер
        var newBuffer = new CircularTickBuffer(newCapacity);

        // Переносим данные из текущего буфера в новый
        // Берем не более newCapacity самых последних элементов
        int elementsToKeep = Math.Min(_count, newCapacity);
        int startIndex = _count - elementsToKeep;

        for (int i = 0; i < elementsToKeep; i++)
        {
            int sourceIndex = (_start + startIndex + i) % Capacity;
            newBuffer.Add(_buffer[sourceIndex]);
        }

        return newBuffer;
    }
}