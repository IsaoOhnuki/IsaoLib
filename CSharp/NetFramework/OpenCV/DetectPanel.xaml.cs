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
                new PropertyMetadata(
                    -1,
                    (s, e) =>
                    {
                        if (s is DetectPanel ctrl &&
                            e.NewValue is int octave)
                        {
                            ctrl.canvas.Children.Clear();
                            ctrl.AnchorCollection.ForEach(x =>
                            {
                                if (octave < 0 || octave == (int)x.Tag)
                                {
                                    ctrl.canvas.Children.Add(x);
                                }
                            });
                        }
                    }));

        public int MaxOctave
        {
            get => (int)GetValue(MaxOctaveProperty);
            private set => SetValue(MaxOctavePropertyKey, value);
        }

        public static readonly DependencyProperty MaxOctaveProperty =
            MaxOctavePropertyKey?.DependencyProperty;

        public static readonly DependencyPropertyKey MaxOctavePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(MaxOctave),
                typeof(int),
                typeof(DetectPanel),
                new PropertyMetadata(0));

        public KeyPoint[] ItemsSource
        {
            get => (KeyPoint[])GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

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
                                if (ctrl.Octave < 0 || ctrl.Octave == x.Octave)
                                {
                                    ctrl.canvas.Children.Add(path);
                                }
                            });
                            ctrl.MaxOctave = ctrl.AnchorCollection.Select(x => (int)x.Tag).Max();
                        }
                    }));
    }
}
