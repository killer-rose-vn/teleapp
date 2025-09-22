using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace server.Frm
{
    /// <summary>
    /// Một RichTextBox tùy chỉnh (custom) được thiết kế chuyên dụng để ghi log có màu.
    /// Nó tự động xử lý việc tô màu, thêm timestamp, an toàn đa luồng (thread-safe),
    /// và tích hợp sẵn menu chuột phải để Xóa và Lưu Log.
    /// </summary>
    public class LogRichTextBox : RichTextBox
    {
        // Định nghĩa các cấp độ log bên trong class
        public enum LogLevel
        {
            INFO,
            WARNING,
            ERROR,
            SUCCESS
        }

        // Biến thành viên để giữ menu chuột phải
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem tsmiSaveLog;
        private ToolStripMenuItem tsmiClearLog;
        private ToolStripSeparator tsSeparator; // Thêm một đường kẻ phân cách

        /// <summary>
        /// Hàm khởi tạo (Constructor)
        /// </summary>
        public LogRichTextBox()
        {
            // Thiết lập các thuộc tính mặc định cho một hộp log
            this.ReadOnly = true;
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.ScrollBars = RichTextBoxScrollBars.Vertical;

            // --- THÊM MỚI: KHỞI TẠO MENU CHUỘT PHẢI ---
            InitializeContextMenu();
        }

        /// <summary>
        /// Hàm private để khởi tạo và gán ContextMenuStrip
        /// </summary>
        private void InitializeContextMenu()
        {
            // Tạo các mục menu
            tsmiSaveLog = new ToolStripMenuItem("Save Log...");
            tsmiClearLog = new ToolStripMenuItem("Clear Log");
            tsSeparator = new ToolStripSeparator();

            // Gán sự kiện click cho chúng
            tsmiSaveLog.Click += TsmiSaveLog_Click;
            tsmiClearLog.Click += TsmiClearLog_Click;

            // Tạo ContextMenuStrip và thêm các mục menu vào
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(tsmiSaveLog);
            contextMenu.Items.Add(tsSeparator); // Thêm đường phân cách
            contextMenu.Items.Add(tsmiClearLog);

            // Quan trọng: Gán menu này cho chính control RichTextBox
            this.ContextMenuStrip = contextMenu;
        }

        // --- CÁC HÀM XỬ LÝ SỰ KIỆN CHO MENU ---

        /// <summary>
        /// Sự kiện khi người dùng nhấn "Clear Log"
        /// </summary>
        private void TsmiClearLog_Click(object sender, EventArgs e)
        {
            // Chỉ cần gọi hàm Clear() có sẵn của RichTextBox
            this.Clear();
        }

        /// <summary>
        /// Sự kiện khi người dùng nhấn "Save Log..."
        /// </summary>
        private void TsmiSaveLog_Click(object sender, EventArgs e)
        {
            // Sử dụng một SaveFileDialog tiêu chuẩn
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text Files (*.txt)|*.txt|Log Files (*.log)|*.log|All Files (*.*)|*.*";
                saveDialog.Title = "Save Log File";
                saveDialog.FileName = $"Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"; // Tên file gợi ý

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // RichTextBox có một hàm tích hợp để lưu nội dung (dưới dạng Plain Text)
                        // ra file. Rất tiện lợi!
                        // Lưu ý: Chúng ta dùng this.Text, không phải this.Rtf.
                        // this.Text sẽ lấy nội dung văn bản thuần, bỏ qua các mã màu RTF.
                        System.IO.File.WriteAllText(saveDialog.FileName, this.Text);
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi khi lưu, chúng ta... ghi log về lỗi đó!
                        this.Log($"Failed to save log file: {ex.Message}", LogLevel.ERROR);
                    }
                }
            }
        }


        // --- CÁC HÀM GHI LOG (Giữ nguyên như trước) ---

        /// <summary>
        /// Phương thức CÔNG KHAI (public) để Form hoặc các lớp khác gọi.
        /// Hàm này AN TOÀN khi gọi từ bất kỳ luồng nào (thread-safe).
        /// </summary>
        public void Log(string message, LogLevel level = LogLevel.INFO, int preEnter = 0)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AppendLogLine(message, level, preEnter)));
            }
            else
            {
                AppendLogLine(message, level, preEnter);
            }
        }

        /// <summary>
        /// Phương thức RIÊNG TƯ (private) thực hiện việc tô màu và thêm text.
        /// </summary>
        private void AppendLogLine(string message, LogLevel level, int preEnter = 0)
        {
            if (preEnter > 0)
            {
                var prefix = string.Join("", Enumerable.Repeat("\r\n", preEnter));
                this.AppendText(prefix);
            }

            Color logColor;
            switch (level)
            {
                case LogLevel.WARNING:
                    logColor = Color.Orange;
                    break;
                case LogLevel.ERROR:
                    logColor = Color.Red;
                    break;
                case LogLevel.SUCCESS:
                    logColor = Color.LimeGreen;
                    break;
                case LogLevel.INFO:
                default:
                    logColor = Color.WhiteSmoke;
                    break;
            }

            this.SelectionStart = this.TextLength;
            this.SelectionLength = 0;

            this.SelectionColor = Color.Gray;
            this.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] ");

            this.SelectionColor = logColor;
            this.AppendText(message + Environment.NewLine);

            this.ScrollToCaret();
        }
    }
}
