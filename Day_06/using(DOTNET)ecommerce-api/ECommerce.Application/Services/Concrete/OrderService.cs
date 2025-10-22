using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services.Abstract;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services.Concrete
{
    public class OrderService : IOrderService
    {
        private readonly ECommerceDbContext _context;

        public OrderService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync(Guid userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    ShippingAddress = o.ShippingAddress,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<OrderDto?> CheckoutAsync(Guid userId, CheckoutRequestDto request)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return null;

            decimal totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            // Apply coupon discount if valid
            string? appliedCouponCode = null;
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c =>
                        c.Code == request.CouponCode &&
                        !c.IsDeleted &&
                        c.IsActive &&
                        c.ExpiryDate > DateTime.UtcNow);

                if (coupon != null)
                {
                    // Global usage limit
                    if (coupon.TotalUsageCount >= coupon.MaxUsageCount)
                    {
                        throw new InvalidOperationException("This coupon has reached its maximum usage limit and cannot be used anymore.");
                    }

                    // User-specific usage check
                    var usage = await _context.CouponUsages
                        .FirstOrDefaultAsync(u => u.UserId == userId && u.CouponId == coupon.Id);

                    if (usage != null && usage.UsageCount >= coupon.MaxUsagePerUser)
                    {
                        throw new InvalidOperationException("You have already used this coupon the maximum number of times allowed per user.");
                    }

                    totalAmount -= coupon.DiscountAmount;
                    if (totalAmount < 0) totalAmount = 0;

                    appliedCouponCode = coupon.Code;

                    // Track total usage
                    coupon.TotalUsageCount++;

                    // Track per-user usage
                    if (usage != null)
                    {
                        usage.UsageCount++;
                    }
                    else
                    {
                        var newUsage = new CouponUsage
                        {
                            CouponId = coupon.Id,
                            UserId = userId,
                            UsageCount = 1
                        };
                        _context.CouponUsages.Add(newUsage);
                    }
                    coupon.UpdatedAt = DateTime.UtcNow;
                }
            }

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Paid,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = totalAmount,
                ShippingAddress = request.Address,
                CouponCode = appliedCouponCode,
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            // Product Stock decrease
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    if (product.StockQuantity < item.Quantity && product.StockQuantity > 0)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}.\nAvailable: {product.StockQuantity}, Requested: {item.Quantity}");
                    }
                    else if (product.StockQuantity <= 0)
                    {
                        throw new InvalidOperationException($"Product {product.Name} is out of stock.");
                    }
                    product.StockQuantity -= item.Quantity; // Decrease stock
                    product.SoldQuantity += item.Quantity; // Increase sold quantity
                }
                else
                {
                    throw new InvalidOperationException($"Product with ID {item.ProductId} not found.");
                }
            }

            await _context.SaveChangesAsync();

            return new OrderDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductName = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
        }

        // Admin
        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    ShippingAddress = o.ShippingAddress,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return false;
            if (order.Status == status || order.Status == OrderStatus.Cancelled) return false; // No change in status

            if (status == OrderStatus.Shipped && order.Status != OrderStatus.Paid)
            {
                return false; // Cannot ship if not paid
            }
            if (status == OrderStatus.Pending && order.Status != OrderStatus.Pending)
            {
                return false; // Cannot set to pending if already past Pending status
            }

            // If new status is Cancelled, get product and restock
            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity; // Restock the product
                        product.SoldQuantity -= item.Quantity; // Decrease sold quantity
                    }
                }
            }
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(Guid userId, Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null
                || order.Status == OrderStatus.Shipped
                || order.Status == OrderStatus.Delivered
                || order.Status == OrderStatus.Cancelled) return false;
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            // Optionally, you can add logic to restock items here
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity; // Restock the product
                    product.SoldQuantity -= item.Quantity; // Decrease sold quantity
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

    }

}
