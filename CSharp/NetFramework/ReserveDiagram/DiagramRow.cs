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
    public interface ColumnSpan
    {
        int Start { get; }
        int End { get; }
    }

    public class DiagramRow : Control
    {
        static DiagramRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramRow),
                new FrameworkPropertyMetadata(typeof(DiagramRow)));
        }

        public int ColumnSpan
        {
            get { return (int)GetValue(ColumnSpanProperty); }
            set { SetValue(ColumnSpanProperty, value); }
        }

        public static readonly DependencyProperty ColumnSpanProperty =
            DependencyProperty.Register(
                nameof(ColumnSpan),
                typeof(int),
                typeof(DiagramRow),
                new PropertyMetadata(
                    12,
                    (s, e) =>
                    {
                        if (s is DiagramRow ctrl &&
                            e.NewValue is int value)
                        {
                            ctrl.SetColumnSpan(value);
                        }
                    }));

        private Grid RowPanel
        {
            get => _rowPanel;
            set
            {
                _rowPanel = value;
                SetColumnSpan(ColumnSpan);
            }
        }
        private Grid _rowPanel;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RowPanel = GetTemplateChild("PART_RowPanel") as Grid;

        }

        private void SetColumnSpan(int value)
        {
            if (RowPanel is Grid panel)
            {
                while (panel.ColumnDefinitions.Count < value)
                {
                    panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                }
                while (panel.ColumnDefinitions.Count > value)
                {
                    panel.ColumnDefinitions.RemoveAt(value - 1);
                }
            }
        }
    }
}
