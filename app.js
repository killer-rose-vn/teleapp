// DÒNG NÀY LÀ QUAN TRỌNG NHẤT
import Alpine from 'https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/module.esm.js';

// Bây giờ Alpine đã được định nghĩa, bạn có thể dùng nó
window.Alpine = Alpine; // Gán vào window để dễ debug từ Console (không bắt buộc)





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
            this.currentView = view;

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