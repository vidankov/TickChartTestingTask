namespace TickChartControl.Abstractions
{
    public interface ITickChart
    {
        public string ChartTheme { get; set; }
        public int MaxVisibleTicks { get; set; }
        void AddTick(decimal price);
    }
}
