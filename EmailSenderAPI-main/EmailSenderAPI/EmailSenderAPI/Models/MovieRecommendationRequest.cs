namespace EmailSenderAPI.Models
{
    public class MovieEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string MovieDescription { get; set; } = string.Empty;
        public string MovieGenre { get; set; } = string.Empty;
        public string ViewingTime { get; set; } = "20:00";
    }
}