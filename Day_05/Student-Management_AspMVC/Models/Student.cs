using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Web.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = default!;

        [Required, StringLength(50)]
        public string LastName { get; set; } = default!;

        [EmailAddress]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
