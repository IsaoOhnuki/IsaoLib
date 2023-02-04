using Microsoft.Win32;
using MVVM;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private ImageSearchViewModel imageSearchViewModel { get; } = new ImageSearchViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = imageSearchViewModel;
        }
    }

    public class ImageSearchViewModel : INotifyPropertyChanged
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

        public ImageSearchViewModel()
        {
            OpenSource = new DefaultCommand(v =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    SourceImage = new BitmapImage(new Uri(dialog.FileName));
                }
            });
            OpenTarget = new DefaultCommand(v =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    TargetImage = new BitmapImage(new Uri(dialog.FileName));
                    Result = false;
                }
            });
            Search = new DefaultCommand(v =>
            {
                if (CvTemplateMatching(TargetImage, SourceImage, Threshold, out System.Drawing.Rectangle match))
                {
                    Result = true;
                    ResultRect = new System.Windows.Rect(match.X, match.Y, match.Width, match.Height);
                }
                else
                {
                    Result = false;
                }
            });
        }

        public DefaultCommand OpenSource { get; }

        public DefaultCommand OpenTarget { get; }

        public DefaultCommand Search { get; }

        public BitmapImage SourceImage
        {
            get => _sourceImage;
            set => SetProperty(ref _sourceImage, value);
        }
        private BitmapImage _sourceImage;

        public BitmapImage TargetImage
        {
            get => _targetImage;
            set => SetProperty(ref _targetImage, value);
        }
        private BitmapImage _targetImage;

        public System.Windows.Rect ResultRect
        {
            get => _resultRect;
            set => SetProperty(ref _resultRect, value);
        }
        private System.Windows.Rect _resultRect;

        public bool Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }
        private bool _result;

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
        public static bool CvTemplateMatching(Mat target_image, Mat rect_image, double threshold, out System.Drawing.Rectangle res_rectangle)
        {
            res_rectangle = new System.Drawing.Rectangle();

            using (Mat result = new Mat())
            {
                //画像認識
                Cv2.MatchTemplate(target_image, rect_image, result, TemplateMatchModes.CCoeffNormed);

                // 類似度が最大/最小となる画素の位置を調べる
                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(result, out minval, out maxval, out minloc, out maxloc);

                // しきい値で判断
                if (maxval >= threshold)
                {
                    //閾値を超えたものを取得
                    res_rectangle.X = maxloc.X;
                    res_rectangle.Y = maxloc.Y;
                    res_rectangle.Width = rect_image.Width;
                    res_rectangle.Height = rect_image.Height;

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ImageをMatに変換して画像認識処理に渡す
        /// </summary>
        /// <param name="target_image"></param>
        /// <param name="rect_image"></param>
        /// <param name="threshold"></param>
        /// <param name="res_rectangle"></param>
        /// <returns></returns>
        public static bool CvTemplateMatching(BitmapSource target_image, BitmapSource rect_image, double threshold, out System.Drawing.Rectangle res_rectangle)
        {
            using (Mat target_mat = BitmapSourceConverter.ToMat(target_image))
            using (Mat rect_mat = BitmapSourceConverter.ToMat(rect_image))
            {
                return CvTemplateMatching(target_mat, rect_mat, threshold, out res_rectangle);
            }
        }

        /// <summary>
        /// ImageとFileをMatに変換して画像認識処理に渡す
        /// </summary>
        /// <param name="target_image_file"></param>
        /// <param name="rect_image"></param>
        /// <param name="threshold"></param>
        /// <param name="res_rectangle"></param>
        /// <returns></returns>
        public static bool CvTemplateMatching(string target_image_file, BitmapSource rect_image, double threshold, out System.Drawing.Rectangle res_rectangle)
        {
            using (Mat target_mat = new Mat(target_image_file, ImreadModes.Unchanged))
            using (Mat rect_mat = BitmapSourceConverter.ToMat(rect_image))
            {
                return CvTemplateMatching(target_mat, rect_mat, threshold, out res_rectangle);
            }
        }

        /// <summary>
        /// ImageとFileをMatに変換して画像認識処理に渡す
        /// </summary>
        /// <param name="target_image"></param>
        /// <param name="rect_image_file"></param>
        /// <param name="threshold"></param>
        /// <param name="res_rectangle"></param>
        /// <returns></returns>
        public static bool CvTemplateMatching(BitmapSource target_image, string rect_image_file, double threshold, out System.Drawing.Rectangle res_rectangle)
        {
            using (Mat target_mat = BitmapSourceConverter.ToMat(target_image))
            using (Mat rect_mat = new Mat(rect_image_file, ImreadModes.Unchanged))
            {
                return CvTemplateMatching(target_mat, rect_mat, threshold, out res_rectangle);
            }
        }

        /// <summary>
        /// FileをMatに変換して画像認識処理に渡す
        /// </summary>
        /// <param name="target_image_file"></param>
        /// <param name="rect_image_file"></param>
        /// <param name="threshold"></param>
        /// <param name="res_rectangle"></param>
        /// <returns></returns>
        public static bool CvTemplateMatching(string target_image_file, string rect_image_file, double threshold, out System.Drawing.Rectangle res_rectangle)
        {
            using (Mat target_mat = new Mat(target_image_file, ImreadModes.Unchanged))
            using (Mat rect_mat = new Mat(rect_image_file, ImreadModes.Unchanged))
            {
                return CvTemplateMatching(target_mat, rect_mat, threshold, out res_rectangle);
            }
        }
    }
}
