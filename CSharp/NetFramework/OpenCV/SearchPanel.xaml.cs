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
    /// SearchPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchPanel : UserControl
    {
        public SearchPanel()
        {
            InitializeComponent();
        }

        public List<Shape> SearchCollection { get; } = new List<Shape>();

        public System.Windows.Point[][] ItemsSource
        {
            get => (System.Windows.Point[][])GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(System.Windows.Point[][]),
                typeof(SearchPanel),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        if (s is SearchPanel ctrl &&
                            e.NewValue is System.Windows.Point[][] keyPoints)
                        {
                            ctrl.canvas.Children.Clear();
                            ctrl.SearchCollection.Clear();
                            Array.ForEach(keyPoints, x =>
                            {
                                PathGeometry geometry = new PathGeometry()
                                {
                                    Figures = new PathFigureCollection()
                                    {
                                        new PathFigure(x[0],
                                            x.Skip(1).Select(y => new LineSegment(y, true)), true),
                                    },
                                };
                                Path path = new Path()
                                {
                                    Data = geometry,
                                    Stroke = Brushes.Red,
                                    StrokeThickness = 2,
                                };
                                ctrl.SearchCollection.Add(path);
                                ctrl.canvas.Children.Add(path);
                            });
                        }
                    }));
    }
}
