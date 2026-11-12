using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using NetPace.Core;
using NetPace.Core.Clients.Ookla;
using ScottPlot.Avalonia;

namespace NitroMonitor.ViewModel;

public class NitroMonitorViewModel:INotifyPropertyChanged
{
    private readonly ISpeedTestService _tester;
    private readonly Timer _timer = new(1000);
    private GraficaMonitor _cpu;
    private GraficaMonitor _gpu;
    private GraficaPastel _ram;
    public bool BtnAuto { get; set; } = true;
    public IBrush BtnAutoBrush { get; set; } = Brushes.Transparent;
    public IBrush[] BtnAutoBackground { get; set; } = [SolidColorBrush.Parse("#C6FEFB"), SolidColorBrush.Parse("#65D9D0"), SolidColorBrush.Parse("#20AFA5")];
    public IBrush[] BtnAutoForeground { get; set; } = [Brushes.Transparent, Brushes.Transparent, Brushes.Transparent];
    private IEnumerable<byte> _cantidadVentiladores = [];
    public IEnumerable<byte> CantidadVentiladores
    {
        get => _cantidadVentiladores;
        set
        {
            _cantidadVentiladores = value;
            PropertyChanged?.Invoke(this, new(nameof(CantidadVentiladores)));
        }
    }
    public double Rpm { get; set; } = 50;
    private int _slider = 50;
    public int Slider
    {
        get => _slider;
        set
        {
            _slider = value;
            PropertyChanged?.Invoke(this, new(nameof(Slider)));
            CambiarValorVentilador();
        }
    }
    private Ventilador _ventiladorInView;
    private List<Ventilador> _ventiladores = [];
    private string _ventiladorSeleccionado = "0";
    public string VentiladorSeleccionado
    {
        get => _ventiladorSeleccionado;
        set
        {
            _ventiladorSeleccionado = value;
            CambioVentilador();
        }
    }
    public string DownloadSpeed { get; set; } = "--- Mbps";
    public string UploadSpeed { get; set; } = "--- Mbps";
    public string Latency { get; set; } = "--- ms";
    public ICommand ToggleAutoButtonCommand { get; set; }

    public NitroMonitorViewModel()
    {
        EjecutarComando("echo Y | sudo tee /sys/module/ec_sys/parameters/write_support\n");
        EjecutarComando("sudo nbfc start");
        
        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
        string nbfcstatus = EjecutarComando("nbfc status");
        bool[] autoControlEnabled = Regex.Matches(nbfcstatus, @"Auto Control Enabled\s*:\s*(true|false)")
            .Select(m => bool.Parse(m.Groups[1].Value))
            .ToArray();
        byte[] targetFanSpeeds = Regex.Matches(nbfcstatus, @"Target Fan Speed\s*:\s*(\d+)")
            .Select(m => byte.Parse(m.Groups[1].Value))
            .ToArray();
        byte cantidad = byte.Parse(EjecutarComando("nbfc sensors show | grep -c \"fan\""));
        List<byte> lista = [];
        for (int i = 0; i < cantidad; i++)
        {
            lista.Add((byte)i);
            Ventilador v = new()
            {
                Auto = autoControlEnabled[i],
                Velocidad = autoControlEnabled[i] ? (byte)50 : targetFanSpeeds[i]
            };
            _ventiladores.Add(v);
        }

        _slider = _ventiladores[0].Velocidad;
        PropertyChanged?.Invoke(this,new(nameof(Slider)));
        
        CambioVentilador();
        
        _ventiladorInView = _ventiladores[0];
        _cantidadVentiladores = lista;
        
        _tester = new OoklaSpeedtest();
        TestSpeed();
        
        ToggleAutoButtonCommand = new RelayCommand(ToggleAutoButton);
    }
    
    public async void TestSpeed()
    {
        try
        {
            var servers = await _tester.GetServersAsync();
            var fastest = await _tester.GetFastestServerByLatencyAsync(servers);
            var download = await _tester.GetDownloadSpeedAsync(fastest.Server);
            var upload = await _tester.GetUploadSpeedAsync(fastest.Server);

            DownloadSpeed = download.GetSpeedString(SpeedUnit.BitsPerSecond, SpeedUnitSystem.SI);
            UploadSpeed = upload.GetSpeedString(SpeedUnit.BitsPerSecond, SpeedUnitSystem.SI);
            Latency = $"{fastest.LatencyMilliseconds.ToString()} ms";
        }
        catch{}
        PropertyChanged?.Invoke(this,new(nameof(DownloadSpeed)));
        PropertyChanged?.Invoke(this,new(nameof(UploadSpeed)));
        PropertyChanged?.Invoke(this,new(nameof(Latency)));
        await Task.Delay(5000);
        TestSpeed();
    }

    private void ActualizarVelocidadVentiladorUI()
    {
        if (BtnAuto)
        {
            Rpm = 50;
        }
        else
        {
            Rpm = _slider;
        }
        PropertyChanged?.Invoke(this, new(nameof(Rpm)));
    }

    private void CambioVentilador()
    {
        _ventiladorInView = _ventiladores[int.Parse(_ventiladorSeleccionado)];
        Slider = _ventiladorInView.Velocidad;
        if (_ventiladorInView.Auto != BtnAuto)
        {
            _ventiladorInView.Auto = BtnAuto;
            ToggleAutoButton();
        }
        ActualizarVelocidadVentiladorUI();
        PropertyChanged?.Invoke(this,new(nameof(Slider)));
    }

