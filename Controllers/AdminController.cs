using Microsoft.AspNetCore.Mvc;
using CecGradingSystem.Models;
using CecGradingSystem.Services;

namespace CecGradingSystem.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private const string ADMIN_USER = "admin";
    private const string ADMIN_PASS = "cec2024";

    private readonly DatabaseService _svc;
    public AdminController(DatabaseService svc) => _svc = svc;

    // POST /api/admin/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminLoginRequest body)
    {
        if (!string.Equals(body.Username?.Trim(), ADMIN_USER, StringComparison.OrdinalIgnoreCase) ||
            body.Password != ADMIN_PASS)
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(new { message = "Login successful.", username = ADMIN_USER });
    }

    // GET /api/admin/stats
    [HttpGet("stats")]
    public IActionResult Stats()
    {
        return Ok(new
        {
            totalStudents   = _svc.Db.Students.Count,
            pendingGrades   = _svc.Db.Students.Count(s =>
                !_svc.Db.StudentGrades.Any(g => g.StudentId == s.Id && g.Published)),
            publishedGrades = _svc.Db.StudentGrades.Count(g => g.Published)
        });
    }

    // GET /api/admin/students
    [HttpGet("students")]
    public IActionResult Students()
    {
        var list = _svc.Db.Students.Select(s =>
        {
            var sg = _svc.Db.StudentGrades.FirstOrDefault(g => g.StudentId == s.Id);
            return new
            {
                s.Id, s.Name, s.SchoolId, s.Year, s.Course, s.Section, s.Code,
                gradeStatus = sg?.Published == true ? "Published" : "Pending",
                gradeCount  = sg?.Grades.Count ?? 0
            };
        }).OrderBy(s => s.Name).ToList();

        return Ok(list);
    }

    // GET /api/admin/grades/{studentId}
    [HttpGet("grades/{studentId}")]
    public IActionResult GetGrades(string studentId)
    {
        var sg = _svc.Db.StudentGrades.FirstOrDefault(g => g.StudentId == studentId);
        if (sg == null)
            return Ok(new { grades = Array.Empty<object>(), academicYear = "", published = false });

        return Ok(new { sg.Grades, sg.AcademicYear, sg.Published });
    }

    // PUT /api/admin/grades/{studentId}
    [HttpPut("grades/{studentId}")]
    public IActionResult SaveGrades(string studentId, [FromBody] SaveGradesRequest body)
    {
        var student = _svc.Db.Students.FirstOrDefault(s => s.Id == studentId);
        if (student == null)
            return NotFound(new { message = "Student not found." });

        var sg = _svc.Db.StudentGrades.FirstOrDefault(g => g.StudentId == studentId);
        if (sg == null)
        {
            sg = new StudentGrades { StudentId = studentId };
            _svc.Db.StudentGrades.Add(sg);
        }

        sg.Grades        = body.Grades ?? new();
        sg.AcademicYear  = body.AcademicYear ?? "";
        sg.Published     = body.Published;
        sg.UpdatedAt     = DateTime.UtcNow;
        _svc.Save();

        var action = body.Published ? "published" : "saved (draft)";
        Console.WriteLine($"[*] Grades {action} for: {student.Name}");
        return Ok(new { message = $"Grades {action} successfully." });
    }

    // DELETE /api/admin/students/{studentId}
    [HttpDelete("students/{studentId}")]
    public IActionResult DeleteStudent(string studentId)
    {
        var student = _svc.Db.Students.FirstOrDefault(s => s.Id == studentId);
        if (student == null)
            return NotFound(new { message = "Student not found." });

        _svc.Db.Students.Remove(student);

        var sg = _svc.Db.StudentGrades.FirstOrDefault(g => g.StudentId == studentId);
        if (sg != null) _svc.Db.StudentGrades.Remove(sg);

        _svc.Save();
        Console.WriteLine($"[-] Deleted: {student.Name} [{student.Code}]");
        return Ok(new { message = $"Student '{student.Name}' and all their data have been deleted." });
    }
}
