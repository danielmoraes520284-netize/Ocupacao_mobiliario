using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BimBlaster.OcupacaoMobiliario;

public static class IconLoader
{
    public static BitmapImage Load(string relativePath)
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var uri = new Uri(Path.Combine(baseDir, relativePath));
        return new BitmapImage(uri);
    }
}

public static class StyleLoader
{
    public static ResourceDictionary LoadBimBlasterStyles()
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var path = Path.Combine(baseDir, "BimBlasterStyles.xaml");
        return new ResourceDictionary { Source = new Uri(path, UriKind.Absolute) };
    }
}
