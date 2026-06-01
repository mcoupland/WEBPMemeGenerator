using Microsoft.Win32;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using WEBPMemeGenerator.Models;
using System.Windows.Data;

namespace WEBPMemeGenerator;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();
    private readonly DispatcherTimer _playbackTimer = new();
    private bool _isPlaying;
    private SKBitmap? _bitmap;
    private SKCodec? _codec;
    private SKBitmap[] _frames = [];
    private double[] _frameStartTimes = [];
    private int[] _frameDurations = [];
    private string? _selectedWebpPath = string.Empty;

    public ObservableCollection<TextSegmentModel> TextSegments { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        _vm = new MainViewModel();
        DataContext = _vm;
        _playbackTimer.Tick += PlaybackTimer_Tick;
        _vm.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentFrame))
        {
            LoadFrame(_vm.CurrentFrame);
            SkiaControl.InvalidateVisual();
        }
    }

    #region Add/Remove Text Segments
    private void AddTextSegment_Click(object sender, RoutedEventArgs e)
    {
        var textSegment = new TextSegmentModel
        {
            Text = "New text",
            StartFrame = 0,
            StopFrame = _vm.MaxFrameIndex,
            MaxFrame = _vm.MaxFrameIndex
        };

        _vm.TextSegments.Add(textSegment);

        var label = new Label
        {
            DataContext = textSegment,
            Width = _vm.ImageWidth,
            Foreground = System.Windows.Media.Brushes.Red,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        label.SetBinding(ContentControl.ContentProperty, new Binding(nameof(TextSegmentModel.Text)));

        Canvas.SetLeft(label, 0);
        Canvas.SetTop(label, 0);

        TextOverlayCanvas.Children.Add(label);
    }
    private void RemoveTextSegment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.DataContext is TextSegmentModel textSegment)
        {
            _vm.TextSegments.Remove(textSegment);
        }
    }
    #endregion

    #region Load WEBP File
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select WEBP file",
            Filter = "WEBP files (*.webp)|*.webp",
            Multiselect = false,
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true)
            return;

        LoadWebp(dialog.FileName);
    }
    private void LoadWebp(string path)
    {
        _selectedWebpPath = path;
        _vm.SetFile(path);

        _codec?.Dispose();
        _bitmap?.Dispose();

        using var stream = File.OpenRead(path);
        _codec = SKCodec.Create(stream);

        if (_codec is null)
            return;

        var info = _codec.Info;

        _bitmap = new SKBitmap(info.Width, info.Height);
        _codec.GetPixels(_bitmap.Info, _bitmap.GetPixels());
        _vm.ImageWidth = info.Width;
        _vm.ImageHeight = info.Height;
        _vm.FrameCount = _codec.FrameCount;
        _vm.CurrentFrame = 0;
        SkiaControl.InvalidateVisual(); // use your SKElement name here
    }
    #endregion

    #region Display
    private void SkiaImage_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (_bitmap is null)
            return;

        canvas.DrawBitmap(_bitmap, e.Info.Rect);
    }
    private void LoadFrame(int frameIndex)
    {
        if (_codec is null || _bitmap is null)
            return;

        if (frameIndex < 0 || frameIndex >= _codec.FrameCount)
            return;

        _bitmap.Erase(SKColors.Transparent);

        var options = new SKCodecOptions(frameIndex);
        _codec.GetPixels(_bitmap.Info, _bitmap.GetPixels(), options);
    }
    #endregion

    #region Playback
    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_codec is null || _vm.FrameCount <= 0)
            return;

        if (_isPlaying)
        {
            PausePlayback();
        }
        else
        {
            StartPlayback();
        }
    }
    private void StartPlayback()
    {
        _isPlaying = true;
        PlayPauseButton.Content = "Pause";

        SetPlaybackIntervalForCurrentFrame();
        _playbackTimer.Start();
    }
    private void PausePlayback()
    {
        _isPlaying = false;
        PlayPauseButton.Content = "Play";

        _playbackTimer.Stop();
    }
    private void PlaybackTimer_Tick(object? sender, EventArgs e)
    {
        if (_vm.FrameCount <= 0)
            return;

        var nextFrame = _vm.CurrentFrame + 1;

        if (nextFrame >= _vm.FrameCount)
            nextFrame = 0;

        _vm.CurrentFrame = nextFrame;

        SetPlaybackIntervalForCurrentFrame();
    }
    private void SetPlaybackIntervalForCurrentFrame()
    {
        var delayMs = 100;

        if (_frameDurations is not null &&
            _vm.CurrentFrame >= 0 &&
            _vm.CurrentFrame < _frameDurations.Length)
        {
            delayMs = _frameDurations[_vm.CurrentFrame];
        }

        if (delayMs <= 0)
            delayMs = 100;

        _playbackTimer.Interval = TimeSpan.FromMilliseconds(delayMs);
    }
    #endregion
}