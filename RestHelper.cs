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
                string requestString = ApiURL + "v1/instruments?accountId=" + AccountID;

                string responseString = MakeRequest(requestString);

                DTO.Oanda.InstrumentDto instrument = JsonConvert.DeserializeObject<DTO.Oanda.InstrumentDto>(responseString);
                

                return new List<InstrumentDto>();
            }

            public List<DTO.Oanda.InstrumentDto> Accounts()
            {
                string responseString = MakeRequest(ApiURL + "v1/accounts");

                DTO.Oanda.InstrumentDto instrument = JsonConvert.DeserializeObject<DTO.Oanda.InstrumentDto>(responseString);
                
                return new List<InstrumentDto>();
            }

            //

            private string MakeRequest(string requestString, string method = "GET", string postData = null)
            {
                HttpWebRequest request = WebRequest.CreateHttp(requestString);

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