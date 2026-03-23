namespace Postgirl.Domain.Http;

public class ResponseFile
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Bytes { get; set; } = [];
}
