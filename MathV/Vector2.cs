namespace MathV
{
    public struct Vector2
    {
        public double X; 
        public double Y;
        public Vector2 (double x, double y)
        {
            this.X = x; 
            this.Y = y;
        }
        public static Vector2 operator *(Vector2 input, double multiplicator) => new Vector2(input.X * multiplicator, input.Y * multiplicator);
        public static Vector2 operator -(Vector2 input, Vector2 input2) => new Vector2(input.X - input2.X, input.Y - input2.Y);
        public static Vector2 operator +(Vector2 input, Vector2 input2) => new Vector2(input.X + input2.X, input.Y + input2.Y);
    }
}
