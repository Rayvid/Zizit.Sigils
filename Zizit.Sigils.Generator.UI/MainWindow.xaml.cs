using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zizit.Sigils.Model;

namespace Zizit.Sigils.Generator.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Label[] _labels = { new Label(), new Label(), new Label(), new Label(), new Label(), new Label(), new Label(), new Label(), new Label() };
        private readonly List<Shape> _shapes = new List<Shape>();

        readonly IEnumerable<ITextTransformer> _transformers = new ITextTransformer[]
        {
            new NonUSLettersRemovalTransformer(),
            new VovelsRemovalTransformer(),
            new LowercasingAndDuplicateLettersRlRemovalTransformer(),
            new ToNumbersTransformer()
        };

        public MainWindow()
        {
            InitializeComponent();

            LayoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MaxHeight = 30});

            LayoutRoot.RowDefinitions.Add(new RowDefinition { MinHeight = 100, MaxHeight = 300 });
            LayoutRoot.RowDefinitions.Add(new RowDefinition { MinHeight = 100, MaxHeight = 300 });
            LayoutRoot.RowDefinitions.Add(new RowDefinition { MinHeight = 100, MaxHeight = 300 });
            LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 100, MaxWidth = 300 });
            LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 100, MaxWidth = 300 });
            LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 100, MaxWidth = 300 });

            LayoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MaxHeight = 30 });

            Grid.SetColumn(Menu, 0);
            Grid.SetColumnSpan(Menu, 3);
            Grid.SetRow(Menu, 0);

            Grid.SetColumn(Text, 0);
            Grid.SetColumnSpan(Text, 3);
            Grid.SetRow(Text, 4);
            Text.Focus();

            var viewBoxes = new[] { new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox() };
            var index = 0;
            foreach (var viewBoxGrid in viewBoxes)
            {
                _labels[index].Content = index + 1;
                _labels[index].Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192));
                viewBoxGrid.Child = _labels[index];
                viewBoxGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                viewBoxGrid.VerticalAlignment = VerticalAlignment.Stretch;

                var rect = new Rectangle();
                rect.Fill = new SolidColorBrush(index % 2 == 0 ? Color.FromRgb(224, 224, 224) : Color.FromRgb(255, 255, 255));

                Grid.SetColumn(rect, index % 3);
                Grid.SetRow(rect, index / 3 + 1);
                Grid.SetZIndex(rect, 1);
                LayoutRoot.Children.Add(rect);

                Grid.SetColumn(viewBoxGrid, index % 3);
                Grid.SetRow(viewBoxGrid, index / 3 + 1);
                Grid.SetZIndex(viewBoxGrid, 2);
                LayoutRoot.Children.Add(viewBoxGrid);

                index++;
            }

            Grid.SetColumn(FrameBuffer, 0);
            Grid.SetRow(FrameBuffer, 1);
            Grid.SetColumnSpan(FrameBuffer, 3);
            Grid.SetRowSpan(FrameBuffer, 3);
            Grid.SetZIndex(FrameBuffer, 9999);
        }

        private Point[] CalculatePoints()
        {
            var result = new Point[9];

            for (int i = 0; i < 9; i++)
            {
                result[i] =
                    _labels[i]
                        .TranslatePoint(
                            new Point(_labels[i].ActualWidth/2, _labels[i].ActualHeight/2),
                            FrameBuffer);
            }

            return result;
        }

        private void DrawLines()
        {
            foreach (var shape in _shapes)
            {
                FrameBuffer.Children.Remove(shape);
            }
            _shapes.Clear();

            Point? lastPoint = null;
            var transformedText = Text.Text;
            _transformers.ToList().ForEach(x => transformedText = x.Transform(transformedText));
            var points = CalculatePoints();
            foreach (var symbol in transformedText)
            {
                if (symbol < '1' || symbol > '9') continue;
                var point = points[symbol - '1'];
                      
                if (lastPoint != null)
                {
                    if (lastPoint == point)
                    {
                        var circle = new Ellipse
                        {
                            Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                            StrokeThickness = 15
                        };
                        circle.Width = 30;
                        circle.Height = 30;
                        Canvas.SetLeft(circle, point.X - 15);
                        Canvas.SetTop(circle, point.Y - 15);
                        FrameBuffer.Children.Add(circle);
                        _shapes.Add(circle);
                    }
                    else
                    {
                        var line = new Line
                        {
                            Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                            StrokeThickness = 10,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round
                        };
                        line.X1 = lastPoint.Value.X;
                        line.Y1 = lastPoint.Value.Y;
                        line.X2 = point.X;
                        line.Y2 = point.Y;
                        FrameBuffer.Children.Add(line);
                        _shapes.Add(line);
                    }
                }

                lastPoint = point;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized) return;

            DrawLines();
        }

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsInitialized) return;

            DrawLines();
        }

        private void MenuItem_OnCloseClick(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            Close();
        }

        private RenderTargetBitmap GetImage()
        {
            var size = new Size(FrameBuffer.ActualWidth, FrameBuffer.ActualHeight);
            var result = new RenderTargetBitmap((int)size.Width * 10, (int)size.Height * 10, 96, 96, PixelFormats.Pbgra32);

            ScaleTransform myScaleTransform = new ScaleTransform();
            myScaleTransform.ScaleY = 10;
            myScaleTransform.ScaleX = 10;

            FrameBuffer.RenderTransform = myScaleTransform;
            FrameBuffer.UpdateLayout();

            result.Render(FrameBuffer);

            FrameBuffer.RenderTransform = null;
            FrameBuffer.UpdateLayout();

            return result;
        }

        private void MenuItem_OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "sigil"; 
            dlg.DefaultExt = ".png";
            dlg.Filter = "Png images (.png)|*.png";

            if (dlg.ShowDialog() ?? false)
            {
                using (var file = File.OpenWrite(dlg.FileName))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(GetImage()));
                    encoder.Save(file);
                }
            }
        }
    }
}
