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
using System.Windows.Threading;

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
            DataContext = new ViewModel(Dispatcher);
            Closing += VideoCaptureClosing;
        }

        private void VideoCaptureClosing(object sender, CancelEventArgs e)
        {
            if (DataContext is ViewModel model)
            {
                model.Cancel();
            }
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

        CancellationTokenSource LockObj;
        Dispatcher Dispatcher;

        public ViewModel(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            OpenSource = new DefaultCommand(v =>
            {
                if (v is bool check)
                {
                    if (check)
                    {
                        LockObj = new CancellationTokenSource();
                        Video(LockObj, LockObj.Token);
                    }
                    else
                    {
                        LockObj?.Cancel();
                        LockObj = null;
                    }
                }
            });
        }

        public DefaultCommand OpenSource { get; }

        public BitmapSource SourceImage
        {
            get => _sourceImage;
            set => SetProperty(ref _sourceImage, value);
        }
        private BitmapSource _sourceImage;

        public void SetSourceImage(Mat sourceImage)
        {
            if (Dispatcher.CheckAccess())
            {
                SourceImage = sourceImage != null ? BitmapSourceConverter.ToBitmapSource(sourceImage) : null;
            }
            else
            {
                Dispatcher.Invoke(() => SetSourceImage(sourceImage));
            }
        }

        public void Cancel()
        {
            LockObj?.Cancel();
            LockObj = null;
        }

        public void Video(object lockObj, CancellationToken token)
        {
            Thread thread = new Thread(() =>
            {
                OpenCvSharp.VideoCapture video = new OpenCvSharp.VideoCapture();
                video.Open(0);
                if (video.IsOpened())
                {
                    var fs = FrameSource.CreateFrameSource_Camera(0);
                    using (var normalFrame = new Mat())
                    using (var srFrame = new Mat())
                    {
                        while (true)
                        {
                            video.Read(normalFrame);
                            if (normalFrame.Empty())
                            {
                                break;
                            }

                            try
                            {
                                token.ThrowIfCancellationRequested();
                            }
                            catch
                            {
                                break;
                            }
                            lock (lockObj)
                            {
                                SetSourceImage(normalFrame);
                            }
                        }
                        lock (lockObj)
                        {
                            SetSourceImage(null);
                        }
                    }
                }
            });
            thread.Start();
        }
    }
}
