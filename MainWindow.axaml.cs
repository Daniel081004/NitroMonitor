using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NitroMonitor.ViewModel;

namespace NitroMonitor;

public partial class MainWindow : Window
{
    private readonly NitroMonitorViewModel _vm;
    
    public MainWindow()
    {
        InitializeComponent();

        _vm = (NitroMonitorViewModel)DataContext!;

        _vm.InicializarGraficaCpu(CpuGrafica, CpuTooltip, CpuTextoTiempo, CpuTextoTemperatura, CpuTextoUso);
        _vm.InicializarGraficaGpu(GpuGrafica, GpuTooltip, GpuTextoTiempo, GpuTextoTemperatura, GpuTextoUso);
        _vm.InicializarGraficaRam(RamGrafica);
    }

    
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void BtnCerrarVentana_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}