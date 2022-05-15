using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpectrumEngine.Client.Avalonia.Extensions
{
    /// <summary>
    /// Application extension methods
    /// </summary>
    public static class ApplicationExtensions
    {
        private static readonly Regex resourceNameRegex = new(@"^.*/I18n/Strings_(?<lang>\w{2}).xaml$");

        private static string AssemblyName => Assembly.GetExecutingAssembly()?.GetName().Name ?? throw new InvalidOperationException();

        /// <summary>
        /// Function for set current application language
        /// </summary>
        /// <param name="app">current application</param>
        /// <param name="locale">Locale for set language (in ISO Two-letters)</param>
        public static void SetCurrentLanguage(this Application app, string locale)
        {
            locale = locale ?? throw new ArgumentNullException(nameof(locale));

            locale = locale.ToLower();
            var resInclude = new ResourceInclude
            {
                Source = new Uri($"avares://{AssemblyName}/I18n/Strings_{locale}.xaml", UriKind.RelativeOrAbsolute)
            };

            var current = app.Resources.MergedDictionaries.OfType<ResourceInclude>()
                .FirstOrDefault(item => item.Source != null && resourceNameRegex.IsMatch(item.Source.OriginalString));
            if (current != null)
            {
                app.Resources.MergedDictionaries.Remove(current);
            }

            app.Resources.MergedDictionaries.Add(resInclude);
        }

        /// <summary>
        /// Function for get current application language
        /// </summary>
        /// <param name="app">current application</param>
        /// <returns>language (in ISO Two-letters)</returns>
        public static string GetCurrentLanguage(this Application app)
        {
            var current = app.Resources.MergedDictionaries.OfType<ResourceInclude>()
                .FirstOrDefault(item => item.Source != null && resourceNameRegex.IsMatch(item.Source.OriginalString));

            return current?.Source != null ? resourceNameRegex.Match(current.Source.OriginalString).Groups["lang"].Value : string.Empty;
        }

        /// <summary>
        /// Find resource value of T type by key
        /// </summary>
        /// <param name="app">current application</param>
        /// <param name="key">resource key</param>
        /// <returns>true if exists, false otherwise</returns>
        public static T FirstResource<T>(this Application app, string key)
        {
            var resourceValue = app.FindResource(key);
            return resourceValue is T value ? value : throw new KeyNotFoundException($"FindResource: {key}");
        }

        /// <summary>
        /// Try find resource value of T type by key
        /// </summary>
        /// <param name="app">current application</param>
        /// <param name="key">resource key</param>
        /// <param name="value">resource value if exists</param>
        /// <returns>true if exists, false otherwise</returns>
        public static bool TryFindResource<T>(this Application app, string key, out T? value)
        {
            var result = false;
            value = default;

            var resourceValue = app.FindResource(key);
            if (resourceValue is T typedResourceValue)
            {
                value = typedResourceValue;
                result = true;
            }

            return result;
        }
    }
}
