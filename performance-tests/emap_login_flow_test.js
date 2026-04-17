// JavaScript source code

import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 10 },
        { duration: '1m', target: 20 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<1500'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5281';

// change these to a real test account in your DB
const LOGIN_EMAIL = __ENV.LOGIN_EMAIL || 'coordinator@emap.local';
const LOGIN_PASSWORD = __ENV.LOGIN_PASSWORD || 'Coordinator@123';

function extractVerificationToken(html) {
    const match = html.match(/name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"/i)
        || html.match(/name="__RequestVerificationToken"\s+value="([^"]+)"/i);

    return match ? match[1] : null;
}

export default function () {
    const jar = http.cookieJar();

    // 1) Open login page
    const loginPage = http.get(`${BASE_URL}/Identity/Account/Login`);

    check(loginPage, {
        'login page opened': (r) => r.status === 200,
    });

    const token = extractVerificationToken(loginPage.body);

    check(token, {
        'verification token found': (t) => t !== null,
    });

    if (!token) {
        return;
    }

    // 2) Submit login form
    const payload = {
        Input_Email: LOGIN_EMAIL,
        Input_Password: LOGIN_PASSWORD,
        Input_RememberMe: 'false',
        __RequestVerificationToken: token,
    };

    const headers = {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Referer': `${BASE_URL}/Identity/Account/Login`,
    };

    const loginRes = http.post(
        `${BASE_URL}/Identity/Account/Login`,
        payload,
        { headers, redirects: 0, jar }
    );

    check(loginRes, {
        'login post returned redirect or success': (r) => r.status === 302 || r.status === 200,
    });

    // 3) Open protected pages using the same session
    const coordinatorEval = http.get(`${BASE_URL}/FypMidEvaluation`, { jar });
    check(coordinatorEval, {
        'coordinator evaluation page accessible': (r) => r.status === 200 || r.status === 302,
    });

    const studentPortal = http.get(`${BASE_URL}/Fyp`, { jar });
    check(studentPortal, {
        'student portal returned response': (r) => r.status === 200 || r.status === 302,
    });

    sleep(1);
}
