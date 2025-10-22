using DinkToPdf;
using DinkToPdf.Contracts;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services.Abstract;

namespace ECommerce.Infrastructure.Services.Pdf
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IConverter _converter;

        public InvoiceService(IConverter converter)
        {
            _converter = converter;
        }

        public Task<byte[]> GenerateInvoicePdfAsync(OrderDto order)
        {
            var html = $@"
            <html>
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        body {{ font-family: Arial; }}
                        table {{ width: 100%; border-collapse: collapse; }}
                        th, td {{ padding: 8px; border: 1px solid #ccc; }}
                    </style>
                </head>
                <body>
                    <h2>Invoice - Order #{order.Id}</h2>
                    <p>Date: {order.CreatedAt:yyyy-MM-dd HH:mm}</p>
                    <table>
                        <thead>
                            <tr>
                                <th>Product</th>
                                <th>Quantity</th>
                                <th>Price</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", order.Items.Select(i => $@"
                            <tr>
                                <td>{i.ProductName}</td>
                                <td>{i.Quantity}</td>
                                <td>{i.Price:C}</td>
                                <td>{i.TotalPrice:C}</td>
                            </tr>"))}
                        </tbody>
                    </table>
                    <h3>Total: {order.TotalAmount:C}</h3>
                </body>
            </html>";

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4
                },
                Objects = {
                    new ObjectSettings {
                        HtmlContent = html
                    }
                }
            };

            var pdf = _converter.Convert(doc);
            return Task.FromResult(pdf);
        }
    }
}
