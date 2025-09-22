
Alpine.data('withdraw', () => ({
    currentStep: 1,
    withdraw_address: '',
    withdraw_amount: 0,
    withdraw_otp: '',


    openCam() {
        // Mở camera để quét mã QR
        Alpine.store('app').openQrcodeScanner((scannedText => { this.withdraw_address = scannedText; }));
    },
    async openPaste() {
        Alpine.store('app').getClipboard((x) => { this.withdraw_address = x; });
    },
    async openPaste2() {
        Alpine.store('app').getClipboard((x) => { this.withdraw_otp = x; });
    },

    goToStep2() {
        this.currentStep = 2;
    },

    goToStep3() {
        this.currentStep = 3;
    },

}));
