using Microsoft.Win32;
using MVVM;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private ImageSearchViewModel ImageSearchViewModel { get; } = new ImageSearchViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ImageSearchViewModel;
        }
    }

    public class ImageSearchViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            UndoSource.Update();
            RedoSource.Update();
            UndoTarget.Update();
            RedoTarget.Update();
            SourceToTarget.Update();
            TargetToSource.Update();
            Gray.Update();
        }
        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            field = value;
            OnPropertyChanged(name);
        }

        public MatStack Source { get; } = new MatStack();
        public MatStack Target { get; } = new MatStack();

        public ImageSearchViewModel()
        {
            OpenSource = new DefaultCommand(v =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    Source.Push(new BitmapImage(new Uri(dialog.FileName)));
                    SourceImage = Source.Get();
                    SourcePoints = null;
                }
            });
            OpenTarget = new DefaultCommand(v =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    Target.Push(new BitmapImage(new Uri(dialog.FileName)));
                    TargetImage = Target.Get();
                    TargetPoints = null;
                    SearchResult = false;
                }
            });
            UndoSource = new DefaultCommand(v =>
            {
                Source.Undo();
                SourceImage = Source.Get();
            }, v => Source.CanUndo);
            RedoSource = new DefaultCommand(v =>
            {
                Source.Redo();
                SourceImage = Source.Get();
            }, v => Source.CanRedo);
            UndoTarget = new DefaultCommand(v =>
            {
                Target.Undo();
                TargetImage = Target.Get();
            }, v => Target.CanUndo);
            RedoTarget = new DefaultCommand(v =>
            {
                Target.Redo();
                TargetImage = Target.Get();
            }, v => Target.CanRedo);
            SourceToTarget = new DefaultCommand(v =>
            {
                Target.Push(Source.Get());
                TargetImage = Target.Get();
            }, v => TargetImage != null && SourceImage != null);
            TargetToSource = new DefaultCommand(v =>
            {
                Source.Push(Target.Get());
                SourceImage = Source.Get();
            }, v => TargetImage != null && SourceImage != null);
            Gray = new DefaultCommand(v =>
            {
                if (bool.Parse(v.ToString()) is bool flg)
                {
                    Mat dst = CvGray(flg ? Target.Mat() : Source.Mat());
                    if (dst != null)
                    {
                        if (flg)
                        {
                            Target.Push(dst);
                            TargetImage = Target.Get();
                        }
                        else
                        {
                            Source.Push(dst);
                            SourceImage = Source.Get();
                        }
                    }
                }
            }, v => SourceImage != null);
            Detect = new DefaultCommand(v =>
            {
                if (bool.Parse(v.ToString()) is bool flg)
                {
                    if (flg)
                    {
                        TargetPoints = CvDetect(Target.Mat());
                    }
                    else
                    {
                        SourcePoints = CvDetect(Source.Mat());
                    }
                }
            });
            Search = new DefaultCommand(v =>
            {
                TemplateMatchModes matchMode =
                    (TemplateMatchModes)Enum.Parse(typeof(TemplateMatchModes), SelectedTemplateMatchMode);
                if (CvTemplateMatching(Target.Mat(), Source.Mat(), Threshold, matchMode, out System.Drawing.Rectangle match))
                {
                    SearchResult = true;
                    ResultRect = new System.Windows.Rect(match.X, match.Y, match.Width, match.Height);
                }
                else
                {
                    SearchResult = false;
                }
            });

            Enum.GetNames(typeof(TemplateMatchModes)).ToList().
                ForEach(name => TemplateMatchModeList.Add(name));
            SelectedTemplateMatchMode = TemplateMatchModes.CCoeffNormed.ToString();
        }

        public DefaultCommand OpenSource { get; }
        public DefaultCommand UndoSource { get; }
        public DefaultCommand RedoSource { get; }

        public DefaultCommand OpenTarget { get; }
        public DefaultCommand UndoTarget { get; }
        public DefaultCommand RedoTarget { get; }

        public DefaultCommand Gray { get; }
        public DefaultCommand Detect { get; }

        public DefaultCommand SourceToTarget { get; }
        public DefaultCommand TargetToSource { get; }

        public DefaultCommand Search { get; }

        public ObservableCollection<string> TemplateMatchModeList { get; } = new ObservableCollection<string>();

        public KeyPoint[] SourcePoints
        {
            get => _sourcePoints;
            set
            {
                SetProperty(ref _sourcePoints, value);
                OnPropertyChanged(nameof(SourceResult));
            }
        }
        private KeyPoint[] _sourcePoints;

        public KeyPoint[] TargetPoints
        {
            get => _targetPoints;
            set
            {
                SetProperty(ref _targetPoints, value);
                OnPropertyChanged(nameof(TargetResult));
            }
        }
        private KeyPoint[] _targetPoints;

        public string SelectedTemplateMatchMode
        {
            get => _selectedTemplateMatchMode;
            set => SetProperty(ref _selectedTemplateMatchMode, value);
        }
        private string _selectedTemplateMatchMode;

        public BitmapSource SourceImage
        {
            get => _sourceImage;
            set => SetProperty(ref _sourceImage, value);
        }
        private BitmapSource _sourceImage;

        public BitmapSource TargetImage
        {
            get => _targetImage;
            set => SetProperty(ref _targetImage, value);
        }
        private BitmapSource _targetImage;

        public System.Windows.Rect ResultRect
        {
            get => _resultRect;
            set => SetProperty(ref _resultRect, value);
        }
        private System.Windows.Rect _resultRect;

        public bool SourceResult => SourcePoints != null && SourcePoints.Length > 0;
        public bool TargetResult => TargetPoints != null && TargetPoints.Length > 0;
        public bool SearchResult
        {
            get => _searchResult;
            set => SetProperty(ref _searchResult, value);
        }
        private bool _searchResult;

        public double Threshold
        {
            get => _threshold;
            set => SetProperty(ref _threshold, value);
        }
        private double _threshold;

        /// <summary>
        /// 画像認識処理
        /// </summary>
        /// <param name="target_image"></param>
        /// <param name="rect_image"></param>
        /// <param name="threshold"></param>
        /// <param name="res_rectangle"></param>
        /// <returns></returns>
        public static bool CvTemplateMatching(Mat target_image, Mat rect_image, double threshold, TemplateMatchModes matchMode, out System.Drawing.Rectangle res_rectangle)
        {
            res_rectangle = new System.Drawing.Rectangle();

            using (Mat result = new Mat())
            {
                try
                {
                    //画像認識
                    Cv2.MatchTemplate(target_image, rect_image, result, matchMode);
                }
                catch
                {
                    return false;
                }

                // 類似度が最大/最小となる画素の位置を調べる
                Cv2.MinMaxLoc(result, out double minval, out double maxval, out OpenCvSharp.Point minloc, out OpenCvSharp.Point maxloc);

                // しきい値で判断
                if (maxval >= threshold)
                {
                    //閾値を超えたものを取得

                    OpenCvSharp.Point topLeft;
                    if (matchMode == TemplateMatchModes.SqDiff || matchMode == TemplateMatchModes.SqDiffNormed)
                    {
                        topLeft = minloc;
                    }
                    else
                    {
                        topLeft = maxloc;
                    }
                    res_rectangle.X = topLeft.X;
                    res_rectangle.Y = topLeft.Y;
                    res_rectangle.Width = rect_image.Width;
                    res_rectangle.Height = rect_image.Height;

                    return true;
                }
            }
            return false;
        }

        public Mat CvGray(Mat Src)
        {
            Mat dst = new Mat();
            try
            {
                Cv2.CvtColor(Src, dst, ColorConversionCodes.RGBA2GRAY);
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public KeyPoint[] CvDetect(Mat src)
        {
            //AKAZEのセットアップ
            AKAZE akaze = AKAZE.Create();
            //特徴量の検出と特徴量ベクトルの計算
            return akaze.Detect(src, null);
        }
    }
}
