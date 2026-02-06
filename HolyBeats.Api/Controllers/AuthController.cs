using HolyBeats.Api.Data;
using HolyBeats.Api.Dtos;
using HolyBeats.Api.Models;
using HolyBeats.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HolyBeats.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        // REGISTER
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest req)
        {
            if (req.Password != req.ConfirmPassword)
                return BadRequest("Passwords do not match");

            if (_db.Users.Any(x => x.Email == req.Email))
                return BadRequest("User already exists");

            var user = new User
            {
                Email = req.Email,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(req.Password)
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok("Registered");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest req)
        {
            var user = _db.Users.FirstOrDefault(x => x.Email == req.Email);
            if (user == null)
                return Unauthorized();

            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized();

            var accessToken = _jwt.GenerateAccessToken(user.Id);
            var refreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _db.SaveChanges();

            return Ok(new
            {
                accessToken,
                refreshToken
            });
        }

        // REFRESH
        [HttpPost("refresh")]
        public IActionResult Refresh(TokenRequest req)
        {
            var user = _db.Users
                .FirstOrDefault(x => x.RefreshToken == req.RefreshToken);

            if (user == null)
                return Unauthorized();

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized("Refresh expired");

            var newAccess =
                _jwt.GenerateAccessToken(user.Id);

            return Ok(new
            {
                accessToken = newAccess
            });
        }
    }

    public class TokenRequest
    {
        public string RefreshToken { get; set; } = "";
    }
}
