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

namespace ReserveDiagram
{
    public enum ReserveDuration
    {

    }

    public interface IReserve
    {
        DateTime Start { get; }
        DateTime End { get; }
        IDictionary<ReserveDuration, TimeSpan> Duration { get; }
    }

    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public int TimeSpan
        {
            get { return (int)GetValue(TimeSpanProperty); }
            set { SetValue(TimeSpanProperty, value); }
        }

        public static readonly DependencyProperty TimeSpanProperty =
            DependencyProperty.Register(
                nameof(TimeSpan),
                typeof(int),
                typeof(UserControl1),
                new PropertyMetadata(
                    12,
                    (s, e) =>
                    {
                        if (s is UserControl1 ctrl &&
                            e.NewValue is int span)
                        {
                            ctrl.SetTimeSpan(span);
                        }
                    }));

        private void SetTimeSpan(int span)
        {

        }
    }
}
