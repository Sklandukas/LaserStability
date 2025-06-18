using LaserStability.Helpers;
using LaserStability.Utility;
using System.Drawing;
using System.Text.Json;
using LaserStability.Measurements;

public class Settings
{
    public float PixelSizeUm { get; set; }
    public string InputFolderPath { get; set; }
}

class Program
{
    static void Main()
    {
        string settingsPath = "settings.json";
        string json = File.ReadAllText(settingsPath);
        Settings config = JsonSerializer.Deserialize<Settings>(json);

        string folderPath = config.InputFolderPath;
        float pixelSizeUm = config.PixelSizeUm;

        var ImagePaths = ImageLoader.GetImagePaths(folderPath);

        List<double> xList = new List<double>();
        List<double> yList = new List<double>();

        foreach (var path in ImagePaths)
        {
            Bitmap bmp = new Bitmap(path);

            var res = BeamProcessing.Start(bmp);

            double x = res.Item2;
            double y = res.Item3;

            xList.Add(x);
            yList.Add(y);

        }

        var (rmsShift, maxShift) = RmsCalculator.CalculateRms(xList, yList);

        Console.WriteLine("\n=== RMS stability analysis ===");
        Console.WriteLine($"RMS displacement: {rmsShift * pixelSizeUm:F3} µm");
    }
}
