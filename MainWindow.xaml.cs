using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WEBPMemeGenerator.Models;

namespace WEBPMemeGenerator;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();
    private readonly DispatcherTimer _timer = new DispatcherTimer();

    private SKBitmap? _bitmap;
    private DateTime _playStartedAt;
    private double _playStartSeconds;
    private bool _isPlaying;

    private SKCodec? _codec;
    private SKBitmap[] _frames = [];
    private double[] _frameStartTimes = [];
    private double[] _frameDurations = [];
    public ObservableCollection<TextSegmentModel> TextSegments { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    private void AddTextSegment_Click(object sender, RoutedEventArgs e)
    {
        var model = new TextSegmentModel
        {
            Text = "New text",
            StartFrame = 1,
            StopFrame = _frames.Length,
            MaxFrame = _frames.Length
        };

        _vm.TextSegments.Add(model);
    }
    private void RemoveTextSegment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.DataContext is TextSegmentModel textSegment)
        {
            _vm.TextSegments.Remove(textSegment);
        }
    }
}