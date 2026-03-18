namespace EvaluatorApp.Models;

public class TeamMemberInfo
{
    public string Matricula { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public bool HasImage { get; set; }

    // For avatar display: first 2 chars of matrícula as fallback
    public string Initials => Matricula.Length >= 2 ? Matricula[..2] : Matricula;
}
