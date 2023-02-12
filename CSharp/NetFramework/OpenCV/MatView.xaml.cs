using Microsoft.Win32;
using MVVM;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// MatView.xaml の相互作用ロジック
    /// </summary>
    public partial class MatView : UserControl
    {
        public MatView()
        {
            InitializeComponent();

            MatViewModel model = new MatViewModel();
            model.PropertyChanged += ModelChanged;
            DataContext = model;
        }

        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is MatViewModel model &&
                e.PropertyName == nameof(MatViewModel.SourceImage))
            {
                Image = model.Source.Mat();
            }
        }

        public Mat Image
        {
            get { return (Mat)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                nameof(Image),
                typeof(Mat),
                typeof(MatView),
                new FrameworkPropertyMetadata(
                    default,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public void Set(Mat mat)
        {
            if (DataContext is MatViewModel model)
            {
                model.Source.Push(mat);
                model.SourceImage = model.Source.Get();
            }
        }

        public System.Windows.Point[][] SearchElements
        {
            get => searchPanel.ItemsSource;
            set
            {
                searchPanel.ItemsSource = value;
                SearchResult = searchPanel.ItemsSource != null;
            }
        }

        public bool SearchResult
        {
            get { return (bool)GetValue(SearchResultProperty); }
            set { SetValue(SearchResultProperty, value); }
        }

        public static readonly DependencyProperty SearchResultProperty =
            DependencyProperty.Register(
                nameof(SearchResult),
                typeof(bool),
                typeof(MatView),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }

    public class MatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            UndoSource.Update();
            RedoSource.Update();
            Gray.Update();
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
                    SourcePoints = null;
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
            Gray = new DefaultCommand(v =>
            {
                Mat dst = CvGray(Source.Mat());
                if (dst != null)
                {
                    Source.Push(dst);
                    SourceImage = Source.Get();
                }
            }, v => SourceImage != null);
            Detect = new DefaultCommand(v =>
            {
                SourcePoints = CvDetect(Source.Mat());
                SelectedSourceOctave = null;
                SourceOctave.Clear();
                SourcePoints.
                    Select(x => x.Octave).Distinct().OrderBy(x => x).ToList().
                        ForEach(x => SourceOctave.Add(x));
                SourceOctave.Insert(0, -1);
                SelectedSourceOctave = -1;
            }, v => SourceImage != null);
        }

        public DefaultCommand OpenSource { get; }
        public DefaultCommand UndoSource { get; }
        public DefaultCommand RedoSource { get; }

        public DefaultCommand Gray { get; }
        public DefaultCommand Detect { get; }

        public ObservableCollection<int> SourceOctave { get; } = new ObservableCollection<int>();

        public int? SelectedSourceOctave
        {
            get => _selectedSourceOctave;
            set => SetProperty(ref _selectedSourceOctave, value);
        }
        private int? _selectedSourceOctave;

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

        public BitmapSource SourceImage
        {
            get => _sourceImage;
            set => SetProperty(ref _sourceImage, value);
        }
        private BitmapSource _sourceImage;

        public System.Windows.Rect? SearchRect
        {
            get => _searchRect;
            set
            {
                SetProperty(ref _searchRect, value);
                OnPropertyChanged(nameof(SearchResult));
            }
        }
        private System.Windows.Rect? _searchRect;

        public bool SourceResult => SourcePoints != null && SourcePoints.Length > 0;
        
        public bool SearchResult => SearchRect.HasValue;

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
