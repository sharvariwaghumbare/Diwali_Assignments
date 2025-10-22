using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Enums;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class AdminOrdersControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IInvoiceService> _invoiceServiceMock;
        private readonly AdminOrdersController _controller;

        public AdminOrdersControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _invoiceServiceMock = new Mock<IInvoiceService>();
            _controller = new AdminOrdersController(_orderServiceMock.Object, _invoiceServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfOrders()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto { Id = Guid.NewGuid(), TotalAmount = 100, Status = OrderStatus.Paid, ShippingAddress = "abc" },
                new OrderDto { Id = Guid.NewGuid(), TotalAmount = 200, Status = OrderStatus.Shipped, ShippingAddress = "abc" }
            };
            _orderServiceMock.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);
            // Act
            var result = await _controller.GetAll();
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<ApiResponse<List<OrderDto>>>();
            var response = okResult.Value as ApiResponse<List<OrderDto>>;
            response.Data.Should().BeEquivalentTo(orders);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new OrderDto { Id = orderId, TotalAmount = 100, Status = OrderStatus.Paid, ShippingAddress = "abc" };
            _orderServiceMock.Setup(s => s.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            // Act
            var result = await _controller.GetById(orderId);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<ApiResponse<OrderDto>>();
            var response = okResult.Value as ApiResponse<OrderDto>;
            response.Data.Should().Be(order);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderServiceMock.Setup(s => s.GetOrderByIdAsync(orderId)).ReturnsAsync((OrderDto)null);
            // Act
            var act = async () => await _controller.GetById(orderId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Order not found.");
        }

        [Fact]
        public async Task UpdateStatus_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateDto = new OrderStatusUpdateDto { Status = OrderStatus.Shipped };
            _orderServiceMock.Setup(s => s.UpdateOrderStatusAsync(orderId, updateDto.Status)).ReturnsAsync(true);
            // Act
            var result = await _controller.UpdateStatus(orderId, updateDto);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<ApiResponse<string?>>();
            var response = okResult.Value as ApiResponse<string?>;
            response.Message.Should().Be("Order status updated.");
        }

        [Fact]
        public async Task UpdateStatus_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateDto = new OrderStatusUpdateDto { Status = OrderStatus.Shipped };
            _orderServiceMock.Setup(s => s.UpdateOrderStatusAsync(orderId, updateDto.Status)).ReturnsAsync(false);
            // Act
            var act = async () => await _controller.UpdateStatus(orderId, updateDto);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Order not found or status update failed.");
        }
    }
}
