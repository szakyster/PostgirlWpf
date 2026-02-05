using System.Collections.Generic;
using Postgirl.Domain.History;

namespace Postgirl.Domain.Persistence;

public class AppState
{
    public List<RequestHistoryEntry> History { get; set; } = [];

    // később:
    // public List<SavedRequest> SavedRequests { get; set; }
    // public List<OpenDocument> OpenDocuments { get; set; }
}