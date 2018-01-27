using System;
using System.Collections.Generic;
using System.Linq;
using YouRock.DTO.Oanda;

namespace YouRock
{
    public static class FinanceHelper
    {
        public static decimal ToStandardPrice(this decimal data)
        {
            return Convert.ToDecimal(data.ToString("######.#####"));
        }

        public static decimal SimpleMovingAverages(List<CandleDto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].Time.AddDays(-day);
            return candleList.Where(a=> a.Time > startDate).Select(a => a.CloseBid).Average().ToStandardPrice();
        }

        public static decimal WeightedMovingAverage(List<CandleDto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].Time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.Time > startDate).Select(a => a.CloseBid).ToList();
            decimal x = 0;
            int total = 0;
            for (int i = priceList.Count; i >= 1; i--)
            {
                total += i;
                x += (priceList[i-1] * i);
            }

            return (x / total).ToStandardPrice();
        }

        public static decimal ExponentialMovingAverage(List<CandleDto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].Time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.Time > startDate).Select(a => a.CloseBid).ToList();

            decimal smaX = SimpleMovingAverages(candleList, day);

            int k = 2 / (priceList.Count + 1);
            decimal ema = ((candleList[candleList.Count - 1].CloseBid - smaX) * k) + smaX;


            return ema.ToStandardPrice();
        }

        public static decimal StandardDeviation(List<CandleDto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].Time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.Time > startDate).Select(a => a.CloseBid).ToList();

            decimal smaX = SimpleMovingAverages(candleList, day);
            decimal total = 0;

            for (int i = 0; i < priceList.Count; i++)
            {
                total += (decimal) Math.Sqrt((double) (Math.Abs(priceList[i] - smaX)));
            }

            total = (decimal) Math.Sqrt((double)(total / (priceList.Count - 1)));

            return total.ToStandardPrice();
        }

        public static Tuple<decimal, decimal, decimal> BollingerBands(List<CandleDto.Candle> candleList, int day)
        {
            decimal standardDeviation = StandardDeviation(candleList, day);
            decimal middleBand = SimpleMovingAverages(candleList, day);
            decimal upperBand = middleBand + (standardDeviation * 2).ToStandardPrice();
            decimal lowerBand = middleBand - (standardDeviation * 2).ToStandardPrice();

            return new Tuple<decimal, decimal, decimal>(lowerBand, middleBand, upperBand);
        }

        /// <summary>
        /// 80 üzerinde olması fiyatların aşırı yükseldiğini ve satılması için uygun bir seviyede olduğunu belirtir.
        /// 20 altında olması fiyatların aşırı ucuzladığı ve satın alınması için uygun seviye olduğunu belirtir.
        /// </summary>
        /// <param name="candleList"></
        /// param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static Tuple<decimal, int> RSI(List<CandleDto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].Time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.Time > startDate).Select(a => a.CloseBid).ToList();

            decimal positiveSum = 0;
            decimal negativeSum = 0;
            decimal lastPrice = 0;

            for (int i = 0; i < priceList.Count; i++)
            {
                if (lastPrice != 0)
                {
                    decimal diff = lastPrice - priceList[i];
                    if (diff > 0)
                    {
                        positiveSum += diff;
                    }
                    else if (diff < 0)
                    {
                        negativeSum += Math.Abs(diff);
                    }
                }

                lastPrice = priceList[i];
            }


            decimal positiveAverage = positiveSum / priceList.Count;
            decimal negativeAverage = negativeSum / priceList.Count;
            decimal rs = positiveAverage / negativeAverage;
            decimal rsi = 100 - (100 / (1 + rs));

            return new Tuple<decimal, int>(rsi, (int) Math.Round(MathHelper.CalculatePercentage(80, 20, rsi)));
        }
    }
}