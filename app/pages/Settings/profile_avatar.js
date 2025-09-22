
Alpine.data('profile_avatar', () => ({
    newAvatarFile: null,
    newAvatarPreview: null,

    handleFileChange(event) {
        console.log(event);

        const file = event.target.files[0];
        if (file) {
            this.newAvatarFile = file;
            const reader = new FileReader();
            reader.onload = (e) => {
                this.newAvatarPreview = e.target.result;
            };
            reader.readAsDataURL(file);
        } else {
            this.newAvatarFile = null;
            this.newAvatarPreview = null;
        }
    },

    saveAvatar() {
        if (this.newAvatarFile) {
            // TODO: Gửi this.newAvatarFile lên server để lưu
            // Ví dụ: dùng FormData và fetch API
            // const formData = new FormData();
            // formData.append('avatar', this.newAvatarFile);
            // fetch('/api/upload-avatar', {
            //     method: 'POST',
            //     body: formData
            // })
            // .then(response => response.json())
            // .then(data => {
            //     alert('Cập nhật ảnh đại diện thành công!');
            //     // Cập nhật đường dẫn ảnh đại diện trong user object của Alpine nếu có
            //     // this.user.avatar = data.newAvatarUrl;
            //     this.navigate('pages/settings/profile.html');
            // })
            // .catch(error => {
            //     console.error('Lỗi khi tải ảnh:', error);
            //     alert('Có lỗi xảy ra khi cập nhật ảnh.');
            // });

            alert('Ảnh đại diện đã được giả lập lưu thành công!');
            // Nếu thành công, có thể cập nhật ảnh ngay lập tức
            this.currentAvatar = this.newAvatarPreview;
            // Quay về trang hồ sơ
            this.navigate('pages/settings/profile.html');

        } else {
            alert('Vui lòng chọn một ảnh để cập nhật.');
        }
    }
}));
