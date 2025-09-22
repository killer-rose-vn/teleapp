using Serilog.Sinks.File;
using server.Cls;
using server.db;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace server
{
    public partial class main : Form
    {
        HttpSelfHostServer serverAPI = null;
        public static main Instance = null;

        private ITelegramBotClient _botClient;
        private CancellationTokenSource _cts;

        binanceClient bc = null;



        public main()
        {
            InitializeComponent();
        }

        private void main_Load(object sender, EventArgs e)
        {

        }

        async Task startTele()
        {
            if (_botClient != null || string.IsNullOrWhiteSpace(Properties.Settings.Default.teleBot_token))
                return;

            try
            {
                _botClient = new TelegramBotClient(Properties.Settings.Default.teleBot_token);
                _cts = new CancellationTokenSource();

                // Cấu hình cách nhận tin nhắn
                var receiverOptions = new ReceiverOptions
                {
                    // AllowedUpdates = Array.Empty<UpdateType>() // Nhận tất cả các loại update
                    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
                };

                // Bắt đầu nhận tin nhắn
                _botClient.StartReceiving(
                    updateHandler: HandleUpdateAsync,      // Hàm xử lý khi có tin nhắn mới
                    errorHandler: HandlePollingErrorAsync,  // Hàm xử lý khi có lỗi
                    receiverOptions: receiverOptions,
                    cancellationToken: _cts.Token
                );

                // Lấy thông tin bot để xác nhận kết nối
                var me = await _botClient.GetMe();
                log1.Log($"[Tele] Bot đã kết nối thành công: @{me.Username}", Frm.LogRichTextBox.LogLevel.SUCCESS);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kết nối thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                log1.Log($"[Tele] Lỗi kết nối: {ex.Message}");
                _botClient = null; // Đặt lại
            }
        }
        void stopTele()
        {
            if (_cts != null)
            {
                _cts.Cancel(); // Gửi tín hiệu yêu cầu dừng
                _cts = null;
            }
            _botClient = null;
            log1.Log("Bot đã dừng.");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Chỉ xử lý tin nhắn dạng text
            if (update.Type != UpdateType.Message || update.Message.Type != MessageType.Text)
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;
            var messageText = message.Text;
            var fromUser = message.From;

            // Log ra màn hình WinForms
            log1.Log($"[Tele] Nhận được tin nhắn từ '{fromUser.FirstName}' (ChatID: {chatId}): '{messageText}'");

            try
            {
                if (messageText.StartsWith("/start"))
                {
                    //check user
                    var db = new db.dcDataContext(dbProvider.connStr);
                    var u = db.tbUsers.SingleOrDefault(t => t.tele_id == fromUser.Id);
                    if (u != null)
                    {
                        //await botClient.SendMessage(chatId, $"Chào mừng trở lại, {getDisplayName(fromUser)}!", cancellationToken: cancellationToken);
                        // "Mở Mini App" là văn bản sẽ hiển thị trên nút
                        var webAppButton = InlineKeyboardButton.WithWebApp("🚀 Mở App", new WebAppInfo { Url = "https://vnmteleminiapp.vnn.pw/" });
                        var inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] { webAppButton } });
                        await botClient.SendMessage(chatId, $"Chào mừng bạn, *{getDisplayName(fromUser)}*\\!\r\nVí đã sẵn sàng sử dụng, Địa chỉ: `{u.id}`", ParseMode.MarkdownV2,
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken
                        );
                    }
                    else
                    {
                        u = new tbUser
                        {
                            id = "vnm" + H.GenerateRandomWalletAddress(12),
                            tele_id = fromUser.Id,
                            caption = getDisplayName(fromUser),
                            createDate = DateTime.Now,
                            status = E.user_status.active.ToString(),
                            secretnum1 = H.GenerateRandomSecretNumber(),
                            secretnum2 = H.GenerateRandomSecretNumber(),
                            secretnum3 = H.GenerateRandomSecretNumber(),
                            secretnum4 = H.GenerateRandomSecretNumber(),
                            secretnum5 = H.GenerateRandomSecretNumber(),
                            secretnum6 = H.GenerateRandomSecretNumber(),
                            secretnum7 = H.GenerateRandomSecretNumber(),
                            secretnum8 = H.GenerateRandomSecretNumber(),
                            secretnum9 = H.GenerateRandomSecretNumber(),
                            secretnum10 = H.GenerateRandomSecretNumber(),
                            secretnum11 = H.GenerateRandomSecretNumber(),
                            secretnum12 = H.GenerateRandomSecretNumber(),
                        };
                        db.tbUsers.InsertOnSubmit(u);
                        db.SubmitChanges();

                        // "Mở Mini App" là văn bản sẽ hiển thị trên nút
                        var webAppButton = InlineKeyboardButton.WithWebApp("🚀 Mở App", new WebAppInfo { Url = "https://vnmteleminiapp.vnn.pw/" });
                        var inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] { webAppButton } });
                        await botClient.SendMessage(chatId, $"Chào mừng bạn, *{getDisplayName(fromUser)}*\\!\r\nVí đã sẵn sàng sử dụng, Địa chỉ: `{u.id}`", ParseMode.MarkdownV2,
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chatId, "Có lỗi ngoại lệ sảy ra: " + ex.Message, cancellationToken: cancellationToken);
            }
        }

        static string getDisplayName(User u)
        {
            if (!string.IsNullOrWhiteSpace(u.Username))
                return u.Username;
            if (!string.IsNullOrWhiteSpace(u.FirstName) && !string.IsNullOrWhiteSpace(u.LastName))
                return $"{u.FirstName} {u.LastName}";
            if (!string.IsNullOrWhiteSpace(u.FirstName))
                return u.FirstName;
            if (!string.IsNullOrWhiteSpace(u.LastName))
                return u.LastName;
            return "tele " + u.Id.ToString();
        }

        // 3. HÀM XỬ LÝ LỖI POLLING
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Log lỗi ra màn hình WinForms
            log1.Log($"[Tele] LỖI Polling: {exception.Message}", Frm.LogRichTextBox.LogLevel.ERROR);
            return Task.CompletedTask;
        }




        public void outLog(string s, Frm.LogRichTextBox.LogLevel l = Frm.LogRichTextBox.LogLevel.INFO)
        {
            log1.Log(s, l);
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            outLog("SERVER Started");
            var config = new HttpSelfHostConfiguration(textBox1.Text);
            config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}/{action}", defaults: new { action = "get" });
            config.EnableCors(new System.Web.Http.Cors.EnableCorsAttribute("*", "*", "*"));
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
            if (textBox1.Text.IndexOf("localhost") == -1)
                config.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.Exact;
            config.MessageHandlers.Add(new Cls.LogRequestAndResponseHandler());

            serverAPI = new HttpSelfHostServer(config);
            serverAPI.OpenAsync().Wait();

            await startTele();
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            serverAPI.CloseAsync().Wait();
            serverAPI = null;
            
            stopTele();
            outLog("SERVER stopped!");
        }
        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serverAPI != null)
            {
                MessageBox.Show("Server đang chạy không được đóng");
                e.Cancel = true;
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            MessageBox.Show("Đã lưu cấu hình");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            binanceClient bc = new binanceClient(textBox2.Text, textBox4.Text);
            var x = bc.GetDepositAddress("USDT");
            MessageBox.Show(x.msg);
        }

    }
}
