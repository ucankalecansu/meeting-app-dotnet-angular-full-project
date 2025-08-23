using Company.Project.Api.Services;
using Company.Project.Context;
using Company.Project.Model.DTOs;
using Company.Project.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Company.Project.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;
    private readonly EmailService _emailService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext dbContext,
        JwtService jwtService,
        PasswordService passwordService,
        EmailService emailService,
        IWebHostEnvironment environment,
        ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] UserRegisterRequest request)
    {
        try
        {
            // Email kontrol et
            var userExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
            if (userExists)
            {
                return BadRequest(new ApiResponse<UserResponse>(false, "Bu e-posta adresi zaten kullanımda.", null));
            }

            // Şifreyi hashle
            _passwordService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Yeni kullanıcı oluştur
            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.UtcNow
            };

            // Kullanıcıyı veritabanına kaydet
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // JWT token oluştur
            var token = _jwtService.GenerateToken(user);

            // Hoş geldiniz e-postası gönder (asenkron olarak arka planda)
            try {
                _ = _emailService.SendWelcomeEmailAsync(user.Email, $"{user.FirstName} {user.LastName}").ContinueWith(t => 
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Hoş geldiniz e-postası gönderilirken hata oluştu. Alıcı: {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogInformation("Hoş geldiniz e-postası başarıyla gönderildi. Alıcı: {Email}", user.Email);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hoş geldiniz e-postası gönderilirken beklenmedik bir hata oluştu. Alıcı: {Email}", user.Email);
            }

            // Kullanıcı yanıtını oluştur
            var response = new UserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.ProfileImagePath,
                token
            );

            return Ok(new ApiResponse<UserResponse>(true, "Kullanıcı başarıyla kaydedildi.", response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı kaydı sırasında bir hata oluştu");
            return StatusCode(500, new ApiResponse<UserResponse>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            // Kullanıcıyı e-posta ile bul
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            // Kullanıcı yoksa veya şifre yanlışsa
            if (user == null)
            {
                return Unauthorized(new ApiResponse<UserResponse>(false, "Bu e-posta adresine sahip kullanıcı bulunamadı.", null));
            }
            
            if (!_passwordService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new ApiResponse<UserResponse>(false, "Girdiğiniz şifre yanlış. Lütfen tekrar deneyin.", null));
            }

            // JWT token oluştur
            var token = _jwtService.GenerateToken(user);

            // Kullanıcı yanıtını oluştur
            var response = new UserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.ProfileImagePath,
                token
            );

            return Ok(new ApiResponse<UserResponse>(true, "Giriş başarılı.", response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş sırasında bir hata oluştu");
            return StatusCode(500, new ApiResponse<UserResponse>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpPost("upload-profile-image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadProfileImage([FromForm] IFormFile file, [FromForm] Guid userId)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse<string>(false, "Kullanıcı bulunamadı.", null));
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<string>(false, "Geçersiz dosya.", null));
            }

            // Dosya uzantısını kontrol et
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ApiResponse<string>(false, "Desteklenmeyen dosya formatı. Lütfen .jpg, .jpeg, .png veya .gif uzantılı bir dosya yükleyin.", null));
            }

            // Yüklenen dosya için klasör oluştur
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "ProfileImages");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Dosya adını benzersiz yap
            var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Dosyayı kaydet
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Kullanıcının profil resmini güncelle
            user.ProfileImagePath = $"/Uploads/ProfileImages/{fileName}";
            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse<string>(true, "Profil resmi başarıyla yüklendi.", user.ProfileImagePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profil resmi yükleme sırasında bir hata oluştu");
            return StatusCode(500, new ApiResponse<string>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }
}
