using HydroCalcLib;
using System.Drawing;
using MathV;
using PhysV;
using System.Runtime.InteropServices;

namespace HydroCalculator
{
    class Program
    {
        private static double _maxPressure = 100d;
        private static double _minPressure = 0;
        private static double _totalPressure = 0;
        private static Field2 _pressureField;
        private static Field2Vector2 _pressureTransforms;
        private static Field2Vector2 _velocityField;
        private static Field2Vector2 _accelerationField;
        private static Field2Vector2 _firstMember;
        private static Field2Vector2 _velocityLaplacian;
        private static Field2Vector2 _rightSide;
        private static Bitmap _bitmap;
        public static void Main(string[] args)
        {
            Console.WriteLine("Hydro Calculator");

            _bitmap = new Bitmap(@"Maps\test_map.bmp");
            _pressureField = Field2.FromBitmap(_bitmap, _maxPressure, _minPressure);
            _accelerationField = Field2.Gradient(_pressureField) * (-1);
            _accelerationField.ToBitmap().Save(@"Maps\accelerationMap.bmp");

            _velocityField = new Field2Vector2(_bitmap.Width, _bitmap.Height);
            
            _totalPressure = _pressureField.Total;


            //_pressureTransforms = new Field2Vector2(_bitmap.Width, _bitmap.Height, (new Field2(_bitmap.Width, _bitmap.Height, _velocityField.X) / (Field2.Abs(new Field2(_bitmap.Width, _bitmap.Height, _velocityField.Y)) + Field2.Abs(new Field2(_bitmap.Width, _bitmap.Height, _velocityField.X))) * _pressureField).Values, (new Field2(_bitmap.Width, _bitmap.Height, _velocityField.Y) / (Field2.Abs(new Field2(_bitmap.Width, _bitmap.Height, _velocityField.Y)) + Field2.Abs(new Field2(_bitmap.Width, _bitmap.Height, _velocityField.X))) * _pressureField).Values);

            //_pressureField.ApplyTransform(_pressureTransforms);
            _pressureField.ToBitmap().Save(@"Maps\newPressureMap.bmp");

            Iterate();

            _totalPressure = _pressureField.Total;
            //_velocityField = Field2.Gradient(_pressureField);
            //_firstMember = _velocityField * Field2Vector2.Divergence(_velocityField) * (-1);

            //_velocityLaplacian = Field2Vector2.Laplacian(_velocityField);

            //_rightSide = _firstMember + _velocityLaplacian - _velocityField;

            Console.WriteLine("Map loaded");

            

            //_velocityField.ToBitmap().Save(@"Maps\velocityMap.bmp");
            //_velocityLaplacian.ToBitmap().Save(@"Maps\velocityLaplacian.bmp");
            //_firstMember.ToBitmap().Save(@"Maps\firstMember.bmp");
            //_rightSide.ToBitmap().Save(@"Maps\rightSide.bmp");
        }
        private static void Iterate()
        {
            for (int i = 0 ; i < 600; i++)
            {
                _accelerationField = Field2.Gradient(_pressureField) * (-1);
                _velocityField = _velocityField + _accelerationField;
                AnalyzeFieldPressureVelocity();

                Avg();
                _totalPressure = _pressureField.Total;

                _pressureField.ToBitmap().Save(String.Format(@"Maps\newPressureMap{0}.bmp",i.ToString()));
                _accelerationField.ToBitmap().Save(String.Format(@"Maps\acceleration{0}.bmp", i.ToString()));
                _velocityField.ToBitmap().Save(String.Format(@"Maps\velocity{0}.bmp", i.ToString()));
            }
        }
        private static void Avg()
        {
            var output = new double[_bitmap.Width, _bitmap.Height];
            for(int j = 1 ;j < _bitmap.Height - 1 ;j++)
            {
                for(int i = 1 ; i < _bitmap.Width - 1 ;i++)
                {
                    output[i, j] = (_pressureField.Values[i - 1, j] + _pressureField.Values[i + 1, j] + _pressureField.Values[i, j - 1] + _pressureField.Values[i, j + 1])/4;
                }
            }
            for (int i = 1; i < _bitmap.Width - 1; i++)
            {
                output[i, 0] = (_pressureField.Values[i + 1, 0] + _pressureField.Values[i - 1, 0] + _pressureField.Values[i, 1]) / 3;
            }
            for (int i = 1; i < _bitmap.Width - 1; i++)
            {
                output[i, _bitmap.Height - 1] = (_pressureField.Values[i + 1, _bitmap.Height - 1] + _pressureField.Values[i - 1, _bitmap.Height - 1] + _pressureField.Values[i, _bitmap.Height - 2]) / 3;
            }
            for (int i = 1; i < _bitmap.Height - 1; i++)
            {
                output[0, i] = (_pressureField.Values[0, i + 1] + _pressureField.Values[0, i - 1] + _pressureField.Values[0, i]) / 3;
            }
            for (int i = 1; i < _bitmap.Height - 1; i++)
            {
                output[_bitmap.Width - 1, i] = (_pressureField.Values[_bitmap.Width - 1, i + 1] + _pressureField.Values[_bitmap.Width - 1, i - 1] + _pressureField.Values[_bitmap.Width - 2, i]) / 3;
            }
            output[0, 0] = (_pressureField.Values[1, 0] + _pressureField.Values[0, 1]) / 2;
            output[0, _bitmap.Height - 1] = (_pressureField.Values[0, _bitmap.Height - 2] + _pressureField.Values[1, _bitmap.Height - 1]) / 2;
            output[_bitmap.Width - 1, _bitmap.Height - 1] = (_pressureField.Values[_bitmap.Width - 1, _bitmap.Height - 2] + _pressureField.Values[_bitmap.Width - 2, _bitmap.Height - 1]) / 2;
            output[_bitmap.Width - 1, 0] = (_pressureField.Values[_bitmap.Width - 1, 1] + _pressureField.Values[_bitmap.Width - 2, 0]) / 2;
            _pressureField = new Field2(_bitmap.Width, _bitmap.Height, output);
        }
        
