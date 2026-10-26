using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using E_Commerce.Data;
using E_Commerce.DTOs.Auth;
using E_Commerce.Helpers;
using E_Commerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        JwtSettings jwtSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim();

        var emailExists = await _context.Users
            .AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                !u.IsDeleted);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var customerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Customer");

        if (customerRole is null)
        {
            throw new InvalidOperationException(
                "Customer role was not found. Apply the database migration and seed the roles.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, dto.Password);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = customerRole.Id
        };

        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        var response = await CreateAuthResponseAsync(user);

        await transaction.CommitAsync();

        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                !u.IsDeleted);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(user, dto.Password);

            await _context.SaveChangesAsync();
        }

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            throw new UnauthorizedAccessException(
                "Refresh token is required.");
        }

        var tokenHash = HashRefreshToken(dto.RefreshToken);

        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (refreshToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        if (refreshToken.IsRevoked)
        {
            throw new UnauthorizedAccessException(
                "Refresh token has been revoked.");
        }

        if (refreshToken.IsExpired)
        {
            throw new UnauthorizedAccessException(
                "Refresh token has expired.");
        }

        var user = refreshToken.User;

        if (!user.IsActive || user.IsDeleted)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        var roles = await GetUserRolesAsync(user.Id);

        var now = DateTime.UtcNow;
        refreshToken.RevokedAtUtc = now;

        var newRefreshToken = GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashRefreshToken(newRefreshToken),
            ExpiresAtUtc = now.AddDays(7),
            CreatedAtUtc = now
        };

        await _context.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user, roles);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(LogoutDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            throw new UnauthorizedAccessException(
                "Refresh token is required.");
        }

        var tokenHash = HashRefreshToken(dto.RefreshToken);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (refreshToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        if (!refreshToken.IsRevoked)
        {
            refreshToken.RevokedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(User user)
    {
        var roles = await GetUserRolesAsync(user.Id);

        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = GenerateRefreshToken();
        var now = DateTime.UtcNow;

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashRefreshToken(refreshToken),
            ExpiresAtUtc = now.AddDays(7),
            CreatedAtUtc = now
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private async Task<List<string>> GetUserRolesAsync(long userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }

    private string GenerateAccessToken(
        User user,
        IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expirationTime =
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expirationTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(bytes);
    }
}
