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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
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

            Match = new DefaultCommand(v =>
            {
                //CvMatch(SourceImage, TargetImage);
                TemplateMatchModes matchMode =
                    (TemplateMatchModes)Enum.Parse(typeof(TemplateMatchModes), SelectedTemplateMatchMode);
                CvMatch(SourceImage, TargetImage, SourceMatView.Mask(), Threshold, matchMode, out OpenCvSharp.Rect[] matches);
                TargetMatView.SearchElements = matches.
                    Select(x => new System.Windows.Point[]
                    {
                        new System.Windows.Point(x.Left, x.Top),
                        new System.Windows.Point(x.Right, x.Top),
                        new System.Windows.Point(x.Right, x.Bottom),
                        new System.Windows.Point(x.Left, x.Bottom),
                    }).ToArray();
            });
            StepMatch = new DefaultCommand(v =>
            {
                TemplateMatchModes matchMode =
                    (TemplateMatchModes)Enum.Parse(typeof(TemplateMatchModes), SelectedTemplateMatchMode);
                Mat result = CvStepMatch(SourceImage, TargetImage, SourceMatView.Mask(), Threshold, matchMode, out OpenCvSharp.Rect[] matches);
                TargetMatView.Set(result);
                TargetMatView.SearchElements = matches.
                    Select(x => new System.Windows.Point[]
                    {
                        new System.Windows.Point(x.Left, x.Top),
                        new System.Windows.Point(x.Right, x.Top),
                        new System.Windows.Point(x.Right, x.Bottom),
                        new System.Windows.Point(x.Left, x.Bottom),
                    }).ToArray();
            });

            Enum.GetNames(typeof(TemplateMatchModes)).ToList().
                ForEach(name => TemplateMatchModeList.Add(name));
            SelectedTemplateMatchMode = TemplateMatchModes.CCoeffNormed.ToString();
        }

        public DefaultCommand SourceToTarget { get; }
        public DefaultCommand TargetToSource { get; }

        public DefaultCommand Search { get; }
        public DefaultCommand SearchClear { get; }

        public DefaultCommand Match { get; }
        public DefaultCommand StepMatch { get; }

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
        private double _threshold = 0.1;

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

        public void CvMatch(Mat src, Mat dst)
        {
            using (Mat srcDescriptor = new Mat())
            using (Mat dstDescriptor = new Mat())
            using (Mat output3 = new Mat())
            {
                //AKAZEのセットアップ
                AKAZE akaze = AKAZE.Create();
                //特徴量の検出と特徴量ベクトルの計算
                akaze.DetectAndCompute(src, null, out KeyPoint[] keyPoints1, srcDescriptor);
                akaze.DetectAndCompute(dst, null, out KeyPoint[] keyPoints2, dstDescriptor);

                DescriptorMatcher matcher = DescriptorMatcher.Create("BruteForce");
                //DMatch[] matches = matcher.Match(srcDescriptor, dstDescriptor);
                //Cv2.DrawMatches(src, keyPoints1, dst, keyPoints2, matches, output3);
                //Cv2.ImShow("output3", output3);

                DMatch[][] matchess = matcher.KnnMatch(srcDescriptor, dstDescriptor, 3);
                matchess.Select((x, i) => (x, i)).ToList().ForEach(x =>
                {
                    Cv2.DrawMatches(src, keyPoints1, dst, keyPoints2, x.x, output3);
                    Cv2.ImShow("output" + x.i.ToString(), output3);
                });
            }
        }

        //https://surigoma.hateblo.jp/entry/2017/01/05/005119
        //https://moitkfm.blogspot.com/2014/06/blog-post_10.html
        //https://pfpfdev.hatenablog.com/entry/20200716/1594909766
        //https://qiita.com/sitar-harmonics/items/41d54dbfc6c81b87b905
        public void CvMatch(Mat tmpMat, Mat refMat, Mat mask, double threshold, TemplateMatchModes matchMode, out OpenCvSharp.Rect[] matches)
        {
            using (Mat res = new Mat(refMat.Rows - tmpMat.Rows + 1, refMat.Cols - tmpMat.Cols + 1, MatType.CV_32FC1))
            //Convert input images to gray
            using (Mat grayRef = refMat.Type().Channels > 1
                ? refMat.CvtColor(ColorConversionCodes.BGR2GRAY) : refMat.Clone())
            using (Mat grayTmp = tmpMat.Type().Channels > 1 ? tmpMat.CvtColor(ColorConversionCodes.BGR2GRAY) : tmpMat.Clone())
            {
                Cv2.MatchTemplate(grayRef, grayTmp, res, matchMode, mask);
                Cv2.Threshold(res, res, threshold, 1.0, ThresholdTypes.Tozero);

                List<OpenCvSharp.Rect> rects = new List<OpenCvSharp.Rect>();

                while (true)
                {
                    double minval, maxval;
                    OpenCvSharp.Point minloc, maxloc;
                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                    if (maxval >= threshold)
                    {
                        //Setup the rectangle to draw
                        OpenCvSharp.Rect rect = new OpenCvSharp.Rect(
                            new OpenCvSharp.Point(maxloc.X, maxloc.Y),
                            new OpenCvSharp.Size(tmpMat.Width, tmpMat.Height));
                        rects.Add(rect);

                        //Fill in the res Mat so you don't find the same area again in the MinMaxLoc
                        Cv2.FloodFill(res, maxloc, new Scalar(0), out OpenCvSharp.Rect _, new Scalar(0.1), new Scalar(1.0));
                    }
                    else
                        break;
                }

                matches = rects.ToArray();
            }
        }

        public Mat CvStepMatch(Mat tmpMat, Mat refMat, Mat mask, double threshold, TemplateMatchModes matchMode, out OpenCvSharp.Rect[] matches)
        {
            using (Mat res = new Mat(refMat.Rows - tmpMat.Rows + 1, refMat.Cols - tmpMat.Cols + 1, MatType.CV_32FC1))
            //Convert input images to gray
            using (Mat grayRef = refMat.Type().Channels > 1
                ? refMat.CvtColor(ColorConversionCodes.BGR2GRAY) : refMat.Clone())
            using (Mat grayTmp = tmpMat.Type().Channels > 1 ? tmpMat.CvtColor(ColorConversionCodes.BGR2GRAY) : tmpMat.Clone())
            {
                Mat result = grayRef.Clone();
                Cv2.MatchTemplate(grayRef, grayTmp, res, matchMode, mask);
                Cv2.Threshold(res, res, threshold, 1.0, ThresholdTypes.Tozero);

                List<OpenCvSharp.Rect> rects = new List<OpenCvSharp.Rect>();

                double minval, maxval;
                OpenCvSharp.Point minloc, maxloc;
                Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                if (maxval >= threshold)
                {
                    //Setup the rectangle to draw
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(
                        new OpenCvSharp.Point(maxloc.X, maxloc.Y),
                        new OpenCvSharp.Size(tmpMat.Width, tmpMat.Height));
                    rects.Add(rect);

                    //Fill in the res Mat so you don't find the same area again in the MinMaxLoc
                    Cv2.FloodFill(result, maxloc, new Scalar(0), out OpenCvSharp.Rect _, new Scalar(0.1), new Scalar(1.0), FloodFillFlags.Link4);
                }

                matches = rects.ToArray();
                return result;
            }
        }
    }
}
