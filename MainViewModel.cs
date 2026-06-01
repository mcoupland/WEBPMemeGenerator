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

    private int _frameCount;
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
    public int MaxFrameIndex => Math.Max(0, FrameCount - 1);    

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

    public void SetFile(string path)
    {
        FileName = Path.GetFileName(path);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}