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
using Path = System.IO.Path;

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
            foreach (var viewBox in viewBoxes)
            {
                _labels[index].Content = index + 1;
                _labels[index].Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192));
                viewBox.Child = _labels[index];
                viewBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                viewBox.VerticalAlignment = VerticalAlignment.Stretch;

                var rect = new Rectangle();
                rect.Fill = new SolidColorBrush(index % 2 == 0 ? Color.FromRgb(224, 224, 224) : Color.FromRgb(240, 240, 240));

                Grid.SetColumn(rect, index % 3);
                Grid.SetRow(rect, index / 3 + 1);
                Grid.SetZIndex(rect, 1);
                LayoutRoot.Children.Add(rect);

                Grid.SetColumn(viewBox, index % 3);
                Grid.SetRow(viewBox, index / 3 + 1);
                Grid.SetZIndex(viewBox, 2);
                LayoutRoot.Children.Add(viewBox);

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
                LayoutRoot.Children.Remove(shape.Parent as Canvas);
            }
            _shapes.Clear();

            var glowBrush = new ImageBrush {ImageSource = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "glow.png")))};
            var glowBrushCircle = new ImageBrush { ImageSource = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "glow.png"))) };
            glowBrushCircle.Stretch = Stretch.UniformToFill;
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1.1, 1));
            transformGroup.Children.Add(new TranslateTransform(-2, 0));
            glowBrushCircle.Transform = transformGroup;

            Point? lastPoint = null;
            var transformedText = Text.Text;
            _transformers.ToList().ForEach(x => transformedText = x.Transform(transformedText));
            var points = CalculatePoints();

            foreach (var symbol in transformedText)
            {
                if (symbol < '1' || symbol > '9') continue;
                var point = points[symbol - '1'];
                var viewBox = _labels[symbol - '1'].Parent as Viewbox;
                      
                if (lastPoint != null)
                {
                    if (lastPoint == point)
                    {
                        var circle = new Ellipse
                        {
                            Stroke = glowBrushCircle,
                            StrokeThickness = 20
                        };
                        circle.Width = 40;
                        circle.Height = 40;
                        Canvas.SetLeft(circle, LayoutRoot.ColumnDefinitions[Grid.GetColumn(viewBox)].ActualWidth / 2 - 20);
                        Canvas.SetTop(circle, LayoutRoot.RowDefinitions[Grid.GetRow(viewBox)].ActualHeight / 2 - 20);

                        var circle2 = new Ellipse
                        {
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeThickness = 15
                        };
                        circle2.Width = 30;
                        circle2.Height = 30;
                        Canvas.SetLeft(circle2, LayoutRoot.ColumnDefinitions[Grid.GetColumn(viewBox)].ActualWidth / 2 - 15);
                        Canvas.SetTop(circle2, LayoutRoot.RowDefinitions[Grid.GetRow(viewBox)].ActualHeight / 2 - 15);

                        var canvas = new Canvas();
                        Grid.SetZIndex(canvas, 555);
                        Grid.SetColumn(canvas, Grid.GetColumn(viewBox));
                        Grid.SetRow(canvas, Grid.GetRow(viewBox));
                        LayoutRoot.Children.Add(canvas);
                        canvas.Children.Add(circle);

                        var canvas2 = new Canvas();
                        Grid.SetZIndex(canvas2, 888);
                        Grid.SetColumn(canvas2, Grid.GetColumn(viewBox));
                        Grid.SetRow(canvas2, Grid.GetRow(viewBox));
                        LayoutRoot.Children.Add(canvas2);
                        canvas2.Children.Add(circle2);

                        _shapes.Add(circle);
                        _shapes.Add(circle2);
                    }
                    else
                    {
                        var lineLength =
                            Math.Sqrt(
                                Math.Abs(point.X - lastPoint.Value.X) *
                                Math.Abs(point.X - lastPoint.Value.X) +
                                Math.Abs(point.Y - lastPoint.Value.Y) *
                                Math.Abs(point.Y - lastPoint.Value.Y));
                        var proportion = lineLength / Math.Sin(Math.PI / 180 * 90);

                        var angle = Math.Asin(Math.Abs(point.Y - lastPoint.Value.Y) / proportion) * (180 / Math.PI);
                        if (point.X > lastPoint.Value.X && point.Y == lastPoint.Value.Y)
                        {
                            angle += 90;
                        }

                        if (point.X < lastPoint.Value.X && point.Y == lastPoint.Value.Y)
                        {
                            angle -= 90;
                        }

                        if (point.X == lastPoint.Value.X && point.Y > lastPoint.Value.Y)
                        {
                            angle += 90;
                        }

                        if (point.X == lastPoint.Value.X && point.Y < lastPoint.Value.Y)
                        {
                            angle -= 90;
                        }

                        if (point.X > lastPoint.Value.X && point.Y > lastPoint.Value.Y)
                        {
                            angle += 90;
                        }

                        if (point.X > lastPoint.Value.X && point.Y < lastPoint.Value.Y)
                        {
                            angle = Math.Asin(Math.Abs(point.X - lastPoint.Value.X) / proportion) * (180 / Math.PI);
                        }

                        if (point.X < lastPoint.Value.X && point.Y < lastPoint.Value.Y)
                        {
                            angle -= 90;
                        }

                        if (point.X < lastPoint.Value.X && point.Y > lastPoint.Value.Y)
                        {
                            angle = Math.Asin(Math.Abs(point.X - lastPoint.Value.X) / proportion) * (180 / Math.PI);
                            angle -= 180;
                        }

                        var line = new Line
                        {                       
                            Stroke = glowBrush,
                            StrokeThickness = 20,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round,
                        };
                        line.X1 = LayoutRoot.ColumnDefinitions[Grid.GetColumn(viewBox)].ActualWidth / 2;
                        line.Y1 = LayoutRoot.RowDefinitions[Grid.GetRow(viewBox)].ActualHeight / 2;
                        line.X2 = LayoutRoot.ColumnDefinitions[Grid.GetColumn(viewBox)].ActualWidth / 2;
                        line.Y2 = LayoutRoot.RowDefinitions[Grid.GetRow(viewBox)].ActualHeight / 2 + lineLength;

                        var line2 = new Line
                        {
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeThickness = 10,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round,
                        };
                        line2.X1 = LayoutRoot.ColumnDefinitions[Grid.GetColumn(viewBox)].ActualWidth / 2;
                        line2.Y1 = LayoutRoot.RowDefinitions[Grid.GetRow(viewBox)].ActualHeight / 2;
                        line2.X2 = LayoutRoot.ColumnDefinitions[Grid.GetColumn(viewBox)].ActualWidth / 2;
                        line2.Y2 = LayoutRoot.RowDefinitions[Grid.GetRow(viewBox)].ActualHeight / 2 + lineLength;

                        var canvas = new Canvas();
                        canvas.RenderTransformOrigin = new Point(0.5, 0.5);
                        canvas.RenderTransform = new RotateTransform(angle);
                        Grid.SetZIndex(canvas, 666);
                        Grid.SetColumn(canvas, Grid.GetColumn(viewBox));
                        Grid.SetRow(canvas, Grid.GetRow(viewBox));
                        LayoutRoot.Children.Add(canvas);
                        canvas.Children.Add(line);

                        var canvas2 = new Canvas();
                        canvas2.RenderTransformOrigin = new Point(0.5, 0.5);
                        canvas2.RenderTransform = new RotateTransform(angle);
                        Grid.SetZIndex(canvas2, 999);
                        Grid.SetColumn(canvas2, Grid.GetColumn(viewBox));
                        Grid.SetRow(canvas2, Grid.GetRow(viewBox));
                        LayoutRoot.Children.Add(canvas2);
                        canvas2.Children.Add(line2);

                        _shapes.Add(line);
                        _shapes.Add(line2);
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
            var size = new Size(LayoutRoot.ActualWidth, LayoutRoot.ActualHeight);
            var result = new RenderTargetBitmap((int)size.Width * 10, (int)size.Height * 10, 96, 96, PixelFormats.Pbgra32);

            ScaleTransform myScaleTransform = new ScaleTransform();
            myScaleTransform.ScaleY = 10;
            myScaleTransform.ScaleX = 10;

            LayoutRoot.RenderTransform = myScaleTransform;
            Menu.Visibility = Visibility.Collapsed;
            Text.Visibility = Visibility.Collapsed;

            foreach (var children in LayoutRoot.Children)
            {
                if (!(children is Canvas))
                {
                    (children as UIElement).Visibility = Visibility.Hidden;
                }
            }

            LayoutRoot.UpdateLayout();
            result.Render(LayoutRoot);

            foreach (var children in LayoutRoot.Children)
            {
                if (!(children is Canvas))
                {
                    (children as UIElement).Visibility = Visibility.Visible;
                }
            }

            Menu.Visibility = Visibility.Visible;
            Text.Visibility = Visibility.Visible;
            LayoutRoot.RenderTransform = null;
            LayoutRoot.UpdateLayout();

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
