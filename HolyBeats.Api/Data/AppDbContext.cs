using HolyBeats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HolyBeats.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Track> Tracks => Set<Track>();
        public DbSet<FavoriteTrack> FavoriteTracks => Set<FavoriteTrack>();
        public DbSet<Playlist> Playlists => Set<Playlist>();
        public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    }
}
