using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WEBPMemeGenerator.Classes;

namespace WEBPMemeGenerator
{
    /// <summary>
    /// Interaction logic for TextSegment.xaml
    /// </summary>
    public partial class TextSegmentControl : UserControl
    {
        private TextSegmentModel _textSegment;

        public TextSegmentControl()
        {
            InitializeComponent();
        }
    }
}
