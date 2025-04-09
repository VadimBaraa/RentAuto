document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('registration-form');
    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');
    const step3 = document.getElementById('step-3');
    const nextStep1 = document.getElementById('next-step-1');
    const nextStep2 = document.getElementById('next-step-2');
    const prevStep1 = document.getElementById('prev-step-1');
    const prevStep2 = document.getElementById('prev-step-2');
    const progressBarFill = document.getElementById('progress-bar-fill');
    const errorMessages = document.getElementById('errorMessages');

    let currentStep = 1;

    nextStep1.addEventListener('click', function () {
        if (!validateStep1()) return;
        const email = document.getElementById('email').value;
        const lastName = document.getElementById('lastName').value;
        const firstName = document.getElementById('firstName').value;
        const middleName = document.getElementById('middleName').value;
        const phone = document.getElementById('phone').value;
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;
        const step1Data = { email, lastName, firstName, middleName, phone, password, confirmPassword };

        fetch('/Registration/RegistrationStep1', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(step1Data)
        })
        .then(response => {
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
            step1.style.display = 'none';
            step2.style.display = 'block';
            currentStep = 2;
            progressBarFill.style.width = '66%';
            errorMessages.innerHTML = '';
        })
        .catch(error => {
            console.error('Общая ошибка:', error);
            errorMessages.innerHTML = '';
            if (error.errors) {
                displayValidationErrors(error.errors);
            } else {
                errorMessages.innerHTML = `<p>${error.message}</p>`;
            }
        });
    });

    nextStep2.addEventListener('click', function () {
        if (!validateStep2()) return;
        const passportData = document.getElementById('passportData').value;
        const driverLicense = document.getElementById('driverLicense').value;
        const step2Data = { passportData, driverLicense };

        fetch('/Registration/RegistrationStep2', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(step2Data),
        })
        .then(response => {
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
            step2.style.display = 'none';
            step3.style.display = 'block';
            currentStep = 3;
            progressBarFill.style.width = '100%';
            errorMessages.innerHTML = '';
        })
        .catch(error => {
            console.error('Общая ошибка:', error);
            errorMessages.innerHTML = '';
            if (error.errors) {
                displayValidationErrors(error.errors);
            } else {
                errorMessages.innerHTML = `<p>${error.message}</p>`;
            }
        });
    });

    prevStep1.addEventListener('click', function () {
        step2.style.display = 'none';
        step1.style.display = 'block';
        currentStep = 1;
        progressBarFill.style.width = '33%';
        errorMessages.innerHTML = '';
    });

    prevStep2.addEventListener('click', function () {
        step3.style.display = 'none';
        step2.style.display = 'block';
        currentStep = 2;
        progressBarFill.style.width = '66%';
        errorMessages.innerHTML = '';
    });

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        const passportPhoto = document.getElementById('passportPhoto').files[0];
        const driverLicensePhoto = document.getElementById('driverLicensePhoto').files[0];
        const step3Data = new FormData();
        step3Data.append("PassportPhoto", passportPhoto);
        step3Data.append("DriverLicensePhoto", driverLicensePhoto);

        fetch('/Registration/RegistrationStep3', {
            method: 'POST',
            body: step3Data
        })
        .then(response => {
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
            alert(data.message);
        })
        .catch(error => {
            console.error('Общая ошибка:', error);
            errorMessages.innerHTML = '';
            if (error.errors) {
                displayValidationErrors(error.errors);
            } else {
                errorMessages.innerHTML = `<p>${error.message}</p>`;
            }
        });
    });

    function validateStep1() {
        const requiredFields = step1.querySelectorAll('[required]');
        for (const field of requiredFields) {
            if (!field.value.trim()) {
                alert(`Поле "${field.previousElementSibling.textContent.trim()}" обязательно для заполнения.`);
                return false;
            }
        }
        return true;
    }

    function validateStep2() {
        const requiredFields = step2.querySelectorAll('[required]');
        for (const field of requiredFields) {
            if (!field.value.trim()) {
                alert(`Поле "${field.previousElementSibling.textContent.trim()}" обязательно для заполнения.`);
                return false;
            }
        }
        return true;
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
});
