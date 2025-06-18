using System.Drawing;
using System.Drawing.Imaging;
using LaserStability.Background;
using LaserStability.Centre;

namespace LaserStability.Utility
{
    public class BeamProcessing
    {
        private readonly float _cameraPixelPeriod;
        private static bool _backgroundVisible = false;
        private static int previousBackgroundLevel = 0;

        private static long pixelIntensitySum = 0;
        private static long numberOfBrightPixels = 0;
        private static int[] width;
        private static int[] height;
        private static int maxWidthValue = 0;

        private static long[] heightConvolution;
        private static long[] widthConvolution;
        private const int ACTIVE_PIXEL_CHANNEL = 3;

        private static (float x, float y) centerPoint;

        public static unsafe (Bitmap, float, float) Start(Bitmap inputBitmap)
        {
            Bitmap bitmap;
            if (inputBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                bitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height, PixelFormat.Format24bppRgb);
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    gr.DrawImage(inputBitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                }
            }
            else
            {
                bitmap = (Bitmap)inputBitmap.Clone();
            }

            int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmap.Height;
            int widthInPixels = bitmap.Width;
            int widthInBytes = widthInPixels * bytesPerPixel;

            int measurementRangeX1 = 0;
            int measurementRangeX2 = heightInPixels;
            int measurementRangeY1 = 0;
            int measurementRangeY2 = widthInBytes;

            width = new int[widthInPixels];
            height = new int[heightInPixels];
            heightConvolution = new long[heightInPixels];
            widthConvolution = new long[widthInPixels];

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat);

            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

            long[] histogram = GetHistogram(ptrFirstPixel, heightInPixels, widthInBytes, bytesPerPixel);

            long totalPixels = heightInPixels * widthInPixels;
            int otsuThreshold = BackgroundTreshold.CalculateTreshold(histogram, totalPixels);

            Parallel.For(measurementRangeX1, measurementRangeX2, y =>
            {
                int localPixelIntensitySum = 0;
                int localNumberOfBrightPixels = 0;
                int maxPixelValueInWidth = 0;
                int[] localWidth = new int[widthInPixels];
                int[] localHeight = new int[heightInPixels];

                try
                {
                    byte* currentLine = ptrFirstPixel + y * bitmapData.Stride;

                    for (int x = measurementRangeY1; x < measurementRangeY2; x += bytesPerPixel)
                    {
                        int pixelValue = currentLine[x];

                        if (_backgroundVisible)
                            pixelValue = BackgroundTreshold.RemoveAdaptiveBackgroundLevelFromPixel(x / bytesPerPixel, y, ptrFirstPixel, widthInPixels, heightInPixels, 5);

                        if (pixelValue > otsuThreshold)
                        {
                            localPixelIntensitySum += pixelValue;
                            localNumberOfBrightPixels++;
                        }

                        int pixelIndex = x / bytesPerPixel;
                        localWidth[pixelIndex] += pixelValue;

                        if (localWidth[pixelIndex] > maxPixelValueInWidth)
                            maxPixelValueInWidth = localWidth[pixelIndex];

                        localHeight[y] += pixelValue;
                    }

                    Interlocked.Add(ref pixelIntensitySum, localPixelIntensitySum);
                    Interlocked.Add(ref numberOfBrightPixels, localNumberOfBrightPixels);

                    for (int i = 0; i < localWidth.Length; i++)
                        Interlocked.Add(ref width[i], localWidth[i]);

                    if (maxPixelValueInWidth > maxWidthValue)
                        Interlocked.Exchange(ref maxWidthValue, maxPixelValueInWidth);

                    Interlocked.Add(ref height[y], localHeight[y]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            Calculate2DConvolution(ref heightConvolution, ref widthConvolution, height, width,
                measurementRangeX1, measurementRangeX2, measurementRangeY1, measurementRangeY2, ACTIVE_PIXEL_CHANNEL);

            int centerH = CalculateCentre.CalculateCenterH(measurementRangeX1, measurementRangeX2, heightConvolution);
            centerPoint.y = CalculateCentre.CalculateCenterPointY(centerH, heightConvolution, measurementRangeX2);

            int centerW = CalculateCentre.CalculateCenterW(measurementRangeY1, measurementRangeY2, widthConvolution, ACTIVE_PIXEL_CHANNEL);
            centerPoint.x = CalculateCentre.CalculateCenterPointX(centerW, widthConvolution, measurementRangeY2, ACTIVE_PIXEL_CHANNEL);

            bitmap.UnlockBits(bitmapData);

            return (bitmap, centerPoint.x, centerPoint.y);
        }

        private static unsafe long[] GetHistogram(byte* ptrFirstPixel, int heightInPixels, int widthInBytes, int bytesPerPixel)
        {
            var histogram = new long[256];

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = ptrFirstPixel + y * widthInBytes;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    int value = currentLine[x];
                    if (value is > 0 and < 255)
                    {
                        Interlocked.Increment(ref histogram[value]);
                    }
                }
            });

            return histogram;
        }

        private static void Calculate2DConvolution(ref long[] heightConv, ref long[] widthConv,
            int[] heightArr, int[] widthArr,
            int x1, int x2, int y1, int y2, int pixelChannel)
        {
            int heightLength = Math.Min(x2, heightConv.Length);
            for (int i = x1 + 1; i < heightLength; i++)
                heightConv[i] = heightConv[i - 1] + heightArr[i];

            int widthMax = Math.Min(y2 / pixelChannel, widthConv.Length);
            for (int i = y1 / pixelChannel + 1; i < widthMax; i++)
                widthConv[i] = widthConv[i - 1] + widthArr[i];
        }

    }
}
