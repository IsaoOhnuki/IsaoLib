using OpenCvSharp;
using System;
using System.Collections.Generic;
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
    /// MatchPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class MatchPanel : UserControl
    {
        public MatchPanel()
        {
            InitializeComponent();
        }

        public List<Shape> AnchorCollection { get; } = new List<Shape>();

        public KeyPoint[] ItemsSource
        {
            get => (KeyPoint[])GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(KeyPoint[]),
                typeof(MatchPanel),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        if (s is MatchPanel ctrl &&
                            e.NewValue is KeyPoint[] keyPoints)
                        {
                            ctrl.canvas.Children.Clear();
                            ctrl.AnchorCollection.Clear();
                            Array.ForEach(keyPoints, x =>
                            {
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
                                    Tag = x.Octave,
                                    Data = geometry,
                                    Stroke = Brushes.Black,
                                    StrokeThickness = 1,
                                };
                                path.SetValue(Canvas.LeftProperty, (double)x.Pt.X);
                                path.SetValue(Canvas.TopProperty, (double)x.Pt.Y);
                                ctrl.AnchorCollection.Add(path);
                                ctrl.canvas.Children.Add(path);
                            });
                        }
                    }));
    }
}
