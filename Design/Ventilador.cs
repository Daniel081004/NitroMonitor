using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;

namespace NitroMonitor.Design;

public class Ventilador : Viewbox
{
    // Espacio de diseño fijo: todo se dibuja en 200x200 y el Viewbox lo escala.
    private const double Design = 200;
    private const double C = Design / 2;
    private const int NumAspas = 4;

    private static readonly Color ColorRelleno = Colors.Transparent;
    private static readonly Color ColorBorde = Color.Parse("#C6FEFB");
    private static readonly Color ColorRejilla = Color.FromArgb(90, 0xC6, 0xFE, 0xFB);

    private static readonly IBrush SinRelleno = Brushes.Transparent;

    private readonly Canvas _lienzo;
    private readonly Canvas _rotor;
    private readonly RotateTransform _giro = new();
    private readonly DispatcherTimer _timer;
    private DateTime _ultimo;

    public static readonly StyledProperty<double> RpmProperty =
        AvaloniaProperty.Register<Ventilador, double>(nameof(Rpm));

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Ventilador, double>(nameof(Size), 200);

    public double Rpm
    {
        get => GetValue(RpmProperty);
        set => SetValue(RpmProperty, value);
    }

    /// <summary>Tamaño final (ancho y alto) del ventilador.</summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    static Ventilador()
    {
        SizeProperty.Changed.AddClassHandler<Ventilador>((v, e) =>
        {
            double s = (double)e.NewValue!;
            v.Width = s;
            v.Height = s;
        });
    }

    public Ventilador()
    {
        Stretch = Stretch.Uniform;
        Width = Size;
        Height = Size;

        _lienzo = new Canvas { Width = Design, Height = Design };
        Child = _lienzo;

        var hijos = _lienzo.Children;

        hijos.Add(Circulo(Design - 2, 1, SinRelleno, ColorBorde, 4));

        _rotor = new Canvas
        {
            Width = Design,
            Height = Design,
            RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            RenderTransform = _giro
        };

        for (int i = 0; i < NumAspas; i++)
            _rotor.Children.Add(CrearAspa(i * 360.0 / NumAspas));

        _rotor.Children.Add(Circulo(46, C - 23, SinRelleno, ColorBorde, 4));

        for (int i = 0; i < 3; i++)
        {
            double a = (i * 120 - 90) * Math.PI / 180;
            _rotor.Children.Add(Circulo(5,
                C + Math.Cos(a) * 14 - 2.5,
                C + Math.Sin(a) * 14 - 2.5,
                SinRelleno, ColorBorde, 0.8));
        }

        hijos.Add(_rotor);

        foreach (double r in new[] { 30.0, 52.0, 74.0, 90.0 })
            hijos.Add(Circulo(r * 2, C - r, SinRelleno, ColorRejilla, r > 85 ? 2 : 0.8));

        for (int i = 0; i < 12; i++)
        {
            double a = i * Math.PI / 6;
            hijos.Add(new Line
            {
                StartPoint = new Point(C + Math.Cos(a) * 24, C + Math.Sin(a) * 24),
                EndPoint = new Point(C + Math.Cos(a) * 92, C + Math.Sin(a) * 92),
                Stroke = new SolidColorBrush(ColorRejilla),
                StrokeThickness = 0.8
            });
        }

        hijos.Add(Circulo(16, C - 8, SinRelleno, ColorBorde, 4));

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) =>
        {
            var ahora = DateTime.UtcNow;
            double dt = (ahora - _ultimo).TotalSeconds;
            _ultimo = ahora;
            _giro.Angle = (_giro.Angle + Rpm * 6.0 * dt) % 360;
        };

        AttachedToVisualTree += (_, _) => { _ultimo = DateTime.UtcNow; _timer.Start(); };
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    private static Path CrearAspa(double angulo)
    {
        var geo = Geometry.Parse(
            "M -4,-12 C -26,-36 -30,-68 -4,-96 L 20,-34 C 20,-24 12,-16 4,-12 Z");

        geo.Transform = new TransformGroup
        {
            Children =
            {
                new RotateTransform(angulo),
                new TranslateTransform(C, C)
            }
        };

        return new Path
        {
            Data = geo,
            Fill = new SolidColorBrush(ColorRelleno),
            Stroke = new SolidColorBrush(ColorBorde),
            StrokeThickness = 4,
            StrokeJoin = PenLineJoin.Miter
        };
    }

    private static Ellipse Circulo(double diametro, double pos, IBrush relleno, Color borde, double grosor)
        => Circulo(diametro, pos, pos, relleno, borde, grosor);

    private static Ellipse Circulo(double diametro, double left, double top, IBrush relleno, Color borde, double grosor)
    {
        var e = new Ellipse
        {
            Width = diametro,
            Height = diametro,
            Fill = relleno,
            Stroke = new SolidColorBrush(borde),
            StrokeThickness = grosor
        };
        Canvas.SetLeft(e, left);
        Canvas.SetTop(e, top);
        return e;
    }
}