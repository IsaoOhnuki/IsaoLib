using Microsoft.Win32;
using MVVM;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenCV
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class VideoCapture : System.Windows.Window
    {
        public VideoCapture()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            field = value;
            OnPropertyChanged(name);
        }

        public ViewModel()
        {
            OpenSource = new DefaultCommand(v =>
            {
                Video();
            });
        }

        public DefaultCommand OpenSource { get; }

        public BitmapImage SourceImage
        {
            get => _sourceImage;
            set => SetProperty(ref _sourceImage, value);
        }
        private BitmapImage _sourceImage;

        public void Video()
        {
            Thread thread = new Thread(() =>
            {
                OpenCvSharp.VideoCapture video = new OpenCvSharp.VideoCapture();
                video.Open(0);
                if (video.IsOpened())
                {
                    var fs = FrameSource.CreateFrameSource_Camera(0);
                    using (var normalWindow = new OpenCvSharp.Window("normal"))
                    using (var normalFrame = new Mat())
                    using (var srFrame = new Mat())
                    {
                        while (true)
                        {
                            video.Read(normalFrame);
                            if (normalFrame.Empty())
                                break;

                            normalWindow.ShowImage(normalFrame);
                            int key = Cv2.WaitKey(100);
                            if (key == 27) break;   // ESC キーで閉じる
                        }
                    }
                }
            });
            thread.Start();
        }
    }
}
