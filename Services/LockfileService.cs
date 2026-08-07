using System;
using System.IO;

namespace Postgirl.Services;

/// <summary>
/// Prevents the application from running in multiple instances simultaneously.
/// Holds the lockfile open in the data folder for the entire lifetime of the application.
/// </summary>
public sealed class LockfileService : IDisposable
{
    private const string LockFileName = "postgirl.lock";

    private FileStream? _lockStream;

    private string GetLockFilePath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Postgirl");

        Directory.CreateDirectory(dir);
        return Path.Combine(dir, LockFileName);
    }

    /// <summary>
    /// Attempts to acquire the lock.
    /// </summary>
    /// <returns>true if successful (this is the first instance); false if another instance is already running.</returns>
    public bool TryAcquire()
    {
        try
        {
            _lockStream = new FileStream(
                GetLockFilePath(),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);

            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        _lockStream?.Dispose();
        _lockStream = null;

        try
        {
            File.Delete(GetLockFilePath());
        }
        catch
        {
            // non-critical if deletion fails – next startup will fail on FileShare.None instead
        }
    }
}