    private void CambiarValorVentilador()
    {
        _ventiladorInView.Velocidad = (byte)_slider;
        if (!_ventiladores[byte.Parse(VentiladorSeleccionado)].Auto)
        {
            EjecutarComando($"sudo nbfc set -f {VentiladorSeleccionado} -s {_slider}");
            ActualizarVelocidadVentiladorUI();
        }
    }

    private void ToggleAutoButton()
    {
        BtnAuto = !BtnAuto;
        BtnAutoBrush = BtnAuto ? Brushes.Transparent : SolidColorBrush.Parse("#C6FEFB");
        BtnAutoBackground = BtnAuto ? [SolidColorBrush.Parse("#C6FEFB"), SolidColorBrush.Parse("#65D9D0"), SolidColorBrush.Parse("#20AFA5")] : [Brushes.Transparent, Brushes.Transparent, Brushes.Transparent];
        BtnAutoForeground = !BtnAuto ? [SolidColorBrush.Parse("#C6FEFB"), SolidColorBrush.Parse("#65D9D0"), SolidColorBrush.Parse("#20AFA5")] : [Brushes.Transparent, Brushes.Transparent, Brushes.Transparent];
        PropertyChanged?.Invoke(this, new(nameof(BtnAuto)));
        PropertyChanged?.Invoke(this, new(nameof(BtnAutoBrush)));
        PropertyChanged?.Invoke(this, new(nameof(BtnAutoBackground)));
        PropertyChanged?.Invoke(this, new(nameof(BtnAutoForeground)));
        _ventiladorInView.Auto = BtnAuto;
        if (BtnAuto)
        {
            EjecutarComando($"sudo nbfc set -f {_ventiladorSeleccionado} --auto");
        }
        else
        {
            EjecutarComando($"sudo nbfc set -f {VentiladorSeleccionado} -s {_slider}");
        }
        ActualizarVelocidadVentiladorUI();
    }

    ~NitroMonitorViewModel()
    {
        _timer.Stop();
        _timer.Dispose();
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        double cpuUso = double.Parse(EjecutarComando("top -bn1 | grep \"Cpu(s)\" | awk '{print 100 - $8}'"), CultureInfo.InvariantCulture);
        double cpuTemperatura = double.Parse(EjecutarComando("sensors | grep -i 'Package id 0' | awk '{print $4}' | tr -d '+°C'"), CultureInfo.InvariantCulture);
        AgregarDatosCpu(cpuTemperatura, cpuUso);
        
        double gpuTemperatura = double.Parse(EjecutarComando("nvidia-smi --query-gpu=temperature.gpu --format=csv,noheader,nounits"), CultureInfo.InvariantCulture);
        double gpuUso = double.Parse(EjecutarComando("nvidia-smi --query-gpu=utilization.gpu --format=csv,noheader,nounits\n"),CultureInfo.InvariantCulture);
        AgregarDatosGpu(gpuTemperatura, gpuUso);
        
        string x = EjecutarComando("free -b | grep Mem");
        double[] valores = x.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .TakeLast(6)
            .SkipLast(1)
            .Select(v => double.Parse(v, CultureInfo.InvariantCulture) / (1024.0 * 1024.0 * 1024.0))
            .ToArray();
        AgregarDatosRam(valores);
    }

    public void AgregarDatosRam(double[] valores) =>
        Dispatcher.UIThread.Post(() => _ram.Actualizar(valores));
    public void AgregarDatosCpu(double temperatura, double uso) =>
        Dispatcher.UIThread.Post(() => _cpu.AgregarDatos(temperatura, uso));
    public void AgregarDatosGpu(double temperatura, double uso) =>
        Dispatcher.UIThread.Post(() => _gpu.AgregarDatos(temperatura, uso));
    public void InicializarGraficaCpu(AvaPlot Grafica, Border CpuTooltip, TextBlock CpuTextoTiempo, TextBlock CpuTextoTemperatura,  TextBlock CpuTextoUso)
    {
        _cpu = new GraficaMonitor(
            Grafica, "CPU",
            colorTemperatura: "#FF746C",
            colorUso: "#B3EBF2",
            CpuTooltip, CpuTextoTiempo, CpuTextoTemperatura, CpuTextoUso);
    }
    public void InicializarGraficaGpu(AvaPlot Grafica, Border GpuTooltip, TextBlock GpuTextoTiempo, TextBlock GpuTextoTemperatura,  TextBlock GpuTextoUso)
    {
        _gpu = new GraficaMonitor(
            Grafica, "GPU",
            colorTemperatura: "#FFB86C",
            colorUso: "#9CFF9C",
            GpuTooltip, GpuTextoTiempo, GpuTextoTemperatura, GpuTextoUso);
    }
    public void InicializarGraficaRam(AvaPlot Grafica)
    {
        _ram = new(Grafica);
    }
    public string EjecutarComando(string comando)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"" + comando.Replace("\"", "\\\"") + "\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process proceso = Process.Start(psi))
        {
            string salida = proceso.StandardOutput.ReadToEnd();
            string error = proceso.StandardError.ReadToEnd();

            proceso.WaitForExit();

            return proceso.ExitCode == 0 ? salida : error;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}