        private static void AnalyzeFieldPressureVelocity()
        {
            double[,] newValues = new double[_bitmap.Width, _bitmap.Height];
            //newValues = _pressureField.Values.Clone() as double[,];
            void indexIterator(int i, int j)
            {
                //var sum = newValues.Cast<double>().Sum();
                //if (_pressureField.Values[i, j] == 0) return;
                double summaryOut = 0;
                List<(int, int, double)> outs = new List<(int, int,double)>();
                if (i != _bitmap.Width - 1 && _velocityField.Values[i + 1, j].X > 0 && _pressureField.Values[i + 1, j] != _pressureField.Values[i, j])
                {
                    summaryOut += _velocityField.Values[i + 1, j].X;
                    outs.Add((1,0, _velocityField.Values[i + 1, j].X));
                    if (j != _bitmap.Height - 1 && _velocityField.Values[i, j + 1].X > 0)
                    {
                        summaryOut += _velocityField.Values[i, j + 1].X;
                        outs.Add((1, 0, _velocityField.Values[i, j + 1].X));
                    }
                    if (j != 0 && _velocityField.Values[i, j - 1].X > 0)
                    {
                        summaryOut += _velocityField.Values[i, j - 1].X;
                        outs.Add((1, 0, _velocityField.Values[i, j - 1].X));
                    }
                }
                if (i != 0 && _velocityField.Values[i - 1, j].X < 0 && _pressureField.Values[i - 1, j] != _pressureField.Values[i, j])
                {
                    summaryOut += Math.Abs(_velocityField.Values[i - 1, j].X);
                    outs.Add((-1, 0, Math.Abs(_velocityField.Values[i - 1, j].X)));
                    if (j != _bitmap.Height - 1 && _velocityField.Values[i, j + 1].X < 0)
                    {
                        summaryOut += Math.Abs(_velocityField.Values[i, j + 1].X);
                        outs.Add((-1, 0, Math.Abs(_velocityField.Values[i, j + 1].X)));
                    }
                    if (j != 0 && _velocityField.Values[i, j - 1].X < 0)
                    {
                        summaryOut += Math.Abs(_velocityField.Values[i, j - 1].X);
                        outs.Add((-1, 0, Math.Abs(_velocityField.Values[i, j - 1].X)));
                    }
                }
                if (j != _bitmap.Height - 1 && _velocityField.Values[i, j + 1].Y > 0 && _pressureField.Values[i, j + 1] != _pressureField.Values[i, j])
                {
                    summaryOut += _velocityField.Values[i, j + 1].Y;
                    outs.Add((0, 1, _velocityField.Values[i, j + 1].Y));
                    if (i != _bitmap.Width - 1 && _velocityField.Values[i + 1, j].Y > 0)
                    {
                        summaryOut += _velocityField.Values[i + 1, j].Y;
                        outs.Add((0, 1, _velocityField.Values[i + 1, j].Y));
                    }
                    if (i != 0 && _velocityField.Values[i - 1, j].Y > 0)
                    {
                        summaryOut += _velocityField.Values[i - 1,j].Y;
                        outs.Add((0, 1, _velocityField.Values[i - 1, j].Y));
                    }
                }
                if (j != 0 && _velocityField.Values[i, j - 1].Y < 0 && _pressureField.Values[i, j - 1] != _pressureField.Values[i, j])
                {
                    summaryOut += Math.Abs(_velocityField.Values[i, j - 1].Y);
                    outs.Add((0, -1, Math.Abs(_velocityField.Values[i, j - 1].Y)));
                    if (i != _bitmap.Width - 1 && _velocityField.Values[i + 1, j].Y < 0)
                    {
                        summaryOut += Math.Abs(_velocityField.Values[i + 1, j].Y);
                        outs.Add((0, -1, Math.Abs(_velocityField.Values[i + 1, j].Y)));
                    }
                    if (i != 0 && _velocityField.Values[i - 1, j].Y < 0)
                    {
                        summaryOut += Math.Abs(_velocityField.Values[i - 1, j].Y);
                        outs.Add((0, -1, Math.Abs(_velocityField.Values[i - 1,j].Y)));
                    }
                }
                for (int c = 0; c < outs.Count; c++)
                {
                    //if(c != 0 && c == outs.Count - 1)
                    //{
                    //    var last = outs.Sum(e => e.Item3) - outs[outs.Count - 1].Item3;
                    //    newValues[i + outs[c].Item1, j + outs[c].Item2] += _pressureField.Values[i, j] - _pressureField.Values[i, j] * last;
                    //    continue;
                    //}
                    outs[c] = (outs[c].Item1, outs[c].Item2, outs[c].Item3 / summaryOut);
                    newValues[i + outs[c].Item1, j + outs[c].Item2] += outs[c].Item3 * _pressureField.Values[i,j];
                }
                if (summaryOut == 0)
                {
                    newValues[i, j] += _pressureField.Values[i, j];
                }
            }
            for (int j = 0; j < _bitmap.Height; j++)
            {
                for (int i = 0; i < _bitmap.Width; i++)
                {
                    indexIterator(i,j);
                }
            }
            _pressureField = new Field2(_bitmap.Width, _bitmap.Height, newValues);
        }
    }
}