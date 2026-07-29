using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(IUserRepository userRepository, JwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<int> RegisterAsync(RegisterRequestDto request)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            Role = request.Role is "Admin" or "FraudAnalyst" ? request.Role : "FraudAnalyst"
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string? ipAddress)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user is null || !user.IsActive)
            return null;

        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        await _userRepository.RecordLoginAttemptAsync(user.UserId, ipAddress, isValid);

        if (!isValid)
            return null;

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }
}
