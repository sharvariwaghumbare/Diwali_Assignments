﻿namespace ECommerce.Application.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string? Phone { get; set; }
        public List<string> Roles { get; set; }
        public bool? isBanned { get; set; }
    }
}
