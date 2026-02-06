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
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly R2Service _r2;

        public FavoritesController(AppDbContext db, R2Service r2)
        {
            _db = db;
            _r2 = r2;
        }

        int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ❤️ добавить
        [HttpPost("{trackId}")]
        public IActionResult Add(int trackId)
        {
            var userId = GetUserId();

            if (_db.FavoriteTracks.Any(x =>
                x.UserId == userId &&
                x.TrackId == trackId))
                return Ok();

            _db.FavoriteTracks.Add(new FavoriteTrack
            {
                UserId = userId,
                TrackId = trackId
            });

            _db.SaveChanges();
            return Ok();
        }

        // ❌ удалить
        [HttpDelete("{trackId}")]
        public IActionResult Remove(int trackId)
        {
            var userId = GetUserId();

            var fav = _db.FavoriteTracks.FirstOrDefault(x =>
                x.UserId == userId &&
                x.TrackId == trackId);

            if (fav == null)
                return NotFound();

            _db.FavoriteTracks.Remove(fav);
            _db.SaveChanges();

            return Ok();
        }

        // 📄 список избранных
        [HttpGet]
        public IActionResult Get()
        {
            var userId = GetUserId();

            var result = _db.FavoriteTracks
                .Where(x => x.UserId == userId)
                .Include(x => x.Track)
                .Select(x => new
                {
                    x.Track.Id,
                    x.Track.Title,
                    Url = _r2.GetPublicUrl(x.Track.FileName)
                })
                .ToList();

            return Ok(result);
        }
    }


}
