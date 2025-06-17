using System.Reflection;
using System.Runtime.InteropServices;
using LaserStability.Helpers;
using System.Drawing;  

class Program
{
    static void Main(string[] args)
    {
        string folderPath = "C:/Users/andro/Desktop/LC/bin_and_bmp_position_images/bmp";
        var ImagePaths = ImageLoader.GetImagePaths(folderPath);

        foreach (var path in ImagePaths)
        {
            Console.WriteLine("Path: " + path);
            Bitmap bmp = new Bitmap(path);
            Console.WriteLine("BMP: " + bmp);

        }
    }
}