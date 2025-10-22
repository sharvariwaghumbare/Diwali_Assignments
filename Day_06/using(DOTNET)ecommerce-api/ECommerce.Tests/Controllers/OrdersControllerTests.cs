using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services.Abstract;
using ECommerce.Application.Services.Concrete;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using ECommerce.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerce.Tests.Controllers
{
    public class OrdersControllerTests : ServiceTestBase
    {
        private readonly OrderService _orderService;
        private readonly OrdersController _ordersController;
        private readonly Guid userId = Guid.NewGuid();

        public OrdersControllerTests()
        {
            _orderService = new OrderService(Context);
            var _httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = TestHttpContextFactory.CreateHttpContextWithUserId(userId)
            };
            var _invoiceServiceMock = new Mock<IInvoiceService>();
            _ordersController = new OrdersController(_orderService, _invoiceServiceMock.Object, _httpContextAccessor);
        }

        [Fact]
        public async Task GetMyOrders_ShouldReturnOrders_WhenUserHasOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "abc" },
                new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 200, Status = OrderStatus.Delivered, ShippingAddress = "abc" }
            };
            Context.Orders.AddRange(orders);
            await Context.SaveChangesAsync();
            // Act
            var result = await _ordersController.GetMyOrders();
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as ApiResponse<List<OrderDto>>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.Should().HaveCount(2);
            response.Data.Should().ContainSingle(o => o.TotalAmount == 100);
            response.Data.Should().ContainSingle(o => o.TotalAmount == 200);
            response.Data.Should().ContainSingle(o => o.Status == OrderStatus.Pending);
            response.Data.Should().ContainSingle(o => o.Status == OrderStatus.Delivered);
        }

        [Fact]
        public async Task Checkout_ShouldReturnOrder_WhenCartHasItems()
        {
            // Arrange
            var category = await TestDataSeeder.SeedCategoryAsync(Context);
            await TestDataSeeder.SeedCartWithItemsAsync(Context, userId, category.Id, itemCount: 2);
            var request = new CheckoutRequestDto
            {
                Address = "123 Test Street",
                CouponCode = null,
                PaymentMethod = "PayPal"
            };
            // Act
            var result = await _ordersController.Checkout(request);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as ApiResponse<OrderDto>;
            response.Should().NotBeNull();
            response.Data.Should().NotBeNull();
            response.Data.ShippingAddress.Should().Be("123 Test Street");
        }

        [Fact]
        public async Task Checkout_ShouldReturnNotFound_WhenCartIsEmpty()
        {
            // Arrange
            var request = new CheckoutRequestDto
            {
                Address = "Empty Cart Street",
                CouponCode = null,
                PaymentMethod = "PayPal"
            };
            // Act
            var act = async () => await _ordersController.Checkout(request);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Your cart is empty.");
        }

        [Fact]
        public async Task Cancel_ShouldReturnOk_WhenOrderExists()
        {
            // Arrange
            var order = new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "abc" };
            Context.Orders.Add(order);
            await Context.SaveChangesAsync();
            // Act
            var result = await _ordersController.Cancel(order.Id);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Cancel_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            // Act
            var act = async () => await _ordersController.Cancel(orderId);
            // Assert
            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Order not found or cannot be canceled.");
        }

        [Fact]
        public async Task DownloadInvoice_ShouldReturnFile_WhenOrderExists()
        {
            // Arrange
            var order = new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "abc" };
            Context.Orders.Add(order);
            await Context.SaveChangesAsync();
            // Act
            var result = await _ordersController.DownloadInvoice(order.Id);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult.Should().NotBeNull();
            fileResult.ContentType.Should().Be("application/pdf");
        }

        [Fact]
        public async Task DownloadInvoice_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            // Act
            var result = await _ordersController.DownloadInvoice(orderId);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
