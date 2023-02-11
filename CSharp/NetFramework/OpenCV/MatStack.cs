using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenCV
{
    public class MatStack : IDisposable
    {
        private List<Mat> Stack { get; } = new List<Mat>();

        public int Position { get; private set; }

        public bool CanUndo => Position > 0;

        public bool CanRedo => Position < Stack.Count;

        public void Undo()
        {
            if (CanUndo)
            {
                --Position;
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                ++Position;
            }
        }

        public Mat Mat()
        {
            if (Position > 0)
            {
                return Stack[Position - 1];
            }
            return null;
        }

        public BitmapSource Get()
        {
            if (Position > 0)
            {
                return BitmapSourceConverter.ToBitmapSource(Stack[Position - 1]);
            }
            return null;
        }

        public void Push(BitmapSource image)
        {
            Push(BitmapSourceConverter.ToMat(image));
        }

        public void Push(Mat image)
        {
            while (Position < Stack.Count)
            {
                Mat mat = Stack.Last();
                Stack.Remove(mat);
                mat.Dispose();
            }
            Stack.Add(image);
            ++Position;
        }

        public BitmapSource Pop()
        {
            if (Position > 0)
            {
                --Position;
                return BitmapSourceConverter.ToBitmapSource(Stack[Position]);
            }
            return null;
        }

        public void Dispose()
        {
            Stack.ForEach(x => x.Dispose());
        }
    }
}
