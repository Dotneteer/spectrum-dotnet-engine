using Avalonia.Media.Imaging;
using Avalonia.Shared.PlatformSupport;
using System;

namespace SpectrumEngine.Client.Avalonia.Controls
{
    /// <summary>
    /// Control for allow define bitmaps in resource files
    /// </summary>
    public class BitmapResourcesControl : Bitmap
    {
        public BitmapResourcesControl(Uri uri) : base(new AssetLoader().Open(uri))
        {
        }
    }
}
