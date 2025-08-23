using Company.Project.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Company.Project.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MigrationController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(AppDbContext dbContext, ILogger<MigrationController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("update-meetings-table")]
    public async Task<IActionResult> UpdateMeetingsTable()
    {
        try
        {
            // ParticipantEmails sütununu ekle
            if (!await ColumnExistsAsync("Meetings", "ParticipantEmails"))
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE Meetings ADD ParticipantEmails NVARCHAR(MAX)");
                _logger.LogInformation("ParticipantEmails sütunu eklendi");
            }
            else
            {
                _logger.LogInformation("ParticipantEmails sütunu zaten mevcut");
            }

            // Status sütununu ekle
            if (!await ColumnExistsAsync("Meetings", "Status"))
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE Meetings ADD Status NVARCHAR(50) DEFAULT 'active'");
                _logger.LogInformation("Status sütunu eklendi");
            }
            else
            {
                _logger.LogInformation("Status sütunu zaten mevcut");
            }

            // Mevcut kayıtların Status değerlerini güncelle
            await _dbContext.Database.ExecuteSqlRawAsync(
                @"UPDATE Meetings 
                SET Status = CASE WHEN IsCancelled = 1 THEN 'cancelled' ELSE 'active' END 
                WHERE Status IS NULL");

            return Ok(new { success = true, message = "Meetings tablosu başarıyla güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meetings tablosu güncellenirken hata oluştu");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private async Task<bool> ColumnExistsAsync(string tableName, string columnName)
    {
        var sql = @"
            SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName";

        var parameters = new[] {
            new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName),
            new Microsoft.Data.SqlClient.SqlParameter("@ColumnName", columnName)
        };

        var result = await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        return result > 0;
    }
}
