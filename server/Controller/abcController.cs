using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using System.Collections.Specialized;
using System.Threading;
using System.CodeDom;
using server.Models;
using System.Security.Cryptography;
using System.Text;
using server.Cls;

namespace server.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class abcController : ApiController
    {
        [HttpGet]
        public AjaxResult sum(int a, int b)
        {
            try
            {
                return new AjaxResult(1, "OK", a + b);
            }
            catch (Exception ex)
            {
                return new AjaxResult(-1, ex.Message);
            }
        }

        [HttpPost]
        public AjaxResult validateTelegramUser(JObject pa)
        {
            main.Instance.outLog(pa.ToString());

            string myBotToken = Properties.Settings.Default.teleBot_token;
            var validator = new TelegramValidator(myBotToken);

            if (validator.ValidateTelegramData(pa.Value<string>("InitData"), out var userData))
            {
                // Hợp lệ
                // Bạn có thể đọc userData["user"] để lấy chuỗi JSON thông tin người dùng
                return new AjaxResult(1, "Validation successful.", userData);
            }
            else
            {
                // Không hợp lệ
                return new AjaxResult(0, "Không hợp lệ");
            }
        }
    }
}

