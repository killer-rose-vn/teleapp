using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace server.Cls
{
    public class LogRequestAndResponseHandler : DelegatingHandler, IDisposable
    {
        private readonly ILogger _logger;
        bool _isDisposed = false;


        public LogRequestAndResponseHandler()
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", $"main-.log");

            // 2. Tạo một cấu hình logger MỚI tại thời điểm runtime.
            // Đây KHÔNG phải là cấu hình tĩnh/toàn cục.
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithProperty("TaskId", "main") // (Tùy chọn) Gắn tag TaskId vào mọi log entry
                .WriteTo.File(logFilePath) // Ghi ra file DÀNH RIÊNG cho đối tượng này
                .CreateLogger(); // Tạo ra instance logger

            _logger.Information("--- ProcessWorker đã được KHỞI TẠO ---");
        }
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _logger.Information("--- ProcessWorker đang được GIẢI PHÓNG (Dispose) ---");

                // Đây là bước quan trọng nhất:
                // Biến đổi ILogger (interface) thành Logger (class cụ thể) để gọi CloseAndFlush
                // Nếu bạn không làm điều này, log cuối cùng có thể không được ghi vào file!
                if (_logger is Logger loggerInstance)
                {
                    loggerInstance.Dispose(); // Dispose sẽ tự động gọi CloseAndFlush
                }
            }

            _isDisposed = true;
        }
        // (Tùy chọn) Finalizer phòng trường hợp quên gọi Dispose()
        ~LogRequestAndResponseHandler()
        {
            Dispose(false);
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (!request.RequestUri.AbsolutePath.StartsWith("/api/"))
                return await base.SendAsync(request, cancellationToken);

            string msg = "";
            //if (Cfg.D.Setting_allowLogFileRequest)
            {
                // log request body
                string requestBody = await request.Content.ReadAsStringAsync();
                msg = string.Format("{0:dd/MM/yyyy HH:mm:ss} | {1,15} -> {2} :: {3}", DateTime.Now, GetClientIp(request), request.RequestUri.PathAndQuery, requestBody);
            }

            // let other handlers process the request
            var result = await base.SendAsync(request, cancellationToken);

            //if (Cfg.D.Setting_allowLogFileRequest)
            {
                string responseBody = "No-Response";
                if (result.Content != null)
                {
                    responseBody = await result.Content.ReadAsStringAsync();
                }
                msg += string.Format("\r\n        └► {0:HH:mm:ss}                   -> {1}", DateTime.Now, responseBody);

                _logger.Information(msg);
            }
            return result;
        }

        static string GetClientIp(HttpRequestMessage request)
        {
            string ip = "";

            try
            {
                // Web-hosting
                if (request.Properties.ContainsKey("HttpContext"))
                {
                    HttpContextWrapper ctx = (HttpContextWrapper)request.Properties["HttpContext"];
                    if (ctx != null)
                        ip = ctx.Request.UserHostAddress;
                }

                if (string.IsNullOrWhiteSpace(ip))
                {
                    // Self-hosting
                    if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
                    {
                        RemoteEndpointMessageProperty prop;
                        prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                        ip = prop.Address;
                    }
                }

                // Self-hosting using Owin
                //if (request.Properties.ContainsKey(OwinContext))
                //{
                //    OwinContext owinContext = (OwinContext)request.Properties[OwinContext];
                //    if (owinContext != null)
                //    {
                //        return owinContext.Request.RemoteIpAddress;
                //    }
                //}

                if (string.IsNullOrWhiteSpace(ip))
                {
                    if (request.Properties.ContainsKey("MS_HttpContext"))
                        ip = ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                }
            }
            catch { }

            return string.IsNullOrWhiteSpace(ip) ? "np-ip" : ip;
        }
    }
}
