using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Helpers
{
    public abstract class ServiceTestBase : IDisposable
    {
        protected readonly ECommerceDbContext Context;

        protected ServiceTestBase()
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // new DB per test class
                .Options;

            Context = new ECommerceDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
