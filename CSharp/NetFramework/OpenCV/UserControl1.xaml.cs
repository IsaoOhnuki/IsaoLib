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
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public List<Ellipse> AnchorCollection { get; } = new List<Ellipse>();

        public KeyPoint[] ItemsSource
        {
            get { return (KeyPoint[])GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(KeyPoint[]),
                typeof(UserControl1),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        if (s is UserControl1 ctrl &&
                            e.NewValue is KeyPoint[] keyPoints)
                        {
                            while (ctrl.AnchorCollection.Count() > 0)
                            {
                                Ellipse ellipse = ctrl.AnchorCollection.Last();
                                ctrl.AnchorCollection.Remove(ellipse);
                                ctrl.canvas.Children.Remove(ellipse);
                            }
                            Array.ForEach(keyPoints, x =>
                            {
                                Ellipse ellipse = new Ellipse()
                                {
                                    Width = 10,
                                    Height = 10,
                                    Stroke = Brushes.Black,
                                    StrokeThickness = 1,
                                };
                                ellipse.SetValue(Canvas.LeftProperty, (double)x.Pt.X - 5);
                                ellipse.SetValue(Canvas.TopProperty, (double)x.Pt.Y - 5);
                                ctrl.AnchorCollection.Add(ellipse);
                                ctrl.canvas.Children.Add(ellipse);
                            });
                        }
                    }));
    }
}
