using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YouRock.DTO.Oanda
{
    public class CandleDto
    {
        public class Candle
        {
            [JsonProperty(PropertyName = "time")]
            public DateTime Time { get; set; }
            [JsonProperty(PropertyName = "openBid")]
            public double OpenBid { get; set; }
            [JsonProperty(PropertyName = "openAsk")]
            public double OpenAsk { get; set; }
            [JsonProperty(PropertyName = "highBid")]
            public double HighBid { get; set; }
            [JsonProperty(PropertyName = "highAsk")]
            public double HighAsk { get; set; }
            [JsonProperty(PropertyName = "lowBid")]
            public double LowBid { get; set; }
            [JsonProperty(PropertyName = "lowAsk")]
            public double LowAsk { get; set; }
            [JsonProperty(PropertyName = "closeBid")]
            public double CloseBid { get; set; }
            [JsonProperty(PropertyName = "closeAsk")]
            public double CloseAsk { get; set; }
            [JsonProperty(PropertyName = "volume")]
            public int Volume { get; set; }
            [JsonProperty(PropertyName = "complete")]
            public bool Complete { get; set; }
        }

        public class Root
        {
            [JsonProperty(PropertyName = "instrument")]
            public string Instrument { get; set; }
            [JsonProperty(PropertyName = "granularity")]
            public string Granularity { get; set; }
            [JsonProperty(PropertyName = "candles")]
            public List<Candle> Candles { get; set; }
        }
    }
}
