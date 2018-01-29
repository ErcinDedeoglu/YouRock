using System.ComponentModel.DataAnnotations;

namespace YouRock.DTO.Oanda
{
    public class Enums
    {
        public enum Parity
        {
            [Display(Name = "EUR_USD")]
            EURUSD = 1,

            [Display(Name = "GBP_USD")]
            GBPUSD = 2,

            [Display(Name = "USD_JPY")]
            USDJPY = 3,

            [Display(Name = "USD_CHF")]
            USDCHF = 4,

            [Display(Name = "USD_CAD")]
            USDCAD = 5,

            [Display(Name = "AUD_USD")]
            AUDUSD = 6,
        }

        public enum OrderDirection
        {
            Long = 1,
            Short = 2
        }


        public enum Granularity
        {
            S5 = 1,
            S10 = 2,
            S15 = 3,
            S30 = 4,
            M1 = 5,
            M2 = 6,
            M3 = 7,
            M4 = 8,
            M5 = 9,
            M10 = 10,
            M15 = 11,
            M30 = 12,
            H1 = 13,
            H2 = 14,
            H3 = 15,
            H4 = 16,
            H6 = 17,
            H8 = 18,
            H12 = 19,
            D = 20,
            W = 21,
            M = 22
        }
    }
}