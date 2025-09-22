
Alpine.data('setting_recovery', () => ({
    step: 1,
    hasCopied: false,
    recoveryNumbers: [123, 456, 789, 101, 112, 131, 415, 161, 718, 192, 202, 212], /* Tải 12 số thật từ server */
    userInput: '',
    error: '',
    validateRecovery() {
        /* Phép toán: ( (số thứ 2 + 10) + (số thứ 5 - số thứ 6) ) / 3 (lấy phần nguyên) */
        /* Lưu ý: index mảng là 1, 4, 5 */
        const correctResult = Math.floor(((this.recoveryNumbers[1] + 10) + (this.recoveryNumbers[4] - this.recoveryNumbers[5])) / 3);

        if (parseInt(this.userInput) === correctResult) {
            this.error = '';
            alert('Xác thực thành công!');
            this.navigate('pages/settings.html'); // Quay về trang cài đặt
        } else {
            this.error = 'Sai kết quả. Vui lòng thử lại.';
        }
    }
}));
