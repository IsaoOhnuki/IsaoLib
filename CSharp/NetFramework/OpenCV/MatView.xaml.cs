using Microsoft.Win32;
using MVVM;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public Mat MatImage
        {
            get => ((MatViewModel)DataContext).MatImage;
            set => ((MatViewModel)DataContext).MatImage = value;
        }

        public Mat MatMask => ((MatViewModel)DataContext).MatMask;

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
                //CvMatch(SourceImage, TargetImage, SourceMatView.Mask(), Threshold, matchMode, out OpenCvSharp.Rect[] matches);
                //TargetMatView.SearchElements = matches.
                //    Select(x => new System.Windows.Point[]
                //    {
                //        new System.Windows.Point(x.Left, x.Top),
                //        new System.Windows.Point(x.Right, x.Top),
                //        new System.Windows.Point(x.Right, x.Bottom),
                //        new System.Windows.Point(x.Left, x.Bottom),
                //    }).ToArray();
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
            GrayReverse.Update();
            Binary.Update();
            Contour.Update();
            Dilate.Update();
            Erode.Update();
            MedianBlur.Update();
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
            OpenSource = new DefaultCommand(v =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    Source.Push(new BitmapImage(new Uri(dialog.FileName)));
                    SourceImage = Source.Get();
                    DetectPoints = null;
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
            ToMask = new DefaultCommand(v =>
            {
                MaskImage = Source.Get();
            }, v => Source != null);
            Gray = new DefaultCommand(v =>
            {
                Mat dst = CvGray(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            GrayReverse = new DefaultCommand(v =>
            {
                Mat dst = CvGrayReverse(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            Binary = new DefaultCommand(v =>
            {
                Mat dst = CvBinary(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            MedianBlur = new DefaultCommand(v =>
            {
                Mat dst = CvMedianBlur(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            Contour = new DefaultCommand(v =>
            {
                Mat dst = CvContour(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            Dilate = new DefaultCommand(v =>
            {
                Mat dst = CvDilate(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            Erode = new DefaultCommand(v =>
            {
                Mat dst = CvErode(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            HSV = new DefaultCommand(v =>
            {
                Mat dst = CvHSV(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            Detect = new DefaultCommand(v =>
            {
                DetectPoints = CvDetect(Source.Mat());
                SelectedDetectOctave = null;
                DetectOctave.Clear();
                DetectPoints.
                    Select(x => x.Octave).Distinct().OrderBy(x => x).ToList().
                        ForEach(x => DetectOctave.Add(x));
                DetectOctave.Insert(0, -1);
                SelectedDetectOctave = -1;
            }, v => SourceImage != null);
        }

        public DefaultCommand OpenSource { get; }
        public DefaultCommand UndoSource { get; }
        public DefaultCommand RedoSource { get; }
        public DefaultCommand ToMask { get; }

        public DefaultCommand Gray { get; }
        public DefaultCommand GrayReverse { get; }
        public DefaultCommand Binary { get; }
        public DefaultCommand MedianBlur { get; }
        public DefaultCommand Contour { get; }
        public DefaultCommand Dilate { get; }
        public DefaultCommand Erode { get; }
        public DefaultCommand HSV { get; }
        public DefaultCommand Detect { get; }

        public ObservableCollection<int> DetectOctave { get; } = new ObservableCollection<int>();

        public int? SelectedDetectOctave
        {
            get => _selectedDetectOctave;
            set => SetProperty(ref _selectedDetectOctave, value);
        }
        private int? _selectedDetectOctave;

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

        public Mat MatImage
        {
            get => Source.Mat();
            set
            {
                Source.Push(value);
                SourceImage = Source.Get();
            }
        }

        public Mat MatMask => BitmapSourceConverter.ToMat(MaskImage);

        public System.Windows.Rect? SearchRect
        {
            get => _searchRect;
            set
            {
                SetProperty(ref _searchRect, value);
                SearchResult = _searchRect.HasValue;
            }
        }
        private System.Windows.Rect? _searchRect;

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
            //特徴量の検出
            return akaze.Detect(src, null);
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
}
