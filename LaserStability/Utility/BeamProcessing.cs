using System.Drawing;
using System.Drawing.Imaging;

public class BeamProcessing
{
    private readonly float _cameraPixelPeriod;

    public BeamProcessing()
    {
        _cameraPixelPeriod = 3.25f;
    }

    public Bitmap Start(Bitmap bitmap)
    { 
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite,
            bitmap.PixelFormat);

        return bitmap;
    }

}