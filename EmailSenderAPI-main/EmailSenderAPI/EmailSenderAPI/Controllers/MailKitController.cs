using EmailSenderAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace EmailSenderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailController> _logger;

        public GmailController(IConfiguration configuration, ILogger<GmailController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                _logger.LogInformation("Gmail: отправка на {Email}", request.ToEmail);

                // Получаем настройки из конфига
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"] ?? "smtp.gmail.com";
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"] ?? username;
                var fromName = smtpSettings["FromName"] ?? "Служба фильмов";

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Настройки Gmail не заданы"
                    });
                }

                // Создаем сообщение
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", request.ToEmail));
                message.Subject = request.Subject;

                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = request.Body
                };

                // Отправляем через Gmail
                using var client = new SmtpClient();

                // Подключаемся с STARTTLS (порт 587)
                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

                // Аутентификация
                await client.AuthenticateAsync(username, password);

                // Отправка
                await client.SendAsync(message);

                // Отключение
                await client.DisconnectAsync(true);

                _logger.LogInformation("✅ Gmail: письмо отправлено на {Email}", request.ToEmail);

                return Ok(new
                {
                    success = true,
                    message = $"Письмо отправлено на {request.ToEmail}",
                    provider = "Gmail"
                });
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                _logger.LogError(authEx, "Gmail: ошибка аутентификации");
                return BadRequest(new
                {
                    success = false,
                    message = "Ошибка аутентификации Gmail",
                    error = "Проверьте пароль приложения и двухфакторную аутентификацию",
                    tip = "Убедитесь что создали пароль приложения, а не используете обычный пароль"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gmail: ошибка отправки");
                return BadRequest(new
                {
                    success = false,
                    message = "Ошибка отправки через Gmail",
                    error = ex.Message
                });
            }
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestGmail()
        {
            try
            {
                _logger.LogInformation("Тестирование Gmail SMTP");

                // Можно хардкодить для теста
                var host = "smtp.gmail.com";
                var port = 587;
                var username = "ваш_email@gmail.com";  // ЗАМЕНИТЕ на ваш
                var password = "ваш_пароль_приложения"; // ЗАМЕНИТЕ на пароль приложения
                var testEmail = username; // Отправляем себе

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Тест Gmail", username));
                message.To.Add(new MailboxAddress("", testEmail));
                message.Subject = $"✅ Тест Gmail SMTP {DateTime.Now:HH:mm:ss}";
                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = @"
                    <h1 style='color: green;'>✅ ТЕСТ GMAIL УСПЕШЕН!</h1>
                    <p>Поздравляем! Ваш Gmail аккаунт настроен для отправки писем через SMTP.</p>
                    <p><strong>Время отправки:</strong> " + DateTime.Now.ToString("F") + @"</p>
                    <p><strong>SMTP сервер:</strong> smtp.gmail.com:587</p>
                    <p><strong>Метод:</strong> STARTTLS</p>
                    <hr>
                    <p>Теперь вы можете отправлять фильмы на любые email адреса!</p>
                    "
                };

                using var client = new SmtpClient();

                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Ok(new
                {
                    success = true,
                    message = "✅ Тест Gmail пройден! Проверьте вашу почту Gmail.",
                    details = new
                    {
                        host,
                        port,
                        from = username,
                        to = testEmail,
                        authentication = "OAuth2 via App Password"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "❌ Тест Gmail не пройден",
                    error = ex.Message,
                    troubleshooting = new[]
                    {
                        "1. Убедитесь, что включена двухфакторная аутентификация в Google",
                        "2. Создайте пароль приложения (не используйте обычный пароль)",
                        "3. Разрешите 'менее безопасные приложения' (если отключено)",
                        "4. Попробуйте порт 465 с SSL"
                    }
                });
            }
        }

        [HttpGet("test-simple")]
        public IActionResult TestSimple()
        {
            // Простой тест без отправки - только проверка настроек
            var smtpSettings = _configuration.GetSection("SmtpSettings");

            return Ok(new
            {
                configured = !string.IsNullOrEmpty(smtpSettings["Username"]),
                settings = new
                {
                    host = smtpSettings["Host"],
                    port = smtpSettings["Port"],
                    username = smtpSettings["Username"],
                    passwordSet = !string.IsNullOrEmpty(smtpSettings["Password"]),
                    from = smtpSettings["FromEmail"]
                },
                instructions = new[]
                {
                    "1. Включите 2FA в Google Аккаунте",
                    "2. Создайте пароль приложения для 'Почты'",
                    "3. Используйте этот пароль в настройках",
                    "4. Порт: 587, Метод: STARTTLS"
                }
            });
        }

        [HttpPost("send-with-fallback")]
        public async Task<IActionResult> SendWithFallback([FromBody] EmailRequest request)
        {
            // Пытаемся отправить через Gmail, если не получается - демо режим
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    // Пробуем реальную отправку
                    var result = await SendEmail(request);
                    return result;
                }
                else
                {
                    // Демо режим
                    _logger.LogInformation("📧 ДЕМО: Письмо для {Email}", request.ToEmail);

                    return Ok(new
                    {
                        success = true,
                        message = $"Демо: письмо было бы отправлено на {request.ToEmail}",
                        demo = true,
                        preview = new
                        {
                            to = request.ToEmail,
                            subject = request.Subject,
                            bodyLength = request.Body.Length
                        }
                    });
                }
            }
            catch
            {
                // Если ошибка - тоже демо режим
                return Ok(new
                {
                    success = true,
                    message = $"Демо режим: письмо для {request.ToEmail}",
                    demo = true
                });
            }
        }

        [HttpPost("send-movie")]
        public async Task<IActionResult> SendMovieEmail([FromBody] MovieEmailRequest request)
        {
            try
            {
                _logger.LogInformation("📧 Отправка рекомендации фильма для {Email}", request.Email);

                // Формируем красивое HTML письмо с фильмом
                var emailBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                            .content {{ padding: 30px; background: #f9f9f9; border: 1px solid #ddd; }}
                            .movie-title {{ color: #2c3e50; font-size: 24px; margin-bottom: 10px; }}
                            .movie-info {{ background: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
                            .viewing-time {{ background: #3498db; color: white; padding: 15px; text-align: center; border-radius: 5px; font-size: 18px; }}
                            .footer {{ text-align: center; margin-top: 30px; color: #7f8c8d; font-size: 14px; }}
                        </style>
                    </head>
                    <body>
                        <div class='header'>
                            <h1>🎬 Ваша киноподборка на {request.Date}</h1>
                        </div>
                        
                        <div class='content'>
                            <p>Здравствуйте!</p>
                            <p>Вот ваш персональный фильм на {request.Date}:</p>
                            
                            <div class='movie-info'>
                                <h2 class='movie-title'>{request.MovieTitle}</h2>
                                <p><strong>Описание:</strong> {request.MovieDescription}</p>
                                <p><strong>Жанр:</strong> {request.MovieGenre}</p>
                            </div>
                            
                            <div class='viewing-time'>
                                ⏰ <strong>Идеальное время для просмотра:</strong> {request.ViewingTime}
                            </div>
                            
                            <p>Приятного просмотра!</p>
                        </div>
                        
                        <div class='footer'>
                            <p>Служба рекомендаций фильмов</p>
                            <p>Это письмо отправлено автоматически</p>
                        </div>
                    </body>
                    </html>
                ";

                var emailRequest = new EmailRequest
                {
                    ToEmail = request.Email,
                    Subject = $"🎬 Киноподборка на {request.Date}: {request.MovieTitle}",
                    Body = emailBody
                };

                // Используем существующий метод с fallback
                return await SendWithFallback(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки рекомендации фильма");
                return Ok(new
                {
                    success = true,
                    message = $"Демо режим: письмо с фильмом для {request.Email}",
                    demo = true
                });
            }
        }
    }
}