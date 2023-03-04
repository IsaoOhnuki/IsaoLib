using Microsoft.Win32;
using MVVM;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
using static OpenCvSharp.Stitcher;

namespace OpenCV
{
    /// <summary>
    /// MatView.xaml の相互作用ロジック
    /// </summary>
    public partial class MatView : UserControl
    {
        public MatView()
        {
            InitializeComponent();

            MatViewModel model = new MatViewModel();
            DataContext = model;
        }

        public BitmapSource SourceImage
        {
            get => ((MatViewModel)DataContext).SourceImage;
            set => ((MatViewModel)DataContext).MatImage = BitmapSourceConverter.ToMat(value);
        }

        public BitmapSource MaskImage
        {
            get => ((MatViewModel)DataContext).MaskImage;
            set => ((MatViewModel)DataContext).MaskImage = value;
        }

        public double Scale
        {
            get => ((MatViewModel)DataContext).Scale;
            set => ((MatViewModel)DataContext).Scale = value;
        }

        public FeatureDetect SelectedFeatureDetect
        {
            get => ((MatViewModel)DataContext).SelectedFeatureDetect;
            set => ((MatViewModel)DataContext).SelectedFeatureDetect = value;
        }

        public void Detect()
        {
            ((MatViewModel)DataContext).Detect.Execute(null);
        }

        public bool CvDetectAndCompute(out (KeyPoint[] keyPoints, Mat mat) ret)
        {
            if (DataContext is MatViewModel model)
            {
                ret = model.CvDetectAndCompute(model.MatImage, model.MatMask);
                return ret.mat != null;
            }
            ret = (null, null);
            return false;
        }

        public void CvTemplateMatching(Mat tmpMat, Mat mask, double threshold, TemplateMatchModes matchMode)
        {
            if (DataContext is MatViewModel model)
            {
                if (model.CvTemplateMatching(model.Source.Mat(), tmpMat, threshold, matchMode, out System.Windows.Rect matches))
                {
                    model.SearchElements = new System.Windows.Point[][]
                    {
                        new System.Windows.Point[]
                        {
                            new System.Windows.Point(matches.Left, matches.Top),
                            new System.Windows.Point(matches.Right, matches.Top),
                            new System.Windows.Point(matches.Right, matches.Bottom),
                            new System.Windows.Point(matches.Left, matches.Bottom),
                        }
                    };
                }
            }
        }

        public void CvMatch(Mat tmpMat, Mat mask)
        {
            if (DataContext is MatViewModel model)
            {
                if (model.CvMatch(tmpMat, model.Source.Mat(), mask, out (int, int, float) ret))
                {
                    model.MatchElements = new KeyPoint[]
                    {
                        new KeyPoint
                        {
                            Pt = new Point2f(ret.Item1, ret.Item2),
                            Size = 10,
                            Angle = ret.Item3,
                        },
                    };
                }
            }
        }
    }

