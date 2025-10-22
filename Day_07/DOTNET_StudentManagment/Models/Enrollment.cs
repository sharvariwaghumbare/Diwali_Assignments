using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Web.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [Range(0, 100)]
        public int? Grade { get; set; }
    }
}
