using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using WEBPMemeGenerator.Classes;

namespace WEBPMemeGenerator;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private string _fileName = "No file selected";
    private double _durationSeconds;
    private double _currentTimeSeconds;

    public ObservableCollection<TextSegmentModel> TextSegments { get; } = new();

    public string FileName
    {
        get => _fileName;
        set { _fileName = value; OnPropertyChanged(); }
    }

    public double DurationSeconds
    {
        get => _durationSeconds;
        set { _durationSeconds = value; OnPropertyChanged(); }
    }

    public double CurrentTimeSeconds
    {
        get => _currentTimeSeconds;
        set
        {
            _currentTimeSeconds = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentTimeDisplay));
        }
    }

    public string CurrentTimeDisplay =>
        $"{CurrentTimeSeconds:0.00}s / {DurationSeconds:0.00}s";

    public void SetFile(string path)
    {
        FileName = Path.GetFileName(path);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}