    public class NullableToBoolianConverter : ConverterBase<object, bool>
    {
        public override bool Convert(object value, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public override object ConvertBack(bool value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullableToVisibilityConverter : ConverterBase<object, Visibility>
    {
        public override Visibility Convert(object value, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(Visibility value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum MatcherType
    {
        BruteForce,
        BruteForce_L1,
        BruteForce_Hamming,
        BruteForce_Hamming2,
        FlannBased,
    }

    public enum FeatureDetect
    {
        FastFeatureDetector,
        MSER,
        BRISK,
        ORB,
        StarDetector,
        SIFT,
        SURF,
        AKAZE,
    }

    public class MatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            UndoSource.Update();
            RedoSource.Update();
            ToMask.Update();
            Gray.Update();
            BinaryReverse.Update();
            Binary.Update();
            MedianBlur.Update();
            Contour.Update();
            Dilate.Update();
            Erode.Update();
            HSV.Update();
            Detect.Update();
        }
        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (field == null && value != null ||
                field != null && !field.Equals(value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }

        public MatStack Source { get; } = new MatStack();

        public MatViewModel()
        {
            FeatureDetects = Enum.GetValues(typeof(FeatureDetect)).OfType<FeatureDetect>().Select(x => x).ToList();
            OpenSource = new DefaultCommand(v =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    Source.Push(new BitmapImage(new Uri(dialog.FileName)));
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                    DetectPoints = null;
                }
            });
            UndoSource = new DefaultCommand(v =>
            {
                Source.Undo();
                SourceImage = Source.Get();
                SourceType = Source.Mat()?.Type().ToString();
            }, v => Source.CanUndo);
            RedoSource = new DefaultCommand(v =>
            {
                Source.Redo();
                SourceImage = Source.Get();
                SourceType = Source.Mat()?.Type().ToString();
            }, v => Source.CanRedo);
            ToMask = new DefaultCommand(v =>
            {
                MaskImage = Source.Get();
                MaskType = Source.Mat()?.Type().ToString();
            }, v => SourceImage != null);
            Gray = new DefaultCommand(v =>
            {
                Mat dst = CvGray(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            BinaryReverse = new DefaultCommand(v =>
            {
                Mat dst = CvGrayReverse(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            Binary = new DefaultCommand(v =>
            {
                Mat dst = CvBinary(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            MedianBlur = new DefaultCommand(v =>
            {
                Mat dst = CvMedianBlur(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            Contour = new DefaultCommand(v =>
            {
                Mat dst = CvContour(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            Dilate = new DefaultCommand(v =>
            {
                Mat dst = CvDilate(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            Erode = new DefaultCommand(v =>
            {
                Mat dst = CvErode(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            HSV = new DefaultCommand(v =>
            {
                Mat dst = CvHSV(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                    SourceType = Source.Mat()?.Type().ToString();
                }
            }, v => SourceImage != null);
            Detect = new DefaultCommand(v =>
            {
                DetectPoints = CvDetect(MatImage, MatMask);
                if (DetectPoints != null)
                {
                    SelectedDetectOctave = null;
                    DetectOctave.Clear();
                    DetectPoints.
                        Select(x => x.Octave).Distinct().OrderBy(x => x).ToList().
                            ForEach(x => DetectOctave.Add(x));
                    DetectOctave.Insert(0, -1);
                    SelectedDetectOctave = -1;
                }
            }, v => SourceImage != null);
            DetectAndCompute = new DefaultCommand(v =>
            {
                (KeyPoint[] keyPoints, Mat descriptorImage) = CvDetectAndCompute(MatImage, MatMask);
                DetectPoints = keyPoints;
                if (DetectPoints != null)
                {
                    SelectedDetectOctave = null;
                    DetectOctave.Clear();
                    DetectPoints.
                        Select(x => x.Octave).Distinct().OrderBy(x => x).ToList().
                            ForEach(x => DetectOctave.Add(x));
                    DetectOctave.Insert(0, -1);
                    SelectedDetectOctave = -1;
                }
                descriptorImage?.Dispose();
            });

            Enumerable.Range(-1, 11).ToList().
                ForEach(x => DetectResponse.Add(x));
        }

        public DefaultCommand OpenSource { get; }
        public DefaultCommand UndoSource { get; }
        public DefaultCommand RedoSource { get; }
        public DefaultCommand ToMask { get; }

        public DefaultCommand Gray { get; }
        public DefaultCommand BinaryReverse { get; }
        public DefaultCommand Binary { get; }
        public DefaultCommand MedianBlur { get; }
        public DefaultCommand Contour { get; }
        public DefaultCommand Dilate { get; }
        public DefaultCommand Erode { get; }
        public DefaultCommand HSV { get; }
        public DefaultCommand Detect { get; }
        public DefaultCommand DetectAndCompute { get; set; }

        public List<FeatureDetect> FeatureDetects { get; }

        public FeatureDetect SelectedFeatureDetect
        {
            get => _selectedFeatureDetect;
            set => SetProperty(ref _selectedFeatureDetect, value);
        }
        private FeatureDetect _selectedFeatureDetect;

        // https://pfpfdev.hatenablog.com/entry/20200716/1594909766
        // https://campkougaku.com/2020/07/16/estimateaffine2d/
        public Feature2D GetFeature2D()
        {
            Feature2D feature = null;
            switch (SelectedFeatureDetect)
            {
                case FeatureDetect.StarDetector:
                    feature = StarDetector.Create();
                    break;
                case FeatureDetect.FastFeatureDetector:
                    feature = FastFeatureDetector.Create();
                    break;
                case FeatureDetect.ORB:
                    feature = ORB.Create();
                    break;
                case FeatureDetect.MSER:
                    feature = MSER.Create();
                    break;
                case FeatureDetect.BRISK:
                    feature = BRISK.Create();
                    break;
                case FeatureDetect.AKAZE:
                    feature = AKAZE.Create();
                    break;
                case FeatureDetect.SIFT:
                    feature = SIFT.Create();
                    break;
                case FeatureDetect.SURF:
                    //feature = SURF.Create();
                    break;
            }
            return feature;
        }

        public ObservableCollection<int> DetectOctave { get; } = new ObservableCollection<int>();

        public int? SelectedDetectOctave
        {
            get => _selectedDetectOctave;
            set => SetProperty(ref _selectedDetectOctave, value);
        }
        private int? _selectedDetectOctave;

        public ObservableCollection<int> DetectResponse { get; } = new ObservableCollection<int>();

        public int SelectedDetectResponse
        {
            get => _selectedDetectResponse;
            set => SetProperty(ref _selectedDetectResponse, value);
        }
        private int _selectedDetectResponse;

        public KeyPoint[] DetectPoints
        {
            get => _detectPoints;
            set
            {
                SetProperty(ref _detectPoints, value);
                DetectResult = DetectPoints != null && DetectPoints.Length > 0;
            }
        }
        private KeyPoint[] _detectPoints;

        public KeyPoint[] MatchElements
        {
            get => _matchElements;
            set
            {
                SetProperty(ref _matchElements, value);
                MatchResult = _matchElements != null;
            }
        }
        private KeyPoint[] _matchElements;

        public System.Windows.Point[][] SearchElements
        {
            get => _searchElements;
            set
            {
                SetProperty(ref _searchElements, value);
                SearchResult = _searchElements != null;
            }
        }
        private System.Windows.Point[][] _searchElements;

        public BitmapSource SourceImage
        {
            get => _sourceImage;
            set => SetProperty(ref _sourceImage, value);
        }
        private BitmapSource _sourceImage;

        public BitmapSource MaskImage
        {
            get => _maskImage;
            set => SetProperty(ref _maskImage, value);
        }
        private BitmapSource _maskImage;

        public string SourceType
        {
            get => _sourceType;
            set => SetProperty(ref _sourceType, value);
        }
        private string _sourceType = null;

        public string MaskType
        {
            get => _maskType;
            set => SetProperty(ref _maskType, value);
        }
        private string _maskType = null;

        public Mat MatImage
        {
            get => Source.Mat();
            set
            {
                Source.Push(value);
                SourceImage = Source.Get();
                SourceType = value?.Type().ToString();
            }
        }

        public Mat MatMask => MaskImage == null ? null : BitmapSourceConverter.ToMat(MaskImage);

        public bool DetectResult
        {
            get => _detectResult;
            set => SetProperty(ref _detectResult, value);
        }
        private bool _detectResult;

        public bool SearchResult
        {
            get => _searchResult;
            set => SetProperty(ref _searchResult, value);
        }
        private bool _searchResult;

        public bool MatchResult
        {
            get => _matchResult;
            set => SetProperty(ref _matchResult, value);
        }
        private bool _matchResult;

        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }
        private double _scale = 1;

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

        public KeyPoint[] CvDetect(Mat src, Mat mask)
        {
            Feature2D feature = GetFeature2D();
            try
            {
                return feature?.Detect(src, mask);
            }
            catch
            {
                return null;
            }
        }

        public (KeyPoint[], Mat) CvDetectAndCompute(Mat src, Mat mask)
        {
            Mat mat = new Mat();
            try
            {
                Feature2D feature = GetFeature2D();
                feature.DetectAndCompute(src, mask, out KeyPoint[] keyPoints, mat);
                return (keyPoints, mat);
            }
            catch
            {
                mat.Dispose();
                return (null, null);
            }
        }

        public Mat CvHSV(Mat src)
        {
            Mat dst = new Mat();
            try
            {
                Cv2.CvtColor(src, dst, ColorConversionCodes.RGB2HSV);
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public Mat CvBinary(Mat src)
        {
            Mat dst = new Mat();
            try
            {
                Cv2.InRange(src, new Scalar(0, 0, 0), new Scalar(255, 200, 255), dst);
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public Mat CvMedianBlur(Mat src)
        {
            Mat dst = new Mat();
            try
            {
                Cv2.MedianBlur(src, dst, 5);
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public Mat CvContour(Mat src)
        {
            Mat dst = new Mat();
            try
            {
                Cv2.FindContours(src, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] hierarchy,
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public Mat CvGrayReverse(Mat src)
        {
            Mat dst = new Mat();
            try
            {
                dst = 255 - src;
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public Mat CvErode(Mat src)
        {
            Mat dst = new Mat();
            try
            {
                using (Mat noise = new Mat(new OpenCvSharp.Size(3, 3), MatType.CV_8UC1))
                {
                    Cv2.Erode(src, dst, noise);
                }
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public Mat CvDilate(Mat src)
        {
            Mat dst = new Mat();
            try
            {
                using (Mat noise = new Mat(new OpenCvSharp.Size(3, 3), MatType.CV_8UC1))
                {
                    Cv2.Dilate(src, dst, noise);
                }
            }
            catch
            {
                dst.Dispose();
                dst = null;
            }
            return dst;
        }

        public void CvMatch(KeyPoint[] keyPoints1, Mat srcDescriptor, string matcherType)
        {
            (KeyPoint[] keyPoints2, Mat dstDescriptor) = CvDetectAndCompute(MatImage, MatMask);
            using (srcDescriptor)
            using (dstDescriptor)
            using (Mat output = new Mat())
            {
                DescriptorMatcher matcher = DescriptorMatcher.Create(matcherType);
                DMatch[][] matchess = matcher.KnnMatch(srcDescriptor, dstDescriptor, 1);
                matchess.Select((x, i) => (x, i)).ToList().ForEach(x =>
                {
                    //Cv2.DrawMatches(src, keyPoints1, dst, keyPoints2, x.x, output);
                    //Cv2.ImShow("output" + x.i.ToString(), output);
                });
            }
        }

        public bool CvTemplateMatching(Mat targetImage, Mat tempImage, double threshold,
            TemplateMatchModes matchMode, out System.Windows.Rect res_rectangle)
        {
            res_rectangle = new System.Windows.Rect();

            //Convert input images to gray
            using (Mat grayRef = targetImage.Type().Channels > 1 ?
                targetImage.CvtColor(ColorConversionCodes.BGR2GRAY) : targetImage.Clone())
            using (Mat grayTmp = tempImage.Type().Channels > 1 ?
                tempImage.CvtColor(ColorConversionCodes.BGR2GRAY) : tempImage.Clone())
            using (Mat result = new Mat())
            {
                try
                {
                    //画像認識
                    Cv2.MatchTemplate(grayRef, grayTmp, result, matchMode);
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
                    res_rectangle.Width = grayTmp.Width;
                    res_rectangle.Height = grayTmp.Height;

                    return true;
                }
            }
            return false;
        }

        // https://qiita.com/grouse324/items/74988134a9073568b32d
        public bool CvMatch(Mat tmpMat, Mat refMat, Mat mask, out (int X, int Y, float Angle) ret)
        {
            ret = (0, 0, 0);
            using (Mat srcDescriptor = new Mat())
            using (Mat dstDescriptor = new Mat())
            using (Mat output3 = new Mat())
            {
                //AKAZEのセットアップ
                AKAZE akaze = AKAZE.Create();
                //特徴量の検出と特徴量ベクトルの計算
                akaze.DetectAndCompute(tmpMat, mask, out KeyPoint[] query_kp, srcDescriptor);
                akaze.DetectAndCompute(refMat, null, out KeyPoint[] map_kp, dstDescriptor);

                DescriptorMatcher matcher = DescriptorMatcher.Create("BruteForce");
                DMatch[][] matchess = matcher.KnnMatch(srcDescriptor, dstDescriptor, 2);
                //query_img = cv2.imread('./img/query/img_camera1.png', 0);
                //query_img = cv2.LUT(query_img, gamma_cvt)  # gamma補正;
                //query_img = cv2.resize(query_img, (int(query_img.shape[1] * expand_query), int(query_img.shape[0] * expand_query)));
                //kp_query, des_query = akaze.detectAndCompute(query_img, None);

                //# マップ画像を読み込んで特徴量計算
                //map_img = cv2.imread('./img/map/field.png', 0)
                //map_img = cv2.resize(map_img, (int(map_img.shape[1] * expand_map),
                //                     int(map_img.shape[0] * expand_map)))
                //kp_map, des_map = akaze.detectAndCompute(map_img, None)

                //# 特徴量マッチング実行，k近傍法
                //bf = cv2.BFMatcher()
                //matches = bf.knnMatch(des_query, des_map, k = 2)

                //# 近傍点の第一候補と第二候補の差がある程度大きいものをgoodに追加
                //                ratio = 0.8
                float ratio = 0.8f;
                //good = []
                List<DMatch[]> good = new List<DMatch[]>();

                //for m, n in matches:
                //    if m.distance < ratio * n.distance:
                //        good.append([m])
                Array.ForEach(matchess, matches =>
                {
                    if (matches[0].Distance < ratio * matches[1].Distance)
                    {
                        good.Add(matches);
                    }
                });

                //# 対応点が１個以下なら相対関係を求められないのでNoneを返す
                //if len(good) <= 1:
                //    print("[error] can't detect matching feature point")
                //    return None, None, None
                if (good.Count <= 1)
                    return false;

                //# 精度が高かったもののうちスコアが高いものから指定個取り出す
                //good = sorted(good, key = lambda x: x[0].distance)
                //print("valid point number: ", len(good))
                //good = good.OrderBy(x => x[0].Distance).ToList();

                ////////good.Select((x, i) => (x, i)).ToList().ForEach(x =>
                ////////{
                ////////    Cv2.DrawMatches(tmpMat, query_kp, refMat, map_kp, x.x, output3);
                ////////    Cv2.ImShow("output" + x.i.ToString(), output3);
                ////////});

                //# 上位何個の点をマッチングに使うか
                //point_num = 20
                //if len(good) < point_num:
                //    point_num = len(good)  # もし20個なかったら全て使う
                int point_num = 20;
                if (good.Count < point_num)
                {
                    point_num = good.Count;
                }

                //# マッチング結果の描画
                //result_img = cv2.drawMatchesKnn(query_img, kp_query, map_img, kp_map, good[:point_num], None, flags = 0)
                //img_matching = cv2.cvtColor(result_img, cv2.COLOR_BGR2RGB)
                //plt.imshow(img_matching)
                //plt.show()


                //# 点i, jの相対角度と相対長さを格納する配列
                //deg_cand = np.zeros((point_num, point_num))
                //len_cand = np.zeros((point_num, point_num))
                List<List<double>> deg_cand = Enumerable.Range(0, point_num).
                    Select(x => Enumerable.Range(0, point_num).Select(y => 0D).ToList()).ToList();
                List<List<double>> len_cand = Enumerable.Range(0, point_num).
                    Select(x => Enumerable.Range(0, point_num).Select(y => 0D).ToList()).ToList();

                //# 全ての点のサイズ比，相対角度を求める
                //for i in range(point_num):
                for (int i = 0; i < point_num; i++)
                {
                    //    for j in range(i + 1, point_num):
                    for (int j = i; j < point_num; j++)
                    {
                        // # クエリ画像から特徴点間の角度と距離を計算
                        // q_x1, q_y1 = query_kp[i].pt
                        double q_x1 = query_kp[i].Pt.X;
                        double q_y1 = query_kp[i].Pt.Y;
                        // q_x2, q_y2 = query_kp[j].pt
                        double q_x2 = query_kp[j].Pt.X;
                        double q_y2 = query_kp[j].Pt.Y;
                        // q_deg = math.atan2(q_y2 - q_y1, q_x2 - q_x1) * 180 / math.pi
                        // q_len = math.sqrt((q_x2 - q_x1) * *2 + (q_y2 - q_y1) * *2)
                        double q_deg = Math.Atan2(q_y2 - q_y1, q_x2 - q_x1) * 180 / Math.PI;
                        double q_len = Math.Sqrt(Math.Pow((q_x2 - q_x1), 2) + Math.Pow((q_y2 - q_y1), 2));
                        // # マップ画像から特徴点間の角度と距離を計算
                        // m_x1, m_y1 = map_kp[i].pt
                        double m_x1 = map_kp[i].Pt.X;
                        double m_y1 = map_kp[i].Pt.Y;
                        // m_x2, m_y2 = map_kp[j].pt
                        double m_x2 = map_kp[j].Pt.X;
                        double m_y2 = map_kp[j].Pt.Y;
                        // m_deg = math.atan2(m_y2 - m_y1, m_x2 - m_x1) * 180 / math.pi
                        // m_len = math.sqrt((m_x2 - m_x1) * *2 + (m_y2 - m_y1) * *2)
                        double m_deg = Math.Atan2(m_y2 - m_y1, m_x2 - m_x1) * 180 / Math.PI;
                        double m_len = Math.Sqrt(Math.Pow((m_x2 - m_x1), 2) + Math.Pow((m_y2 - m_y1), 2));

                        // # 2つの画像の相対角度と距離
                        // deg_value = q_deg - m_deg
                        double deg_value = q_deg - m_deg;
                        // if deg_value < 0:
                        //    deg_value += 360
                        if (deg_value < 0)
                        {
                            deg_value += 360;
                        }
                        // if m_len <= 0:
                        //    continue
                        if (m_len <= 0)
                        {
                            continue;
                        }
                        // size_rate = q_len / m_len
                        double size_rate = q_len / m_len;

                        // deg_cand[i][j] = deg_value
                        // deg_cand[j][i] = deg_value
                        deg_cand[i][j] = deg_value;
                        deg_cand[j][i] = deg_value;

                        // len_cand[i][j] = size_rate
                        // len_cand[j][i] = size_rate
                        len_cand[i][j] = size_rate;
                        len_cand[j][i] = size_rate;
                    }
                }

                // # 多数決を取る
                // # ある点iについて，j, kとの相対関係が一致するかを各jについて調べる
                // cand_count = np.zeros((point_num, point_num))
                List<List<int>> cand_count = Enumerable.Range(0, point_num).
                    Select(x => Enumerable.Range(0, point_num).Select(y => 0).ToList()).ToList();
                // size_range_min = 0.3  # 明らかに違う比率の結果を弾く重要パラメータ
                double size_range_min = 0.3; // # 明らかに違う比率の結果を弾く重要パラメータ
                // size_range_max = 3  # 明らかに違う比率の結果を弾く重要パラメータ
                double size_range_max = 3; // # 明らかに違う比率の結果を弾く重要パラメータ
                // dif_range = 0.05  # 重要パラメータ
                double dif_range = 0.05; // # 重要パラメータ

                // for i in range(len(deg_cand)):
                for (int i = 0; i < deg_cand.Count; i++)
                {
                    // for j in range(len(deg_cand)):
                    for (int j = 0; j < deg_cand.Count; j++)
                    {
                        // # 明らかに違う比率の結果を弾く
                        // if len_cand[i][j] < size_range_min or len_cand[i][j] > size_range_max:
                        //    continue
                        if (len_cand[i][j] < size_range_min || len_cand[i][j] > size_range_max)
                        {
                            continue;
                        }
                        //                for k in range(len(deg_cand)):
                        for (int k = 0; k < deg_cand.Count; k++)
                        {
                            // # 明らかに違う比率の結果を弾く
                            // if len_cand[i][k] < size_range_min or len_cand[i][k] > size_range_max:
                            //    continue
                            if (len_cand[i][k] < size_range_min || len_cand[i][k] > size_range_max)
                            {
                                continue;
                            }
                            // # 誤差がある範囲以下の値なら同じ値とみなす
                            // deg_dif = np.abs(deg_cand[i][k] - deg_cand[i][j])
                            double deg_dif = Math.Abs(deg_cand[i][k] - deg_cand[i][j]);
                            // size_dif = np.abs(len_cand[i][k] - len_cand[i][j])
                            double size_dif = Math.Abs(len_cand[i][k] - len_cand[i][j]);
                            // if deg_dif <= deg_cand[i][j] * dif_range and size_dif <= len_cand[i][j] * dif_range:
                            //    cand_count[i][j] += 1
                            if (deg_dif <= deg_cand[i][j] * dif_range && size_dif <= len_cand[i][j] * dif_range)
                            {
                                cand_count[i][j] += 1;
                            }
                        }
                    }
                }
                // # print(cand_count)
                // # どの2点も同じ相対関係になかった場合
                // if np.max(cand_count) <= 1:
                //    print("[error] no matching point pair")
                //    return None, None, None
                if (cand_count.Count <= 1)
                {
                    return false;
                }

                // # もっとも多く相対関係が一致する2点を取ってくる
                // maxidx = np.unravel_index(np.argmax(cand_count), cand_count.shape)
                int max = cand_count.SelectMany(x => x).Max();
                var cand_max = cand_count.
                    SelectMany((z, y) => z.Select((v, x) => (x, y, v))).
                        Where(x => x.v == max);

                // deg_value = deg_cand[maxidx]
                // size_rate = len_cand[maxidx]
                var deg_value_max = cand_max.Select(z => (z.x, z.y, deg_cand[z.x][z.y])).OrderBy(z => z.Item3).First();
                var size_rate_max = cand_max.Select(z => (z.x, z.y, deg_cand[z.x][z.y])).OrderBy(z => z.Item3).First();

                int mx1 = deg_value_max.x;
                int my1 = deg_value_max.y;
                // return deg_value, size_rate, maxidx[0], maxidx[1]

                //# クエリ画像の1点目とクエリ画像の中心の相対的な関係
                //q_x1, q_y1 = query_kp[m1].pt
                double q_mx1 = query_kp[mx1].Pt.X;
                double q_my1 = query_kp[my1].Pt.Y;
                //m_x1, m_y1 = map_kp[m2].pt
                double m_mx1 = map_kp[mx1].Pt.X;
                double m_my1 = map_kp[my1].Pt.Y;
                //q_xcenter = int(width_query / 2)
                int q_xcenter = (int)tmpMat.Size().Width / 2;
                //q_ycenter = int(q_ycenter / 2)
                int q_ycenter = (int)tmpMat.Size().Height / 2;
                //q_center_deg = math.atan2(q_ycenter - q_y1, q_xcenter - q_x1) * 180 / math.pi
                double q_center_deg = Math.Atan2(q_ycenter - q_my1, q_xcenter - q_mx1) * 180 / Math.PI;
                //q_center_len = math.sqrt((q_xcenter - q_x1) * *2 + (q_ycenter - q_y1) * *2)
                double q_center_len = Math.Sqrt(Math.Pow(q_xcenter - q_mx1, 2) + Math.Pow(q_ycenter - q_my1, 2));

                //# 上の関係をマップ画像上のパラメータに変換
                //m_center_deg = q_center_deg - deg_value
                double m_center_deg = q_center_deg - deg_value_max.Item3;
                //m_center_len = q_center_len / size_rate
                double m_center_len = q_center_len / size_rate_max.Item3;

                //# 中心点のマップ画像上での位置
                //m_center_rad = m_center_deg * math.pi / 180
                double m_center_rad = m_center_deg * Math.PI / 180;
                //m_xcenter = m_x1 + m_center_len * math.cos(m_center_rad)
                double m_xcenter = m_mx1 + m_center_len * Math.Cos(m_center_rad);
                //m_ycenter = m_y1 + m_center_len * math.sin(m_center_rad)
                double m_ycenter = m_my1 + m_center_len * Math.Sin(m_center_rad);

                //# 算出された値が正しい座標範囲に入っているかどうか
                //if (m_xcenter < 0) or(m_xcenter > width_map):
                //    print("[error] invalid x value")
                //    return None, None, None
                if ((m_xcenter < 0) || (m_xcenter > refMat.Size().Width))
                {
                    return false;
                }

                //if (m_ycenter < 0) or(m_ycenter > height_map):
                //    print("[error] invalid y value")
                //    return None, None, None
                if ((m_ycenter < 0) || (m_ycenter > refMat.Size().Height))
                {
                    return false;
                }

                //if (deg_value < 0) or(deg_value > 360):
                //    print("[error] invalid deg value")
                //    return None, None, None
                if ((deg_value_max.Item3 < 0) || (deg_value_max.Item3 > 360))
                {
                    return false;
                }

                //x_current = int(m_xcenter / expand_map)
                int x_current = (int)m_xcenter;// / expand_map;
                //y_current = int(m_ycenter / expand_map)
                int y_current = (int)m_ycenter;// / expand_map)
                //drc_current = deg_value
                double drc_current = deg_value_max.Item3;

                //# 最終結果
                //print('*****detection scceeded!*****')
                //print("final output score-> x: {}, y: {}, drc: {}".format(x_current, y_current, drc_current))
                ret = (x_current, y_current, (float)drc_current);
                return true;
            }
        }
    }

    public class ResponseToBrushConverter : ConverterBase<int, Brush>
    {
        public override Brush Convert(int value, object parameter, CultureInfo culture)
        {
            return value < 0 ? null : DetectPanel.ResponseBrushes[value];
        }

        public override int ConvertBack(Brush value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
