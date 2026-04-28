// JavaScript source code

import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 50 },
        { duration: '1m', target: 100 },
        { duration: '1m', target: 200 },
        { duration: '1m', target: 400 },
        { duration: '1m', target: 600 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<3000'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5281';

export default function () {
    let res = http.get(`${BASE_URL}/`);
    check(res, {
        'home ok': (r) => r.status === 200,
    });

    res = http.get(`${BASE_URL}/Identity/Account/Login`);
    check(res, {
        'login ok': (r) => r.status === 200,
    });

    res = http.get(`${BASE_URL}/FypMidEvaluation`);
    check(res, {
        'eval ok': (r) => r.status === 200 || r.status === 302,
    });

    sleep(1);
}
