using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using Serilog;
using Splat;
using Splat.Serilog;

namespace Pathfinding;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, 0); // hide console window

        var listener = new SerilogTraceListener.SerilogTraceListener();
        Trace.Listeners.Add(listener);
        Locator.CurrentMutable.UseSerilogFullLogger();
        
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        var app = BuildAvaloniaApp();
        app.StartWithClassicDesktopLifetime(args);
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    public static AppBuilder BuildAvaloniaApp()
    {
        var app = AppBuilder.Configure<App>();
        app.UsePlatformDetect();
        app.LogToTrace(LogEventLevel.Information);
        app.UseReactiveUI();
        
        return app;
    }
}