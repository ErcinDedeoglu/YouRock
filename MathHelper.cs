namespace YouRock
{
    public class MathHelper
    {
        public static decimal CalculatePercentage(int maxPoint, int minPoint, decimal input)
        {
            decimal result = 0;

            if (maxPoint > minPoint)
            {
                decimal middlePoint = ((decimal)maxPoint + minPoint) / 2;

                if (input < middlePoint)
                {
                    result = 100 * (middlePoint - input) / (maxPoint - middlePoint);
                }

                if (input > middlePoint)
                {
                    result = -100 * (input - middlePoint) / (maxPoint - middlePoint);
                }
            }

            return result;
        }
    }
}