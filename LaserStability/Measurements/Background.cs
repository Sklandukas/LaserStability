namespace LaserStability.Background
{
    public static class BackgroundTreshold
    {
        public static int CalculateTreshold(long[] histogram, long totalPixels)
        {
            long sum = 0;
            for (int t = 0; t < 256; t++)
            {
                sum += t * histogram[t];
            }

            long sumB = 0;
            long wB = 0, wF = 0;
            long maxVar = 0;
            int threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += histogram[t];
                if (wB == 0)
                    continue;

                wF = totalPixels - wB;
                if (wF == 0)
                    break;

                sumB += t * histogram[t];
                long sumF = sum - sumB;

                long mB = sumB / wB;
                long mF = sumF / wF;
                long betweenVar = wB * wF * (mB - mF) * (mB - mF);

                if (betweenVar > maxVar)
                {
                    maxVar = betweenVar;
                    threshold = t;
                }
            }

            return threshold;
        }
        
        public static unsafe int RemoveAdaptiveBackgroundLevelFromPixel(int x, int y, byte* ptrFirstPixel, int imageWidth, int imageHeight, int windowSize)
        {
            int startX = Math.Max(0, x - windowSize);
            int endX = Math.Min(imageWidth - 1, x + windowSize);
            int startY = Math.Max(0, y - windowSize);
            int endY = Math.Min(imageHeight - 1, y + windowSize);

            int pixelCount = 0;
            long pixelSum = 0;

            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    int pixelIndex = j * imageWidth + i;
                    pixelSum += ptrFirstPixel[pixelIndex];
                    pixelCount++;
                }
            }

            int averageBackground = (int)(pixelSum / pixelCount);

            int pixelValue = ptrFirstPixel[y * imageWidth + x];
            return Math.Max(0, pixelValue - averageBackground);
        }
    }
}