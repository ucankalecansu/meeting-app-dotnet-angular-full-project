using Company.Project.Context;
using Company.Project.Model.DTOs;
using Company.Project.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Company.Project.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        AppDbContext dbContext,
        ILogger<UsersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        try
        {
            var users = await _dbContext.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    ProfileImagePath = u.ProfileImagePath,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<UserDto>>(true, "Kullanıcılar başarıyla getirildi.", users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcılar listelenirken bir hata oluştu.");
            return StatusCode(500, new ApiResponse<List<UserDto>>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> Get(Guid id)
    {
        try
        {
            var user = await _dbContext.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    ProfileImagePath = u.ProfileImagePath,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto>(false, "Kullanıcı bulunamadı.", null));
            }

            return Ok(new ApiResponse<UserDto>(true, "Kullanıcı başarıyla getirildi.", user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı getirilirken bir hata oluştu. Kullanıcı ID: {Id}", id);
            return StatusCode(500, new ApiResponse<UserDto>(false, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.", null));
        }
    }
}
