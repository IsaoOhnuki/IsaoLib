using Microsoft.Win32;
using MVVM;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
        public MainWindow()
        {
            InitializeComponent();

            ImageSearchViewModel viewModel = new ImageSearchViewModel()
            {
                SourceMatView = sourceImage,
                TargetMatView = targetImage
            };
            DataContext = viewModel;
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

    public class ImageSearchViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            SourceToTarget.Update();
            TargetToSource.Update();
            Search.Update();
            //SearchClear.Update();
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
                if (SourceImage != null)
                {
                    TargetMatView.SourceImage = SourceImage;
                }
            });
            TargetToSource = new DefaultCommand(v =>
            {
                if (TargetImage != null)
                {
                    SourceMatView.SourceImage = TargetImage;
                }
            });
            Search = new DefaultCommand(v =>
            {
                if (SourceImage != null && TargetImage != null)
                {
                    using (Mat temp = BitmapSourceConverter.ToMat(SourceImage))
                    using (Mat mask = SourceMask != null ? BitmapSourceConverter.ToMat(SourceMask) : null)
                    {
                        TemplateMatchModes matchMode =
                            (TemplateMatchModes)Enum.Parse(typeof(TemplateMatchModes), SelectedTemplateMatchMode);
                        TargetMatView.CvTemplateMatching(temp, mask, Threshold, matchMode);
                    }
                }
            });
            Match = new DefaultCommand(v =>
            {
                if (SourceImage != null && TargetImage != null)
                {
                    using (Mat temp = BitmapSourceConverter.ToMat(SourceImage))
                    using (Mat mask = SourceMask != null ? BitmapSourceConverter.ToMat(SourceMask) : null)
                    {
                        TargetMatView.CvMatch(temp, mask);
                    }
                }
            });
            StepMatch = new DefaultCommand(v =>
            {
                CvMatch();
            });

            Enum.GetNames(typeof(TemplateMatchModes)).ToList().
                ForEach(name => TemplateMatchModeList.Add(name));
            SelectedTemplateMatchMode = TemplateMatchModes.CCoeffNormed.ToString();

            Matchers = Enum.GetValues(typeof(MatcherType)).OfType<MatcherType>().Select(x => x).ToList();
        }

        public DefaultCommand SourceToTarget { get; }
        public DefaultCommand TargetToSource { get; }

        public DefaultCommand Search { get; }

        public DefaultCommand Match { get; }
        public DefaultCommand StepMatch { get; }

        public List<MatcherType> Matchers { get; }

        public MatcherType SelectedMatcher
        {
            get => _selectedMatcher;
            set => SetProperty(ref _selectedMatcher, value);
        }
        private MatcherType _selectedMatcher;

        public string GetMatcher()
        {
            switch (SelectedMatcher)
            {
                case MatcherType.BruteForce:
                    return "BruteForce";
                case MatcherType.BruteForce_L1:
                    return "BruteForce-L1";
                case MatcherType.BruteForce_Hamming:
                    return "BruteForce-Hamming";
                case MatcherType.BruteForce_Hamming2:
                    return "BruteForce-Hamming(2)";
                case MatcherType.FlannBased:
                    return "FlannBased";
            }
            return string.Empty;
        }

        public ObservableCollection<string> TemplateMatchModeList { get; } = new ObservableCollection<string>();

        public MatView SourceMatView { get; set; }
        public MatView TargetMatView { get; set; }

        public BitmapSource SourceImage => SourceMatView?.SourceImage;

        public BitmapSource SourceMask => SourceMatView?.MaskImage;

        public BitmapSource TargetImage => TargetMatView?.SourceImage;

        public string SelectedTemplateMatchMode
        {
            get => _selectedTemplateMatchMode;
            set => SetProperty(ref _selectedTemplateMatchMode, value);
        }
        private string _selectedTemplateMatchMode;

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

        public void CvMatch()
        {
            if (SourceMatView.CvDetectAndCompute(out (KeyPoint[] keyPoints1, Mat srcDescriptor) ret1) &&
                TargetMatView.CvDetectAndCompute(out (KeyPoint[] keyPoints2, Mat dstDescriptor) ret2))
            {
                using (ret1.srcDescriptor)
                using (ret2.dstDescriptor)
                using (Mat src = BitmapSourceConverter.ToMat(SourceMatView.SourceImage))
                using (Mat dst = BitmapSourceConverter.ToMat(TargetMatView.SourceImage))
                using (Mat output = new Mat())
                {
                    DescriptorMatcher matcher = DescriptorMatcher.Create(GetMatcher());
                    DMatch[][] matchess = matcher.KnnMatch(ret1.srcDescriptor, ret2.dstDescriptor, 3);
                    matchess.SelectMany((x, i) => x.Select((y, ii) => (y, i, ii))).
                        GroupBy(x => x.ii).ToList().
                            ForEach(x =>
                            {
                                DMatch[] ds = x.OrderBy(y => y.i).Select(y => y.y).ToArray();
                                Cv2.DrawMatches(src, ret1.keyPoints1, dst, ret2.keyPoints2, ds, output);
                                Cv2.ImShow("output" + x.Key.ToString(), output);
                            });
                }
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
