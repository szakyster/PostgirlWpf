namespace Postgirl.Domain.Execution;

public enum HttpErrorType
{
    Unknown,
    Timeout,
    Network,
    DnsFailure,
    SslError,
    Cancelled
}