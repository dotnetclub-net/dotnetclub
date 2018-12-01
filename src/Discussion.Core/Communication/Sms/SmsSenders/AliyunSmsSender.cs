using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Time;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Discussion.Core.Communication.Sms.SmsSenders
{
    public class AliyunSmsSender: ISmsSender
    {
        private readonly HttpMessageInvoker _httpInvoker;
        private readonly AliyunSmsOptions _aliyunSmsOptions;
        private readonly IClock _clock;

        public AliyunSmsSender(IOptions<AliyunSmsOptions> smsSendingOptions, HttpMessageInvoker httpInvoker, IClock clock)
        {
            _httpInvoker = httpInvoker;
            _clock = clock;
            _aliyunSmsOptions = smsSendingOptions.Value;
            _accessKeyId = _aliyunSmsOptions.AccountKeyId;
            _accessKeySecret = _aliyunSmsOptions.AccessKeySecret;
        }
        
        public async Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            var smsTemplateParams = JsonConvert.SerializeObject(new {code});
            var parameters = new Dictionary<string, string>
            {
                {"PhoneNumbers", phoneNumber},
                {"SignName", _aliyunSmsOptions.SmsServiceSignName},
                {"TemplateCode", _aliyunSmsOptions.SmsServiceTemplateCode},
                {"TemplateParam", smsTemplateParams},
                {"OutId", Guid.NewGuid().ToString()}
            };
            var signed = SignRequestParameters(parameters);
            
            var response = await InvokeAliyunSmsApi(signed);
            if (response.Code != "OK")
            {
                throw new AliyunSmsException(null, 
                    response.Code + " " + response.Message, 
                    response);
            }
        }

        private readonly string _accessKeyId;
        private readonly string _accessKeySecret;
        private const string AliSmsHost = "https://dysmsapi.aliyuncs.com/";


        async Task<AliyunSmsResponse> InvokeAliyunSmsApi(string signedParameters)
        {
            string responseContent = null;
            AliyunSmsResponse smsResponse = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, AliSmsHost)
                {
                    Content = new StringContent(signedParameters, Encoding.ASCII, "application/x-www-form-urlencoded")
                };
                request.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
                
                var response = await _httpInvoker.SendAsync(request, CancellationToken.None);
                responseContent = await response.Content.ReadAsStringAsync();
                smsResponse = JsonConvert.DeserializeObject<AliyunSmsResponse>(responseContent);
                return smsResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new AliyunSmsException(ex, responseContent, smsResponse);
            }
            catch (WebException ex)
            {
                try
                {
                    string resp;
                    using (var sr = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        resp = sr.ReadToEnd();
                    }

                    throw new AliyunSmsException(ex, resp, null);
                }
                catch
                {
                    throw new AliyunSmsException(ex, null, null);
                }
                    
            }
            catch (JsonException jsonException)
            {
                throw new AliyunSmsException(jsonException, responseContent, null);
            }
        }
        
        string SignRequestParameters(Dictionary<string, string> requestParameters)
        {
            var nonce = Guid.NewGuid().ToString("N").Substring(4, 8);
            var parameters = new Dictionary<string, string>
            {
                {"Format", "JSON"},
                {"Version", "2017-05-25"},
                {"SignatureMethod", "HMAC-SHA1"},
                {"SignatureVersion", "1.0"},
                {"AccessKeyId", _accessKeyId},
                {"SignatureNonce", nonce},
                {"Timestamp", _clock.Now.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")},
                
                {"Action", "SendSms"},
                {"RegionId", "cn-hangzhou"}
            };

            foreach (var key in requestParameters.Keys)
            {
                parameters.Add(key, requestParameters[key]);
            }

            var signature = GenerateSignature(_accessKeySecret, parameters);
            parameters.Add("Signature", signature);

            return string.Join("&", parameters.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
        }
    
        static string GenerateSignature(string accessKeySecret, Dictionary<string, string> parametersBeforeSign)
        {
            string Encode(string p)
            {
                return WebUtility.UrlEncode(p)
                                .Replace("+", "20%")
                                .Replace("*", "%2A")
                                .Replace("%7E", "~");
            }
            
            string Hmac(string val, string keySecret)
            {
                var bytes = Encoding.ASCII.GetBytes(val);
                var key = Encoding.ASCII.GetBytes(keySecret + "&");

                using (var hmacsha1 = new HMACSHA1(key))
                using (var stream = new MemoryStream(bytes))
                {
                    return Convert.ToBase64String(hmacsha1.ComputeHash(stream));
                }    
            }
                
            var encodedParameters = parametersBeforeSign
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => $"{Encode(kv.Key)}={Encode(kv.Value)}");
            
            var canonicalizedQueryString = string.Join("&", encodedParameters);
            var stringToSign = $"POST&%2F&{Encode(canonicalizedQueryString)}";
            return Hmac(stringToSign, accessKeySecret);
        }


        class AliyunSmsResponse
        {
            public string RequestId { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public string BizId { get; set; }
        }

        class AliyunSmsException: Exception
        {
            public AliyunSmsResponse ResponseObject { get; set; }
            public string ResponseContent { get; set; }
            
            public AliyunSmsException(Exception exception, 
                string response, AliyunSmsResponse responseObject)
                : base("Failed to request api with action SendSms with response " 
                       + (response ?? string.Empty),  exception)
            {
                this.ResponseContent = response;
                this.ResponseObject = responseObject;
            }
        }
    }

    public class AliyunSmsOptions
    {
        public string AccountKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string SmsServiceSignName { get; set; }
        public string SmsServiceTemplateCode { get; set; }
    }
}