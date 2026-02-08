using System.Diagnostics;
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
            [FromForm] string genre,
            [FromForm] string language
        )
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            // ✅ безопасное имя файла
            var safeFileName = Path.GetFileName(file.FileName);

            // ✅ title = имя файла без расширения
            var title = Path.GetFileNameWithoutExtension(safeFileName);

            // ✅ копируем файл в память
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            // ===== читаем длительность =====
            var tempPath = Path.GetTempFileName();

            await System.IO.File.WriteAllBytesAsync(
                tempPath,
                ms.ToArray()
            );

            var tagFile = TagLib.File.Create(tempPath);

            int duration =
                (int)tagFile.Properties.Duration.TotalSeconds;

            System.IO.File.Delete(tempPath);

            // обязательно вернуть позицию
            ms.Position = 0;

            // ===== загрузка в Cloudflare R2 =====
            await _r2.UploadAsync(safeFileName, ms);

            // ===== сохраняем в БД =====
            var track = new Track
            {
                Title = title,
                FileName = safeFileName,
                Genre = genre,
                Language = language,
                Duration = duration
            };

            _db.Tracks.Add(track);
            await _db.SaveChangesAsync();

            return Ok(track);
        }

    }
}
