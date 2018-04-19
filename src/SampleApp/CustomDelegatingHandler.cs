using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp
{
    public class CustomDelegatingHandler : DelegatingHandler
    {
        private readonly string _appId ; //  = "1e44c49a-c0e8-4cf5-8693-4cce668b5018";
        private readonly string _apiKey ; // = "o+pnJdWg8v/xF0TphTgu2qGbNo5McAmVUK4vD2JVbfo=";

        public CustomDelegatingHandler(string appId, string apiKey)
        {
            _appId = appId;
            _apiKey = apiKey;
        }
       
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            //create random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");

            //Creating the raw signature string
            string signatureRawData = $"{_appId}{requestTimeStamp}{nonce}";

            var secretKeyByteArray = Convert.FromBase64String(_apiKey);

            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                //Setting the values in the Authorization header using custom scheme (amx)
                request.Headers.Authorization = new AuthenticationHeaderValue("amx",
                    $"{_appId}:{requestSignatureBase64String}:{nonce}:{requestTimeStamp}");
            }

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}