namespace TickChartControl.Services.MarketData
{
    public class MarketPriceGenerator : IPriceGenerator
    {
        private decimal _currentPrice;
        private readonly decimal _basePrice;
        private readonly decimal _volatility;
        private readonly Random _random;

        private int _ticksInCurrentState;
        private readonly int _minTicksInState;
        private readonly double _baseChangeProbability;
        private readonly double _maxChangeProbability;

        public MarketState CurrentMarketState { get; set; }

        public MarketPriceGenerator(
            MarketState initialState = MarketState.BullMarket,
            decimal basePrice = 100.0m,
            decimal volatility = 10m,
            int minTicksInState = 50,
            double baseChangeProbability = 0.01,
            double maxChangeProbability = 0.05)
        {
            CurrentMarketState = initialState;
            _basePrice = basePrice;
            _currentPrice = basePrice;
            _volatility = volatility;
            _random = new Random();

            _ticksInCurrentState = 0;
            _minTicksInState = minTicksInState;
            _baseChangeProbability = baseChangeProbability;
            _maxChangeProbability = maxChangeProbability;
        }

        public decimal GenerateNextPriceValue()
        {
            decimal changePercentage = GetBaseChangePercentage();

            decimal randomFactor = (decimal)(_random.NextDouble() * 2 - 1) * _volatility;
            decimal totalChange = changePercentage + randomFactor;
            decimal newPrice = _currentPrice * (1 + totalChange / 100); // Деление на 100 для перевода в проценты
            _currentPrice = Math.Max(0.01m, newPrice);

            _ticksInCurrentState++;
            TryChangeState();

            return _currentPrice;
        }

        public void Reset()
        {
            _currentPrice = _basePrice;
            _ticksInCurrentState = 0;
            ChangeToRandomState();
        }

        private decimal GetBaseChangePercentage()
        {
            return CurrentMarketState switch
            {
                MarketState.BullMarket => _volatility * 0.3m,    // Базовый рост
                MarketState.BearMarket => -_volatility * 0.3m,   // Базовое падение
                MarketState.Stagnation => 0,                     // Без базового тренда
                _ => 0
            };
        }

        private void TryChangeState()
        {
            if (_ticksInCurrentState < _minTicksInState)
                return;

            // Расчет вероятности смены состояния
            double probability = CalculateStateChangeProbability();

            if (_random.NextDouble() < probability)
            {
                ChangeToRandomState();
                _ticksInCurrentState = 0;
            }
        }

        private double CalculateStateChangeProbability()
        {
            double extraTicks = _ticksInCurrentState - _minTicksInState;
            double progress = Math.Min(extraTicks / 100.0, 1.0); // Нормализация до [0, 1]

            return _baseChangeProbability +
                   (_maxChangeProbability - _baseChangeProbability) * progress;
        }

        private void ChangeToRandomState()
        {
            var availableStates = Enum.GetValues(typeof(MarketState))
                .Cast<MarketState>()
                .Where(s => s != CurrentMarketState)
                .ToArray();

            if (availableStates.Length > 0)
            {
                int randomIndex = _random.Next(availableStates.Length);
                CurrentMarketState = availableStates[randomIndex];
            }
        }
    }
}