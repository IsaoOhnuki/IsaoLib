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

            ImageSearchViewModel.SourceMatView = sourceImage;
            ImageSearchViewModel.TargetMatView = targetImage;
            DataContext = ImageSearchViewModel;
        }
    }

    public class ImageSearchViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            SourceToTarget.Update();
            TargetToSource.Update();
            Search.Update();
            SearchClear.Update();
        }
        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            field = value;
            OnPropertyChanged(name);
        }

        public ImageSearchViewModel()
        {
            SourceToTarget = new DefaultCommand(v =>
            {
                TargetMatView.Set(SourceImage);
            }, v => SourceImage != null);
            TargetToSource = new DefaultCommand(v =>
            {
                SourceMatView.Set(TargetImage);
            }, v => TargetImage != null);
            SearchClear = new DefaultCommand(v =>
            {
                TargetMatView.SearchElements = null;
            }, v => TargetMatView.SearchElements != null);
            Search = new DefaultCommand(v =>
            {
                TemplateMatchModes matchMode =
                    (TemplateMatchModes)Enum.Parse(typeof(TemplateMatchModes), SelectedTemplateMatchMode);
                if (CvTemplateMatching(TargetImage, SourceImage, Threshold, matchMode, out System.Windows.Rect match))
                {
                    TargetMatView.SearchElements = new System.Windows.Point[][]
                    {
                        new System.Windows.Point[]
                        {
                            new System.Windows.Point(match.Left, match.Top),
                            new System.Windows.Point(match.Right, match.Top),
                            new System.Windows.Point(match.Right, match.Bottom),
                            new System.Windows.Point(match.Left, match.Bottom),
                        },
                    };
                }
                else
                {
                    TargetMatView.SearchElements = null;
                }
                SearchClear.Update();
            }, v => SourceImage != null && TargetImage != null);

            Enum.GetNames(typeof(TemplateMatchModes)).ToList().
                ForEach(name => TemplateMatchModeList.Add(name));
            SelectedTemplateMatchMode = TemplateMatchModes.CCoeffNormed.ToString();
        }

        public DefaultCommand SourceToTarget { get; }
        public DefaultCommand TargetToSource { get; }

        public DefaultCommand Search { get; }
        public DefaultCommand SearchClear { get; }

        public ObservableCollection<string> TemplateMatchModeList { get; } = new ObservableCollection<string>();

        public MatView SourceMatView { get; set; }
        public MatView TargetMatView { get; set; }

        public Mat SourceImage
        {
            get => _sourceImage;
            set => SetProperty(ref _sourceImage, value);
        }
        private Mat _sourceImage;

        public Mat TargetImage
        {
            get => _targetImage;
            set => SetProperty(ref _targetImage, value);
        }
        private Mat _targetImage;

        public string SelectedTemplateMatchMode
        {
            get => _selectedTemplateMatchMode;
            set => SetProperty(ref _selectedTemplateMatchMode, value);
        }
        private string _selectedTemplateMatchMode;

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
        public static bool CvTemplateMatching(Mat target_image, Mat rect_image, double threshold,
            TemplateMatchModes matchMode, out System.Windows.Rect res_rectangle)
        {
            res_rectangle = new System.Windows.Rect();

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
    }
}
