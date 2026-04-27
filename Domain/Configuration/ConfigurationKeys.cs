namespace Postgirl.Domain.Configuration;

public static class ConfigurationKeys
{
    public const string RetainedHistoryItemCount = "history.retainedItemCount";
    public const string HistoryGroupByDateEnabled = "history.groupByDateEnabled";
    public const string HttpRequestTimeoutSeconds = "http.requestTimeoutSeconds";
    public const string HttpMaxResponseBodySizeKb = "http.maxResponseBodySizeKb";
    public const string HttpDefaultUserAgent = "http.defaultUserAgent";
    public const string StorageKeepHistoryBetweenSessions = "storage.keepHistoryBetweenSessions";
}
