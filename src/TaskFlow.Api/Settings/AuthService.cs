using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Exceptions;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<User> _hasher;

    public AuthService(AppDbContext db, ITokenService tokenService, IPasswordHasher<User> hasher)
    {
        _db = db;
        _tokenService = tokenService;
        _hasher = hasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("A user with this email already exists.");

        if (await _db.Users.AnyAsync(u => u.Username == username))
            throw new ConflictException("This username is already taken.");

        var user = new User { Username = username, Email = email };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same error for "no user" and "wrong password" so attackers can't enumerate accounts
        if (user is null)
            throw new InvalidCredentialsException();

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expires) = _tokenService.CreateToken(user);
        return new AuthResponse(token, expires, user.Id, user.Username, user.Email);
    }
}
