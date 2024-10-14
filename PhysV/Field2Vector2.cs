using MathV;
using System.Drawing;
using System.Net.NetworkInformation;

namespace PhysV
{
    public class Field2Vector2
    {
        private int _width;
        private int _height;
        public int Width => _width;
        public int Height => _height;
        private Vector2[,] _values;
        public Vector2[,] Values => _values;
        public double[,] X
        {
            get
            {
                double[,] output = new double[_width,_height];
                for (int i = 0; i < _height; i++)
                {
                    for (int j = 0; j < _width; j++)
                    {
                        output[i, j] = _values[i,j].X;
                    }
                }
                return output;
            }
        }
        public double[,] Y
        {
            get
            {
                double[,] output = new double[_width, _height];
                for (int i = 0; i < _height; i++)
                {
                    for (int j = 0; j < _width; j++)
                    {
                        output[i, j] = _values[i, j].Y;
                    }
                }
                return output;
            }
        }
        public Field2Vector2(int width, int height)
        {
            _width = width;
            _height = height;
            _values = new Vector2[_width, _height];
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    _values[j, i] = new Vector2();
                }
            }
        }
        public Field2Vector2(int width, int height, Vector2[,] values)
        {
            _width = width;
            _height = height;
            _values = values;
        }
        public Field2Vector2(int width, int height, double[,] x, double[,] y)
        {
            _width = width;
            _height = height;
            _values = new Vector2[_width, _height];
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    _values[i, j] = new Vector2(x[i,j], y[i,j]);
                }
            }
        }


        public Bitmap ToBitmap()
        {
            Bitmap output = new Bitmap(_width, _height);
            double minXValue = _values.Cast<Vector2>().Select(v => v.X).Min();
            double maxXValue = _values.Cast<Vector2>().Select(v => v.X).Max();
            double minYValue = _values.Cast<Vector2>().Select(v => v.Y).Min();
            double maxYValue = _values.Cast<Vector2>().Select(v => v.Y).Max();
            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    int r = 0;
                    if(minXValue != maxXValue)
                    {
                        r = (int)((_values[i, j].X - minXValue) / (maxXValue - minXValue) * byte.MaxValue);
                    }
                    int g = 0;
                    if (minYValue != maxYValue)
                    {
                        g = (int)((_values[i, j].Y - minYValue) / (maxYValue - minYValue) * byte.MaxValue);
                    }
                    output.SetPixel(i, j, Color.FromArgb(r, g,0));
                }
            }
            return output;
        }

        public static Field2 Divergence(Field2Vector2 input)
        {
            double[,] output = new double[input.Width, input.Height];

            for (int j = 1; j < input.Height - 2; j++)
            {
                for (int i = 1; i < input.Width - 2; i++)
                {
                    double value = (input.Values[i - 1, j].X - input.Values[i + 1, j].X)/2 + (input.Values[i - 1, j].Y - input.Values[i + 1, j].Y) / 2;
                    output[i,j] = value;
                }
            }
            return new Field2(input.Width, input.Height, output);
        }
        public static Field2 Rotor(Field2Vector2 input)
        {
            double[,] output = new double[input.Width, input.Height];

            for (int j = 1; j < input.Height - 2; j++)
            {
                for (int i = 1; i < input.Width - 2; i++)
                {
                    double value = (input.Values[i - 1, j].Y - input.Values[i + 1, j].Y) / 2 - (input.Values[i - 1, j].X - input.Values[i + 1, j].X) / 2;
                    output[i, j] = value;
                }
            }
            return new Field2(input.Width, input.Height, output);
        }

        public void CloneTo(ref Field2Vector2 cloneTarget)
        {
            cloneTarget = new Field2Vector2(_width, _height, _values.Clone() as Vector2[,]);
        }

        public static Field2Vector2 Laplacian(Field2Vector2 input) => Field2.Gradient(Divergence(input)) - Field2.Rotor(Rotor(input));

        public static Field2Vector2 operator *(Field2Vector2 input, Field2 multiplicator)
        {
            if (input._height != multiplicator.Height || input._width != multiplicator.Width)
            {
                throw new ArgumentException("Поля должны быть одного размера");
            }
            Vector2[,] output = input.Values.Clone() as Vector2[,];
            for (int i = 0; i < input._height; i++)
            {
                for (int j = 0; j < input._width; j++)
                {
                    output[i,j] = output[i,j] * multiplicator.Values[i,j];
                }
            }
            return new Field2Vector2(input._width, input._height, output);
        }
        public static Field2Vector2 operator *(Field2Vector2 input, double multiplicator)
        {
            Vector2[,] output = input.Values;
            for (int i = 0; i < input._height; i++)
            {
                for (int j = 0; j < input._width; j++)
                {
                    output[j, i] = output[j, i] * multiplicator;
                }
            }
            return new Field2Vector2(input._width, input._height, output);
        }
        public static Field2Vector2 operator -(Field2Vector2 input, Field2Vector2 input2)
        {
            Vector2[,] output = input.Values;
            for (int i = 0; i < input._height; i++)
            {
                for (int j = 0; j < input._width; j++)
                {
                    output[i, j] = output[i, j] - input2.Values[i,j];
                }
            }
            return new Field2Vector2(input._width, input._height, output);
        }
        public static Field2Vector2 operator +(Field2Vector2 input, Field2Vector2 input2)
        {
            Vector2[,] output = input.Values;
            for (int i = 0; i < input._height; i++)
            {
                for (int j = 0; j < input._width; j++)
                {
                    output[j, i] = output[j, i] + input2.Values[j, i];
                }
            }
            return new Field2Vector2(input._width, input._height, output);
        }

        /// <summary>
        /// Формирует поле, заданное изображением
        /// </summary>
        /// <param name="bitmap">Входное изображение (в качестве значения для поля принимается величина зеленого канала)</param>
        /// <param name="maxValue">Минимальное значение поля</param>
        /// <param name="minValue">Максимальное значение поля</param>
        /// <returns></returns>
        //public static Field2 FromBitmap(Bitmap bitmap, double maxValue, double minValue)
        //{
        //    Field2 output = new Field2(bitmap.Width, bitmap.Height);
        //    for (int j = 0; j < bitmap.Height - 1; j++)
        //    {
        //        for (int i = 0; i < bitmap.Width - 1; i++)
        //        {
        //            byte inputValue = bitmap.GetPixel(i, j).G;
        //            double fieldValue = minValue + (maxValue - minValue) / byte.MaxValue * inputValue;
        //            output._values[i, j] = fieldValue;
        //        }
        //    }
        //    return output;
        //}
    }
}
