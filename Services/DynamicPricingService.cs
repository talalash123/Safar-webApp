using Microsoft.ML;
using Microsoft.ML.Data;
using System;

namespace SafarWebApp.Services
{
    // Input schema for training/prediction
    public class TicketPriceData
    {
        public float BasePrice { get; set; }
        public float SeatsRemaining { get; set; }
        public float DaysUntilDeparture { get; set; }
        public float IsEventDay { get; set; } // 1 for True, 0 for False

        [ColumnName("Label")]
        public float Price { get; set; } // Target variable
    }

    public class PricePrediction
    {
        [ColumnName("Score")]
        public float PredictedPrice { get; set; }
    }

    public class DynamicPricingService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;

        public DynamicPricingService()
        {
            _mlContext = new MLContext(seed: 1);
            TrainModelOffline(); // Train a baseline model on startup
        }

        private void TrainModelOffline()
        {
            // Expanded historical training data for robust ML predictions
            // Price field now stores the relative price multiplier (PriceMultiplier = TargetPrice / BasePrice)
            TicketPriceData[] historicalData = new TicketPriceData[]
            {
                // High Demand + Event scenarios
                new() { BasePrice = 1000, SeatsRemaining = 10, DaysUntilDeparture = 2, IsEventDay = 1, Price = 1.8f },
                new() { BasePrice = 1200, SeatsRemaining = 5, DaysUntilDeparture = 1, IsEventDay = 1, Price = 1.833f },
                new() { BasePrice = 1500, SeatsRemaining = 8, DaysUntilDeparture = 1, IsEventDay = 1, Price = 1.867f },
                new() { BasePrice = 800, SeatsRemaining = 12, DaysUntilDeparture = 3, IsEventDay = 1, Price = 1.75f },
                // Standard demand scenarios
                new() { BasePrice = 1000, SeatsRemaining = 80, DaysUntilDeparture = 15, IsEventDay = 0, Price = 1.0f },
                new() { BasePrice = 1500, SeatsRemaining = 60, DaysUntilDeparture = 10, IsEventDay = 0, Price = 1.0f },
                new() { BasePrice = 1200, SeatsRemaining = 70, DaysUntilDeparture = 7, IsEventDay = 0, Price = 1.042f },
                new() { BasePrice = 800, SeatsRemaining = 50, DaysUntilDeparture = 5, IsEventDay = 0, Price = 1.125f },
                // Low demand / clearance scenarios
                new() { BasePrice = 1000, SeatsRemaining = 140, DaysUntilDeparture = 1, IsEventDay = 0, Price = 0.60f },
                new() { BasePrice = 1500, SeatsRemaining = 140, DaysUntilDeparture = 30, IsEventDay = 0, Price = 0.60f },
                new() { BasePrice = 1200, SeatsRemaining = 120, DaysUntilDeparture = 20, IsEventDay = 0, Price = 0.667f },
                new() { BasePrice = 800, SeatsRemaining = 100, DaysUntilDeparture = 25, IsEventDay = 0, Price = 0.625f },
                // Mid-range scenarios
                new() { BasePrice = 1500, SeatsRemaining = 40, DaysUntilDeparture = 3, IsEventDay = 0, Price = 1.2f },
                new() { BasePrice = 1500, SeatsRemaining = 20, DaysUntilDeparture = 2, IsEventDay = 1, Price = 1.733f },
                new() { BasePrice = 1000, SeatsRemaining = 30, DaysUntilDeparture = 5, IsEventDay = 0, Price = 1.2f },
                new() { BasePrice = 2000, SeatsRemaining = 15, DaysUntilDeparture = 1, IsEventDay = 1, Price = 1.75f },
            };

            IDataView trainingData = _mlContext.Data.LoadFromEnumerable(historicalData);

            // Data processing pipeline with FastTree configured for this dataset
            // We exclude BasePrice from features so the predicted multiplier is purely a function of demand conditions.
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(TicketPriceData.SeatsRemaining),
                nameof(TicketPriceData.DaysUntilDeparture),
                nameof(TicketPriceData.IsEventDay))
                .Append(_mlContext.Regression.Trainers.FastTree(
                    numberOfLeaves: 4,
                    minimumExampleCountPerLeaf: 1,
                    numberOfTrees: 100,
                    learningRate: 0.1));

            // Train model
            _model = pipeline.Fit(trainingData);
        }

        public decimal PredictOptimalPrice(decimal basePrice, int seatsRemaining, int daysLeft, bool isEvent)
        {
            // Calculate a sensible rule-based fallback price multiplier
            decimal fallbackPrice = basePrice;
            if (isEvent) fallbackPrice *= 1.25m;
            if (daysLeft < 3) fallbackPrice *= 1.30m;
            else if (daysLeft < 7) fallbackPrice *= 1.15m;
            if (seatsRemaining < 15) fallbackPrice *= 1.20m;
            else if (seatsRemaining > 100) fallbackPrice *= 0.85m;
            fallbackPrice = Math.Round(fallbackPrice, 2);

            if (_model == null)
            {
                return fallbackPrice;
            }

            try
            {
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<TicketPriceData, PricePrediction>(_model);

                var input = new TicketPriceData
                {
                    BasePrice = (float)basePrice,
                    SeatsRemaining = seatsRemaining,
                    DaysUntilDeparture = daysLeft,
                    IsEventDay = isEvent ? 1f : 0f
                };

                var prediction = predictionEngine.Predict(input);
                float predictedMultiplier = prediction.PredictedPrice;

                if (float.IsNaN(predictedMultiplier) || float.IsInfinity(predictedMultiplier) || predictedMultiplier <= 0f)
                {
                    return fallbackPrice;
                }

                // Compute optimal price by multiplying the basePrice by the predicted demand multiplier
                decimal optimalPrice = basePrice * (decimal)predictedMultiplier;
                return Convert.ToDecimal(Math.Round(optimalPrice, 2));
            }
            catch
            {
                return fallbackPrice;
            }
        }
    }
}