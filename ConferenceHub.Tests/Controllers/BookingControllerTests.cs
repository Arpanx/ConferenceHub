using ConferenceHub.Controllers;
using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ConferenceHub.Tests.Controllers;

public class BookingControllerTests
{
    private readonly Mock<IBookingAdditionaService> _bookingServiceMock;
    private readonly BookingController _controller;

    public BookingControllerTests()
    {
        _bookingServiceMock = new Mock<IBookingAdditionaService>();
        _controller = new BookingController(
            _bookingServiceMock.Object);
    }

    [Fact]
    public async Task CreateBooking_ValidModel_ReturnsOk()
    {
        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = [1, 2]
        };

        var booking = new BookingDto
        {
            Id = 1,
            HallId = 1,
            HallName = "Зал А",
            StartTime = model.StartTime,
            EndTime = model.StartTime.AddHours(2),
            RoomCost = 4000m,
            ServicesCost = 800m,
            TotalCost = 4800m,
            Services =
            [
                new ServiceDto
                {
                    Id = 1,
                    Name = "Проєктор",
                    Price = 500m
                },
                new ServiceDto
                {
                    Id = 2,
                    Name = "Wi-Fi",
                    Price = 300m
                }
            ]
        };

        _bookingServiceMock
            .Setup(x => x.CreateAsync(
                model,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await _controller.CreateBooking(
            model,
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var returnedBooking =
            Assert.IsType<BookingDto>(okResult.Value);

        Assert.Equal(1, returnedBooking.Id);
        Assert.Equal(1, returnedBooking.HallId);
        Assert.Equal("Зал А", returnedBooking.HallName);
        Assert.Equal(4000m, returnedBooking.RoomCost);
        Assert.Equal(800m, returnedBooking.ServicesCost);
        Assert.Equal(4800m, returnedBooking.TotalCost);
    }

    [Fact]
    public async Task CreateBooking_ValidModel_CallsService()
    {
        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = [1, 2]
        };

        var booking = new BookingDto
        {
            Id = 1,
            HallId = 1,
            HallName = "Зал А",
            StartTime = model.StartTime,
            EndTime = model.StartTime.AddHours(2),
            RoomCost = 4000m,
            ServicesCost = 800m,
            TotalCost = 4800m
        };

        _bookingServiceMock
            .Setup(x => x.CreateAsync(
                model,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        await _controller.CreateBooking(
            model,
            CancellationToken.None);

        _bookingServiceMock.Verify(
            x => x.CreateAsync(
                model,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBooking_PassesCancellationTokenToService()
    {
        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = []
        };

        var booking = new BookingDto
        {
            Id = 1,
            HallId = 1,
            HallName = "Зал А",
            StartTime = model.StartTime,
            EndTime = model.StartTime.AddHours(2),
            RoomCost = 4000m,
            ServicesCost = 0m,
            TotalCost = 4000m
        };

        _bookingServiceMock
            .Setup(x => x.CreateAsync(
                model,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        await _controller.CreateBooking(
            model,
            cancellationToken);

        _bookingServiceMock.Verify(
            x => x.CreateAsync(
                model,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ServiceThrowsException_ExceptionIsPropagated()
    {
        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = []
        };

        _bookingServiceMock
            .Setup(x => x.CreateAsync(
                model,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Test exception"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.CreateBooking(
                model,
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateBooking_BusinessException_IsPropagated()
    {
        var model = new CreateBookingDto
        {
            HallId = 999,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = []
        };

        var exception = new ConferenceHub.Exceptions.BusinessException(
            "Hall was not found.",
            "HALL_NOT_FOUND",
            StatusCodes.Status404NotFound);

        _bookingServiceMock
            .Setup(x => x.CreateAsync(
                model,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var result = await Assert.ThrowsAsync<
            ConferenceHub.Exceptions.BusinessException>(
            () => _controller.CreateBooking(
                model,
                CancellationToken.None));

        Assert.Equal(
            "HALL_NOT_FOUND",
            result.ErrorCode);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            result.StatusCode);
    }
}
