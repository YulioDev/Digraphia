using Avalonia;

namespace Digraphia;

public struct Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y) { X = x; Y = y; }

    // Convierte coordenada de grilla (celda) a píxeles en el canvas
    public Point ToCanvasPoint(float cellSize = 40f) =>
        new(X * cellSize, Y * cellSize);

    // Posición en píxeles absolutos (sin multiplicar por celda)
    public Point ToPixelPoint() => new(X, Y);

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 a, float s)   => new(a.X * s,   a.Y * s);

    public override string ToString() => $"({X}, {Y})";
}