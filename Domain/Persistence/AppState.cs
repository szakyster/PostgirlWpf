using System.Collections.Generic;
using Postgirl.Domain.History;
using Postgirl.Domain.SavedRequests;
using Postgirl.Domain.Variables;

namespace Postgirl.Domain.Persistence;

public class AppState
{
    public List<RequestHistoryEntry> History { get; set; } = [];
    public List<SavedRequestEntry> SavedRequests { get; set; } = [];
    public List<OpenedDocumentEntry> OpenedDocuments { get; set; } = [];
    public List<VariableEntry> Variables { get; set; } = [];
    public List<ConfigurationStateEntry> Configuration { get; set; } = [];
    public string ActiveSidebarPanel { get; set; } = "SavedExpander";
}