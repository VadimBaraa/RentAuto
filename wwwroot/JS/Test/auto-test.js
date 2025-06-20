import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    vus: 100,
    duration: '30s',
};

export default function () {
    let res = http.get('http://localhost:5001/Home/Auto');

    check(res, {
        'status 200': (r) => r.status === 200,
        'contains cars': (r) => r.body.includes('Car'), // можно уточнить под html
    });

    sleep(1);
}
