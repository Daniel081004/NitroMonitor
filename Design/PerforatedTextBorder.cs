using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace NitroMonitor.Models;

public class PerforatedTextBorder : Control
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, string>(
            nameof(Text), "Auto");

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, IBrush?>(
            nameof(Background), Brushes.LightBlue);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, double>(
            nameof(FontSize), 16);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, FontWeight>(
            nameof(FontWeight), FontWeight.Bold);

    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, double>(
            nameof(CornerRadius), 10);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static PerforatedTextBorder()
    {
        AffectsRender<PerforatedTextBorder>(
            TextProperty,
            BackgroundProperty,
            FontSizeProperty,
            FontWeightProperty,
            CornerRadiusProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Background == null ||
            Bounds.Width <= 0 ||
            Bounds.Height <= 0)
            return;

        // Geometría exterior
        Geometry borderGeometry = new RectangleGeometry
        {
            Rect = new Rect(Bounds.Size),
            RadiusX = CornerRadius,
            RadiusY = CornerRadius
        };

        // Configuración del texto
        var typeface = new Typeface(
            new FontFamily("Arial"),
            FontStyle.Normal,
            FontWeight);

        var formattedText = new FormattedText(
            Text ?? "",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            Brushes.Black);

        // Posición centrada
        double x = (Bounds.Width - formattedText.Width) / 2;
        double y = (Bounds.Height - formattedText.Height) / 2;

        // Geometría de las letras
        Geometry textGeometry = formattedText.BuildGeometry(
            new Point(x, y));

        // Restar las letras del rectángulo
        var finalGeometry = new CombinedGeometry
        {
            Geometry1 = borderGeometry,
            Geometry2 = textGeometry,
            GeometryCombineMode = GeometryCombineMode.Exclude
        };

        // Dibujar la geometría resultante
        context.DrawGeometry(
            Background,
            null,
            finalGeometry);
    }
}
