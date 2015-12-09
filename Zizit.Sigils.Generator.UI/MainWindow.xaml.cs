using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            LayoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MaxHeight = 30 });
            LayoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MaxHeight = 30 });

            Grid.SetColumn(Menu, 0);
            Grid.SetColumnSpan(Menu, 3);
            Grid.SetRow(Menu, 0);
            Grid.SetZIndex(Menu, 99999);

            Grid.SetColumn(Text, 0);
            Grid.SetColumnSpan(Text, 3);
            Grid.SetRow(Text, 4);
            Text.Focus();
            Grid.SetZIndex(Text, 99999);

            Grid.SetColumn(ColorsLabel, 0);
            Grid.SetRow(ColorsLabel, 5);
            Grid.SetZIndex(ColorsLabel, 99999);

            Grid.SetColumn(GlowColor, 1);
            Grid.SetRow(GlowColor, 5);
            Grid.SetZIndex(GlowColor, 99999);

            Grid.SetColumn(LineColor, 2);
            Grid.SetRow(LineColor, 5);
            Grid.SetZIndex(LineColor, 99999);

            Grid.SetColumn(SizeLabel, 0);
            Grid.SetRow(SizeLabel, 6);
            Grid.SetZIndex(SizeLabel, 99999);

            Grid.SetColumn(GlowSize, 1);
            Grid.SetRow(GlowSize, 6);
            Grid.SetZIndex(GlowSize, 99999);

            Grid.SetColumn(LineSize, 2);
            Grid.SetRow(LineSize, 6);
            Grid.SetZIndex(LineSize, 99999);

            var viewBoxes = new [] { new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox(), new Viewbox() };
            var labels = new [] { new Label(), new Label(), new Label(), new Label(), new Label(), new Label(), new Label(), new Label(), new Label() };

            var index = 0;
            foreach (var viewBox in viewBoxes)
            {
                labels[index].Content = index + 1;
                labels[index].Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192));
                viewBox.Child = labels[index];
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

            Grid.SetColumn(GlowCanvas, 0);
            Grid.SetRow(GlowCanvas, 0);
            Grid.SetColumnSpan(GlowCanvas, LayoutRoot.ColumnDefinitions.Count);
            Grid.SetRowSpan(GlowCanvas, LayoutRoot.RowDefinitions.Count);
            Grid.SetZIndex(GlowCanvas, 444);

            Grid.SetColumn(LineCanvas, 0);
            Grid.SetRow(LineCanvas, 0);
            Grid.SetColumnSpan(LineCanvas, LayoutRoot.ColumnDefinitions.Count);
            Grid.SetRowSpan(LineCanvas, LayoutRoot.RowDefinitions.Count);
            Grid.SetZIndex(LineCanvas, 555);
        }

        private Rect GetDrawingCellCoords(int col, int row)
        {
            double left = 0;
            double top = LayoutRoot.RowDefinitions[0].ActualHeight;
            double height = 0;
            double width = 0;

            int i;
            for (i = 0; i < row; i++)
            {
                top += LayoutRoot.RowDefinitions[i + 1].ActualHeight;
            }
            height = LayoutRoot.RowDefinitions[i + 1].ActualHeight;

            for (i = 0; i < col; i++)
            {
                left += LayoutRoot.ColumnDefinitions[i].ActualWidth;
            }
            width = LayoutRoot.ColumnDefinitions[i].ActualWidth;

            return new Rect(new Point(left, top), new Size(width, height));
        }

        private Rect ConvertNumberToCellCoords(int cellNum)
        {
            return GetDrawingCellCoords(cellNum % 3, cellNum / 3);
        }

        private BitmapSource DrawGlow(string transformedText, double scaleX, double scaleY)
        {
            var bitmap = new RenderTargetBitmap((int)(LayoutRoot.ActualWidth * scaleX), (int)(LayoutRoot.ActualHeight * scaleY), 96, 96, PixelFormats.Pbgra32);
            Rect? lastRect = null;

            var glowBrush = new ImageBrush { Opacity = 0.9, ImageSource = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "glow.png"))) };
            var glowBrushCircle = new ImageBrush { ImageSource = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "glow.png"))) };
            glowBrushCircle.Stretch = Stretch.UniformToFill;
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1.2, 1));
            transformGroup.Children.Add(new TranslateTransform(-4 * (scaleX + scaleY) / 2, 0));
            glowBrushCircle.Transform = transformGroup;

            var result = new byte[(int)(LayoutRoot.ActualWidth * scaleX) * 4 * (int)(LayoutRoot.ActualHeight * scaleY)];

            foreach (var symbol in transformedText)
            {
                if (symbol < '1' || symbol > '9') continue;
                var rect = ConvertNumberToCellCoords(symbol - '1');
                rect.X *= scaleX;
                rect.Y *= scaleY;
                rect.Width *= scaleX;
                rect.Height *= scaleY;

                if (lastRect != null)
                {
                    if (lastRect == rect)
                    {
                        // Glow
                        var canvas = new Canvas();

                        var circle = new Ellipse
                        {
                            Stroke = glowBrushCircle,
                            StrokeThickness = (GlowSize.Value - 3.5) * (scaleX + scaleY) / 2
                        };
                        circle.Width = (GlowSize.Value - 3.5) * 2 * scaleX;
                        circle.Height = (GlowSize.Value - 3.5) * 2 * scaleY;
                        circle.Opacity = 0.35;
                        Canvas.SetLeft(circle, rect.Width / 2 - (GlowSize.Value - 3.5) * scaleX + rect.Left);
                        Canvas.SetTop(circle, rect.Height / 2 - (GlowSize.Value - 3.5) * scaleY + rect.Top);
                        canvas.Children.Add(circle);

                        var circle2 = new Ellipse
                        {
                            Stroke = glowBrushCircle,
                            StrokeThickness = (GlowSize.Value - 3) * (scaleX + scaleY) / 2
                        };
                        circle2.Width = (GlowSize.Value - 3) * 2 * scaleX;
                        circle2.Height = (GlowSize.Value - 3) * 2 * scaleY;
                        circle2.Opacity = 0.2;
                        Canvas.SetLeft(circle2, rect.Width / 2 - (GlowSize.Value - 3) * scaleX + rect.Left);
                        Canvas.SetTop(circle2, rect.Height / 2 - (GlowSize.Value - 3) * scaleY + rect.Top);
                        canvas.Children.Add(circle2);

                        var circle3 = new Ellipse
                        {
                            Stroke = glowBrushCircle,
                            StrokeThickness = (GlowSize.Value - 2.5) * (scaleX + scaleY) / 2
                        };
                        circle3.Width = (GlowSize.Value - 2.5) * 2 * scaleX;
                        circle3.Height = (GlowSize.Value - 2.5) * 2 * scaleY;
                        circle3.Opacity = 0.5;
                        Canvas.SetLeft(circle3, rect.Width / 2 - (GlowSize.Value - 2.5) * scaleX + rect.Left);
                        Canvas.SetTop(circle3, rect.Height / 2 - (GlowSize.Value - 2.5) * scaleY + rect.Top);
                        canvas.Children.Add(circle3);

                        canvas.Measure(new Size(rect.Width, rect.Height));
                        canvas.Arrange(new Rect(new Size(rect.Width, rect.Height)));
                        canvas.UpdateLayout();
                        
                        bitmap.Render(canvas);
                    }
                    else
                    {
                        var lineLength =
                           Math.Sqrt(
                               Math.Abs(rect.Left - lastRect.Value.Left) *
                               Math.Abs(rect.Left - lastRect.Value.Left) +
                               Math.Abs(rect.Top - lastRect.Value.Top) *
                               Math.Abs(rect.Top - lastRect.Value.Top));
                        var proportion = lineLength / Math.Sin(Math.PI / 180 * 90);

                        var angle = Math.Asin(Math.Abs(rect.Top - lastRect.Value.Top) / proportion) * (180 / Math.PI);
                        if (rect.Left > lastRect.Value.Left && rect.Top == lastRect.Value.Top)
                        {
                            angle += 90;
                        }

                        if (rect.Left < lastRect.Value.Left && rect.Top == lastRect.Value.Top)
                        {
                            angle -= 90;
                        }

                        if (rect.Left == lastRect.Value.Left && rect.Top > lastRect.Value.Top)
                        {
                            angle += 90;
                        }

                        if (rect.Left == lastRect.Value.Left && rect.Top < lastRect.Value.Top)
                        {
                            angle -= 90;
                        }

                        if (rect.Left > lastRect.Value.Left && rect.Top > lastRect.Value.Top)
                        {
                            angle += 90;
                        }

                        if (rect.Left > lastRect.Value.Left && rect.Top < lastRect.Value.Top)
                        {
                            angle = Math.Asin(Math.Abs(rect.Left - lastRect.Value.Left) / proportion) * (180 / Math.PI);
                        }

                        if (rect.Left < lastRect.Value.Left && rect.Top < lastRect.Value.Top)
                        {
                            angle -= 90;
                        }

                        if (rect.Left < lastRect.Value.Left && rect.Top > lastRect.Value.Top)
                        {
                            angle = Math.Asin(Math.Abs(rect.Left - lastRect.Value.Left) / proportion) * (180 / Math.PI);
                            angle -= 180;
                        }

                        // Glow
                        var canvas = new Canvas();

                        var line = new Line
                        {
                            Stroke = glowBrush,
                            StrokeThickness = GlowSize.Value * (scaleX + scaleY) / 2,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round,
                        };
                        line.X1 = rect.Width / 2;
                        line.Y1 = rect.Height / 2;
                        line.X2 = rect.Width / 2;
                        line.Y2 = rect.Height / 2 + lineLength;
                        line.Opacity = 0.5;
                        canvas.Children.Add(line);

                        var line2 = new Line
                        {
                            Stroke = glowBrush,
                            StrokeThickness = (GlowSize.Value + 0.5) * (scaleX + scaleY) / 2,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round,
                        };
                        line2.X1 = rect.Width / 2;
                        line2.Y1 = rect.Height / 2 - 0.5 * (scaleX + scaleY) / 2;
                        line2.X2 = rect.Width / 2;
                        line2.Y2 = rect.Height / 2 + lineLength + 0.5 * (scaleX + scaleY) / 2;
                        line2.Opacity = 0.35;
                        canvas.Children.Add(line2);

                        var line3 = new Line
                        {
                            Stroke = glowBrush,
                            StrokeThickness = (GlowSize.Value + 1) * (scaleX + scaleY) / 2,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round,
                        };
                        line3.X1 = rect.Width / 2;
                        line3.Y1 = rect.Height / 2 - 1 * (scaleX + scaleY) / 2;
                        line3.X2 = rect.Width / 2;
                        line3.Y2 = rect.Height / 2 + lineLength + 1 * (scaleX + scaleY) / 2;
                        line3.Opacity = 0.15;
                        canvas.Children.Add(line3);

                        canvas.RenderTransformOrigin = new Point(0.5, 0.5);

                        transformGroup = new TransformGroup();
                        transformGroup.Children.Add(new RotateTransform(angle));
                        transformGroup.Children.Add(new TranslateTransform(rect.Left, rect.Top));
                        canvas.RenderTransform = transformGroup;

                        canvas.Measure(new Size(rect.Width, rect.Height));
                        canvas.Arrange(new Rect(new Size(rect.Width, rect.Height)));
                        canvas.UpdateLayout();

                        bitmap.Render(canvas);
                        //
                    }
                }

                lastRect = rect;
            }

            bitmap.CopyPixels(
                result,
                (int)(LayoutRoot.ActualWidth * scaleX) * 4,
                0);

            for (var offset = 0; offset < result.Length; offset += 4)
            {
                if (result[offset + 3] == 0) continue;

                result[offset] = (byte)Math.Round(result[offset] / (result[offset + 3] / 255.0));
                result[offset + 1] = (byte)Math.Round(result[offset + 1] / (result[offset + 3] / 255.0));
                result[offset + 2] = (byte)Math.Round(result[offset + 2] / (result[offset + 3] / 255.0));
            }

            for (var offset = 0; offset < result.Length; offset += 4)
            {
                if (result[offset + 3] == 0) continue;

                result[offset] = Math.Min((byte)(result[offset] * (GlowColor.SelectedColor.Value.B / 255.0)), (byte)255);
                result[offset + 1] = Math.Min((byte)(result[offset + 1] * (GlowColor.SelectedColor.Value.G / 255.0)), (byte)255);
                result[offset + 2] = Math.Min((byte)(result[offset + 2] * (GlowColor.SelectedColor.Value.R / 255.0)), (byte)255);
                result[offset + 3] = Math.Min((byte)(result[offset + 3] * (GlowColor.SelectedColor.Value.A / 255.0)), (byte)255);
            }

            return
                BitmapSource.Create(
                    (int)(LayoutRoot.ActualWidth * scaleX),
                    (int)(LayoutRoot.ActualHeight * scaleY),
                    96, 96, PixelFormats.Bgra32, null, result, (int)(LayoutRoot.ActualWidth * scaleX) * 4);
        }

        private BitmapSource DrawLines(string transformedText, double scaleX, double scaleY)
        {
            var bitmap = new RenderTargetBitmap((int)(LayoutRoot.ActualWidth * scaleX), (int)(LayoutRoot.ActualHeight * scaleY), 96, 96, PixelFormats.Pbgra32);
            Rect? lastRect = null;

            var result = new byte[(int)(LayoutRoot.ActualWidth * scaleX) * 4 * (int)(LayoutRoot.ActualHeight * scaleY)];

            foreach (var symbol in transformedText)
            {
                if (symbol < '1' || symbol > '9') continue;
                var rect = ConvertNumberToCellCoords(symbol - '1');
                rect.X *= scaleX;
                rect.Y *= scaleY;
                rect.Width *= scaleX;
                rect.Height *= scaleY;
                var color = new Color
                {
                    A = 255,
                    R = LineColor.SelectedColor.Value.R,
                    G = LineColor.SelectedColor.Value.G,
                    B = LineColor.SelectedColor.Value.B
                };

                if (lastRect != null)
                {
                    if (lastRect == rect)
                    {
                        var canvas = new Canvas();

                        var circle = new Ellipse
                        {
                            Stroke = new SolidColorBrush(color),
                            StrokeThickness = (LineSize.Value + 2) * (scaleX + scaleY) / 2
                        };
                        circle.Width = (LineSize.Value + 2) * 2 * scaleX;
                        circle.Height = (LineSize.Value + 2) * 2 * scaleY;
                        Canvas.SetLeft(circle, rect.Width / 2 - (LineSize.Value + 2) * scaleX + rect.Left);
                        Canvas.SetTop(circle, rect.Height / 2 - (LineSize.Value + 2) * scaleY + rect.Top);
                        canvas.Children.Add(circle);
                        
                        canvas.Measure(new Size(rect.Width, rect.Height));
                        canvas.Arrange(new Rect(new Size(rect.Width, rect.Height)));
                        canvas.UpdateLayout();

                        bitmap.Render(canvas);
                    }
                    else
                    {
                        var lineLength =
                           Math.Sqrt(
                               Math.Abs(rect.Left - lastRect.Value.Left) *
                               Math.Abs(rect.Left - lastRect.Value.Left) +
                               Math.Abs(rect.Top - lastRect.Value.Top) *
                               Math.Abs(rect.Top - lastRect.Value.Top));
                        var proportion = lineLength / Math.Sin(Math.PI / 180 * 90);

                        var angle = Math.Asin(Math.Abs(rect.Top - lastRect.Value.Top) / proportion) * (180 / Math.PI);
                        if (rect.Left > lastRect.Value.Left && rect.Top == lastRect.Value.Top)
                        {
                            angle += 90;
                        }

                        if (rect.Left < lastRect.Value.Left && rect.Top == lastRect.Value.Top)
                        {
                            angle -= 90;
                        }

                        if (rect.Left == lastRect.Value.Left && rect.Top > lastRect.Value.Top)
                        {
                            angle += 90;
                        }

                        if (rect.Left == lastRect.Value.Left && rect.Top < lastRect.Value.Top)
                        {
                            angle -= 90;
                        }

                        if (rect.Left > lastRect.Value.Left && rect.Top > lastRect.Value.Top)
                        {
                            angle += 90;
                        }

                        if (rect.Left > lastRect.Value.Left && rect.Top < lastRect.Value.Top)
                        {
                            angle = Math.Asin(Math.Abs(rect.Left - lastRect.Value.Left) / proportion) * (180 / Math.PI);
                        }

                        if (rect.Left < lastRect.Value.Left && rect.Top < lastRect.Value.Top)
                        {
                            angle -= 90;
                        }

                        if (rect.Left < lastRect.Value.Left && rect.Top > lastRect.Value.Top)
                        {
                            angle = Math.Asin(Math.Abs(rect.Left - lastRect.Value.Left) / proportion) * (180 / Math.PI);
                            angle -= 180;
                        }

                        var canvas = new Canvas();

                        var line = new Line
                        {
                            Stroke = new SolidColorBrush(color),
                            StrokeThickness = LineSize.Value * (scaleX + scaleY) / 2,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round,
                        };
                        line.X1 = rect.Width / 2;
                        line.Y1 = rect.Height / 2;
                        line.X2 = rect.Width / 2;
                        line.Y2 = rect.Height / 2 + lineLength;
                        canvas.Children.Add(line);
                        canvas.RenderTransformOrigin = new Point(0.5, 0.5);

                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(new RotateTransform(angle));
                        transformGroup.Children.Add(new TranslateTransform(rect.Left, rect.Top));
                        canvas.RenderTransform = transformGroup;

                        canvas.Measure(new Size(rect.Width, rect.Height));
                        canvas.Arrange(new Rect(new Size(rect.Width, rect.Height)));
                        canvas.UpdateLayout();

                        bitmap.Render(canvas);
                    }
                }

                lastRect = rect;
            }

            bitmap.CopyPixels(
                result,
                (int)(LayoutRoot.ActualWidth * scaleX) * 4,
                0);

            for (var offset = 0; offset < result.Length; offset += 4)
            {
                if (result[offset + 3] == 0) continue;

                result[offset] = (byte)Math.Round(result[offset] / (result[offset + 3] / 255.0));
                result[offset + 1] = (byte)Math.Round(result[offset + 1] / (result[offset + 3] / 255.0));
                result[offset + 2] = (byte)Math.Round(result[offset + 2] / (result[offset + 3] / 255.0));
            }

            for (var offset = 0; offset < result.Length; offset += 4)
            {
                if (result[offset + 3] == 0) continue;
                result[offset + 3] = Math.Min((byte)(result[offset + 3] * (LineColor.SelectedColor.Value.A / 255.0)), (byte)255);
            }

            return
                BitmapSource.Create(
                    (int)(LayoutRoot.ActualWidth * scaleX),
                    (int)(LayoutRoot.ActualHeight * scaleY),
                    96, 96, PixelFormats.Bgra32, null, result, (int)(LayoutRoot.ActualWidth * scaleX) * 4);
        }

        private void DrawAll()
        {
            if (!IsInitialized) return;

            var transformedText = Text.Text;
            _transformers.ToList().ForEach(x => transformedText = x.Transform(transformedText));

            GlowCanvas.Source = DrawGlow(transformedText, 0.5, 0.5);
            LineCanvas.Source = DrawLines(transformedText, 1, 1);
        }

        private RenderTargetBitmap GetImage()
        {
            var transformedText = Text.Text;
            _transformers.ToList().ForEach(x => transformedText = x.Transform(transformedText));

            var glowImage = new Image();
            var linesImage = new Image();

            glowImage.Source = DrawGlow(transformedText, 5, 5);
            glowImage.Measure(new Size((int)(LayoutRoot.ActualWidth * 5), (int)(LayoutRoot.ActualHeight * 5)));
            glowImage.Arrange(new Rect(new Size((int)(LayoutRoot.ActualWidth * 5), (int)(LayoutRoot.ActualHeight * 5))));
            glowImage.UpdateLayout();

            linesImage.Source = DrawLines(transformedText, 5, 5);
            linesImage.Measure(new Size((int)(LayoutRoot.ActualWidth * 5), (int)(LayoutRoot.ActualHeight * 5)));
            linesImage.Arrange(new Rect(new Size((int)(LayoutRoot.ActualWidth * 5), (int)(LayoutRoot.ActualHeight * 5))));
            linesImage.UpdateLayout();

            var bitmap = new RenderTargetBitmap((int)(LayoutRoot.ActualWidth * 5), (int)(LayoutRoot.ActualHeight * 5), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(glowImage);
            bitmap.Render(linesImage);

            return bitmap;
        }

        private void MenuItem_OnSaveClick(object sender, RoutedEventArgs e)
        {
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
                MessageBox.Show("Saved", "Success", MessageBoxButton.OK);
            }
        }

        private void MenuItem_OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LineColor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            DrawAll();
        }

        private void GlowColor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            DrawAll();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DrawAll();
        }

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawAll();
        }

        private void GlowSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DrawAll();
        }

        private void LineSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DrawAll();
        }
    }
}
