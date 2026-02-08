using System.Security.Claims;
using HolyBeats.Api.Data;
using HolyBeats.Api.Models;
using HolyBeats.Api.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolyBeats.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly R2Service _r2;

        public PlaylistsController(AppDbContext db, R2Service r2)
        {
            _db = db;
            _r2 = r2;
        }

        int UserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null)
                throw new UnauthorizedAccessException("No user id in token");

            return int.Parse(id);
        }

        // ➕ создать плейлист
        [HttpPost]
        public IActionResult Create([FromBody] string name)
        {
            var pl = new Playlist
            {
                Name = name,
                UserId = UserId()
            };

            _db.Playlists.Add(pl);
            _db.SaveChanges();

            return Ok(pl);
        }

        // 📄 мои плейлисты
        [HttpGet]
        public IActionResult My()
        {
            return Ok(
                _db.Playlists
                  .Where(x => x.UserId == UserId())
                  .ToList());
        }

        // ❌ удалить
        [HttpDelete("{playlistId}/tracks/{trackId}")]
        public IActionResult RemoveTrack(int playlistId, int trackId)
        {
            // (опционально) проверка владельца плейлиста
            var pl = _db.Playlists.FirstOrDefault(x =>
                x.Id == playlistId && x.UserId == UserId());

            if (pl == null)
                return NotFound();

            var item = _db.PlaylistTracks.FirstOrDefault(x =>
                x.PlaylistId == playlistId && x.TrackId == trackId);

            if (item == null)
                return NotFound();

            _db.PlaylistTracks.Remove(item);
            _db.SaveChanges();

            return Ok();
        }

        // ➕ трек в плейлист
        [HttpPost("{playlistId}/tracks/{trackId}")]
        public IActionResult AddTrack(int playlistId, int trackId)
        {
            if (!_db.PlaylistTracks.Any(x =>
                x.PlaylistId == playlistId &&
                x.TrackId == trackId))
            {
                _db.PlaylistTracks.Add(new PlaylistTrack
                {
                    PlaylistId = playlistId,
                    TrackId = trackId
                });
                _db.SaveChanges();
            }

            return Ok();
        }

        // 📄 треки плейлиста
        [HttpGet("{playlistId}/tracks")]
        public IActionResult Tracks(int playlistId)
        {
            return Ok(
                _db.PlaylistTracks
                    .Where(x => x.PlaylistId == playlistId)
                    .Include(x => x.Track)
                    .Select(x => new
                    {
                        x.Track.Id,
                        x.Track.Title,
                        Url = _r2.GetPublicUrl(x.Track.FileName)
                    })
                    .ToList()
            );
        }
        // 🗑 удалить плейлист целиком
        [HttpDelete("{id}")]
        public IActionResult DeletePlaylist(int id)
        {
            var playlist = _db.Playlists
                .FirstOrDefault(x => x.Id == id && x.UserId == UserId());

            if (playlist == null)
                return NotFound();

            // удалить все связи треков
            var tracks = _db.PlaylistTracks
                .Where(x => x.PlaylistId == id);

            _db.PlaylistTracks.RemoveRange(tracks);

            _db.Playlists.Remove(playlist);
            _db.SaveChanges();

            return Ok();
        }

    }
}
