namespace LaserStability.Measurements
{
    public static class RmsCalculator
    {
        public static (double rms, double max) CalculateRms(List<double> xList, List<double> yList)
        {
            double avgX = xList.Average();
            double avgY = yList.Average();

            List<double> shifts = new List<double>();

            for (int i = 0; i < xList.Count; i++)
            {
                double dx = xList[i] - avgX;
                double dy = yList[i] - avgY;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                shifts.Add(distance);
            }

            double sumSq = shifts.Sum(s => s * s);
            double rmsShift = Math.Sqrt(sumSq / shifts.Count);
            double maxShift = shifts.Max();

            return (rmsShift, maxShift);
        }
    }
}
