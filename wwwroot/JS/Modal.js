const hamburgerMenu = document.getElementById('hamburger-menu');
const navigationMenu = document.getElementById('navigation-menu');
const openModalButton = document.getElementById('open-modal');
const closeModalButton = document.getElementById('close-modal');
const loginModal = document.getElementById('login-modal');
const loginForm = document.getElementById('login-form');
const errorMessages = document.getElementById('login-error-messages');
const logoutButton = document.getElementById('logout-button'); // Получаем кнопку "Выйти"
const userName = document.getElementById('user-name');

if (openModalButton) {
    openModalButton.addEventListener('click', () => {
        loginModal.style.display = 'block';
    });
}

if (closeModalButton) {
    closeModalButton.addEventListener('click', () => {
        loginModal.style.display = 'none';
        errorMessages.innerHTML = '';
    });
}

window.addEventListener('click', (event) => {
    if (event.target === loginModal) {
        loginModal.style.display = 'none';
        errorMessages.innerHTML = '';
    }
});

if (hamburgerMenu) {
    hamburgerMenu.addEventListener('click', function () {
        navigationMenu.classList.toggle('active');
    });
}

// Обработчик отправки формы
if (loginForm) {
    loginForm.addEventListener('submit', (event) => {
        event.preventDefault(); // Предотвращаем стандартную отправку формы
        console.log('Форма отправлена');
        const email = document.getElementById('login').value;
        const password = document.getElementById('password').value;
        const loginData = { email, password };

        errorMessages.innerHTML = '';

        fetch('/Login/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(loginData),
        })
        .then(response => {
            console.log('response:', response);
            if (response.ok) {
                return response.json();
            } else if (response.status === 400) {
                return response.json().then(errors => {
                    throw { errors: errors };
                });
            } else {
                return response.text().then(text => {
                    throw { message: `Ошибка сервера: ${response.status} - ${text}` };
                });
            }
        })
        .then(data => {
            console.log('Успешный ответ:', data);
            loginModal.style.display = 'none';
            openModalButton.style.display = 'none'; // Скрываем кнопку "Войти"
            logoutButton.style.display = 'block'; // Показываем кнопку "Выйти"
            userName.textContent = `Привет, ${data.userName}`;
            // Здесь можно обновить интерфейс, например, показать имя пользователя
            //alert(data.message); // Убираем alert
        })
        .catch(error => {
            console.error('Ошибка:', error);
            errorMessages.innerHTML = '';
            if (error.errors) {
                displayValidationErrors(error.errors);
            } else {
                errorMessages.innerHTML = `<p>${error.message}</p>`;
            }
        });
    });
}

// Обработчик нажатия на кнопку "Выйти"
console.log('logoutButton:', logoutButton);
if (logoutButton) {
    logoutButton.addEventListener('click', () => {
        fetch('/Login/Logout', { // Замените на ваш URL для выхода
            method: 'POST',
        })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Ошибка при выходе');
            }
        })
        .then(data => {
            console.log('Успешный выход:', data);
            openModalButton.style.display = 'block'; // Показываем кнопку "Войти"
            logoutButton.style.display = 'none'; // Скрываем кнопку "Выйти"
            userName.textContent = '';
            //alert(data.message); // Убираем alert
        })
        .catch(error => {
            console.error('Ошибка при выходе:', error);
            alert('Ошибка при выходе');
        });
    });
}

function displayValidationErrors(errors) {
    errorMessages.innerHTML = '';
    for (const key in errors) {
        if (errors.hasOwnProperty(key)) {
            const errorMessagesForKey = errors[key];
            errorMessagesForKey.forEach(message => {
                const errorElement = document.createElement('p');
                errorElement.textContent = message;
                errorMessages.appendChild(errorElement);
            });
        }
    }
}
