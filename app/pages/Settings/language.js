
Alpine.data('setting_language', () => ({
    selectedLang: 'vi',

    init() {
        this.selectedLang = Alpine.store('ui').lng;
    },

    saveLanguage(lng) {
        Alpine.store('ui').lng = lng;
        Alpine.store('app').toast.fire('Đã lưu ngôn ngữ', '', 'success');
    }
}));
