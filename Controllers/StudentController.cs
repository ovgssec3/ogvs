using Microsoft.AspNetCore.Mvc;
using CecGradingSystem.Models;
using CecGradingSystem.Services;

namespace CecGradingSystem.Controllers;

[ApiController]
[Route("api/student")]
public class StudentController : ControllerBase
{
    private readonly DatabaseService _svc;
    public StudentController(DatabaseService svc) => _svc = svc;

    // POST /api/student/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.SchoolId))
            return BadRequest(new { message = "Name and School ID are required." });

        // Return existing code if already registered
        var existing = _svc.Db.Students.FirstOrDefault(s =>
            s.SchoolId.Equals(body.SchoolId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            return Ok(new { code = existing.Code, message = "Already registered." });

        // Build access code: LASTNAMEFIRSTNAME-SCHOOLID
        var parts    = body.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lastName = parts.Length > 1 ? parts.Last().ToUpper() : parts[0].ToUpper();
        var firstName = parts.Length > 1 ? parts[0].ToUpper() : "STUDENT";
        var idClean  = body.SchoolId.Trim().Replace("-", "").Replace(" ", "");
        var code     = $"{lastName}{firstName}-{idClean}";

        var student = new Student
        {
            Name     = body.Name.Trim(),
            SchoolId = body.SchoolId.Trim(),
            Year     = body.Year?.Trim()    ?? "",
            Course   = body.Course?.Trim()  ?? "",
            Section  = body.Section?.Trim() ?? "",
            Code     = code
        };

        _svc.Db.Students.Add(student);
        _svc.Save();

        Console.WriteLine($"[+] Registered: {student.Name} [{student.Code}]");
        return Ok(new { code, message = "Registration successful." });
    }

    // POST /api/student/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.Code))
            return BadRequest(new { message = "Access code is required." });

        var student = _svc.Db.Students.FirstOrDefault(s =>
            s.Code.Equals(body.Code.Trim(), StringComparison.OrdinalIgnoreCase));

        if (student == null)
            return Unauthorized(new { message = "Invalid access code. Please check and try again." });

        var sg = _svc.Db.StudentGrades.FirstOrDefault(g => g.StudentId == student.Id);

        if (sg == null || !sg.Published)
        {
            return Ok(new
            {
                student = new { student.Name, student.SchoolId, student.Year, student.Course, student.Section, academicYear = "" },
                grades  = Array.Empty<object>(),
                gpa     = (string?)null,
                message = "No grades published yet."
            });
        }

        var numericGrades = sg.Grades
            .Where(g => double.TryParse(g.Grade, out _))
            .Select(g => double.Parse(g.Grade))
            .ToList();

        string? gpa = numericGrades.Any()
            ? Math.Round(numericGrades.Average(), 2).ToString("F2")
            : null;

        return Ok(new
        {
            student = new { student.Name, student.SchoolId, student.Year, student.Course, student.Section, academicYear = sg.AcademicYear },
            grades  = sg.Grades,
            gpa
        });
    }

    // POST /api/student/forgot
    [HttpPost("forgot")]
    public IActionResult Forgot([FromBody] ForgotRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.SchoolId))
            return BadRequest(new { message = "Name and School ID are required." });

        var student = _svc.Db.Students.FirstOrDefault(s =>
            s.Name.Equals(body.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
            s.SchoolId.Equals(body.SchoolId.Trim(), StringComparison.OrdinalIgnoreCase));

        if (student == null)
            return NotFound(new { message = "No account found with that name and School ID." });

        return Ok(new { code = student.Code });
    }
}
