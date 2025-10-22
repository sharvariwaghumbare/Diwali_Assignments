namespace ECommerce.Application.DTOs.User
{
    public class AssignRoleRequest
    {
        // Role names in an array to allow multiple roles to be assigned at once
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
