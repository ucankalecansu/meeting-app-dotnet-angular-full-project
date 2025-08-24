using Company.Project.Api.Services;
using Company.Project.Context;
using Company.Project.Model.DTOs;
using Company.Project.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Company.Project.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly EmailService _emailService;
    private readonly ILogger<MeetingsController> _logger;

    public MeetingsController(
        AppDbContext dbContext, 
        IWebHostEnvironment environment, 
        EmailService emailService,
        ILogger<MeetingsController> logger)
    {
        _dbContext = dbContext;
        _environment = environment;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Meeting>>> GetAll()
    {
        return await _dbContext.Meetings
            .Where(m => !m.IsCancelled)
            .Include(m => m.Documents)
            .OrderByDescending(m => m.StartAt)
            .ToListAsync();
    }

    [HttpGet("cancelled")]
    public async Task<ActionResult<IEnumerable<Meeting>>> GetCancelled()
    {
        return await _dbContext.Meetings
            .Where(m => m.IsCancelled)
            .Include(m => m.Documents)
            .OrderByDescending(m => m.CancelledAt)
            .ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Meeting>> Get(Guid id)
    {
        var meeting = await _dbContext.Meetings
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (meeting == null)
        {
            return NotFound(new ApiResponse<Meeting>(false, "Toplantı bulunamadı.", null));
        }
        
        return Ok(meeting);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Meeting>>> Create([FromBody] MeetingCreateRequest request)
    {
        try
        {
            // Başlangıç bitiş kontrolü
            if (request.EndAt < request.StartAt)
            {
                return BadRequest(new ApiResponse<Meeting>(false, "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.", null));
            }

            // Yeni toplantı oluştur
            var meeting = new Meeting
            {
                Title = request.Title,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Description = request.Description,
                IsCancelled = false,
                Status = request.Status,
                Participants = request.Participants ?? Array.Empty<string>()
            };

            _dbContext.Meetings.Add(meeting);
            await _dbContext.SaveChangesAsync();

            // Katılımcılara bildirim e-postası gönder
            if (request.Participants != null && request.Participants.Count() > 0)
            {
                try
                {
                    foreach (var participantEmail in request.Participants)
                    {
                        // Kullanıcının tam adını bul (eğer kullanıcı veritabanında kayıtlı ise)
                        if (participantEmail == null) continue;
                        string participantName = participantEmail; // Varsayılan olarak e-posta adresini kullan
                        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == participantEmail);
                        if (user != null)
                        {
                            participantName = $"{user.FirstName} {user.LastName}";
                        }
                        
                        // Asenkron olarak e-posta gönder
                        _ = _emailService.SendMeetingNotificationAsync(
                            participantEmail,
                            participantName,
                            meeting.Title,
                            meeting.StartAt,
                            meeting.EndAt
                        ).ContinueWith(t => 
                        {
                            if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, "Toplantı bildirimi gönderilirken hata oluştu. Alıcı: {Email}", participantEmail);
                            }
                        });
                    }
                    _logger.LogInformation("Toplantı bildirimleri {Count} katılımcıya gönderiliyor.", request.Participants.Count());
                }
                catch (Exception ex)
                {
                    // E-posta gönderimindeki hata toplantı oluşturmayı etkilemesin
                    _logger.LogError(ex, "Toplantı bildirimleri gönderilirken hata oluştu. Toplantı ID: {Id}", meeting.Id);
                }
            }

            // Başarılı yanıt döndür
            return Ok(new ApiResponse<Meeting>(true, "Toplantı başarıyla oluşturuldu.", meeting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplantı oluşturulurken bir hata oluştu.");
            return StatusCode(500, new ApiResponse<Meeting>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Meeting>>> Update(Guid id, [FromBody] MeetingUpdateRequest request)
    {
        try
        {
            // Toplantıyı bul
            var meeting = await _dbContext.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound(new ApiResponse<Meeting>(false, "Toplantı bulunamadı.", null));
            }

            // İptal edilmiş toplantı güncellenemez
            if (meeting.IsCancelled)
            {
                return BadRequest(new ApiResponse<Meeting>(false, "İptal edilmiş toplantı güncellenemez.", null));
            }

            // Başlangıç bitiş kontrolü
            if (request.EndAt < request.StartAt)
            {
                return BadRequest(new ApiResponse<Meeting>(false, "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.", null));
            }

            // Toplantıyı güncelle
            meeting.Title = request.Title;
            meeting.StartAt = request.StartAt;
            meeting.EndAt = request.EndAt;
            meeting.Description = request.Description;
            meeting.Status = request.Status;
            meeting.Participants = request.Participants ?? Array.Empty<string>();

            await _dbContext.SaveChangesAsync();
            
            // Katılımcılara bildirim e-postası gönder
            if (meeting.Participants != null && meeting.Participants.Count() > 0)
            {
                try
                {
                    foreach (var participantEmail in meeting.Participants)
                    {
                        // Kullanıcının tam adını bul (eğer kullanıcı veritabanında kayıtlı ise)
                        if (participantEmail == null) continue;
                        string participantName = participantEmail; // Varsayılan olarak e-posta adresini kullan
                        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == participantEmail);
                        if (user != null)
                        {
                            participantName = $"{user.FirstName} {user.LastName}";
                        }
                        
                        // Asenkron olarak güncelleme e-postası gönder
                        _ = _emailService.SendMeetingUpdateNotificationAsync(
                            participantEmail,
                            participantName,
                            meeting.Title,
                            meeting.StartAt,
                            meeting.EndAt
                        ).ContinueWith(t => 
                        {
                            if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, "Toplantı güncelleme bildirimi gönderilirken hata oluştu. Alıcı: {Email}", participantEmail);
                            }
                        });
                    }
                    _logger.LogInformation("Toplantı güncelleme bildirimleri {Count} katılımcıya gönderiliyor.", meeting.Participants.Count());
                }
                catch (Exception ex)
                {
                    // E-posta gönderimindeki hata toplantı güncellemeyi etkilemesin
                    _logger.LogError(ex, "Toplantı güncelleme bildirimleri gönderilirken hata oluştu. Toplantı ID: {Id}", meeting.Id);
                }
            }

            // Başarılı yanıt döndür
            return Ok(new ApiResponse<Meeting>(true, "Toplantı başarıyla güncellendi.", meeting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplantı güncellenirken bir hata oluştu. Toplantı ID: {Id}", id);
            return StatusCode(500, new ApiResponse<Meeting>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            // Toplantıyı bul
            var meeting = await _dbContext.Meetings
                .Include(m => m.Documents)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (meeting == null)
            {
                return NotFound(new ApiResponse<object>(false, "Toplantı bulunamadı.", null));
            }

            // Toplantıyı ve dokümanlarını sil
            _dbContext.Meetings.Remove(meeting);
            await _dbContext.SaveChangesAsync();

            // Başarılı yanıt döndür
            return Ok(new ApiResponse<object>(true, "Toplantı başarıyla silindi.", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplantı silinirken bir hata oluştu. Toplantı ID: {Id}", id);
            return StatusCode(500, new ApiResponse<object>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<Meeting>>> Cancel(Guid id, [FromBody] MeetingCancelRequest? request = null)
    {
        try
        {
            // Toplantıyı bul
            var meeting = await _dbContext.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound(new ApiResponse<Meeting>(false, "Toplantı bulunamadı.", null));
            }

            // Zaten iptal edilmişse
            if (meeting.IsCancelled)
            {
                return BadRequest(new ApiResponse<Meeting>(false, "Bu toplantı zaten iptal edilmiş.", null));
            }

            // Toplantıyı iptal et
            meeting.IsCancelled = true;
            meeting.CancelledAt = DateTime.UtcNow;
            meeting.Status = "cancelled";
            
            // İptal nedeni varsa ekle (description alanına)
            if (request != null && !string.IsNullOrEmpty(request.CancellationReason))
            {
                meeting.Description = $"{meeting.Description}\n\nİptal Nedeni: {request.CancellationReason}";
            }

            await _dbContext.SaveChangesAsync();
            
            // Katılımcılara iptal bildirimi e-postası gönder
            if (meeting.Participants != null && meeting.Participants.Count() > 0)
            {
                try
                {
                    foreach (var participantEmail in meeting.Participants)
                    {
                        // Kullanıcının tam adını bul (eğer kullanıcı veritabanında kayıtlı ise)
                        if (participantEmail == null) continue;
                        string participantName = participantEmail; // Varsayılan olarak e-posta adresini kullan
                        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == participantEmail);
                        if (user != null)
                        {
                            participantName = $"{user.FirstName} {user.LastName}";
                        }
                        
                        // Asenkron olarak iptal e-postası gönder
                        _ = _emailService.SendMeetingCancelNotificationAsync(
                            participantEmail,
                            participantName,
                            meeting.Title
                        ).ContinueWith(t => 
                        {
                            if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, "Toplantı iptal bildirimi gönderilirken hata oluştu. Alıcı: {Email}", participantEmail);
                            }
                        });
                    }
                    _logger.LogInformation("Toplantı iptal bildirimleri {Count} katılımcıya gönderiliyor.", meeting.Participants.Count());
                }
                catch (Exception ex)
                {
                    // E-posta gönderimindeki hata toplantı iptal işlemini etkilemesin
                    _logger.LogError(ex, "Toplantı iptal bildirimleri gönderilirken hata oluştu. Toplantı ID: {Id}", meeting.Id);
                }
            }

            // Başarılı yanıt döndür
            return Ok(new ApiResponse<Meeting>(true, "Toplantı başarıyla iptal edildi.", meeting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplantı iptal edilirken bir hata oluştu. Toplantı ID: {Id}", id);
            return StatusCode(500, new ApiResponse<Meeting>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpPost("{id:guid}/upload-document")]
    public async Task<ActionResult<ApiResponse<MeetingDocument>>> UploadDocument(Guid id, IFormFile file)
    {
        try
        {
            // Toplantıyı bul
            var meeting = await _dbContext.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound(new ApiResponse<MeetingDocument>(false, "Toplantı bulunamadı.", null));
            }

            // İptal edilmiş toplantıya doküman yüklenemez
            if (meeting.IsCancelled)
            {
                return BadRequest(new ApiResponse<MeetingDocument>(false, "İptal edilmiş toplantıya doküman yüklenemez.", null));
            }

            // Dosya kontrolü
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<MeetingDocument>(false, "Geçersiz dosya.", null));
            }

            // Yükleme klasörünü oluştur
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "MeetingDocuments");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Dosya adını benzersiz yap
            var fileName = $"{meeting.Id}_{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Dosyayı kaydet
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Doküman kaydını oluştur
            var document = new MeetingDocument
            {
                MeetingId = meeting.Id,
                FileName = file.FileName,
                FilePath = $"/Uploads/MeetingDocuments/{fileName}",
                UploadedAt = DateTime.UtcNow
            };

            _dbContext.MeetingDocuments.Add(document);
            await _dbContext.SaveChangesAsync();

            // Başarılı yanıt döndür
            return Ok(new ApiResponse<MeetingDocument>(true, "Doküman başarıyla yüklendi.", document));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman yüklenirken bir hata oluştu. Toplantı ID: {Id}", id);
            return StatusCode(500, new ApiResponse<MeetingDocument>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpGet("{id:guid}/notify")]
    public async Task<ActionResult<ApiResponse<object>>> SendMeetingNotification(Guid id)
    {
        try
        {
            // Toplantıyı bul
            var meeting = await _dbContext.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound(new ApiResponse<object>(false, "Toplantı bulunamadı.", null));
            }

            // İptal edilmiş toplantı için bildirim gönderilmez
            if (meeting.IsCancelled)
            {
                return BadRequest(new ApiResponse<object>(false, "İptal edilmiş toplantı için bildirim gönderilemez.", null));
            }

            // Tüm kullanıcılara bildirim gönder (gerçek uygulamada toplantı katılımcıları seçilir)
            var users = await _dbContext.Users.ToListAsync();
            foreach (var user in users)
            {
                // Asenkron olarak mail gönder
                _ = _emailService.SendMeetingNotificationAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    meeting.Title,
                    meeting.StartAt,
                    meeting.EndAt
                );
            }

            // Başarılı yanıt döndür
            return Ok(new ApiResponse<object>(true, "Toplantı bildirimleri başarıyla gönderildi.", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplantı bildirimleri gönderilirken bir hata oluştu. Toplantı ID: {Id}", id);
            return StatusCode(500, new ApiResponse<object>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }
}
