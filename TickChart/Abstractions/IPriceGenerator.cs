namespace TickChartControl.Abstractions
{
    public interface IPriceGenerator
    {
        decimal GenerateNextPriceValue();
        void Reset();
    }
}
