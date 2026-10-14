using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

var path = @"C:\Temp\game-capture.png";
Console.WriteLine("exists=" + File.Exists(path) + " path=" + path);
var fileBytes = await File.ReadAllBytesAsync(path);
Console.WriteLine("bytes=" + fileBytes.Length);
using var ras = new InMemoryRandomAccessStream();
await ras.WriteAsync(fileBytes.AsBuffer());
ras.Seek(0);
var decoder = await BitmapDecoder.CreateAsync(ras);
var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
Console.WriteLine($"bitmap {bitmap.PixelWidth}x{bitmap.PixelHeight}");
var engine = OcrEngine.TryCreateFromUserProfileLanguages()
    ?? OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
Console.WriteLine("engine=" + engine?.RecognizerLanguage.LanguageTag);
if (engine == null)
    return;
var result = await engine.RecognizeAsync(bitmap);
Console.WriteLine("lines=" + result.Lines.Count);
foreach (var line in result.Lines)
{
    var first = line.Words[0].BoundingRect;
    var last = line.Words[^1].BoundingRect;
    var x = (int)first.X;
    var y = (int)first.Y;
    var w = (int)(last.X + last.Width - first.X);
    Console.WriteLine($"{x,4},{y,4} {w,3}x{(int)first.Height,-3} {line.Text}");
}
