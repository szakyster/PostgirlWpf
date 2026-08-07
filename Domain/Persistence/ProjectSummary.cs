using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Postgirl.Domain.Persistence;

public class ProjectSummary : INotifyPropertyChanged
{
    private string _id = string.Empty;
    private string _name = string.Empty;
    private bool _isDefault;

    public string Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public bool IsDefault
    {
        get => _isDefault;
        set { _isDefault = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
