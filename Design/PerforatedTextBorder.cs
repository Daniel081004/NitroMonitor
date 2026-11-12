using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace NitroMonitor.Design;

public class PerforatedTextBorder : Control
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, string>(
            nameof(Text), "Auto");

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, IBrush?>(
            nameof(Background), Brushes.LightBlue);

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, IBrush?>(
            nameof(BorderBrush));

    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, double>(
            nameof(BorderThickness), 0);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, IBrush?>(
            nameof(Foreground), Brushes.Black);

    public static readonly StyledProperty<bool> IsPerforatedProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, bool>(
            nameof(IsPerforated), true);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, double>(
            nameof(FontSize), 16);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, FontWeight>(
            nameof(FontWeight), FontWeight.Bold);

    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, double>(
            nameof(CornerRadius), 10);

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, ICommand?>(
            nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<PerforatedTextBorder, object?>(
            nameof(CommandParameter));

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

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>Color del texto cuando IsPerforated = false.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// true  = las letras se "recortan" (transparentes).
    /// false = las letras se dibujan como texto normal.
    /// </summary>
    public bool IsPerforated
    {
        get => GetValue(IsPerforatedProperty);
        set => SetValue(IsPerforatedProperty, value);
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

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    static PerforatedTextBorder()
    {
        AffectsRender<PerforatedTextBorder>(
            TextProperty,
            BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            ForegroundProperty,
            IsPerforatedProperty,
            FontSizeProperty,
            FontWeightProperty,
            CornerRadiusProperty);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            PseudoClasses.Set(":pressed", true);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!PseudoClasses.Contains(":pressed"))
            return;

        PseudoClasses.Set(":pressed", false);

        if (e.Pointer.Captured == this)
            e.Pointer.Capture(null);

        var point = e.GetPosition(this);
        if (new Rect(Bounds.Size).Contains(point))
        {
            if (Command?.CanExecute(CommandParameter) == true)
                Command.Execute(CommandParameter);
        }

        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        PseudoClasses.Set(":pressed", false);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var rect = new Rect(Bounds.Size);

        // Rectángulo transparente para que toda el área reciba eventos de puntero
        context.DrawRectangle(Brushes.Transparent, null, rect, CornerRadius, CornerRadius);

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
            Foreground ?? Brushes.Black);

        var textOrigin = new Point(
            (Bounds.Width - formattedText.Width) / 2,
            (Bounds.Height - formattedText.Height) / 2);

        // ---------- Fondo ----------
        if (Background != null)
        {
            if (IsPerforated)
            {
                Geometry borderGeometry = new RectangleGeometry
                {
                    Rect = rect,
                    RadiusX = CornerRadius,
                    RadiusY = CornerRadius
                };

                Geometry textGeometry = formattedText.BuildGeometry(textOrigin)!;

                var finalGeometry = new CombinedGeometry
                {
                    Geometry1 = borderGeometry,
                    Geometry2 = textGeometry,
                    GeometryCombineMode = GeometryCombineMode.Exclude
                };

                context.DrawGeometry(Background, null, finalGeometry);
            }
            else
            {
                // Fondo normal + texto normal encima
                context.DrawRectangle(Background, null, rect, CornerRadius, CornerRadius);
                context.DrawText(formattedText, textOrigin);
            }
        }
        else if (!IsPerforated)
        {
            // Sin fondo: solo el texto
            context.DrawText(formattedText, textOrigin);
        }

        // ---------- Borde ----------
        if (BorderBrush != null && BorderThickness > 0)
        {
            // El trazo se centra sobre la línea, así que se mete hacia adentro
            // la mitad del grosor para que no se salga de los límites del control
            double half = BorderThickness / 2;
            var borderRect = rect.Deflate(half);
            double radius = System.Math.Max(0, CornerRadius - half);

            var pen = new Pen(BorderBrush, BorderThickness);
            context.DrawRectangle(null, pen, borderRect, radius, radius);
        }
    }
}