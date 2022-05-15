using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SpectrumEngine.Client.Avalonia.ViewModels;
using Splat;

namespace SpectrumEngine.Client.Avalonia
{
    public class ViewLocator : IDataTemplate
    {
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public IControl? Build(object? data)
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            if (data is null)
                return null;

            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            
            return new TextBlock { Text = name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}