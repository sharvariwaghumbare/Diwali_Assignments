using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services.Abstract;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// Controller for managing orders by admin.
    /// </summary>
    /// <response code="403">Forbidden. Only admins can access this endpoint.</response>
    /// <response code="401">Unauthorized. User must be authenticated.</response>
    /// <response code="429">Too many requests.</response>
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status429TooManyRequests)]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IInvoiceService _invoiceService;

        public AdminOrdersController(IOrderService orderService, IInvoiceService invoiceService)
        {
            _orderService = orderService;
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Get all orders.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns the list of orders.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Get order by id.
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/admin/orders/{id}
        ///     {
        ///         "id": "12345678-1234-1234-1234-123456789012"
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Returns the order.</response>
        /// <response code="400">If the order is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order is null)
                throw new BusinessRuleException("Order not found.");

            return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
        }

        /// <summary>
        /// Update order status.
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <param name="dto">Pending 0, Paid 1, Shipped 2, Delivered 3, Cancelled 4</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/admin/orders/{id}/status
        ///     {
        ///         "Status": 2
        ///     }
        /// </remarks>
        /// <exception cref="BusinessRuleException"></exception>
        /// <response code="200">Returns a message indicating the order status was updated.</response>
        /// <response code="400">If the order is not found or status is equal to new value or order is already cancelled.</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<string?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatusUpdateDto dto)
        {
            var updated = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            if (!updated)
                throw new BusinessRuleException("Order not found or status update failed.");

            return Ok(ApiResponse<string?>.SuccessResponse(null, "Order status updated."));
        }

        /// <summary>
        /// Downloads the invoice for a specific order as a PDF file (Admin only).
        /// </summary>
        /// <param name="id">Order ID</param>
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
        [HttpGet("{id}/invoice")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadInvoiceAsAdmin(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(order);
            return File(pdfBytes, "application/pdf", $"invoice_{order.Id}.pdf");
        }
    }

}
