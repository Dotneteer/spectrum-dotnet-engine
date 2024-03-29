﻿using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.ReactiveUI;

namespace SpectrumEngine.Client.Avalonia
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // --- Prepare BASS resources to provide audio on Windows, Mac, and Linux
            var asm = Assembly.GetEntryAssembly();

            var targetFolder = Path.GetDirectoryName(asm!.Location);

            var (bassResource, targetFile) = (Environment.Is64BitOperatingSystem 
                ? "bass64.dll" : "bass32.dll", "bass.dll");
            if (Environment.OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
            {
                bassResource = targetFile = "libbass.dylib";
            }

            var bassDllName = $"{asm.GetName().Name}.BassResources.{bassResource}";
            var resStream = asm.GetManifestResourceStream(bassDllName);
            var targetName = Path.Combine(targetFolder!, targetFile);
            if (!File.Exists(targetName) && resStream != null)
            {
                resStream.Seek(0, SeekOrigin.Begin);
                using var fs = new FileStream(targetName, FileMode.OpenOrCreate);
                resStream.CopyTo(fs);
            }
            
            // --- Now, start the application
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}