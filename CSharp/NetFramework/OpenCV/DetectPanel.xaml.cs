using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

namespace OpenCV
{
    /// <summary>
    /// DetectPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class DetectPanel : UserControl
    {
        public DetectPanel()
        {
            InitializeComponent();
        }

        public List<Shape> AnchorCollection { get; } = new List<Shape>();

        public int Octave
        {
            get => (int)GetValue(OctaveProperty);
            set => SetValue(OctaveProperty, value);
        }

        public static readonly DependencyProperty OctaveProperty =
            DependencyProperty.Register(
                nameof(Octave),
                typeof(int),
                typeof(DetectPanel),
                new FrameworkPropertyMetadata(
                    -1,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (s, e) =>
                    {
                        if (s is DetectPanel ctrl &&
                            e.NewValue is int octave)
                        {
                            ctrl.ShowAnchor(octave, ctrl.Response);
                        }
                    }));

        public int Response
        {
            get => (int)GetValue(ResponseProperty);
            set => SetValue(ResponseProperty, value);
        }

        public static readonly DependencyProperty ResponseProperty =
            DependencyProperty.Register(
                nameof(Response),
                typeof(int),
                typeof(DetectPanel),
                new FrameworkPropertyMetadata(
                    -1,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (s, e) =>
                    {
                        if (s is DetectPanel ctrl &&
                            e.NewValue is int response)
                        {
                            ctrl.ShowAnchor(ctrl.Octave, response);
                        }
                    }));

        private void ShowAnchor(int octave, int response)
        {
            canvas.Children.Clear();
            AnchorCollection.ForEach(x =>
            {
                (int oct, int resp) = ((int, int))x.Tag;
                if ((octave < 0 || octave == oct) &&
                    (response < 0 || response == resp))
                {
                    canvas.Children.Add(x);
                }
            });
        }

        public KeyPoint[] ItemsSource
        {
            get => (KeyPoint[])GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static Brush[] ResponseBrushes =
            new Brush[]
            {
                new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xC8)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xFF)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0x40, 0x00, 0xFF)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0xC8, 0x00, 0xC8)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x52, 0x00)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x98, 0x00)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xD2, 0x00)),
                new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0xEC, 0x00)),
            };

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(KeyPoint[]),
                typeof(DetectPanel),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        if (s is DetectPanel ctrl &&
                            e.NewValue is KeyPoint[] keyPoints)
                        {
                            List<float> response = keyPoints.
                                Select(p => p.Response).Distinct().OrderBy(x => x).ToList();
                            float min = response.First();
                            float max = response.Last();

                            ctrl.canvas.Children.Clear();
                            ctrl.AnchorCollection.Clear();
                            ctrl.Octave = -1;
                            ctrl.Response = -1;
                            keyPoints.OrderBy(x => x.Response).ToList().ForEach(x =>
                            {
                                int resp = min == 0 && max == 0 ? 0 :
                                    (int)((x.Response - min) / (max - min) * (ResponseBrushes.Length - 1));

                                double radius = x.Size / 2;
                                PathGeometry geometry = new PathGeometry();
                                geometry.AddGeometry(new EllipseGeometry(
                                    new System.Windows.Point(0, 0), radius, radius));
                                geometry.AddGeometry(
                                    new LineGeometry(
                                        new System.Windows.Point(0, 0), new System.Windows.Point(radius, 0))
                                        {
                                            Transform = new RotateTransform(x.Angle),
                                        });
                                Path path = new Path()
                                {
                                    Tag = (x.Octave, resp),
                                    Data = geometry,
                                    Stroke = ResponseBrushes[resp],
                                    StrokeThickness = 1,
                                };
                                path.SetValue(Canvas.LeftProperty, (double)x.Pt.X);
                                path.SetValue(Canvas.TopProperty, (double)x.Pt.Y);
                                ctrl.AnchorCollection.Add(path);
                                if (ctrl.Octave < 0 || ctrl.Octave == x.Octave)
                                {
                                    ctrl.canvas.Children.Add(path);
                                }
                            });
                        }
                    }));
    }
}
