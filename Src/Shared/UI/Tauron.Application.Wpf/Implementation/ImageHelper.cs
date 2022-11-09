using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NLog;

namespace Tauron.Application.Wpf.Implementation;

public class ImageHelper : IImageHelper
{
    private readonly WeakReferenceCollection<KeyedImage> _cache = new();

    private readonly IPackUriHelper _packUriHelper;

    public ImageHelper(IPackUriHelper packUriHelper) => _packUriHelper = packUriHelper;

    public ImageSource? Convert(Uri target, string assembly)
    {
        KeyedImage? source = _cache.FirstOrDefault(img => img.Key == target);
        ImageSource? temp = source?.GetImage();

        if(temp != null) return temp;

        bool flag = target.IsAbsoluteUri && target.Scheme == Uri.UriSchemeFile && File.Exists(target.OriginalString);
        if(!flag) flag = target.IsAbsoluteUri;

        if(!flag) flag = File.Exists(target.OriginalString);

        if(flag)
        {
            ImageSource imgSource = BitmapFrame.Create(target);
            _cache.Add(new KeyedImage(target, imgSource));

            return imgSource;
        }

        try
        {
            return BitmapFrame.Create(_packUriHelper.LoadStream(target.OriginalString, assembly));
        }
        catch (Exception e)
        {
            LogManager.GetCurrentClassLogger().Warn(e, "Faild To CreateResult image");

            return null;
        }
    }

    public ImageSource? Convert(string uri, string assembly)
        => Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out Uri? target) ? Convert(target, assembly) : null;

    private class KeyedImage : IWeakReference
    {
        private readonly WeakReference _source;

        internal KeyedImage(Uri key, ImageSource source)
        {
            Key = key;
            _source = new WeakReference(source);
        }

        internal Uri Key { get; }

        public bool IsAlive => _source.IsAlive;

        internal ImageSource? GetImage() => _source.Target as ImageSource;
    }
}