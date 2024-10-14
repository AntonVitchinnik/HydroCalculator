using MathV;
using System.Drawing;
using System.Linq;
using System.Diagnostics;

namespace PhysV
{
    public class Field2
    {
        private int _width;
        private int _height;
        public int Width => _width;
        public int Height => _height;
        private double[,] _values;
        public double[,] Values => _values;
        public double Total
        {
            get
            {
                double output = 0;
                for (int j = 0; j < _height; j++)
                {
                    for (int i = 0; i < _width; i++)
                    {
                        output += _values[i,j];
                    }
                }
                return output;
            }
        }
        public Field2(int width, int height)
        {
            _width = width;
            _height = height;
            _values = new double[_width, _height];
        }
        public Field2(int width, int height, double[,] values)
        {
            _width = width;
            _height = height;
            _values = values;
        }

        public void ApplyTransform(Field2Vector2 transform)
        {
            
            double[,] newValues = new double[_width, _height];
            newValues = _values.Clone() as double[,];
            void indexTransformer(int i, int j)
            {
                if (i != _width - 1 && transform.Values[i + 1, j].X < 0)
                {
                    newValues[i, j] += Math.Abs(transform.Values[i + 1, j].X);
                    newValues[i + 1, j] -= Math.Abs(transform.Values[i + 1, j].X);
                }
                if (i != 0 && transform.Values[i - 1, j].X > 0)
                {
                    newValues[i, j] += Math.Abs(transform.Values[i - 1, j].X);
                    newValues[i - 1, j] -= Math.Abs(transform.Values[i - 1, j].X);
                }
                if (j != _height - 1 && transform.Values[i, j + 1].Y < 0)
                {
                    newValues[i, j] += Math.Abs(transform.Values[i, j + 1].Y);
                    newValues[i, j + 1] -= Math.Abs(transform.Values[i, j + 1].Y);
                }
                if (j != 0 && transform.Values[i, j - 1].Y > 0)
                {
                    newValues[i, j] += Math.Abs(transform.Values[i, j - 1].Y);
                    newValues[i, j - 1] -= Math.Abs(transform.Values[i, j - 1].Y);
                }
                if (newValues[i, j] is double.NaN)
                {
                    var temp = _values[i, j];
                    newValues[i, j] = _values[i, j];
                }
            }
            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    indexTransformer(i, j);
                }
            }
            _values = newValues;
        }

        public Bitmap ToBitmap()
        {
            Bitmap output = new Bitmap(_width, _height);
            double minValue = _values.Cast<double>().Min();
            double maxValue = _values.Cast<double>().Max();
            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    output.SetPixel(i, j, Color.FromArgb((int)((_values[i, j] - minValue) / (maxValue - minValue) * byte.MaxValue), (int)((_values[i,j]-minValue)/(maxValue-minValue) * byte.MaxValue), (int)((_values[i, j] - minValue) / (maxValue - minValue) * byte.MaxValue)));
                }
            }
            return output;
        }

        /// <summary>
        /// Формирует поле, заданное изображением
        /// </summary>
        /// <param name="bitmap">Входное изображение (в качестве значения для поля принимается величина зеленого канала)</param>
        /// <param name="maxValue">Минимальное значение поля</param>
        /// <param name="minValue">Максимальное значение поля</param>
        /// <returns></returns>
        public static Field2 FromBitmap(Bitmap bitmap, double maxValue, double minValue)
        {
            Field2 output = new Field2(bitmap.Width, bitmap.Height);
            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    byte inputValue = bitmap.GetPixel(i, j).G;
                    double fieldValue = minValue + (maxValue - minValue) / byte.MaxValue * inputValue;
                    output._values[i, j] = fieldValue;
                }
            }
            return output;
        }
        public static Field2Vector2 Gradient(Field2 input)
        {
            Vector2[,] output = new Vector2[input._width, input._height];

            for(int j = 1; j < input._height -1; j++)
            {
                for(int i = 1; i < input._width -1; i++)
                {
                    output[i, j] = new Vector2((input._values[i + 1, j] - input._values[i - 1, j])/2, (input._values[i, j + 1] - input._values[i,j - 1]) / 2);
                }
            }
            for (int i = 1; i < input._width - 1; i++)
            {
                output[i, 0] = new Vector2((input._values[i + 1, 0] - input._values[i - 1, 0]) / 2, (input._values[i, 1] - input._values[i, 0]) / 2);
            }
            for (int i = 1; i < input._width - 1; i++)
            {
                output[i, input._height - 1] = new Vector2((input._values[i + 1, input._height - 1] - input._values[i - 1, input._height - 1]) / 2, (input._values[i, input._height - 1] - input._values[i, input._height - 2]) / 2);
            }
            for (int i = 1; i < input._height - 1; i++)
            {
                output[0, i] = new Vector2((input._values[1, i] - input._values[0, i]) / 2, (input._values[0, i + 1] - input._values[0, i - 1]) / 2);
            }
            for (int i = 1; i < input._height - 1; i++)
            {
                output[input._width - 1, i] = new Vector2((input._values[input._width - 1, i] - input._values[input._width - 2, i]) / 2, (input._values[input._width - 1, i + 1] - input._values[input._width - 1, i - 1]) / 2);
            }
            output[0, 0] = new Vector2((input._values[1, 0] - input._values[0, 0]) / 2, (input._values[0, 1] - input._values[0, 0]) / 2);
            output[0, input._height - 1] = new Vector2((input._values[1, input._height - 1] - input._values[0, input._height - 1]) / 2, (input._values[0, input._height - 1] - input._values[0, input._height - 2]) / 2);
            output[input._width - 1, input._height - 1] = new Vector2((input._values[input._width - 1, input._height - 1] - input._values[input._width - 2, input._height - 1]) / 2, (input._values[input._width - 1, input._height - 1] - input._values[input._width - 1, input._height - 2]) / 2);
            output[input._width - 1, 0] = new Vector2((input._values[input._width - 1, 0] - input._values[input._width - 2, 0]) / 2, (input._values[input._width - 1, 1] - input._values[input._width - 1, 0]) / 2);
            return new Field2Vector2(input.Width, input.Height, output);
        }
        public static Field2Vector2 Rotor(Field2 input)
        {
            Vector2[,] output = new Vector2[input._width, input._height];

            for (int i = 1; i < input._height - 1; i++)
            {
                for (int j = 1; j < input._width - 1; j++)
                {
                    output[i, j] = new Vector2((input._values[i - 1, j] - input._values[i + 1, j]) / 2, (input._values[i, j - 1] - input._values[i, j + 1]) / 2);
                }
            }

            return new Field2Vector2(input.Width, input.Height, output);
        }
        public static Field2 operator /(Field2 f1, Field2 f2)
        {
            if (f1._height != f2._height || f1._width != f2._width)
            {
                throw new ArgumentException("Поля должны быть одного размера");
            }
            double[,] output = new double[f1._width, f1._height];
            for (int j = 0; j < f1._height; j++)
            {
                for (int i = 0; i < f1._width; i++)
                {
                    output[i,j] = f1._values[i, j]/f2._values[i,j];
                }
            }
            return new Field2(f1._width, f1._height, output);
        }
        public static Field2 operator +(Field2 f1, Field2 f2)
        {
            if (f1._height != f2._height || f1._width != f2._width)
            {
                throw new ArgumentException("Поля должны быть одного размера");
            }
            double[,] output = new double[f1._width, f1._height];
            for (int j = 0; j < f1._height; j++)
            {
                for (int i = 0; i < f1._width; i++)
                {
                    output[i, j] = f1._values[i, j] + f2._values[i, j];
                }
            }
            return new Field2(f1._width, f1._height, output);
        }
        public static Field2 operator *(Field2 f1, Field2 f2)
        {
            if (f1._height != f2._height || f1._width != f2._width)
            {
                throw new ArgumentException("Поля должны быть одного размера");
            }
            double[,] output = new double[f1._width, f1._height];
            for (int j = 0; j < f1._height; j++)
            {
                for (int i = 0; i < f1._width; i++)
                {
                    output[i, j] = f1._values[i, j] * f2._values[i, j];
                }
            }
            return new Field2(f1._width, f1._height, output);
        }
        public static Field2 Abs(Field2 f)
        {
            double[,] output = new double[f._width, f._height];
            for (int j = 0; j < f._height; j++)
            {
                for (int i = 0; i < f._width; i++)
                {
                    output[i, j] = Math.Abs(f._values[i, j]);
                }
            }
            return new Field2(f._width, f._height, output);
        }
    }
}
