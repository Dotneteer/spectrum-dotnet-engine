using Avalonia;
using Splat;
using System;

namespace SpectrumEngine.Client.Avalonia.Extensions
{
    /// <summary>
    /// ReadonlyDependencyResolver extension methods
    /// </summary>
    public static class ReadonlyDependencyResolver
    {
        /// <summary>
        /// Gets an required instance of the given <typeparamref name="T"/>. throw if not exists
        /// </summary>
        /// <typeparam name="T">The type for the object we want to retrieve.</typeparam>
        /// <param name="resolver">The resolver we are getting the service from.</param>
        /// <param name="contract">A optional value which will retrieve only a object registered with the same contract.</param>
        /// <returns>The requested object, if found; <c>null</c> otherwise.</returns>
        public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver, string? contract = null) =>
            resolver.GetService<T>() ?? throw new InvalidProgramException($"Service '{typeof(T)}' is not registered in Locator");
    }
}
