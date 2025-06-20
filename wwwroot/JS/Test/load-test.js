import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 100,
  duration: '10s',
};

const BASE_URL = 'https://localhost:5001';
const USER_EMAIL = 'vadimgpt@gmail.com';
const USER_PASSWORD = 'Rws53710';
const TEST_CAR_ID = 1;

export default function () {
  const jar = http.cookieJar();

  let loginPayload = JSON.stringify({
    email: USER_EMAIL,
    password: USER_PASSWORD,
  });

  let loginHeaders = { 'Content-Type': 'application/json' };

  let loginRes = http.post(`${BASE_URL}/Login/Login`, loginPayload, { headers: loginHeaders });

  check(loginRes, {
    'login status is 200': (r) => r.status === 200,
    'login success message': (r) => {
      try {
        return r.json().message === 'Вход выполнен успешно!';
      } catch {
        return false;
      }
    },
  });

  if (loginRes.cookies) {
    Object.entries(loginRes.cookies).forEach(([name, cookies]) => {
      cookies.forEach((cookie) => jar.set(BASE_URL, name, cookie.value));
    });
  }

  console.log('Cookies after login:', JSON.stringify(jar.cookiesForURL(BASE_URL)));

  sleep(1);

  let cabinetRes = http.get(`${BASE_URL}/Home/PersonalCabinet`, { jar: jar });
  console.log('PersonalCabinet status:', cabinetRes.status);
  console.log('PersonalCabinet body snippet:', cabinetRes.body.substring(0, 200));

  check(cabinetRes, {
    'personal cabinet status is 200': (r) => r.status === 200,
  });

  sleep(1);
}
