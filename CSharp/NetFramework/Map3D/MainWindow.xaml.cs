﻿using System;
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

namespace Map3D
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //// Create the interop host control.
            //System.Windows.Forms.Integration.WindowsFormsHost host =
            //    new System.Windows.Forms.Integration.WindowsFormsHost();

            //// Create the ActiveX control.
            //D3DLib.D3DView d3DView = new D3DLib.D3DView();

            //// Assign the ActiveX control as the host control's child.
            //axHost.Child = (System.Windows.Forms.Control)d3DView;
        }
    }
}
