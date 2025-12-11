namespace MovieRecommendationAPI.Models
{
    // Основной класс ответа API
    public class RecommendationResponse
    {
        // Статус операции: "success" или "error"
        public string Status { get; set; } = "success";

        // Данные рекомендации
        public RecommendationData? Data { get; set; }

        // Сообщение об ошибке
        public string? Message { get; set; }
    }

    // Класс с основными данными рекомендации
    public class RecommendationData
    {
        // Email пользователя
        public string Email { get; set; } = string.Empty;

        // Дата запроса
        public string RecommendationDate { get; set; } = string.Empty;

        // Информация о рекомендованном фильме
        public MovieInfo Movie { get; set; } = new MovieInfo();

        // Время просмотра
        public string ViewingTime { get; set; } = "20:00";
    }

    // Упрощенный класс информации о фильме для ответа
    public class MovieInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Genre { get; set; } = new List<string>();
    }
}