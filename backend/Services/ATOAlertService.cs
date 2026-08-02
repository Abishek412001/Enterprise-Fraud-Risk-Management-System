using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Services;

public class ATOAlertService : IATOAlertService
{
    private readonly IATOAlertRepository _atoRepository;

    public ATOAlertService(IATOAlertRepository atoRepository)
    {
        _atoRepository = atoRepository;
    }

    public async Task<PagedResultDto<AtoAlertResponseDto>> SearchAtoAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _atoRepository.SearchAtoAlertsAsync(status, priority, severity, analystId, searchTerm, page, pageSize);

        return new PagedResultDto<AtoAlertResponseDto>
        {
            Items = pagedResult.Items.Select(MapToAlertResponseDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<AtoAlertDetailResponseDto?> GetByIdDetailAsync(int atoAlertId)
    {
        var alert = await _atoRepository.GetByIdDetailAsync(atoAlertId);
        if (alert == null) return null;

        var prevDevices = await _atoRepository.SearchDevicesAsync(alert.CustomerID, null, 1, 10);
        var recentSessions = await _atoRepository.SearchSessionsAsync(alert.CustomerID, null, 1, 10);

        var dto = new AtoAlertDetailResponseDto
        {
            ATOAlertID = alert.ATOAlertID,
            ATOAlertNumber = alert.ATOAlertNumber,
            CustomerID = alert.CustomerID,
            CustomerName = alert.Customer != null ? $"{alert.Customer.FirstName} {alert.Customer.LastName}" : string.Empty,
            CustomerEmail = alert.Customer?.Email ?? string.Empty,
            CustomerPhone = alert.Customer?.Phone ?? string.Empty,
            SessionID = alert.SessionID,
            IPAddress = alert.Session?.IPAddress ?? string.Empty,
            Country = alert.Session?.Country ?? "Unknown",
            Browser = alert.Session?.Browser ?? "Unknown",
            OperatingSystem = alert.Session?.OperatingSystem ?? "Unknown",
            DeviceFingerprint = alert.Session?.Device?.DeviceFingerprint ?? "N/A",
            AlertType = alert.AlertType,
            Severity = alert.Severity,
            Priority = alert.Priority,
            RiskScore = alert.RiskScore,
            Status = alert.Status,
            AssignedAnalystID = alert.AssignedAnalystID,
            AssignedAnalystName = alert.AssignedAnalyst?.Username,
            CreatedDate = alert.CreatedDate,
            Resolution = alert.Resolution,
            ResolutionNotes = alert.ResolutionNotes,
            CurrentDevice = alert.Session?.Device != null ? MapToDeviceDto(alert.Session.Device) : null,
            PreviousDevices = prevDevices.Items.Select(MapToDeviceDto).ToList(),
            RecentSessions = recentSessions.Items.Select(MapToSessionDto).ToList()
        };

        return dto;
    }

    public async Task AssignAtoAlertAsync(AssignAtoAlertDto dto)
    {
        await _atoRepository.AssignAtoAlertAsync(dto.ATOAlertID, dto.AnalystID);
    }

    public async Task CloseAtoAlertAsync(CloseAtoAlertDto dto)
    {
        await _atoRepository.CloseAtoAlertAsync(dto.ATOAlertID, dto.Resolution, dto.ResolutionNotes);
    }

    public async Task<PagedResultDto<CustomerSessionDto>> SearchSessionsAsync(int? customerId, string? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _atoRepository.SearchSessionsAsync(customerId, status, page, pageSize);

        return new PagedResultDto<CustomerSessionDto>
        {
            Items = pagedResult.Items.Select(MapToSessionDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<PagedResultDto<DeviceDto>> SearchDevicesAsync(int? customerId, bool? isBlocked, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _atoRepository.SearchDevicesAsync(customerId, isBlocked, page, pageSize);

        return new PagedResultDto<DeviceDto>
        {
            Items = pagedResult.Items.Select(MapToDeviceDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<int> RecordCustomerLoginAsync(RecordCustomerLoginDto dto)
    {
        return await _atoRepository.RecordCustomerLoginAsync(dto);
    }

    public async Task SetDeviceStatusAsync(int deviceId, bool isBlocked, bool isTrusted)
    {
        await _atoRepository.SetDeviceStatusAsync(deviceId, isBlocked, isTrusted);
    }

    public async Task<AtoSummaryStatsDto> GetSummaryStatsAsync()
    {
        return await _atoRepository.GetSummaryStatsAsync();
    }

    private static AtoAlertResponseDto MapToAlertResponseDto(ATOAlert a) => new()
    {
        ATOAlertID = a.ATOAlertID,
        ATOAlertNumber = a.ATOAlertNumber,
        CustomerID = a.CustomerID,
        CustomerName = a.Customer != null ? $"{a.Customer.FirstName} {a.Customer.LastName}" : string.Empty,
        SessionID = a.SessionID,
        IPAddress = a.Session?.IPAddress ?? string.Empty,
        Country = a.Session?.Country ?? "Unknown",
        Browser = a.Session?.Browser ?? "Unknown",
        OperatingSystem = a.Session?.OperatingSystem ?? "Unknown",
        DeviceFingerprint = a.Session?.Device?.DeviceFingerprint ?? "N/A",
        AlertType = a.AlertType,
        Severity = a.Severity,
        Priority = a.Priority,
        RiskScore = a.RiskScore,
        Status = a.Status,
        AssignedAnalystID = a.AssignedAnalystID,
        AssignedAnalystName = a.AssignedAnalyst?.Username,
        CreatedDate = a.CreatedDate
    };

    private static DeviceDto MapToDeviceDto(Device d) => new()
    {
        DeviceID = d.DeviceID,
        CustomerID = d.CustomerID,
        CustomerName = d.Customer != null ? $"{d.Customer.FirstName} {d.Customer.LastName}" : string.Empty,
        DeviceFingerprint = d.DeviceFingerprint,
        DeviceName = d.DeviceName,
        Browser = d.Browser,
        OperatingSystem = d.OperatingSystem,
        IPAddress = d.IPAddress,
        Country = d.Country,
        FirstSeen = d.FirstSeen,
        LastSeen = d.LastSeen,
        IsTrusted = d.IsTrusted,
        IsBlocked = d.IsBlocked
    };

    private static CustomerSessionDto MapToSessionDto(CustomerSession s) => new()
    {
        SessionID = s.SessionID,
        CustomerID = s.CustomerID,
        CustomerName = s.Customer != null ? $"{s.Customer.FirstName} {s.Customer.LastName}" : string.Empty,
        DeviceID = s.DeviceID,
        IPAddress = s.IPAddress,
        LoginTime = s.LoginTime,
        Country = s.Country,
        Browser = s.Browser,
        OperatingSystem = s.OperatingSystem,
        AuthenticationMethod = s.AuthenticationMethod,
        LoginStatus = s.LoginStatus,
        RiskScore = s.RiskScore
    };
}
