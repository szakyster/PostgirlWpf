using Postgirl.Domain.History;
using Postgirl.Domain.SavedRequests;
using Postgirl.Domain.Variables;
using System.Collections.Generic;

namespace Postgirl.Domain.Persistence;

public class ProjectState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public List<RequestHistoryEntry> History { get; set; } = [];
    public List<SavedRequestEntry> SavedRequests { get; set; } = [];
    public List<OpenedDocumentEntry> OpenedDocuments { get; set; } = [];
    public List<VariableEntry> Variables { get; set; } = [];
    public string ActiveSidebarPanel { get; set; } = "SavedExpander";
}
