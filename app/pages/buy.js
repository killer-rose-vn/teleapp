
Alpine.data('buy', () => ({
    currentStep: 1,

    data1: {
        network: 'TRC',
        amount: 55,
        tygia: 26.63,
    },
    data2: {
        id: 123,
        network: 'BEP20',
        address: '0x12340938425jksnf9473t2c34kc2h3abcd',
        amount: 1000.00456,
        qrCodeUrl: ''
    },

    async goToStep2() {
        if (!this.data1.network || this.data1.amount <= 0) {
            Swal.fire('Buy', 'Cần nhập dữ liệu đúng', 'warning');
            return;
        }

        if (!Alpine.store('app').isDemo) {
            const response = await fetch(Alpine.store('app').apiUrl + '/api/login/getU?tid=' + telegramUser.id);
            if (!response.ok) {
                Swal.fire('Buy', 'Không thể kết nối tới máy chủ.', 'error');
            }

            this.data2 = await response.json();
        }

        this.data2.qrCodeUrl = this.textToQrImageDataUrl(this.data2.address);
        this.currentStep = 2;
    },

    goToStep3() {
        this.currentStep = 3;
    },

    async checkOrderStatus() {
        if (!this.data1.network || this.data1.amount <= 0) {
            Swal.fire('Buy', 'Cần nhập dữ liệu đúng', 'warning');
            return;
        }

        if (!Alpine.store('app').isDemo) {
            const response = await fetch(Alpine.store('app').apiUrl + '/api/login/getU?tid=' + telegramUser.id);
            if (!response.ok) {
                Swal.fire('Buy', 'Không thể kết nối tới máy chủ.', 'error');
            }

            this.data2 = await response.json();
        }

        this.data2.qrCodeUrl = Alpine.store('app').textToQrImageDataUrl(this.data2.address);
        this.currentStep = 2;
    },

}));
