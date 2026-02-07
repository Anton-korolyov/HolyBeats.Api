namespace HolyBeats.Api.Models
{
    public class Track
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";

        public string FileName { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public string Genre { get; set; } = "";
        public int Duration { get; set; }
        public string Language { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
