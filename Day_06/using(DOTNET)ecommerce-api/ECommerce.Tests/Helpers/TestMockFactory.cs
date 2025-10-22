using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommerce.Tests.Helpers;

public static class TestMockFactory
{
    public static Mock<UserManager<TUser>> CreateMockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(
            store.Object,
            null, null, null, null, null, null, null, null
        );
    }

    public static Mock<RoleManager<TRole>> CreateMockRoleManager<TRole>() where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        return new Mock<RoleManager<TRole>>(
            store.Object,
            null, null, null, null
        );
    }
}
