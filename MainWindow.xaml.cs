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
    private readonly DispatcherTimer _timer;

    private SKBitmap? _bitmap;
    private DateTime _playStartedAt;
    private double _playStartSeconds;
    private bool _isPlaying;

    private SKCodec? _codec;
    private SKBitmap[] _frames = [];
    private double[] _frameStartTimes = [];
    private double[] _frameDurations = [];

    public MainWindow()
    {
        InitializeComponent();

        DataContext = _vm;
                
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        _timer.Tick += (_, _) =>
        {
            if (!_isPlaying)
                return;

            var elapsed = (DateTime.Now - _playStartedAt).TotalSeconds;
            _vm.CurrentTimeSeconds = _playStartSeconds + elapsed;

            if (_vm.CurrentTimeSeconds >= _vm.DurationSeconds)
            {
                _vm.CurrentTimeSeconds = _vm.DurationSeconds;
                _isPlaying = false;
                _timer.Stop();
            }

            PreviewCanvas.InvalidateVisual();
        };

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentTimeSeconds))
                PreviewCanvas.InvalidateVisual();
        };
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "WEBP files (*.webp)|*.webp|Image files (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp"
        };

        if (dialog.ShowDialog() != true)
            return;

        LoadImage(dialog.FileName);
    }

    private void LoadImage(string path)
    {
        _codec?.Dispose();

        using var stream = File.OpenRead(path);
        using var data = SKData.Create(stream);

        _codec = SKCodec.Create(data);

        if (_codec == null)
        {
            MessageBox.Show("Could not load WEBP.");
            return;
        }

        var info = _codec.Info;
        var frameInfos = _codec.FrameInfo;

        _frames = new SKBitmap[frameInfos.Length];
        _frameStartTimes = new double[frameInfos.Length];
        _frameDurations = new double[frameInfos.Length];

        double currentTime = 0;

        for (int i = 0; i < frameInfos.Length; i++)
        {
            var bitmap = new SKBitmap(info.Width, info.Height);

            var options = new SKCodecOptions(i);
            _codec.GetPixels(info, bitmap.GetPixels(), options);

            _frames[i] = bitmap;
            _frameStartTimes[i] = currentTime;

            var duration = Math.Max(frameInfos[i].Duration, 20) / 1000.0;
            _frameDurations[i] = duration;

            currentTime += duration;
        }

        _vm.SetFile(path);
        _vm.DurationSeconds = currentTime;
        _vm.CurrentTimeSeconds = 0;

        PreviewCanvas.InvalidateVisual();
    }

    private void AddSegmentButton_Click(object sender, RoutedEventArgs e)
    {
        var segment = new TextSegment
        {
            Text = "Text",
            StartTime = _vm.CurrentTimeSeconds
        };

        segment.PropertyChanged += (_, _) => PreviewCanvas.InvalidateVisual();

        _vm.TextSegments.Add(segment);
        PreviewCanvas.InvalidateVisual();
    }

    private void RemoveSegmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: TextSegment segment })
        {
            _vm.TextSegments.Remove(segment);
            PreviewCanvas.InvalidateVisual();
        }
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_frames.Length == 0)
            return;

        if (_isPlaying)
        {
            _isPlaying = false;
            _timer.Stop();
            return;
        }

        if (_vm.CurrentTimeSeconds >= _vm.DurationSeconds)
            _vm.CurrentTimeSeconds = 0;

        _playStartSeconds = _vm.CurrentTimeSeconds;
        _playStartedAt = DateTime.Now;

        _isPlaying = true;
        _timer.Start();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_bitmap == null)
            return;

        var dialog = new SaveFileDialog
        {
            Filter = "PNG image (*.png)|*.png|WEBP image (*.webp)|*.webp",
            FileName = "output.png"
        };

        if (dialog.ShowDialog() != true)
            return;

        using var surface = SKSurface.Create(new SKImageInfo(_bitmap.Width, _bitmap.Height));
        DrawScene(surface.Canvas, _bitmap.Width, _bitmap.Height, drawToImageCoordinates: true);

        using var image = surface.Snapshot();

        var format = Path.GetExtension(dialog.FileName).Equals(".webp", StringComparison.OrdinalIgnoreCase)
            ? SKEncodedImageFormat.Webp
            : SKEncodedImageFormat.Png;

        using var data = image.Encode(format, 95);
        using var file = File.Create(dialog.FileName);
        data.SaveTo(file);
    }

    private void PreviewCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        DrawScene(e.Surface.Canvas, e.Info.Width, e.Info.Height, drawToImageCoordinates: false);
    }

    private void DrawScene(SKCanvas canvas, int targetWidth, int targetHeight, bool drawToImageCoordinates)
    {
        canvas.Clear(SKColors.Transparent);

        var frame = GetCurrentFrame();

        if (frame == null)
            return;

        SKRect imageRect;

        if (drawToImageCoordinates)
        {
            imageRect = new SKRect(0, 0, frame.Width, frame.Height);
        }
        else
        {
            imageRect = FitRect(
                sourceWidth: frame.Width,
                sourceHeight: frame.Height,
                targetWidth: targetWidth,
                targetHeight: targetHeight);
        }

        canvas.DrawBitmap(frame, imageRect);

        var scaleX = imageRect.Width / frame.Width;
        var scaleY = imageRect.Height / frame.Height;

        using var fillPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextSize = drawToImageCoordinates ? 42 : 42 * scaleX,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        using var strokePaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextSize = fillPaint.TextSize,
            Typeface = fillPaint.Typeface,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = drawToImageCoordinates ? 5 : 5 * scaleX
        };

        foreach (var segment in _vm.TextSegments)
        {
            if (_vm.CurrentTimeSeconds < segment.StartTime)
                continue;

            var x = drawToImageCoordinates
                ? segment.X
                : imageRect.Left + segment.X * scaleX;

            var y = drawToImageCoordinates
                ? segment.Y
                : imageRect.Top + segment.Y * scaleY;

            canvas.DrawText(segment.Text, x, y, strokePaint);
            canvas.DrawText(segment.Text, x, y, fillPaint);
        }
    }

    private static SKRect FitRect(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        var scale = Math.Min(
            targetWidth / (float)sourceWidth,
            targetHeight / (float)sourceHeight);

        var width = sourceWidth * scale;
        var height = sourceHeight * scale;

        var left = (targetWidth - width) / 2f;
        var top = (targetHeight - height) / 2f;

        return new SKRect(left, top, left + width, top + height);
    }

    private SKBitmap? GetCurrentFrame()
    {
        if (_frames.Length == 0)
            return null;

        var time = _vm.CurrentTimeSeconds % _vm.DurationSeconds;

        for (int i = _frames.Length - 1; i >= 0; i--)
        {
            if (time >= _frameStartTimes[i])
                return _frames[i];
        }

        return _frames[0];
    }
}