// DÒNG NÀY LÀ QUAN TRỌNG NHẤT
import Alpine from 'https://cdn.jsdelivr.net/npm/alpinejs@3.15.0/dist/module.esm.js';
window.Alpine = Alpine;


// STORE MỚI: Quản lý trạng thái giao diện
Alpine.store('ui', {
    isPanelOpen: false, // Biến cờ để điều khiển panel. Mặc định là đóng.
});

// STORE CHUNG: Chứa các biến và hàm dùng chung trong toàn app
Alpine.store('app', {

    resetCacheVer: 8, // Tăng số này lên khi bạn deploy phiên bản mới để tránh cache cũ
    isDemo: true, // Chế độ demo, tắt các tính năng nhạy cảm
    isLocalhost: (window.location.hostname === "localhost"),
    apiUrl: (window.location.hostname === "localhost" ? "http://localhost:20001" : "https://api.yourdomain.com"),





    copyAddress(address) {
        navigator.clipboard.writeText(address).then(() => {
            this.toast.fire('Đã sao chép địa chỉ ví!', '', 'success');
        });
    },

    async getClipboard(callback) {
        if (this.isLocalhost) {
            try {
                // Yêu cầu đọc text từ clipboard
                const text = await navigator.clipboard.readText();
                callback(text);

            } catch (err) {
                // Lỗi xảy ra nếu người dùng từ chối quyền
                // hoặc trình duyệt không hỗ trợ
                Swal.fire('Bảng nhớ tạm', 'Không thể đọc clipboard. Bạn đã cấp quyền chưa?', 'warning');
            }
        }
        else {
            WebApp.readTextFromClipboard((clipboardText) => {
                // Callback sẽ được gọi sau khi người dùng tương tác với popup
                if (clipboardText) {
                    // 3. Xử lý dữ liệu nếu người dùng đồng ý và clipboard có nội dung
                    callback(clipboardText);
                }
            });
        }
    },
    textToQrImageDataUrl(txt) {
        var qrDiv = document.createElement("div");
        new QRCode(qrDiv, {
            text: txt,
            width: 128,
            height: 128,
            colorDark: "#000000",
            colorLight: "#ffffff",
            correctLevel: QRCode.CorrectLevel.H
        });
        return qrDiv.querySelector('canvas').toDataURL();
    },
    toast: Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    }),
    openQrcodeScanner(callback) {
        // 1. Kiểm tra xem phiên bản Telegram có hỗ trợ tính năng này không
        if (!WebApp.isVersionAtLeast('6.4')) {
            WebApp.showAlert("Tính năng này yêu cầu phiên bản Telegram 6.4 trở lên. Vui lòng cập nhật ứng dụng của bạn.");
            return;
        }

        // 2. Hiển thị popup quét QR
        WebApp.showScanQrPopup({ text: "Di chuyển camera đến mã QR" }, (qrText) => {
            // 3. Xử lý kết quả (qrText là dữ liệu chuỗi từ mã QR)
            if (qrText) {
                callback(qrText);

                // Trả về 'true' sẽ tự động đóng popup quét
                return true;
            }
            // Nếu không quét được (người dùng bấm hủy), hàm callback này
            // có thể không được gọi, hoặc gọi với qrText là rỗng.
        });
    }
});







// Lấy đối tượng WebApp từ Telegram
window.WebApp = window.Telegram.WebApp;

if (Alpine.store('app').isDemo) {
    Alpine.store('user', {
        tid: 5184813708,
        id: 'vnmaS2w2rCyRhQL',
        shortId: 'vnmaS2w...CyRhQL',
        createDate: '2025-09-21',
        caption: 'kiều phong',
        photo_url: 'https://i.pravatar.cc/150?u=test',
        desciption: 'Ghi chú...',
        money: 12345.67,
        status: 'active' // 'active', 'inactive', 'banned'
    }); // Dùng user mặc định khi test trên trình duyệt
}
else {
    //start_param (String) Nếu Mini App được mở qua link t.me / your_bot / app ? startapp = XYZ, thì start_param sẽ có giá trị là "XYZ".
    //auth_date, hash
    //const response = await fetch(apiUrl + '/api/login/getU', {
    //    method: 'POST', // Chỉ định rõ là POST
    //    headers: {
    //        'Content-Type': 'application/json'
    //    },
    //    body: JSON.stringify({ someData: 'value' }) // Gửi kèm dữ liệu
    //});


    try {
        const response = await fetch(apiUrl + '/api/login/getU?tid=' + telegramUser.id);

        if (!response.ok) {
            throw new Error('Không thể kết nối tới máy chủ.');
        }

        const data = await response.json();

        Alpine.store('user', data);

    } catch (error) {
        console.error("Lỗi khi fetch: ", error);
    }
}






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

        const htmlPath = `/pages/${view}.html?v=` + Alpine.store('app').resetCacheVer;
        const jsPath = `/pages/${view}.js?v=` + Alpine.store('app').resetCacheVer;

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
WebApp.ready(); // Báo cho Telegram biết app của bạn đã sẵn sàng




