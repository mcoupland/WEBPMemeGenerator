using Microsoft.Win32;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;

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

    private static List<TextSegmentControl> _textSegments = new();

    public MainWindow()
    {
        InitializeComponent();
    }
}