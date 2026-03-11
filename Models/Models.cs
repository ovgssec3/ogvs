using System.Text.Json.Serialization;

namespace CecGradingSystem.Models;

public class Student
{
    [JsonPropertyName("id")]           public string Id           { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("name")]         public string Name         { get; set; } = "";
    [JsonPropertyName("schoolId")]     public string SchoolId     { get; set; } = "";
    [JsonPropertyName("year")]         public string Year         { get; set; } = "";
    [JsonPropertyName("course")]       public string Course       { get; set; } = "";
    [JsonPropertyName("section")]      public string Section      { get; set; } = "";
    [JsonPropertyName("code")]         public string Code         { get; set; } = "";
    [JsonPropertyName("registeredAt")] public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public class GradeEntry
{
    [JsonPropertyName("subjectCode")] public string SubjectCode { get; set; } = "";
    [JsonPropertyName("subjectName")] public string SubjectName { get; set; } = "";
    [JsonPropertyName("units")]       public string Units       { get; set; } = "";
    [JsonPropertyName("period")]      public string Period      { get; set; } = "Final";
    [JsonPropertyName("grade")]       public string Grade       { get; set; } = "";
    [JsonPropertyName("remarks")]     public string Remarks     { get; set; } = "";
}

public class StudentGrades
{
    [JsonPropertyName("studentId")]    public string StudentId    { get; set; } = "";
    [JsonPropertyName("academicYear")] public string AcademicYear { get; set; } = "";
    [JsonPropertyName("grades")]       public List<GradeEntry> Grades { get; set; } = new();
    [JsonPropertyName("published")]    public bool Published      { get; set; } = false;
    [JsonPropertyName("updatedAt")]    public DateTime UpdatedAt  { get; set; } = DateTime.UtcNow;
}

public class GradingDatabase
{
    [JsonPropertyName("students")]      public List<Student>       Students      { get; set; } = new();
    [JsonPropertyName("studentGrades")] public List<StudentGrades> StudentGrades { get; set; } = new();
}

// ── Request models ──────────────────────────────────────────────
public class LoginRequest        { public string? Code     { get; set; } }
public class AdminLoginRequest   { public string? Username { get; set; } public string? Password { get; set; } }
public class ForgotRequest       { public string? Name     { get; set; } public string? SchoolId { get; set; } }

public class RegisterRequest
{
    public string? Name     { get; set; }
    public string? SchoolId { get; set; }
    public string? Year     { get; set; }
    public string? Course   { get; set; }
    public string? Section  { get; set; }
}

public class SaveGradesRequest
{
    public List<GradeEntry>? Grades      { get; set; }
    public string?           AcademicYear { get; set; }
    public bool              Published    { get; set; }
}
