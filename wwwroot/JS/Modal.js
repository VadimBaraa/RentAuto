document.addEventListener('DOMContentLoaded', () => {
    const loginButton = document.getElementById('loginButton');
    const logoutButton = document.getElementById('logoutButton');
    const userName = document.getElementById('user-name');
    const loginModal = document.getElementById('login-modal');
    const loginForm = document.getElementById('login-form');
    const errorMessages = document.getElementById('login-error-messages');
    const closeModalButton = document.getElementById('close-modal');
    
    // Проверяем сохраненное имя пользователя
    const storedUserName = sessionStorage.getItem('userName');
    if (storedUserName && loginButton && logoutButton && userName) {
        loginButton.style.display = 'none';
        logoutButton.style.display = 'block';
        userName.textContent = storedUserName;
        userName.style.display = 'inline';
    }

    // Показ формы входа
    if (loginButton && loginModal) {
        loginButton.addEventListener('click', (e) => {
            e.preventDefault();
            loginModal.style.display = 'flex'; // Сначала показываем
            requestAnimationFrame(() => {
                loginModal.classList.add('active'); // Затем добавляем класс для анимации
            });
        });
    }

    // Закрытие формы
    function closeModal() {
        if (loginModal) {
            loginModal.classList.remove('active');
            setTimeout(() => {
                loginModal.style.display = 'none';
                if (errorMessages) {
                    errorMessages.innerHTML = '';
                }
            }, 300);
        }
    }

    if (closeModalButton) {
        closeModalButton.addEventListener('click', closeModal);
    }

    // Закрытие по клику вне формы
    if (loginModal) {
        loginModal.addEventListener('click', (event) => {
            if (event.target === loginModal) {
                closeModal();
            }
        });
    }

    // Отправка формы входа
    if (loginForm) {
        loginForm.addEventListener('submit', async (event) => {
            event.preventDefault();

            const email = document.getElementById('login').value;
            const password = document.getElementById('password').value;
            const loginData = { email, password };
            errorMessages.innerHTML = '';

            try {
                const response = await fetch('/Login/Login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(loginData)
                });

                if (response.ok) {
                    const data = await response.json();

                    // Скрываем модалку
                    loginModal.style.display = 'none';

                    // Отображаем имя пользователя
                    userName.textContent = `${data.userName}`;
                    userName.style.display = 'inline';

                    // Переключаем кнопки
                    loginButton.style.display = 'none';
                    logoutButton.style.display = 'block';

                    // Перезагрузка страницы или редирект (раскомментируй один из вариантов ниже, если нужно)
                    // location.reload();
                    // window.location.href = '/Home/Index';

                } else if (response.status === 400) {
                    const result = await response.json();
                    displayValidationErrors(result.errors || {});
                } else {
                    const text = await response.text();
                    throw new Error(`Ошибка сервера: ${response.status} - ${text}`);
                }
            } catch (error) {
                errorMessages.innerHTML = `<p>${error.message}</p>`;
                console.error('Ошибка при входе:', error);
            }
        });
    }

    // Выход из аккаунта
    if (logoutButton) {
        logoutButton.addEventListener('click', async () => {
            try {
                const response = await fetch('/Login/Logout', {
                    method: 'POST',
                });

                if (response.ok) {
                    window.location.href = '/';
                } else {
                    throw new Error('Ошибка при выходе');
                }
            } catch (error) {
                alert('Ошибка при выходе');
                console.error(error);
            }
        });
    }

    // Функция для отображения ошибок валидации
    function displayValidationErrors(errors) {
        errorMessages.innerHTML = '';

        for (const key in errors) {
            if (errors.hasOwnProperty(key)) {
                const messages = errors[key];

                if (Array.isArray(messages)) {
                    messages.forEach(message => {
                        const errorElement = document.createElement('p');
                        errorElement.textContent = message;
                        errorMessages.appendChild(errorElement);
                    });
                } else {
                    const errorElement = document.createElement('p');
                    errorElement.textContent = messages;
                    errorMessages.appendChild(errorElement);
                }
            }
        }
    }
});

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.toggle-details-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            const details = btn.closest('.car-card').querySelector('.car-details-hidden');
            const isVisible = details.style.display === 'block';
            details.style.display = isVisible ? 'none' : 'block';
            btn.textContent = isVisible ? 'Подробнее' : 'Скрыть';
        });
    });
});

