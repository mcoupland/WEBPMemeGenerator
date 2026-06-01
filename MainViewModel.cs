using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using WEBPMemeGenerator.Models;

namespace WEBPMemeGenerator;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private string _fileName = "No file selected";
    private double _durationSeconds;
    private double _currentTimeSeconds;
    private int _imageWidth;
    private int _imageHeight;
    private int _currentFrame; 
    private double _fontSize = 48;
    private int _frameCount;

    public int MaxFrameIndex => Math.Max(0, FrameCount - 1);

    public ObservableCollection<TextSegmentModel> TextSegments { get; } = new();

    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize == value)
                return;

            _fontSize = value;
            OnPropertyChanged();
        }
    }

    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            if (_currentFrame == value) return;
            _currentFrame = value;
            OnPropertyChanged();
        }
    }

    public int FrameCount
    {
        get => _frameCount;
        set
        {
            if (_frameCount == value) return;
            _frameCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MaxFrameIndex));
        }
    }

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

    public int ImageWidth
    {
        get => _imageWidth;
        set
        {
            _imageWidth = value;
            OnPropertyChanged();
        }
    }

    public int ImageHeight
    {
        get => _imageHeight;
        set
        {
            _imageHeight = value;
            OnPropertyChanged();
        }
    }

    public string CurrentTimeDisplay => $"{CurrentTimeSeconds:0.00}s / {DurationSeconds:0.00}s";

    public IReadOnlyList<double> FontSizes { get; } =
    [
        8, 9, 10, 11, 12, 14, 16, 18,
        20, 22, 24, 26, 28, 36, 48, 72
    ];

    public void SetFile(string path)
    {
        FileName = Path.GetFileName(path);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}