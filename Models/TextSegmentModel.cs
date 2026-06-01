using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WEBPMemeGenerator.Models;

public class TextSegmentModel : INotifyPropertyChanged
{
    private string _text = string.Empty;
    private int _startFrame = 1;
    private int _stopFrame = 1;
    private int _maxFrame = 1;

    public string Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public int StartFrame
    {
        get => _startFrame;
        set => SetField(ref _startFrame, value);
    }

    public int StopFrame
    {
        get => _stopFrame;
        set => SetField(ref _stopFrame, value);
    }

    public int MaxFrame
    {
        get => _maxFrame;
        set => SetField(ref _maxFrame, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

}