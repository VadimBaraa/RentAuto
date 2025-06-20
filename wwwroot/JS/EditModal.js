document.addEventListener("DOMContentLoaded", () => {
    const editModal = document.getElementById("car-edit-modal");
    const formContainer = document.getElementById("car-edit-form-container");

    function openModal() {
        editModal.classList.add('show');
        editModal.style.display = "block";
        document.body.style.overflow = "hidden";
    }

    function closeModal() {
        editModal.classList.remove('show');
        editModal.style.display = "none";
        document.body.style.overflow = "";
        formContainer.innerHTML = `<p style="text-align:center; padding:20px;">Загрузка формы...</p>`;
    }

    function showError(message) {
        formContainer.innerHTML = `
            <div style="text-align:center; padding:20px; color:red;">
                <h3>Ошибка</h3><p>${message}</p>
                <button type="button" class="btn btn-secondary" onclick="document.getElementById('car-edit-close').click()">Закрыть</button>
            </div>`;
    }

    // Функция для показа красивых уведомлений
    function showNotification(message, type = 'success') {
        // Удаляем предыдущие уведомления
        const existingNotifications = document.querySelectorAll('.custom-notification');
        existingNotifications.forEach(n => n.remove());

        const notification = document.createElement('div');
        notification.className = 'custom-notification';
        
        const colors = {
            success: { bg: '#4CAF50', icon: '✅' },
            error: { bg: '#f44336', icon: '❌' },
            warning: { bg: '#ff9800', icon: '⚠️' },
            info: { bg: '#2196F3', icon: 'ℹ️' }
        };

        const color = colors[type] || colors.success;
        
        notification.innerHTML = `
            <div style="display: flex; align-items: center; gap: 10px;">
                <span style="font-size: 18px;">${color.icon}</span>
                <span>${message}</span>
            </div>
        `;
        
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${color.bg};
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            z-index: 10000;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            font-size: 14px;
            min-width: 300px;
            max-width: 400px;
            transform: translateX(100%);
            transition: transform 0.3s ease-in-out;
            cursor: pointer;
        `;

        document.body.appendChild(notification);

        // Анимация появления
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 10);

        // Автоматическое скрытие через 4 секунды
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        }, 3000);

        // Закрытие по клику
        notification.addEventListener('click', () => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        });
    }

    function loadEditForm(carId) {
        fetch(`/Car/Edit/${carId}`, {
            method: 'GET',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
        .then(res => res.ok ? res.text() : Promise.reject("Ошибка загрузки формы"))
        .then(html => formContainer.innerHTML = html)
        .catch(err => showError(err));
    }

    document.addEventListener("click", e => {
        if (e.target.classList.contains("open-edit-modal")) {
            openModal();
            loadEditForm(e.target.dataset.id);
        }

        if (e.target.id === "car-edit-close" || e.target.classList.contains("close-edit-modal") || e.target === editModal) {
            closeModal();
        }
    });

    document.addEventListener("keydown", e => {
        if (e.key === "Escape" && editModal.style.display === "block") closeModal();
    });

    document.addEventListener("submit", e => {
        const form = e.target;
        if (form.closest("#car-edit-modal")) {
            e.preventDefault();

            const formData = new FormData(form);
            const submitBtn = form.querySelector("button[type='submit']");
            const originalText = submitBtn?.textContent;

            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = "Сохранение...";
            }

            fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    closeModal();
                    showNotification(result.message || "Автомобиль успешно обновлен!", 'success');
                    
                    // Перезагружаем страницу через 2 секунды
                    setTimeout(() => location.reload(), 2000);
                } else {
                    // Обработка ошибок валидации
                    let errorMsg = result.message || "Ошибка валидации";
                    if (result.errors) {
                        errorMsg += ":\n";
                        Object.keys(result.errors).forEach(key => {
                            if (result.errors[key].errors && result.errors[key].errors.length > 0) {
                                result.errors[key].errors.forEach(error => {
                                    errorMsg += `${key}: ${error.errorMessage}\n`;
                                });
                            }
                        });
                    }
                    showNotification(errorMsg, 'error');
                }
            })
            .catch(err => {
                showNotification("Ошибка сохранения: " + err, 'error');
            })
            .finally(() => {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.textContent = originalText;
                }
            });
        }
    });
});