# Student Management System - ASP.NET Core MVC Tutorial

A complete step-by-step tutorial to build a Student Management System using ASP.NET Core MVC, Entity Framework Core, and SQLite.

## 🎯 What You'll Build

A fully functional web application that allows you to:
- Manage Students (Create, Read, Update, Delete)
- Manage Courses 
- Handle Student Enrollments in Courses
- View grades and student details

## 🛠️ Prerequisites

Before starting, make sure you have:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- A code editor (Visual Studio, VS Code, or any text editor)
- Basic knowledge of C# and web development concepts

## 📁 Final Project Structure

```
StudentManagement/
├── StudentManagement.sln
└── StudentManagement.Web/
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Properties/
    │   └── launchSettings.json
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   └── Migrations/
    ├── Models/
    │   ├── Student.cs
    │   ├── Course.cs
    │   └── Enrollment.cs
    ├── Controllers/
    │   ├── HomeController.cs
    │   ├── StudentsController.cs
    │   ├── CoursesController.cs
    │   └── EnrollmentsController.cs
    ├── Views/
    │   ├── Shared/
    │   │   └── _Layout.cshtml
    │   ├── Home/
    │   ├── Students/
    │   ├── Courses/
    │   └── Enrollments/
    └── wwwroot/
        ├── css/
        ├── js/
        └── images/
```

## 🚀 Step-by-Step Tutorial

### Step 1: Create the Project

Open your terminal/command prompt and run:

```bash
# Create solution & MVC project
dotnet new sln -n StudentManagement
dotnet new mvc -n StudentManagement.Web -f net8.0
dotnet sln add StudentManagement.Web

# Navigate to the web project
cd StudentManagement.Web
```

### Step 2: Add Required NuGet Packages

```bash
# Entity Framework Core with SQLite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# Code generation tools
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Install the scaffolding tool globally
dotnet tool install -g dotnet-aspnet-codegenerator
```

### Step 3: Configure Database Connection

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=student.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 4: Create Data Models

Create the `Models` folder and add the following files:

#### `Models/Student.cs`

```csharp
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
```

#### `Models/Course.cs`

```csharp
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
```

#### `Models/Enrollment.cs`

```csharp
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
```

### Step 5: Create Database Context

Create the `Data` folder and add `ApplicationDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentManagement.Web.Models;

namespace StudentManagement.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure unique student-course enrollment
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

### Step 6: Configure Services in Program.cs

Update your `Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentManagement.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Configure routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Students}/{action=Index}/{id?}");

// Optional: Seed sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Courses.Any())
    {
        db.Courses.AddRange(
            new Course { Title = "Introduction to Programming", Credits = 3 },
            new Course { Title = "Data Structures", Credits = 4 },
            new Course { Title = "Web Development", Credits = 3 },
            new Course { Title = "Database Design", Credits = 4 }
        );
        db.SaveChanges();
    }
}

app.Run();
```

### Step 7: Create and Update Database

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Step 8: Generate CRUD Controllers and Views

#### For Students:
```bash
dotnet aspnet-codegenerator controller -name StudentsController -m StudentManagement.Web.Models.Student -dc StudentManagement.Web.Data.ApplicationDbContext -outDir Controllers -udl -f
```

#### For Courses:
```bash
dotnet aspnet-codegenerator controller -name CoursesController -m StudentManagement.Web.Models.Course -dc StudentManagement.Web.Data.ApplicationDbContext -outDir Controllers -udl -f
```

#### For Enrollments:
```bash
dotnet aspnet-codegenerator controller -name EnrollmentsController -m StudentManagement.Web.Models.Enrollment -dc StudentManagement.Web.Data.ApplicationDbContext -outDir Controllers -udl -f
```

### Step 9: Run the Application

```bash
dotnet run
```

Open your browser and navigate to:
- `https://localhost:5001/Students` - Manage students
- `https://localhost:5001/Courses` - Manage courses  
- `https://localhost:5001/Enrollments` - Manage enrollments

## 🎉 What You Can Do Now

After completing these steps, your application will have:

✅ **Student Management**
- Add new students with first name, last name, email, and date of birth
- View all students in a table format
- Edit student information
- Delete students
- View individual student details

✅ **Course Management**
- Create courses with title and credit hours
- View all available courses
- Edit course information
- Delete courses

✅ **Enrollment Management**
- Enroll students in courses
- Assign grades to enrollments
- View all enrollments
- Edit enrollment information

## 🔧 Troubleshooting

### Common Issues and Solutions

**1. Build errors about locked files**
```bash
# Stop any running processes
taskkill /F /IM StudentManagement.Web.exe

# Clean and rebuild
dotnet clean
dotnet build
```

**2. Scaffolding fails with EF Core errors**
```bash
# Make sure you have all required packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

**3. Database not updating**
```bash
# Drop and recreate database
dotnet ef database drop
dotnet ef database update
```

**4. Port already in use**
- Check `Properties/launchSettings.json` and change the port numbers
- Or kill processes using the port: `netstat -ano | findstr :5001`

## 🚀 Next Steps - Enhancements

Once you have the basic system working, you can enhance it with:

### 1. **Search and Filtering**
- Add search functionality to find students by name
- Filter courses by credit hours
- Sort enrollments by grade

### 2. **Validation and Error Handling**
- Add custom validation attributes
- Implement global error handling
- Add client-side validation

### 3. **UI Improvements**
- Enhance Bootstrap styling
- Add responsive design
- Implement data tables with pagination

### 4. **Authentication and Authorization**
- Add user registration/login
- Implement role-based access (Admin, Teacher, Student)
- Secure sensitive operations

### 5. **Reporting Features**
- Generate student transcripts
- Export data to CSV/PDF
- Create grade reports

### 6. **Advanced Features**
- Email notifications
- File uploads for student photos
- Academic year and semester management
- Attendance tracking

## 📚 Learning Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)

## 🤝 Contributing

Feel free to fork this project and submit pull requests for any improvements!

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

---

**Happy Coding! 🎯**

If you encounter any issues or have questions, please create an issue in the repository or refer to the troubleshooting section above.