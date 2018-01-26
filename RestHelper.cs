using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using YouRock.DTO.Oanda;

namespace YouRock
{
    public class RestHelper
    {
        public class Oanda
        {
            public bool Practice { get; set; }
            public string AccessToken { get; set; }
            public int AccountID { get; set; }
            public string ApiURL { get; set; }
            private string ApiTradeServerURL = "https://api-fxtrade.oanda.com/";
            private string ApiPracticeServerURL = "https://api-fxpractice.oanda.com/";

            public Oanda(string accessToken, int accountID, bool practice)
            {
                AccessToken = accessToken;
                Practice = practice;
                AccountID = accountID;

                ApiURL = practice ? ApiPracticeServerURL : ApiTradeServerURL;
            }

            public List<DTO.Oanda.InstrumentDto> Instruments()
            {
                string requestString = "v1/instruments?accountId=" + AccountID;

                string responseString = MakeRequest(requestString);

                DTO.Oanda.InstrumentDto instrument = JsonConvert.DeserializeObject<DTO.Oanda.InstrumentDto>(responseString);
                

                return new List<InstrumentDto>();
            }

            public List<DTO.Oanda.InstrumentDto> Accounts()
            {
                string responseString = MakeRequest("v1/accounts");

                DTO.Oanda.InstrumentDto instrument = JsonConvert.DeserializeObject<DTO.Oanda.InstrumentDto>(responseString);
                
                return new List<InstrumentDto>();
            }

            public CandleDto.Root Candles(YouRock.DTO.Oanda.Enums.Parity instrument = Enums.Parity.EURUSD, DateTime? startDate = null, int count = 5000, YouRock.DTO.Oanda.Enums.Granularity granularity = Enums.Granularity.S5)
            {
                List<string> parameterList = new List<string>();
                string startDateStr = string.Empty;

                if (startDate != null)
                {
                    parameterList.Add("start=" + ((DateTime)startDate).ToString("yyyy") + "-" + ((DateTime)startDate).ToString("MM") + "-" + ((DateTime)startDate).ToString("dd") + "T" + ((DateTime)startDate).ToString("HH") + "%3A" + ((DateTime)startDate).ToString("mm") + "%3A" + ((DateTime)startDate).ToString("ss") + "Z");
                    //parameterList.Add("start=2017-01-14T10%3A10%3A10Z");
                }//'2017-01-14 05:55:00.000'

                parameterList.Add("count=" + count);
                parameterList.Add("granularity=" + granularity);
                parameterList.Add("instrument=" + instrument.DisplayName());

                string responseString = MakeRequest("v1/candles?" + string.Join("&", parameterList));

                return JsonConvert.DeserializeObject<DTO.Oanda.CandleDto.Root>(responseString);
            }

            private string MakeRequest(string requestString, string method = "GET", string postData = null)
            {
                HttpWebRequest request = WebRequest.CreateHttp(ApiURL + requestString);

                // for non-sandbox requests
                request.Headers.Add("Authorization", "Bearer " + AccessToken);


                request.Method = method;
                if (method == "POST")
                {
                    var data = Encoding.UTF8.GetBytes(postData);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseString = reader.ReadToEnd().Trim();

                        return responseString;
                    }
                }
            }
        }
    }
}