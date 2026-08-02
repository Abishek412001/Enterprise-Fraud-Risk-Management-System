using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IAuthService
{
    Task<int> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string? ipAddress);
}
