namespace LaserStability.Centre
{
    public static class CalculateCentre
    {
        public static int CalculateCenterH(int measurementRangeX1, int measurementRangeX2, long[] heightConvolution)
        {
            for (int i = measurementRangeX1; i < measurementRangeX2; i++)
            {
                if (heightConvolution[measurementRangeX2 - 1] < 2 * heightConvolution[i])
                    return i - 1;
            }
            return 1;
        }



        public static int CalculateCenterW(int measurementRangeY1, int measurementRangeY2, long[] widthConvolution, int activePixelChannel)
        {
            int maxIndex = widthConvolution.Length - 1;

            for (int i = measurementRangeY1 / activePixelChannel; i < maxIndex; i++)
            {
                if (widthConvolution[maxIndex] >= 2 * widthConvolution[i])
                    continue;

                return i - 1;
            }

            return 1;
        }

        public static float CalculateCenterPointY(int centerH, long[] heightConvolution, int measurementRangeX2)
        {
            return centerH + (heightConvolution[measurementRangeX2 - 1] / 2 - heightConvolution[centerH]) /
                (float)(heightConvolution[centerH] - heightConvolution[centerH - 1]);
        }

        public static float CalculateCenterPointX(int centerW, long[] widthConvolution, int measurementRangeY2,
        int activePixelChannel)
        {
            int maxIndex = widthConvolution.Length - 1;

            if (centerW <= 0 || centerW >= widthConvolution.Length)
                return centerW;

            return centerW +
                   (widthConvolution[maxIndex] / 2 - widthConvolution[centerW]) /
                   (float)(widthConvolution[centerW] - widthConvolution[centerW - 1]);
        }
    }
}