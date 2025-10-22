using System.Security.Claims;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing user orders and invoices.
    /// </summary>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IInvoiceService _invoiceService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrdersController(IOrderService orderService, IInvoiceService invoiceService, IHttpContextAccessor httpContextAccessor)
        {
            _orderService = orderService;
            _invoiceService = invoiceService;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        /// <summary>
        /// Gets the current user's orders.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns the list of orders.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Downloads the invoice for a specific order as a PDF file.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///     
        ///     GET /api/orders/{orderId}/invoice
        ///     {
        ///         "orderId": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <response code="200">Returns the invoice PDF file.</response>
        /// <response code="404">Order not found.</response>
        [HttpGet("{orderId}/invoice")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadInvoice(Guid orderId)
        {
            var userId = GetUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            var order = orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(order);
            return File(pdfBytes, "application/pdf", $"invoice_{order.Id}.pdf");
        }

        /// <summary>
        /// Places an order based on the current user's cart items.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/orders/checkout
        ///     {
        ///         "ShippingAddress": "123 Main St, City, Country",
        ///         "PaymentMethod": "CreditCard",
        ///         "CouponCode": "SUMMER2025"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Order placed successfully.</response>
        /// <response code="400">Your cart is empty or invalid request.</response>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            var userId = GetUserId();
            var order = await _orderService.CheckoutAsync(userId, request);
            if (order == null)
                throw new BusinessRuleException("Your cart is empty.");

            return Ok(ApiResponse<OrderDto>.SuccessResponse(order, "Order placed successfully."));
        }

        /// <summary>
        /// Cancels an existing order by its ID.
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/orders/{id}/cancel
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Order canceled successfully.</response>
        /// <response code="400">Order not found or cannot be canceled.</response>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = GetUserId();
            var result = await _orderService.CancelOrderAsync(userId, id);

            if (!result)
                throw new BusinessRuleException("Order not found or cannot be canceled.");

            return Ok(ApiResponse<string>.SuccessResponse(null, "Order canceled successfully."));
        }
    }

}
