using System;
using System.Collections.Generic;
using ScottPlot;
using ScottPlot.Avalonia;

namespace NitroMonitor;

public class GraficaPastel
{
    private readonly AvaPlot _grafica;

    private readonly Color _colorTexto = Color.FromHex("#c6fefb");

    public GraficaPastel(AvaPlot grafica)
    {
        _grafica = grafica;

        var plot = _grafica.Plot;

        plot.Clear();

        plot.FigureBackground.Color = Color.FromHex("#00000000");
        plot.DataBackground.Color = Color.FromHex("#00000000");

        plot.Axes.Frameless();
        plot.HideGrid();
        plot.Axes.Color(_colorTexto);

        // Eliminar márgenes innecesarios
        plot.Axes.Margins(0, 0);

        _grafica.UserInputProcessor.IsEnabled = false;
    }

    public void Actualizar(double[] slices)
    {
        if (slices == null || slices.Length < 5)
            return;

        double total = slices[0];

        var plot = _grafica.Plot;

        plot.Clear();

        if (total <= 0)
        {
            _grafica.Refresh();
            return;
        }

        var piezas = new List<PieSlice>
        {
            new()
            {
                Value = Math.Max(0, slices[1]),
                FillColor = Color.FromHex("#FFFFFF80"),
                Label = $"En Uso\n{Porcentaje(slices[1], total):0.00}%"
            },
            new()
            {
                Value = Math.Max(0, slices[2]),
                FillColor = Color.FromHex("#C6FEFB80"),
                Label = $"Libre\n{Porcentaje(slices[2], total):0.00}%"
            },
            new()
            {
                Value = Math.Max(0, slices[3]),
                FillColor = Color.FromHex("#B8D8FF80"),
                Label = $"Compartido\n{Porcentaje(slices[3], total):0.00}%"
            },
            new()
            {
                Value = Math.Max(0, slices[4]),
                FillColor = Color.FromHex("#DCC6FF80"),
                Label = $"Buff/Cache\n{Porcentaje(slices[4], total):0.00}%"
            }
        };

        // Crear el pastel utilizando las piezas personalizadas
        var pie = plot.Add.Pie(piezas);

        pie.DonutFraction = 0.55;

        pie.LineColor = _colorTexto;
        pie.LineWidth = 2;

        // Acercar las etiquetas al pastel
        pie.SliceLabelDistance = 1.3;

        foreach (var slice in piezas)
        {
            slice.LabelFontColor = _colorTexto;
            slice.LabelFontSize = 14;
            slice.LabelBold = true;
        }

        // Evitar márgenes excesivos
        plot.Axes.Margins(0, 0);

        _grafica.Refresh();
    }

    private static double Porcentaje(double valor, double total)
    {
        return Math.Max(0, valor) / total * 100;
    }
}
