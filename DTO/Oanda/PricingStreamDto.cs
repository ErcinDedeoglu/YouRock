using System;
using System.Collections.Generic;

namespace YouRock.DTO.Oanda
{
    public class PricingStreamDto
    {
        public class Bid
        {
            public string price { get; set; }
            public int liquidity { get; set; }
        }

        public class Ask
        {
            public string price { get; set; }
            public int liquidity { get; set; }
        }

        public class Root
        {
            public string type { get; set; }
            public DateTime time { get; set; }
            public List<Bid> bids { get; set; }
            public List<Ask> asks { get; set; }
            public decimal closeoutBid { get; set; }
            public decimal closeoutAsk { get; set; }
            public string status { get; set; }
            public bool tradeable { get; set; }
            public string instrument { get; set; }
        }
    }
}