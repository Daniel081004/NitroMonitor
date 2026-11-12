using System;
using Avalonia.Controls;
using Avalonia.Input;
using ScottPlot;
using ScottPlot.Avalonia;

namespace NitroMonitor;

public class GraficaMonitor
{
    private const int Capacidad = 100;

    private readonly AvaPlot _grafica;
    private readonly Control _tooltip;
    private readonly TextBlock _textoHora;
    private readonly TextBlock _textoTemperatura;
    private readonly TextBlock _textoUso;

    private readonly double[] _tiempos = new double[Capacidad];
    private readonly DateTime[] _horas = new DateTime[Capacidad];
    private readonly double[] _temperaturas = new double[Capacidad];
    private readonly double[] _usos = new double[Capacidad];

    private int _cantidad = 0;
    private double _siguienteTiempo = 0;

    public GraficaMonitor(
        AvaPlot grafica,
        string titulo,
        string colorTemperatura,
        string colorUso,
        Control tooltip,
        TextBlock textoHora,
        TextBlock textoTemperatura,
        TextBlock textoUso)
    {
        _grafica = grafica;
        _tooltip = tooltip;
        _textoHora = textoHora;
        _textoTemperatura = textoTemperatura;
        _textoUso = textoUso;

        Configurar(titulo, colorTemperatura, colorUso);

        _grafica.PointerMoved += Grafica_PointerMoved;
        _grafica.PointerExited += (s, e) => _tooltip.IsVisible = false;
    }

    private void Configurar(string titulo, string colorTemperatura, string colorUso)
    {
        var plot = _grafica.Plot;

        Array.Fill(_temperaturas, double.NaN);
        Array.Fill(_usos, double.NaN);

        for (int i = 0; i < Capacidad; i++)
            _tiempos[i] = i;

        plot.FigureBackground.Color = Color.FromHex("#00000000");
        plot.DataBackground.Color = Color.FromHex("#00000000");

        var color = Color.FromHex("#c6fefb");

        plot.Axes.Color(color);
        plot.Axes.Left.TickLabelStyle.ForeColor = color;
        plot.Axes.Bottom.TickLabelStyle.ForeColor = color;
        plot.Axes.FrameWidth(4);

        /*
        plot.Title(titulo);
        plot.Axes.Title.Label.ForeColor = color;
        plot.Axes.Title.Label.FontSize = 30;
        plot.Axes.Title.Label.Bold = false;
        */

        plot.Axes.SetLimitsY(0, 100, plot.Axes.Left);
        plot.Axes.SetLimitsX(0, 99);

        var lineaTemp = plot.Add.Scatter(_tiempos, _temperaturas);
        lineaTemp.LegendText = "Temperatura";
        lineaTemp.Color = Color.FromHex(colorTemperatura);
        lineaTemp.LineWidth = 2;
        lineaTemp.MarkerSize = 0;

        var lineaUso = plot.Add.Scatter(_tiempos, _usos);
        lineaUso.LegendText = "Uso";
        lineaUso.Color = Color.FromHex(colorUso);
        lineaUso.LineWidth = 2;
        lineaUso.MarkerSize = 0;

        plot.Legend.IsVisible = false;
        _grafica.UserInputProcessor.IsEnabled = false;

        _grafica.Refresh();
    }

    public void AgregarDatos(double temperatura, double uso)
    {
        temperatura = Math.Clamp(temperatura, 0, 100);
        uso = Math.Clamp(uso, 0, 100);

        DateTime ahora = DateTime.Now;

        if (_cantidad < Capacidad)
        {
            _tiempos[_cantidad] = _siguienteTiempo;
            _horas[_cantidad] = ahora;
            _temperaturas[_cantidad] = temperatura;
            _usos[_cantidad] = uso;
            _cantidad++;
        }
        else
        {
            Array.Copy(_tiempos, 1, _tiempos, 0, Capacidad - 1);
            Array.Copy(_horas, 1, _horas, 0, Capacidad - 1);
            Array.Copy(_temperaturas, 1, _temperaturas, 0, Capacidad - 1);
            Array.Copy(_usos, 1, _usos, 0, Capacidad - 1);

            _tiempos[Capacidad - 1] = _siguienteTiempo;
            _horas[Capacidad - 1] = ahora;
            _temperaturas[Capacidad - 1] = temperatura;
            _usos[Capacidad - 1] = uso;
        }

        _siguienteTiempo++;

        double minimo = _tiempos[0];
        double maximo = Math.Max(99, _siguienteTiempo - 1);

        _grafica.Plot.Axes.SetLimitsX(minimo, maximo);
        _grafica.Refresh();
    }

    private void Grafica_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_cantidad == 0)
        {
            _tooltip.IsVisible = false;
            return;
        }

        var posicion = e.GetPosition(_grafica);

        var coordenadas = _grafica.Plot.GetCoordinates(
            (float)(posicion.X * _grafica.DisplayScale),
            (float)(posicion.Y * _grafica.DisplayScale));

        int indice = 0;
        double distanciaMinima = double.MaxValue;

        for (int i = 0; i < _cantidad; i++)
        {
            double distancia = Math.Abs(_tiempos[i] - coordenadas.X);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                indice = i;
            }
        }

        _textoHora.Text = $"Hora: {_horas[indice]:HH:mm:ss}";
        _textoTemperatura.Text = $"Temperatura: {_temperaturas[indice]:0.00} °C";
        _textoUso.Text = $"Uso: {_usos[indice]:0.00}%";

        double x = posicion.X + 15;
        double y = posicion.Y + 15;

        double ancho = _tooltip.Bounds.Width;
        double alto = _tooltip.Bounds.Height;

        if (x + ancho > _grafica.Bounds.Width)
            x = posicion.X - ancho - 15;

        if (y + alto > _grafica.Bounds.Height)
            y = posicion.Y - alto - 15;

        Canvas.SetLeft(_tooltip, Math.Max(0, x));
        Canvas.SetTop(_tooltip, Math.Max(0, y));

        _tooltip.IsVisible = true;
    }
}