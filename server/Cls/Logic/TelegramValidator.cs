using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace server.Cls
{
    internal class TelegramValidator
    {
        private string botToken;

        public TelegramValidator(string botToken)
        {
            this.botToken = botToken;
        }

        public bool ValidateTelegramData(string initData, out NameValueCollection userData)
        {
            // 1. Phân tích chuỗi query bằng HttpUtility
            var parsedQuery = HttpUtility.ParseQueryString(initData);
            userData = parsedQuery; // Trả về dữ liệu đã parse

            // 2. Lấy hash nhận được
            string receivedHash = parsedQuery["hash"];
            if (string.IsNullOrEmpty(receivedHash))
            {
                return false;
            }

            // 3. Loại bỏ hash để chuẩn bị chuỗi kiểm tra
            parsedQuery.Remove("hash");

            // 4. Lấy tất cả các key, sắp xếp và tạo chuỗi data-check-string
            // NameValueCollection trả về 'AllKeys'
            var sortedKeys = parsedQuery.AllKeys.OrderBy(k => k);
            var checkPairs = sortedKeys.Select(k => $"{k}={parsedQuery[k]}");
            var dataCheckString = string.Join("\n", checkPairs);

            // --- 5. Logic mã hóa HMACSHA256 (Giữ nguyên y hệt như code .NET Core) ---
            var botTokenBytes = Encoding.UTF8.GetBytes(this.botToken);
            var keyData = Encoding.UTF8.GetBytes("WebAppData");
            using (var hmacSecret = new HMACSHA256(botTokenBytes))
            {
                var secretKey = hmacSecret.ComputeHash(keyData);

                var dataCheckBytes = Encoding.UTF8.GetBytes(dataCheckString);
                using (var hmacCheck = new HMACSHA256(secretKey))
                {
                    var computedHashBytes = hmacCheck.ComputeHash(dataCheckBytes);

                    // 6. Chuyển sang hex string (Dùng BitConverter, hoạt động trên mọi phiên bản)
                    var computedHashHex = BitConverter.ToString(computedHashBytes).Replace("-", "").ToLower();

                    // 7. So sánh
                    return string.Equals(computedHashHex, receivedHash, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }

}
