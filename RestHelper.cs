using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YouRock.DTO.Oanda;

namespace YouRock
{
    public class RestHelper
    {
        public class Oanda
        {
            public class V1
            {
                public bool Practice { get; set; }
                public string AccessToken { get; set; }
                public string AccountID { get; set; }
                public string ApiURL { get; set; }
                private string ApiTradeServerURL = "https://api-fxtrade.oanda.com/";
                private string ApiPracticeServerURL = "https://api-fxpractice.oanda.com/";

                public V1(string accessToken, string accountID, bool practice)
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

                public CandleDto.Root Candles(YouRock.DTO.Oanda.Enums.Parity instrument = Enums.Parity.EURUSD, DateTime? startDate = null, DateTime? endDate = null, int? count = null, YouRock.DTO.Oanda.Enums.Granularity granularity = Enums.Granularity.S5)
                {
                    DTO.Oanda.CandleDto.Root result = new CandleDto.Root()
                    {
                        Candles = new List<CandleDto.Candle>()
                    };

                    List<string> parameterList = new List<string>();

                    if (startDate != null)
                    {
                        parameterList.Add("start=" + ((DateTime)startDate).ToString("yyyy") + "-" + ((DateTime)startDate).ToString("MM") + "-" + ((DateTime)startDate).ToString("dd") + "T" + ((DateTime)startDate).ToString("HH") + "%3A" + ((DateTime)startDate).ToString("mm") + "%3A" + ((DateTime)startDate).ToString("ss") + "Z");
                    }

                    if (endDate != null)
                    {
                        parameterList.Add("end=" + ((DateTime)endDate).ToString("yyyy") + "-" + ((DateTime)endDate).ToString("MM") + "-" + ((DateTime)endDate).ToString("dd") + "T" + ((DateTime)endDate).ToString("HH") + "%3A" + ((DateTime)endDate).ToString("mm") + "%3A" + ((DateTime)endDate).ToString("ss") + "Z");
                    }

                    if (count != null)
                    {
                        parameterList.Add("count=" + count);
                    }

                    parameterList.Add("granularity=" + granularity);
                    parameterList.Add("instrument=" + instrument.DisplayName());

                    string responseString = MakeRequest("v1/candles?" + string.Join("&", parameterList));
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        return JsonConvert.DeserializeObject<DTO.Oanda.CandleDto.Root>(responseString);
                    }
                    else
                    {
                        return result;
                    }
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

            public class V20
            {
                public bool Practice { get; set; }
                public string AccessToken { get; set; }
                public string AccountID { get; set; }
                public string ApiURL { get; set; }
                private string ApiTradeServerURL = "https://api-fxtrade.oanda.com/";
                private string ApiPracticeServerURL = "https://api-fxpractice.oanda.com/";
                private static DateTime mLastRequestTime = DateTime.UtcNow;

                public V20(string accessToken, string accountID, bool practice)
                {
                    AccessToken = accessToken;
                    Practice = practice;
                    AccountID = accountID;

                    ApiURL = practice ? ApiPracticeServerURL : ApiTradeServerURL;
                    CreateOrder();

                    //MakeRequest("/v3/accounts/" + AccountID + "/orders", "POST", "{\"order\": {\"units\": \"1\", \"instrument\": \"EUR_USD\", \"timeInForce\": \"FOK\", \"type\": \"MARKET\", \"positionFill\": \"DEFAULT\"}}");
                }

                public void CreateOrder(Enums.Parity parity, Enums.OrderDirection orderDirection, int unit = 1)
                {


                    string x = "v3/accounts/" + AccountID + "/orders";

                    object dd = new
                    {
                        order = new
                        {
                            type = "MARKET",
                            instrument = "EUR_USD",
                            units = "1",
                            timeInForce = "FOK",
                            takeProfitOnFill = new
                            {
                                price = "1.25000"
                            },
                            stopLossOnFill = new
                            {
                                price = "1.20000"
                            }
                        }
                    };

                    var s = MakeRequest(x);
                }

                private string MakeRequest(string request, string data)
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ApiURL + request);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToken);
                    httpWebRequest.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";

                    using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = JsonConvert.SerializeObject(data);
                        streamWriter.Write(json);
                    }

                    HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string result = streamReader.ReadToEnd();
                    }

                    return null;
                }

                //private string MakeRequest(string requestString, string method = "GET", Dictionary<string, string> requestParams = null)
                //{
                //    if (requestParams != null && requestParams.Count > 0)
                //    {
                //        string parameters = CreateParamString(requestParams);
                //        requestString = requestString + "?" + parameters;
                //    }
                //    HttpWebRequest request = WebRequest.CreateHttp(ApiURL + requestString);
                //    request.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessToken;
                //    request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                //    request.Method = method;
                //    request.ContentType = "application/json";

                //    return WebResponse(request);
                //}

                protected string CreateParamString(Dictionary<string, string> requestParams)
                {
                    string requestBody = "";
                    foreach (KeyValuePair<string, string> pair in requestParams)
                    {
                        requestBody += WebUtility.UrlEncode(pair.Key) + "=" + WebUtility.UrlEncode(pair.Value) + "&";
                    }
                    requestBody = requestBody.Trim('&');
                    return requestBody;
                }

                private string WebResponse(HttpWebRequest request)
                {
                    string result = null;

                    while (DateTime.UtcNow < mLastRequestTime.AddMilliseconds(501))
                    {
                        // speed bump
                        // http://developer.oanda.com/rest-live-v20/best-practices/
                    }

                    try
                    {
                        using (WebResponse response = request.GetResponse())
                        {
                            Stream stream = response.GetResponseStream();

                            if (response.Headers["Content-Encoding"] == "gzip")
                            {  // if we received a gzipped response, handle that
                                stream = new GZipStream(stream, CompressionMode.Decompress);
                            }
                            else if (response.Headers["Content-Encoding"] == "deflate")
                            {  // if we received a deflated response, handle that
                                stream = new DeflateStream(stream, CompressionMode.Decompress);
                            }

                            StreamReader reader = new StreamReader(stream);
                            result = reader.ReadToEnd();
                        }
                    }
                    catch (WebException ex)
                    {
                    }

                    mLastRequestTime = DateTime.UtcNow;

                    return result;
                }
            }
        }
    }
}