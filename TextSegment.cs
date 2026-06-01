using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WEBPMemeGenerator;

public sealed class TextSegment : INotifyPropertyChanged
{
    private string _text = "New text";
    private double _startTime;
    private double _stopTime;

    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(); }
    }

    public double StartTime
    {
        get => _startTime;
        set { _startTime = value; OnPropertyChanged(); }
    }

    public double StopTime
    {
        get => _stopTime;
        set { _stopTime = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}