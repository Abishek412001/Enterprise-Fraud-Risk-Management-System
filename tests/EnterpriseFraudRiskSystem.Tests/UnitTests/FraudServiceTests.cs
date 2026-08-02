using Xunit;
using Moq;
using FluentAssertions;
using EnterpriseFraudRiskSystem.Services;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.DTOs;
using System.Threading.Tasks;

namespace EnterpriseFraudRiskSystem.Tests.UnitTests;

public class FraudServiceTests
{
    private readonly Mock<IFRMAlertRepository> _mockRepo;
    private readonly FRMAlertService _service;

    public FraudServiceTests()
    {
        _mockRepo = new Mock<IFRMAlertRepository>();
        _service = new FRMAlertService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAlertByIdAsync_ShouldReturnAlert_WhenExists()
    {
        // Arrange
        int alertId = 1;
        var alertDto = new FrmAlertDTO { AlertID = alertId, AlertNumber = "FRM-2026-0001", Status = "Open" };
        _mockRepo.Setup(r => r.GetByIdAsync(alertId)).ReturnsAsync(alertDto);

        // Act
        var result = await _service.GetByIdAsync(alertId);

        // Assert
        result.Should().NotBeNull();
        result!.AlertID.Should().Be(alertId);
        result.AlertNumber.Should().Be("FRM-2026-0001");
    }

    [Fact]
    public async Task CloseAlertAsync_ShouldInvokeRepository()
    {
        // Arrange
        int alertId = 1;
        string reason = "Resolved false positive";
        int analystId = 2;

        // Act
        await _service.CloseAlertAsync(alertId, reason, analystId);

        // Assert
        _mockRepo.Verify(r => r.CloseAlertAsync(alertId, reason, analystId), Times.Once);
    }
}
