namespace TickChartControl.Abstractions
{
    public interface ITickStorage
    {
        int Count { get; }
        TickData? Head { get; }
        TickData? Tail { get; }
        void Add(TickData tick);
        void Clear();
        (double[] times, double[] prices, int count) GetPlotData();
        ITickStorage Resize(int newCapacity);
    }
}
