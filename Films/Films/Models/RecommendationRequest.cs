namespace MovieRecommendationAPI.Models
{
    // Класс для получения данных от пользователя в запросе
    public class RecommendationRequest
    {
        // Дата, на которую запрашивается рекомендация
        public string Date { get; set; } = string.Empty;

        // Email пользователя
        public string Email { get; set; } = string.Empty;
    }
}