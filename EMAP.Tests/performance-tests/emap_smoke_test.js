// JavaScript source code

import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 5,
    duration: '30s',
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<1000'],
    },
};

const BASE_URL = 'http://localhost:5281';

export default function () {
    let res = http.get(`${BASE_URL}/`);
    check(res, {
        'home page status is 200': (r) => r.status === 200,
    });

    res = http.get(`${BASE_URL}/Identity/Account/Login`);
    check(res, {
        'login page status is 200': (r) => r.status === 200,
    });

    res = http.get(`${BASE_URL}/FypMidEvaluation`);
    check(res, {
        'evaluation page returns response': (r) => r.status === 200 || r.status === 302,
    });

    sleep(1);
}