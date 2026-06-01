using System;
using System.Windows;
using System.Windows.Controls;
using WEBPMemeGenerator.Classes;

namespace WEBPMemeGenerator;

public partial class TextSegmentControl : UserControl
{
    public event EventHandler<TextSegmentModel>? Saved;

    public TextSegmentControl()
    {
        InitializeComponent();
    }

    public TextSegmentControl(TextSegmentModel textSegment) : this()
    {
        TextSegment = textSegment;
    }

    public TextSegmentModel? TextSegment
    {
        get => DataContext as TextSegmentModel;
        set => DataContext = value;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (TextSegment is not null)
            Saved?.Invoke(this, TextSegment);
    }
}