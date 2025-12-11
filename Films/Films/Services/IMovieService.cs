using MovieRecommendationAPI.Models;

namespace MovieRecommendationAPI.Services
{
    // Интерфейс для сервиса работы с фильмами
    public interface IMovieService
    {
        // Метод для получения случайного фильма из базы
        Movie GetRandomMovie();

        // Метод для получения всех фильмов (для будущего расширения)
        List<Movie> GetAllMovies();

        // Метод для получения фильма по ID (для будущего расширения)
        Movie GetMovieById(int id);
    }
}