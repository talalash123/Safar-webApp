using Microsoft.ML;
using Microsoft.ML.Data;
using System.Linq;

namespace SafarWebApp.Services
{
    public class ChatInput
    {
        public string UserMessage { get; set; }

        [ColumnName("Label")]
        public string Intent { get; set; }
    }

    public class ChatPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedIntent { get; set; }
        public float[] Score { get; set; }
    }

    public class ChatbotService
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<ChatInput, ChatPrediction> _predictionEngine;

        public ChatbotService()
        {
            _mlContext = new MLContext(seed: 1);
            InitializeChatbot();
        }

        private void InitializeChatbot()
        {
            // Training data mapping common phrases to intents
            ChatInput[] trainingData = new ChatInput[]
            {
                new() { UserMessage = "How can I book a ticket?", Intent = "BookingHelp" },
                new() { UserMessage = "Can I cancel my ticket?", Intent = "CancellationHelp" },
                new() { UserMessage = "What are the payment methods?", Intent = "PaymentHelp" },
                new() { UserMessage = "Is there a train from Lahore to Karachi?", Intent = "RouteHelp" }
            };

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Pipeline for text classification
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(_mlContext.Transforms.Text.FeaturizeText("Features", nameof(ChatInput.UserMessage)))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(dataView);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ChatInput, ChatPrediction>(model);
        }

        public string GetReply(string message)
        {
            var prediction = _predictionEngine.Predict(new ChatInput { UserMessage = message });

            return prediction.PredictedIntent switch
            {
                "BookingHelp" => "To book a ticket, navigate to the Home screen, search for your route, choose your train, select your seats, and checkout safely.",
                "CancellationHelp" => "You can manage your bookings through your Dashboard. Cancellations made 24 hours prior to departure receive a full refund.",
                "PaymentHelp" => "We accept payments via JazzCash, EasyPaisa, Visa, and MasterCard through our checkout portal.",
                "RouteHelp" => "You can view available active schedules and pricing live via our Customer Search Panel.",
                _ => "I am still learning! Could you please phrase your question differently or reach out directly to support@safar.com?"
            };
        }
    }
}