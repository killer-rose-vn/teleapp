using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace server.Cls
{
    internal class H
    {
        /// <summary>
        /// Tạo một chuỗi ngẫu nhiên "giả lập" địa chỉ ví.
        /// CẢNH BÁO: Đây KHÔNG phải là một địa chỉ ví mã hóa thực tế và không có private key.
        /// Nó chỉ tạo một chuỗi ngẫu nhiên với các ký tự thường thấy trong địa chỉ ví (Base58).
        /// </summary>
        /// <param name="length">Độ dài của chuỗi địa chỉ cần tạo.</param>
        /// <returns>Một chuỗi ngẫu nhiên.</returns>
        public static string GenerateRandomWalletAddress(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Độ dài phải lớn hơn 0", nameof(length));
            }

            // Các ký tự thường dùng trong địa chỉ ví (Base58, loại bỏ 0, O, I, l)
            // để tránh nhầm lẫn cho người dùng.
            const string validChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ123456789";

            StringBuilder res = new StringBuilder(length);

            // Sử dụng RandomNumberGenerator để có độ ngẫu nhiên cao, an toàn cho mật mã.
            // Tốt hơn nhiều so với việc dùng 'new Random()'.
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                // Tạo một buffer để giữ số ngẫu nhiên
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (res.Length < length)
                {
                    rng.GetBytes(uintBuffer); // Lấy 4 byte ngẫu nhiên an toàn
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);

                    // Dùng số ngẫu nhiên này để chọn một ký tự
                    res.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }

            return res.ToString();
        }

        public static int GenerateRandomSecretNumber()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                // Tạo 4 byte (đủ cho 1 số nguyên 32-bit)
                byte[] byteBuffer = new byte[4];
                rng.GetBytes(byteBuffer);

                // Chuyển 4 byte thành số nguyên
                uint soNguyenNgauNhien = BitConverter.ToUInt32(byteBuffer, 0);

                // Dùng phép chia lấy dư (modulo) để đưa về 0-99
                // (Cách này thực ra có một chút "thiên vị" (bias) 
                // về mặt toán học, làm đúng 100% còn phức tạp hơn nữa)
                int soNgauNhien = (int)(soNguyenNgauNhien % 100);

                return soNgauNhien;
            }
        }
    }
}
