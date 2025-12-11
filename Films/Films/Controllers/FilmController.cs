using Microsoft.AspNetCore.Mvc;
using MovieRecommendationAPI.Models;
using MovieRecommendationAPI.Services;
using System.Text.RegularExpressions;

namespace MovieRecommendationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(IMovieService movieService, ILogger<RecommendationController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        // GET метод для простой интеграции с фронтендом
        [HttpGet]
        public IActionResult GetRecommendation([FromQuery] string date, [FromQuery] string email)
        {
            try
            {
                // ВАЛИДАЦИЯ
                if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Поля 'date' и 'email' обязательны."
                    });
                }

                if (!IsValidEmail(email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Неверный формат email."
                    });
                }

                // ПОЛУЧЕНИЕ ФИЛЬМА
                var selectedMovie = _movieService.GetRandomMovie();

                _logger.LogInformation("Рекомендация для {Email} на {Date} - Фильм: {MovieTitle}",
                    email, date, selectedMovie.Title);

                // УПРОЩЕННЫЙ ОТВЕТ
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        email = email,
                        date = date,
                        movie = new
                        {
                            title = selectedMovie.Title,
                            description = selectedMovie.Description,
                            genre = string.Join(", ", selectedMovie.Genre)
                        },
                        viewingTime = GenerateViewingTime()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации рекомендации");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Внутренняя ошибка сервера"
                });
            }
        }

        // Тестовый эндпоинт
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                service = "Movie Recommendation API",
                status = "running",
                timestamp = DateTime.UtcNow
            });
        }

        // Вспомогательные методы
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateViewingTime()
        {
            var random = new Random();
            var hour = random.Next(18, 23);
            var minute = random.Next(0, 4) * 15;
            return $"{hour:D2}:{minute:D2}";
        }
    }
}