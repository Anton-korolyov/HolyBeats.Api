namespace HolyBeats.Api.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
