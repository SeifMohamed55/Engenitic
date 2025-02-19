import http from 'k6/http';
import { check, sleep } from 'k6';

// Configuration: Adjust for your API
export const options = {
  vus: 1000,           // Number of virtual users
  duration: '120s',   // Test duration
  thresholds: { 
    http_req_duration: ['p(90)<500'], // 95% of requests should be <500ms
  },
};

const BASE_URL = 'https://localhost:443/api'; // Replace with your API URL
const LOGIN_ENDPOINT = '/authentication/login';
const TEST_ENDPOINT = '/users/profile';

// Login function
function login() {
  let payload = JSON.stringify({
    email: 'user3@example.com',
    password: 'asdasd',
  });

  let params = {
    headers: { 'Content-Type': 'application/json' },
  };

  let res = http.post(`${BASE_URL}${LOGIN_ENDPOINT}`, payload, params);
  check(res, {
    'Login successful': (r) => r.status === 200,
  });

  return res.status === 200 ? JSON.parse(res.body).token : null;
}

// Test API with authenticated request
export default function () {
  let token = login();
  
  if (!token) {
    console.error('❌ Login failed. Skipping test.');
    return;
  }

  let params = {
    headers: { Authorization: `Bearer ${token}` },
  };

  let res = http.get(`${BASE_URL}${TEST_ENDPOINT}`, params);
  check(res, {
    'Request successful': (r) => r.status === 200,
  });

  sleep(1); // Simulate real user behavior
}

// Graceful shutdown: Cleanup logs
export function teardown() {
  console.log('✅ Test completed successfully!');
}

// Handle test summary output
export function handleSummary(data) {
  //console.log('📊 Test Summary:', JSON.stringify(data, null, 2));
  return { 'summary.json': JSON.stringify(data, null, 2) };
}
