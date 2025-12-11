namespace MovieRecommendationAPI.Models
{
    // Класс, представляющий фильм в нашей системе
    public class Movie
    {
        // Уникальный идентификатор фильма
        public int Id { get; set; }

        // Название фильма
        public string Title { get; set; } = string.Empty;

        // Краткое описание фильма
        public string Description { get; set; } = string.Empty;

        // Список жанров фильма
        public List<string> Genre { get; set; } = new List<string>();
    }
}