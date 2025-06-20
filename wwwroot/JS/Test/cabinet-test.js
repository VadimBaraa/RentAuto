import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    vus: 100,       // 100 виртуальных пользователей
    duration: '30s' // время теста
};

const BASE_URL = 'http://localhost:5001';

// Данные реального пользователя (замени на реальные)
const USER_EMAIL = 'vadimgpt@gmail.com';
const USER_PASSWORD = 'Rws53710';

export default function () {
    // 1) Логинимся и получаем куки сессии
    const loginPayload = JSON.stringify({
        Email: USER_EMAIL,
        Password: USER_PASSWORD
    });

    const loginParams = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    let loginRes = http.post(`${BASE_URL}/Login/Login`, loginPayload, loginParams);

    check(loginRes, {
        'login status 200': (r) => r.status === 200,
        'login success message': (r) => r.json('message') === 'Вход выполнен успешно!',
    });

    // Сохраняем куки сессии для дальнейших запросов
    let cookies = loginRes.cookies;

    // Формируем заголовок с куками для последующих запросов
    let cookieHeader = Object.entries(cookies)
        .map(([name, values]) => `${name}=${values[0].value}`)
        .join('; ');

    const authHeaders = {
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'Cookie': cookieHeader,
        },
    };

    // 2) Выполняем POST /Home/ConfirmRental с куками авторизации
    const rentalPayload = {
        CarId: 1,                         // замени на существующий CarId
        StartDate: '2025-06-08T10:00:00',
        EndDate: '2025-06-08T18:00:00'
    };

    let formBody = Object.keys(rentalPayload)
        .map(k => encodeURIComponent(k) + '=' + encodeURIComponent(rentalPayload[k]))
        .join('&');

    let rentalRes = http.post(`${BASE_URL}/Home/ConfirmRental`, formBody, authHeaders);

    check(rentalRes, {
        'confirm rental status 200 or 302': (r) => r.status === 200 || r.status === 302,
    });

    // 3) Запрос личного кабинета с теми же куками
    let cabinetRes = http.get(`${BASE_URL}/Home/PersonalCabinet`, { headers: { 'Cookie': cookieHeader } });

    check(cabinetRes, {
        'personal cabinet status 200': (r) => r.status === 200,
        'cabinet page contains email': (r) => r.body.includes(USER_EMAIL),
    });

    sleep(1);
}
