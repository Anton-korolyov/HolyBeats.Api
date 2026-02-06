using HolyBeats.Api.Data;
using HolyBeats.Api.Models;
using HolyBeats.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HolyBeats.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly R2Service _r2;
        private readonly AppDbContext _db;

        public UploadController(
            R2Service r2,
            AppDbContext db)
        {
            _r2 = r2;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(
            IFormFile file,
            [FromForm] string title)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            // ✅ копируем файл в память
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            await _r2.UploadAsync(file.FileName, ms);

            var track = new Track
            {
                Title = title,
                FileName = file.FileName
            };

            _db.Tracks.Add(track);
            await _db.SaveChangesAsync();

            return Ok(track);
        }
    }
}
