// DÒNG NÀY LÀ QUAN TRỌNG NHẤT
import Alpine from 'https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/module.esm.js';

// Bây giờ Alpine đã được định nghĩa, bạn có thể dùng nó
window.Alpine = Alpine; // Gán vào window để dễ debug từ Console (không bắt buộc)





// Bước 1: Đăng ký Store trước
// Alpine object đã có sẵn vì file thư viện được tải trước.
Alpine.store('user', {
    address: 'UQDO...CYCV',
    balance: 1250000,
    name: 'Nguyen Van A',
    memo: 'Ghi chú cá nhân...', // Thêm trường memo
    avatarUrl: 'https://i.pravatar.cc/150?u=a042581f4e29026704d', // Đường dẫn ảnh avatar
    telegramUsername: '@killerrosevn' // Thêm tên tài khoản Telegram
});

// STORE MỚI: Quản lý trạng thái giao diện
Alpine.store('ui', {
    isPanelOpen: false, // Biến cờ để điều khiển panel. Mặc định là đóng.
});

Alpine.store('app', {
    copyAddress(address) {
        navigator.clipboard.writeText(address).then(() => {
            alert('Đã sao chép địa chỉ ví!');
        });
    }
});

// Bước 2: Đăng ký TẤT CẢ các component bạn cần
Alpine.data('appManager', () => ({
    history: ['home'], // Ngăn xếp lịch sử, bắt đầu với trang 'home'
    pageContent: '',     // Nơi chứa HTML được fetch về
    isLoading: true,

    init() {
        this.loadView('home'); // Tải trang home khi bắt đầu
    },




    // Hàm này sẽ trả về view hiện tại (trang trên cùng của ngăn xếp)
    get currentView() {
        return this.history[this.history.length - 1];
    },
    // Hàm điều hướng chính, có thể đi tới (push) hoặc quay lại (back)
    navigate(view) {
        if (view === this.currentView) return; // Không làm gì nếu click vào trang hiện tại

        this.history.push(view); // Đẩy view mới vào ngăn xếp
        this.loadView(view, 'forward');
    },
    // Hàm xử lý khi nhấn nút back
    goBack() {
        if (this.history.length <= 1) return; // Không thể back nếu chỉ còn 1 trang trong lịch sử

        this.history.pop(); // Lấy view hiện tại ra khỏi ngăn xếp
        this.loadView(this.currentView, 'backward');
    },








    async loadView(view, direction) {
        this.isLoading = true;

        const htmlPath = `./pages/${view}.html`;
        const jsPath = `./pages/${view}.js`;

        try {
            // Promise.all sẽ thực hiện cả 2 tác vụ (tải HTML và JS) cùng một lúc
            const [htmlResponse] = await Promise.all([
                fetch(htmlPath),
                import(jsPath) // Lệnh import() động để tải và chạy file JS
            ]);

            // Sau khi cả 2 hoàn thành:
            if (!htmlResponse.ok) {
                throw new Error(`Không tìm thấy file: ${view}.html`);
            }

            // Lấy nội dung HTML và cập nhật giao diện
            this.pageContent = await htmlResponse.text();

        } catch (error) {
            console.error(`Lỗi khi tải trang ${view}:`, error);
            // Có thể xử lý lỗi cho những trang không có file .js (nếu có)
            // Hoặc hiển thị trang lỗi chung
            if (error.name === 'TypeError' && error.message.includes('module')) {
                this.pageContent = `<p style="text-align: center; color: orange;">Trang này không có file logic (.js) đi kèm.</p>`;
                // Nếu trang đơn giản không có JS, chúng ta vẫn fetch HTML
                const htmlResponse = await fetch(htmlPath);
                this.pageContent = await htmlResponse.text();
            } else {
                this.pageContent = `<p style="text-align: center; color: red;">Đã xảy ra lỗi khi tải trang.</p>`;
            }
        } finally {
            this.isLoading = false;
        }
    }
}));

// Bước 3: Sau khi đã đăng ký xong tất cả mọi thứ,
// ra lệnh cho Alpine bắt đầu quét trang và khởi tạo.
Alpine.start();




