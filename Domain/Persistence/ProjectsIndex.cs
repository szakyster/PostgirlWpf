using System.Collections.Generic;

namespace Postgirl.Domain.Persistence;

public class ProjectsIndex
{
    public string ActiveProjectId { get; set; } = string.Empty;
    public List<ProjectSummary> Projects { get; set; } = [];
}
