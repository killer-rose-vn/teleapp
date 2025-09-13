// Bước 1: Đăng ký Store trước
// Alpine object đã có sẵn vì file thư viện được tải trước.
Alpine.store('user', {
    address: 'UQDO...CYCV',
    balance: 1250000,
    name: 'Nguyen Van A'
});

// Bước 2: Đăng ký TẤT CẢ các component bạn cần
Alpine.data('appManager', () => ({
    currentView: 'home', // Trang mặc định
    pageContent: '',     // Nơi chứa HTML được fetch về
    isLoading: true,

    init() {
        this.loadView('home'); // Tải trang home khi bắt đầu
    },

    async loadView(view) {
        this.isLoading = true;
        try {
            const response = await fetch(`pages/${view}.html`);
            if (!response.ok) throw new Error('Không tìm thấy trang!');
            this.pageContent = await response.text();
            this.currentView = view;
        } catch (error) {
            console.error('Lỗi tải trang:', error);
            this.pageContent = `<p style="text-align: center; color: red;">Không thể tải nội dung.</p>`;
        } finally {
            this.isLoading = false;
        }
    }
}));

Alpine.data('wallet', () => ({
    // Dán logic của component wallet vào đây (nếu có)
    message: 'Đây là component wallet'
}));

Alpine.data('historyPage', () => ({
    // Dán logic của component history vào đây (nếu có)
    message: 'Đây là component history'
}));

// Bước 3: Sau khi đã đăng ký xong tất cả mọi thứ,
// ra lệnh cho Alpine bắt đầu quét trang và khởi tạo.
Alpine.start();