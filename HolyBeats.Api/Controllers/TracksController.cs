using HolyBeats.Api.Data;
using HolyBeats.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolyBeats.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TracksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly R2Service _r2;

        public TracksController(AppDbContext db, R2Service r2)
        {
            _db = db;
            _r2 = r2;
        }

        // GET /api/tracks?page=1&pageSize=20&search=love
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? genre = "all",
        string? language = "all")
        {
            var query = _db.Tracks.AsNoTracking().AsQueryable();

            // 🔎 Search by title
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    EF.Functions.ILike(t.Title, $"%{search}%"));
            }

            // 🎵 Genre filter
            if (!string.IsNullOrWhiteSpace(genre) && genre != "all")
            {
                query = query.Where(t => t.Genre == genre);
            }

            // 🌍 Language filter
            if (!string.IsNullOrWhiteSpace(language) && language != "all")
            {
                query = query.Where(t => t.Language == language);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Genre,
                    t.Language,
                    t.Duration,
                    Url = _r2.GetPublicUrl(t.FileName)
                })
                .ToListAsync();

            return Ok(new
            {
                items,
                hasMore = page * pageSize < total
            });
        }

        [HttpGet("filters")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFilters()
        {
            var genres = await _db.Tracks
                .Select(t => t.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            var languages = await _db.Tracks
                .Select(t => t.Language)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            return Ok(new
            {
                genres,
                languages
            });
        }

    }
}
