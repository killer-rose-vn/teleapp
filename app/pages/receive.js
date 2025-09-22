
Alpine.data('receive', () => ({
    vdt_address: 'Đây là component history',
    transactions: [], // Mảng để chứa danh sách giao dịch từ API

    init() {
        this.vdt_address = Alpine.store('app').textToQrImageDataUrl(Alpine.store('user').id);

        //this.fetchTransactions();
    },

    // 3. Hàm để gọi API lấy dữ liệu
    async fetchTransactions() {
        this.isLoading = true; // Bắt đầu tải, bật cờ loading
        this.errorMessage = ''; // Xóa lỗi cũ

        try {
            // THAY THẾ URL NÀY BẰNG ĐỊA CHỈ API THỰC TẾ CỦA BẠN
            const response = await fetch('https://api.example.com/transactions/recent');

            if (!response.ok) {
                throw new Error('Không thể kết nối tới máy chủ giao dịch.');
            }

            const data = await response.json();

            // Giả sử API trả về một mảng các đối tượng giao dịch
            // Ví dụ: [{ id: 1, description: 'Nhận từ...', amount: 500, date: '2025-09-12T10:30:00Z' }]
            this.transactions = data;

        } catch (error) {
            console.error("Lỗi khi fetch giao dịch:", error);
            this.errorMessage = "Không thể tải được lịch sử giao dịch. Vui lòng thử lại sau.";
        } finally {
            this.isLoading = false; // Tải xong, tắt cờ loading
        }
    }
}));
