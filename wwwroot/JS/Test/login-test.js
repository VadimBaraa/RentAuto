import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    vus: 100,
    duration: '30s',
};

export default function () {
    const payload = JSON.stringify({
        Email: 'testuser@example.com',
        Password: 'Password123!'
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    let res = http.post('http://localhost:5001/Login/Login', payload, params);

    check(res, {
        'status 200 or 400 (validation)': (r) => r.status === 200 || r.status === 400,
        'login success': (r) => r.json('message') === 'Вход выполнен успешно!' || r.status === 400,
    });

    sleep(1);
}
