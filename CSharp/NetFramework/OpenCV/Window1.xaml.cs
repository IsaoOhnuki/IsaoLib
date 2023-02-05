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
    public partial class Window1 : System.Windows.Window
    {
        public Window1()
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
                }
            });
            Search = new DefaultCommand(v =>
            {
                ImageSearch();
            });
            Search2 = new DefaultCommand(v =>
            {
                ImageSearch2();
            });
        }

        public DefaultCommand OpenSource { get; }

        public DefaultCommand OpenTarget { get; }

        public DefaultCommand Search { get; }
        public DefaultCommand Search2 { get; }

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

        public void ImageSearch()
        {
            Mat output1 = new Mat(); //比較元画像の特徴点出力先
            Mat output2 = new Mat(); //比較先画像の特徴点出力先
            Mat output3 = new Mat(); //DrawMatchesの出力先

            AKAZE akaze = AKAZE.Create(); //AKAZEのセットアップ
            KeyPoint[] key_point1;        //比較元画像の特徴点
            KeyPoint[] key_point2;        //比較先画像の特徴点
            Mat descriptor1 = new Mat();  //比較元画像の特徴量
            Mat descriptor2 = new Mat();  //比較先画像の特徴量

            using (Mat mat = BitmapSourceConverter.ToMat(SourceImage))
            using (Mat temp = BitmapSourceConverter.ToMat(TargetImage))
            {
                //特徴量の検出と特徴量ベクトルの計算
                akaze.DetectAndCompute(mat, null, out key_point1, descriptor1);
                akaze.DetectAndCompute(temp, null, out key_point2, descriptor2);

                //画像１の特徴点をoutput1に出力
                Cv2.DrawKeypoints(mat, key_point1, output1);
                Cv2.ImShow("output1", output1);

                //画像２の特徴点をoutput2に出力
                Cv2.DrawKeypoints(temp, key_point2, output2);
                Cv2.ImShow("output2", output2);
            }
        }

        public void ImageSearch2()
        {
            Mat output1 = new Mat(); //比較元画像の特徴点出力先
            Mat output2 = new Mat(); //比較先画像の特徴点出力先
            Mat output3 = new Mat(); //DrawMatchesの出力先

            AKAZE akaze = AKAZE.Create(); //AKAZEのセットアップ
            KeyPoint[] key_point1;        //比較元画像の特徴点
            KeyPoint[] key_point2;        //比較先画像の特徴点
            Mat descriptor1 = new Mat();  //比較元画像の特徴量
            Mat descriptor2 = new Mat();  //比較先画像の特徴量

            DMatch[] matches; //特徴量ベクトル同士のマッチング結果を格納する配列
            DescriptorMatcher matcher; //マッチング方法

            using (Mat mat = BitmapSourceConverter.ToMat(SourceImage))
            using (Mat temp = BitmapSourceConverter.ToMat(TargetImage))
            {
                //特徴量の検出と特徴量ベクトルの計算
                akaze.DetectAndCompute(mat, null, out key_point1, descriptor1);
                akaze.DetectAndCompute(temp, null, out key_point2, descriptor2);

                matcher = DescriptorMatcher.Create("BruteForce");
                matches = matcher.Match(descriptor1, descriptor2);

                Cv2.DrawMatches(mat, key_point1, temp, key_point2, matches, output3);
                Cv2.ImShow("output3", output3);
            }
        }
    }
}
