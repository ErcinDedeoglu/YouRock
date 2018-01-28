using System;

namespace YouRock
{
    public class MathHelper
    {
        public static decimal CalculatePercentage(decimal maxPoint, decimal minPoint, decimal input)
        {
            decimal result = 0;
            
            decimal middlePoint = (maxPoint + minPoint) / 2;

            if (input < middlePoint)
            {
                result = 100 * (middlePoint - input) / (maxPoint - middlePoint);
            }

            if (input > middlePoint)
            {
                result = -100 * (input - middlePoint) / (maxPoint - middlePoint);
            }

            return Math.Round(result);
        }
    }
}