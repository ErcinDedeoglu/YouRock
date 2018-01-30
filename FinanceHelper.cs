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
            if (data == 0) return 0;
            return Convert.ToDecimal(data.ToString("######.#####"));
        }

        public static decimal SimpleMovingAverages(List<CandleV20Dto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].time.AddDays(-day);
            return candleList.Where(a=> a.time > startDate).Select(a => a.bid.c).Average().ToStandardPrice();
        }

        public static decimal WeightedMovingAverage(List<CandleV20Dto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.time > startDate).Select(a => a.bid.c).ToList();
            decimal x = 0;
            int total = 0;
            for (int i = priceList.Count; i >= 1; i--)
            {
                total += i;
                x += (priceList[i-1] * i);
            }

            return (x / total).ToStandardPrice();
        }

        public static decimal ExponentialMovingAverage(List<CandleV20Dto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.time > startDate).Select(a => a.bid.c).ToList();

            decimal smaX = SimpleMovingAverages(candleList, day);

            int k = 2 / (priceList.Count + 1);
            decimal ema = ((candleList[candleList.Count - 1].bid.c - smaX) * k) + smaX;


            return ema.ToStandardPrice();
        }

        public static decimal StandardDeviation(List<CandleV20Dto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.time > startDate).Select(a => a.bid.c).ToList();

            decimal smaX = SimpleMovingAverages(candleList, day);
            decimal total = 0;

            for (int i = 0; i < priceList.Count; i++)
            {
                total += (decimal) Math.Sqrt((double) (Math.Abs(priceList[i] - smaX)));
            }

            if (total != 0)
            {
                total = (decimal)Math.Sqrt((double)(total / (priceList.Count - 1)));
            }

            return total.ToStandardPrice();
        }

        public static Tuple<decimal, decimal, decimal> BollingerBands(List<CandleV20Dto.Candle> candleList, int day)
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
        public static Tuple<decimal, int> RSI(List<CandleV20Dto.Candle> candleList, int day)
        {
            DateTime startDate = candleList[candleList.Count - 1].time.AddDays(-day);
            List<decimal> priceList = candleList.Where(a => a.time > startDate).Select(a => a.bid.c).ToList();

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

            if (positiveAverage == 0 || negativeAverage == 0) return new Tuple<decimal, int>(0, 0);
            decimal rsi = 100 - (100 / (1 + (positiveAverage / negativeAverage)));
            return new Tuple<decimal, int>(Convert.ToDecimal(rsi.ToString("######.##")), (int) Math.Round(MathHelper.CalculatePercentage(80, 20, rsi)));
        }
    }
}