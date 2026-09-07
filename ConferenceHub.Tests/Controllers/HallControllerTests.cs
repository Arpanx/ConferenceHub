using ConferenceHub.Controllers;
using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ConferenceHub.Tests.Controllers;

public class HallControllerTests
{
    private readonly Mock<IHallService> _hallServiceMock;
    private readonly HallController _controller;

    public HallControllerTests()
    {
        _hallServiceMock = new Mock<IHallService>();
        _controller = new HallController(_hallServiceMock.Object);
    }

    [Fact]
    public async Task GetHalls_ReturnsOkWithHalls()
    {
        var halls = new List<HallDto>
        {
            new()
            {
                Id = 1,
                Name = "Зал А",
                Capacity = 50,
                BaseHourlyRate = 2000m,
                IsActive = true
            },
            new()
            {
                Id = 2,
                Name = "Зал B",
                Capacity = 100,
                BaseHourlyRate = 3500m,
                IsActive = true
            }
        };

        _hallServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(halls);

        var result = await _controller.GetHalls(
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var returnedHalls =
            Assert.IsType<List<HallDto>>(okResult.Value);

        Assert.Equal(2, returnedHalls.Count);
        Assert.Equal("Зал А", returnedHalls[0].Name);
        Assert.Equal("Зал B", returnedHalls[1].Name);
    }

    [Fact]
    public async Task GetHall_ExistingHall_ReturnsOk()
    {
        var hall = new HallDto
        {
            Id = 1,
            Name = "Зал А",
            Capacity = 50,
            BaseHourlyRate = 2000m,
            IsActive = true
        };

        _hallServiceMock
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hall);

        var result = await _controller.GetHall(
            1,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var returnedHall =
            Assert.IsType<HallDto>(okResult.Value);

        Assert.Equal(1, returnedHall.Id);
        Assert.Equal("Зал А", returnedHall.Name);
    }

    [Fact]
    public async Task GetHall_NonExistingHall_ReturnsNotFound()
    {
        _hallServiceMock
            .Setup(x => x.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((HallDto?)null);

        var result = await _controller.GetHall(
            999,
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetHallServices_ExistingHall_ReturnsOk()
    {
        var services = new List<ServiceDto>
        {
            new()
            {
                Id = 1,
                Name = "Проєктор",
                Price = 500m
            },
            new()
            {
                Id = 2,
                Name = "Wi-Fi",
                Price = 300m
            }
        };

        _hallServiceMock
            .Setup(x => x.GetServicesAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        var result = await _controller.GetHallServices(
            1,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var returnedServices =
            Assert.IsType<List<ServiceDto>>(okResult.Value);

        Assert.Equal(2, returnedServices.Count);
        Assert.Equal("Проєктор", returnedServices[0].Name);
        Assert.Equal("Wi-Fi", returnedServices[1].Name);
    }

    [Fact]
    public async Task GetHallServices_NonExistingHall_ReturnsNotFound()
    {
        _hallServiceMock
            .Setup(x => x.GetServicesAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ServiceDto>?)null);

        var result = await _controller.GetHallServices(
            999,
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateHall_ValidModel_ReturnsCreatedAtAction()
    {
        var model = new CreateHallDto
        {
            Name = "Новый зал",
            Capacity = 80,
            BaseHourlyRate = 2500m,
            ServiceIds = [1, 2]
        };

        var hall = new HallDto
        {
            Id = 10,
            Name = "Новый зал",
            Capacity = 80,
            BaseHourlyRate = 2500m,
            IsActive = true
        };

        _hallServiceMock
            .Setup(x => x.CreateAsync(
                model,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hall);

        var result = await _controller.CreateHall(
            model,
            CancellationToken.None);

        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(
            nameof(HallController.GetHall),
            createdResult.ActionName);

        Assert.Equal(
            10,
            createdResult.RouteValues!["id"]);

        var returnedHall =
            Assert.IsType<HallDto>(createdResult.Value);

        Assert.Equal(10, returnedHall.Id);
        Assert.Equal("Новый зал", returnedHall.Name);
    }

    [Fact]
    public async Task UpdateHall_ExistingHall_ReturnsOk()
    {
        var model = new UpdateHallDto
        {
            Name = "Зал А Updated",
            Capacity = 60,
            BaseHourlyRate = 2200m,
            IsActive = true,
            ServiceIds = [1, 2]
        };

        var hall = new HallDto
        {
            Id = 1,
            Name = "Зал А Updated",
            Capacity = 60,
            BaseHourlyRate = 2200m,
            IsActive = true
        };

        _hallServiceMock
            .Setup(x => x.UpdateAsync(
                1,
                model,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hall);

        var result = await _controller.UpdateHall(
            1,
            model,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var returnedHall =
            Assert.IsType<HallDto>(okResult.Value);

        Assert.Equal(1, returnedHall.Id);
        Assert.Equal("Зал А Updated", returnedHall.Name);
        Assert.Equal(60, returnedHall.Capacity);
        Assert.Equal(2200m, returnedHall.BaseHourlyRate);
    }

    [Fact]
    public async Task UpdateHall_NonExistingHall_ReturnsNotFound()
    {
        var model = new UpdateHallDto
        {
            Name = "Не существует",
            Capacity = 50,
            BaseHourlyRate = 2000m,
            IsActive = true
        };

        _hallServiceMock
            .Setup(x => x.UpdateAsync(
                999,
                model,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((HallDto?)null);

        var result = await _controller.UpdateHall(
            999,
            model,
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteHall_ExistingHall_ReturnsNoContent()
    {
        _hallServiceMock
            .Setup(x => x.DeleteAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.DeleteHall(
            1,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteHall_NonExistingHall_ReturnsNotFound()
    {
        _hallServiceMock
            .Setup(x => x.DeleteAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.DeleteHall(
            999,
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAvailableHalls_ValidRequest_ReturnsOk()
    {
        var startTime = new DateTime(
            2026, 9, 7, 10, 0, 0);

        var duration = TimeSpan.FromHours(2);

        var halls = new List<HallDto>
        {
            new()
            {
                Id = 1,
                Name = "Зал А",
                Capacity = 50,
                BaseHourlyRate = 2000m,
                IsActive = true
            }
        };

        _hallServiceMock
            .Setup(x => x.GetAvailableAsync(
                startTime,
                duration,
                40,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(halls);

        var result = await _controller.GetAvailableHalls(
            startTime,
            duration,
            40,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var returnedHalls =
            Assert.IsType<List<HallDto>>(okResult.Value);

        Assert.Single(returnedHalls);
        Assert.Equal(1, returnedHalls[0].Id);
    }
}