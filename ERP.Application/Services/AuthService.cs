using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ERP.Application.DTOs.Auth;
using ERP.Application.Options;
using ERP.Domain.Entities.System;
using ERP.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Application.Services;

public sealed class AuthService(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IPasswordHasher<SysUser> passwordHasher,
    IAuditService auditService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var normalizedUsername = request.Username.Trim();
        if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await unitOfWork.Repository<SysUser>()
            .Query()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == normalizedUsername, ct);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        await RevokeActiveRefreshTokensAsync(user.Id, ct);

        var refreshTokenValue = CreateRefreshToken();
        var refreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        await unitOfWork.Repository<SysRefreshToken>().AddAsync(new SysRefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = refreshTokenExpiry,
            CreatedByIp = ipAddress
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);

        await SafeCacheOperationAsync(() => cacheService.SetAsync(GetRefreshCacheKey(user.Id), refreshTokenValue, TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays), ct));

        await auditService.LogAsync("LOGIN", user.Id, user.Username, "sys_users", user.Id.ToString(), null, null, ipAddress, ct);

        return BuildLoginResponse(user, refreshTokenValue, refreshTokenExpiry);
    }

    public async Task LogoutAsync(int userId, string? refreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var tokenQuery = unitOfWork.Repository<SysRefreshToken>()
            .Query()
            .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            tokenQuery = tokenQuery.Where(x => x.Token == refreshToken);
        }

        var tokens = await tokenQuery.ToListAsync(ct);
        if (tokens.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                unitOfWork.Repository<SysRefreshToken>().Update(token);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        await SafeCacheOperationAsync(() => cacheService.RemoveAsync(GetRefreshCacheKey(userId), ct));
        await SafeCacheOperationAsync(() => cacheService.RemoveAsync(GetPermissionCacheKey(userId), ct));

        var user = await unitOfWork.Repository<SysUser>().GetByIdAsync(userId, ct);
        await auditService.LogAsync("LOGOUT", userId, user?.Username, "sys_users", userId.ToString(), null, null, ipAddress, ct);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var tokenEntity = await unitOfWork.Repository<SysRefreshToken>()
            .Query()
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, ct);

        if (tokenEntity is null || tokenEntity.RevokedAt is not null || tokenEntity.ExpiresAt <= now)
        {
            return null;
        }

        if (!tokenEntity.User.IsActive)
        {
            return null;
        }

        tokenEntity.RevokedAt = now;
        unitOfWork.Repository<SysRefreshToken>().Update(tokenEntity);

        var newRefreshToken = CreateRefreshToken();

        await unitOfWork.Repository<SysRefreshToken>().AddAsync(new SysRefreshToken
        {
            UserId = tokenEntity.UserId,
            Token = newRefreshToken,
            ExpiresAt = now.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedByIp = ipAddress
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);

        await SafeCacheOperationAsync(() => cacheService.SetAsync(
            GetRefreshCacheKey(tokenEntity.UserId),
            newRefreshToken,
            TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays),
            ct));

        return BuildLoginResponse(tokenEntity.User, newRefreshToken, now.AddDays(_jwtSettings.RefreshTokenExpiryDays));
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await unitOfWork.Repository<SysUser>()
            .Query()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, ct);

        return user is null ? null : MapUser(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var user = await unitOfWork.Repository<SysUser>()
            .Query()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, ct);

        if (user is null)
        {
            return false;
        }

        var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = user.Username;

        unitOfWork.Repository<SysUser>().Update(user);

        await RevokeActiveRefreshTokensAsync(userId, ct);
        await SafeCacheOperationAsync(() => cacheService.RemoveAsync(GetRefreshCacheKey(userId), ct));

        await unitOfWork.SaveChangesAsync(ct);

        await auditService.LogAsync("PASSWORD_CHANGE", userId, user.Username, "sys_users", userId.ToString(), null, null, ipAddress, ct);

        return true;
    }

    private LoginResponse BuildLoginResponse(SysUser user, string refreshToken, DateTimeOffset refreshTokenExpiresAt)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("full_name", user.FullName),
            new("language", user.LanguagePreference)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKeyRaw = _jwtSettings.SigningKey?.Trim() ?? string.Empty;
        if (signingKeyRaw.Length < 32)
        {
            throw new InvalidOperationException("JwtSettings:SigningKey must be at least 32 characters.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKeyRaw));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new AuthUserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                LanguagePreference = user.LanguagePreference,
                Roles = roles
            }
        };
    }

    private static AuthUserDto MapUser(SysUser user)
    {
        return new AuthUserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            LanguagePreference = user.LanguagePreference,
            Roles = user.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    private async Task RevokeActiveRefreshTokensAsync(int userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var activeTokens = await unitOfWork.Repository<SysRefreshToken>()
            .Query()
            .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
            unitOfWork.Repository<SysRefreshToken>().Update(token);
        }
    }

    private static async Task SafeCacheOperationAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch
        {
            // Cache failure should not block authentication flow.
        }
    }

    private static string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string GetRefreshCacheKey(int userId) => $"ERP_auth:refresh:{userId}";
    private static string GetPermissionCacheKey(int userId) => $"ERP_cfg:permissions:user:{userId}";
}



