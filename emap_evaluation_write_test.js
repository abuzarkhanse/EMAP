// JavaScript source code

import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 20 },
        { duration: '1m', target: 50 },
        { duration: '1m', target: 100 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<3000'],
    },
};

const BASE_URL = 'http://localhost:5281';

// ⚠️ Replace with real test user
const USERNAME = 'coordinator@emap.local';
const PASSWORD = 'Coordinator@123';

export default function () {

    // 🔹 Step 1: Load login page
    let res = http.get(`${BASE_URL}/Identity/Account/Login`);

    check(res, {
        'login page loaded': (r) => r.status === 200,
    });

    // 🔹 Extract AntiForgery token
    const tokenMatch = res.body.match(/name="__RequestVerificationToken" type="hidden" value="(.+?)"/);

    if (!tokenMatch) {
        console.error("CSRF token not found");
        return;
    }

    const token = tokenMatch[1];

    // 🔹 Step 2: Login POST
    const loginPayload = {
        Input_Email: USERNAME,
        Input_Password: PASSWORD,
        __RequestVerificationToken: token
    };

    const loginHeaders = {
        'Content-Type': 'application/x-www-form-urlencoded',
    };

    res = http.post(`${BASE_URL}/Identity/Account/Login`, loginPayload, {
        headers: loginHeaders,
        redirects: 0,
    });

    check(res, {
        'login successful': (r) => r.status === 302 || r.status === 200,
    });

    // 🔹 Step 3: Open evaluation page
    res = http.get(`${BASE_URL}/FypMidEvaluation`);

    check(res, {
        'evaluation page opened': (r) => r.status === 200,
    });

    // 🔹 Step 4: Simulate evaluation save (basic POST)
    const evalPayload = {
        EvaluationId: 1,
        EvaluatorName: "Test Coordinator",
        OverallRemarks: "Performance test submission"
    };

    res = http.post(`${BASE_URL}/FypMidEvaluation/Evaluate`, evalPayload);

    check(res, {
        'evaluation post responded': (r) => r.status === 200 || r.status === 302,
    });

    sleep(1);
}
