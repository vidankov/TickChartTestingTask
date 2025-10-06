namespace TickChartControl.Services.MarketData
{
    public class MarketPriceGenerator
    {
        private decimal _currentPrice;
        private readonly decimal _basePrice;
        private readonly decimal _volatility;
        private readonly Random _random;

        // Параметры смены состояния
        private int _ticksInCurrentState;
        private readonly int _minTicksInState;
        private readonly double _baseChangeProbability;
        private readonly double _maxChangeProbability;

        // Параметры для усиления трендов и скачков
        private readonly decimal _trendStrength;
        private readonly decimal _jumpVolatility;
        private readonly double _jumpProbability;
        private int _consecutiveDirection;

        // Аккумулятор тренда и параметры усиления
        private decimal _trendAccumulator;
        private readonly decimal _trendAcceleration;
        private readonly decimal _maxTrendAccumulation;

        // НОВЫЕ ПАРАМЕТРЫ: для выраженных трендов
        private readonly decimal _baseTrendFactor; // Базовая сила тренда
        private readonly decimal _momentumFactor;  // Фактор инерции

        public MarketState CurrentMarketState { get; private set; }

        public MarketPriceGenerator(
            MarketState initialState = MarketState.BullMarket,
            decimal basePrice = 100.0m,
            decimal volatility = 0.02m, // Уменьшил базовую волатильность
            int minTicksInState = 50,
            double baseChangeProbability = 0.01,
            double maxChangeProbability = 0.05,
            decimal trendStrength = 2.0m,           // Усилил силу тренда
            decimal jumpVolatility = 0.1m,
            double jumpProbability = 0.05,
            decimal trendAcceleration = 0.001m,     // Увеличил ускорение тренда
            decimal maxTrendAccumulation = 0.5m,    // Увеличил максимальное накопление
            decimal baseTrendFactor = 0.004m,       // НОВЫЙ: базовая сила тренда
            decimal momentumFactor = 0.002m)        // НОВЫЙ: фактор инерции
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

            _trendStrength = trendStrength;
            _jumpVolatility = jumpVolatility;
            _jumpProbability = jumpProbability;
            _consecutiveDirection = 0;

            _trendAccumulator = 0;
            _trendAcceleration = trendAcceleration;
            _maxTrendAccumulation = maxTrendAccumulation;

            // Новые параметры для выраженных трендов
            _baseTrendFactor = baseTrendFactor;
            _momentumFactor = momentumFactor;
        }

        public decimal GenerateNextPriceValue()
        {
            // ОБНОВЛЕНО: Усиленная формула с приоритетом тренда
            UpdateTrendAccumulator();

            // ОСНОВНОЙ ТРЕНД - стал главным компонентом
            decimal baseTrend = GetBaseTrendComponent();
            decimal accumulatedTrend = GetAccumulatedTrendComponent();

            // Случайная волатильность - уменьшенная роль
            decimal randomVolatility = (decimal)(_random.NextDouble() * 2 - 1) * _volatility * 0.3m;

            // Скачки цены
            decimal jumpFactor = GetJumpFactor();

            // ИНЕРЦИЯ - усиление при последовательном движении
            decimal momentum = GetMomentumComponent();

            // КОМБИНИРОВАНИЕ: тренд преобладает над шумом
            decimal totalChange = baseTrend +
                                accumulatedTrend +
                                randomVolatility +
                                jumpFactor +
                                momentum;

            decimal newPrice = _currentPrice * (1 + totalChange);
            newPrice = Math.Max(0.01m, newPrice);

            // Обновляем состояние
            UpdateConsecutiveDirection(newPrice);
            _currentPrice = newPrice;
            _ticksInCurrentState++;
            TryChangeState();

            return _currentPrice;
        }

        public void Reset()
        {
            _currentPrice = _basePrice;
            _ticksInCurrentState = 0;
            _consecutiveDirection = 0;
            _trendAccumulator = 0;
            CurrentMarketState = MarketState.BullMarket;
        }

        public void ForceStateChange(MarketState newState)
        {
            CurrentMarketState = newState;
            _ticksInCurrentState = 0;
            _consecutiveDirection = 0;
            _trendAccumulator = 0;
        }

        private void UpdateTrendAccumulator()
        {
            if (CurrentMarketState != MarketState.Stagnation)
            {
                // Более агрессивное накопление тренда
                decimal acceleration = _trendAcceleration * (1 + _ticksInCurrentState / 200.0m);
                _trendAccumulator += acceleration * GetTrendDirection();

                _trendAccumulator = Math.Max(-_maxTrendAccumulation,
                    Math.Min(_maxTrendAccumulation, _trendAccumulator));
            }
            else
            {
                // В стагнации быстро сбрасываем тренд
                _trendAccumulator *= 0.8m;
            }
        }

        private decimal GetBaseTrendComponent()
        {
            // ОСНОВНОЙ ТРЕНД - явное направленное движение
            decimal direction = GetTrendDirection();
            return _baseTrendFactor * direction * (1 + _ticksInCurrentState / 100.0m);
        }

        private decimal GetAccumulatedTrendComponent()
        {
            // НАКОПЛЕННЫЙ ТРЕНД - усиливается со временем
            decimal timeFactor = Math.Min(_ticksInCurrentState / 50.0m, 3.0m);
            return _trendAccumulator * timeFactor * _trendStrength;
        }

        private decimal GetMomentumComponent()
        {
            // ИНЕРЦИЯ - усиление при длительных движениях в одном направлении
            if (Math.Abs(_consecutiveDirection) > 10)
            {
                decimal momentumPower = Math.Min(0.1m, _momentumFactor * Math.Abs(_consecutiveDirection));
                return momentumPower * Math.Sign(_consecutiveDirection);
            }
            return 0;
        }

        private int GetTrendDirection()
        {
            return CurrentMarketState switch
            {
                MarketState.BullMarket => 1,
                MarketState.BearMarket => -1,
                _ => 0
            };
        }

        private decimal GetJumpFactor()
        {
            if (_random.NextDouble() < _jumpProbability)
            {
                double directionBias = CurrentMarketState switch
                {
                    MarketState.BullMarket => 0.8,
                    MarketState.BearMarket => 0.2,
                    _ => 0.5
                };

                bool upwardJump = _random.NextDouble() < directionBias;
                decimal jumpSize = (decimal)_random.NextDouble() * _jumpVolatility;

                return upwardJump ? jumpSize : -jumpSize;
            }

            return 0;
        }

        private void UpdateConsecutiveDirection(decimal newPrice)
        {
            if (newPrice > _currentPrice)
            {
                _consecutiveDirection = _consecutiveDirection > 0 ?
                    _consecutiveDirection + 1 : 1;
            }
            else if (newPrice < _currentPrice)
            {
                _consecutiveDirection = _consecutiveDirection < 0 ?
                    _consecutiveDirection - 1 : -1;
            }
            else
            {
                _consecutiveDirection = 0;
            }
        }

        private void TryChangeState()
        {
            if (_ticksInCurrentState < _minTicksInState)
                return;

            double probability = CalculateStateChangeProbability();

            if (_random.NextDouble() < probability)
            {
                ChangeToRandomState();
                _ticksInCurrentState = 0;
                _consecutiveDirection = 0;
                _trendAccumulator = 0;
            }
        }

        private double CalculateStateChangeProbability()
        {
            double extraTicks = _ticksInCurrentState - _minTicksInState;
            double progress = Math.Min(extraTicks / 100.0, 1.0);

            return _baseChangeProbability +
                   (_maxChangeProbability - _baseChangeProbability) * progress;
        }

        private void ChangeToRandomState()
        {
            var availableStates = Enum.GetValues(typeof(MarketState))
                .Cast<MarketState>()
                .Where(s => s != CurrentMarketState)
                .ToArray();

            int randomIndex = _random.Next(availableStates.Length);
            CurrentMarketState = availableStates[randomIndex];
        }

        // Методы для диагностики
        public decimal GetTrendAccumulation() => _trendAccumulator;
        public int GetTicksInCurrentState() => _ticksInCurrentState;
        public int GetConsecutiveDirection() => _consecutiveDirection;
    }

}