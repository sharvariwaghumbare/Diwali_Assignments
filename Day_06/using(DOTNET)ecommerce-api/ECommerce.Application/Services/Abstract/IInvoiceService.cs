using ECommerce.Application.DTOs.Order;

namespace ECommerce.Application.Services.Abstract
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(OrderDto order);
    }
}
