document.addEventListener('alpine:init', () => {

    // Component chính quản lý toàn bộ App và điều hướng
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

    // Component cho trang chủ (đã có ở phần 1)
    Alpine.data('wallet', () => ({
        // ... code của wallet component ...
    }));
    
    // Component cho trang lịch sử
    Alpine.data('historyPage', () => ({
        // ... logic riêng cho trang lịch sử ...
    }));
});