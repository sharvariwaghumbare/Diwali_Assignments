using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Web.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = default!;

        [Range(0, 10)]
        public int Credits { get; set; } = 3;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
