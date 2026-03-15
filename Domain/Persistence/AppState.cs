using System.Collections.Generic;
using Postgirl.Domain.History;
using Postgirl.Domain.SavedRequests;

namespace Postgirl.Domain.Persistence;

public class AppState
{
    public List<RequestHistoryEntry> History { get; set; } = [];
    public List<SavedRequestEntry> SavedRequests { get; set; } = [];
    public List<OpenedDocumentEntry> OpenedDocuments { get; set; } = [];
}