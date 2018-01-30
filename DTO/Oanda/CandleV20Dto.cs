using System;
using System.Collections.Generic;

namespace YouRock.DTO.Oanda
{
    public class CandleV20Dto
    {
        public class Bid
        {
            public decimal o { get; set; }
            public decimal h { get; set; }
            public decimal l { get; set; }
            public decimal c { get; set; }
        }

        public class Mid
        {
            public decimal o { get; set; }
            public decimal h { get; set; }
            public decimal l { get; set; }
            public decimal c { get; set; }
        }

        public class Ask
        {
            public decimal o { get; set; }
            public decimal h { get; set; }
            public decimal l { get; set; }
            public decimal c { get; set; }
        }

        public class Candle
        {
            public bool complete { get; set; }
            public int volume { get; set; }
            public DateTime time { get; set; }
            public Bid bid { get; set; }
            public Mid mid { get; set; }
            public Ask ask { get; set; }
        }

        public class Root
        {
            public string instrument { get; set; }
            public string granularity { get; set; }
            public List<Candle> candles { get; set; }
        }
    }
}