using System;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PixelRuler.UI.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromTicks(50) };

        public MainWindow()
        {
            this.InitializeComponent();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Win32Point pPoint);

        public override void BeginInit()
        {
            base.BeginInit();

            this.Deactivated += this.MainWindow_Deactivated;
            this.SizeChanged += this.MainWindow_SizeChanged;

            this.timer.Tick += this.Timer_Tick;
            this.timer.Start();
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var iStart = 0;
            if (this.RuleCanvas.Children.Count > 0)
            {
                var lastLine = this.RuleCanvas.Children[this.RuleCanvas.Children.Count - 1] as Line;
                iStart = (int)lastLine.X1;
            }

            for (var i = iStart + 2; i < this.RuleCanvas.ActualWidth; i += 2)
            {
                var startingPoint = this.RuleCanvas.ActualHeight;

                if (i % 100 == 0)
                {
                    startingPoint -= 30;
                    var label = new Label()
                    {
                        Content = i.ToString(CultureInfo.InvariantCulture)
                    };

                    this.RuleCanvas.Children.Add(label);
                    Canvas.SetTop(label, startingPoint - 10);
                    Canvas.SetLeft(label, i);
                }
                else if (i % 10 == 0)
                {
                    startingPoint -= 20;
                }
                else
                {
                    startingPoint -= 10;
                }

                var line = new Line
                {
                    X1 = i,
                    X2 = i,
                    Y1 = startingPoint,
                    Y2 = this.RuleCanvas.ActualHeight,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 1
                };

                this.RuleCanvas.Children.Add(line);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();

            var pointToWindow = Mouse.GetPosition(this);
            var pointToScreen = this.PointToScreen(pointToWindow);
            GetCursorPos(out var p);
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mousePoint = transform.Transform(p);

            Console.WriteLine($"Window: {this.Left}");
            Console.WriteLine($"Screen: {pointToScreen}");
            Console.WriteLine($"Mouse: {mousePoint}");

            if (Math.Abs(this.Left - mousePoint.X) < 200)
            {
                this.Width = 200;
            }
            else if (Math.Abs(this.Left - mousePoint.X) > 600)
            {
                this.Width = 600;
            }
            else
            {
                this.Width = Math.Abs(this.Left - mousePoint.X);
            }

            this.timer.Start();
        }
    }

    public struct Win32Point
    {
        public Win32Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }

        public int Y { get; set; }

        public override string ToString()
        {
            return $"{this.X},{this.Y}";
        }

        public static implicit operator Point(Win32Point point)
        {
            return new Point(point.X, point.Y);
        }
    }
}
