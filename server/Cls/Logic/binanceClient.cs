using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace server.Cls
{
    internal class binanceClient
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.binance.com";



        public enum network_enum { ETH, BSC, TRX, MATIC, XRP }



        public binanceClient(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", _apiKey);
        }

        /// <summary>
        /// Lấy địa chỉ nạp tiền cho một coin và mạng lưới cụ thể (phiên bản đồng bộ).
        /// </summary>
        public AjaxResult GetDepositAddress(string coin = "USDT", network_enum network = network_enum.ETH)
        {
            try
            {
                var endpoint = "/sapi/v1/capital/deposit/address";
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var parameters = $"coin={coin}&timestamp={timestamp}&network={network}";

                var signature = CreateSignature(parameters);
                var requestUrl = $"{ApiUrl}{endpoint}?{parameters}&signature={signature}";

                // Sử dụng .Result để thực hiện cuộc gọi một cách đồng bộ (blocking)
                var response = _httpClient.GetAsync(requestUrl).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Lỗi từ API Binance: {responseString}");
                }

                var depositAddress = JsonConvert.DeserializeObject<DepositAddress>(responseString);
                depositAddress.Network = network;
                return new AjaxResult(1, "Ok", depositAddress);
            }
            catch (Exception ex)
            {
                return new AjaxResult() { stt = -1, msg = ex.Message };
            }
        }

        public AjaxResult GetDepositHistory(string coin = "USDT")
        {
            var endpoint = "/sapi/v1/capital/deposit/hisrec";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Tạo chuỗi tham số để tạo chữ ký
            var parameters = $"coin={coin}&timestamp={timestamp}";
            var signature = CreateSignature(parameters);

            var requestUrl = $"{ApiUrl}{endpoint}?{parameters}&signature={signature}";

            var response = _httpClient.GetAsync(requestUrl).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                // Ném lỗi nếu API trả về lỗi
                throw new Exception($"Lỗi từ API Binance: {responseString}");
            }

            var lst = new List<DepositHistoryItem>();
            foreach (JObject it in JArray.Parse(responseString))
            {
                if (it.Value<int>("status") != 1) //chỉ load success
                    continue;
                if (("" + it.Value<string>("coin")).ToUpper() != "USDT") //chỉ load "coin": "USDT"
                    continue;

                var transaction = new DepositHistoryItem()
                {
                    txId = "" + it.Value<string>("txId"),
                    amount = it.Value<double>("amount"),
                    //user = data,

                    address =     it.Value<string>("address"),
                    addressTag =   it.Value<string>("addressTag"),
                    network =   it.Value<string>("network"),

                    finishTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1).AddMilliseconds(it.Value<long>("insertTime"))),
                    transferType = "" + it.Value<string>("transferType"),
                };

                lst.Add(transaction);
            }

            return new AjaxResult(1, "Ok", lst);
        }




        // --- HÀM RÚT TIỀN (ĐỒNG BỘ) ---
        public AjaxResult Withdraw(network_enum network, string address, decimal amount, string coin = "USDT")
        {
            try
            {
                var endpoint = "/sapi/v1/capital/withdraw/apply";
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var parameters = new Dictionary<string, string>
                {
                    { "coin", coin },
                    { "address", address },
                    { "amount", amount.ToString(CultureInfo.InvariantCulture) },
                    { "network", network.ToString() },
                    { "timestamp", timestamp.ToString() }
                };

                var queryString = string.Join("&", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
                var signature = CreateSignature(queryString);
                parameters.Add("signature", signature);

                var content = new FormUrlEncodedContent(parameters);
                var requestUrl = $"{ApiUrl}{endpoint}";

                var response = _httpClient.PostAsync(requestUrl, content).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Lỗi từ API Binance: {responseString}");

                return new AjaxResult(1, "Thực hiện toàn tất: " + responseString, JsonConvert.DeserializeObject<WithdrawalResponse>(responseString));
            }
            catch (Exception ex)
            {
                return new AjaxResult() { stt = -1, msg = ex.Message };
            }
        }

        // --- HÀM LẤY LỊCH SỬ RÚT TIỀN (ĐỒNG BỘ) ---
        public List<WithdrawalHistoryItem> GetWithdrawalHistory(string coin = "USDT")
        {
            var endpoint = "/sapi/v1/capital/withdraw/history";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var parameters = $"coin={coin}&timestamp={timestamp}";
            var signature = CreateSignature(parameters);
            var requestUrl = $"{ApiUrl}{endpoint}?{parameters}&signature={signature}";

            var response = _httpClient.GetAsync(requestUrl).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode) throw new Exception($"Lỗi từ API Binance: {responseString}");
            return JsonConvert.DeserializeObject<List<WithdrawalHistoryItem>>(responseString);
        }


































        public class DepositHistoryItem
        {
            public string txId {  get; set; }
            public double amount { get; set; }
            public DateTime finishTime { get; set; }
            public string transferType {  get; set; }
            public string network {  get; set; }
            public string addressTag {  get; set; }
            public string address {  get; set; }
        }

        private string CreateSignature(string message)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_apiSecret);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hash = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public class DepositAddress
        {
            [JsonProperty("coin")]
            public string Coin { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("tag")]
            public string Tag { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            public network_enum Network { get; set; }
        }

        // --- CÁC LỚP DỮ LIỆU (DATA MODELS) ---
        public class WithdrawalResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class WithdrawalHistoryItem
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("amount")]
            public string Amount { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("coin")]
            public string Coin { get; set; }

            [JsonProperty("txId")]
            public string TxId { get; set; } // ID giao dịch trên blockchain

            [JsonProperty("network")]
            public string Network { get; set; }

            [JsonProperty("status")]
            public int Status { get; set; } // Mã trạng thái

            // Helper để chuyển mã trạng thái sang text dễ hiểu
            public string StatusText
            {
                get
                {
                    switch (Status)
                    {
                        case 0: return "Email Sent";
                        case 1: return "Cancelled";
                        case 2: return "Awaiting Approval";
                        case 3: return "Rejected";
                        case 4: return "Processing";
                        case 5: return "Failure";
                        case 6: return "Completed";
                        default: return "Unknown Status";
                    }
                }
            }
        }
    }
}
