import http from 'k6/http';
import { check, sleep } from 'k6';

// ✅ Configure test options (adjust as needed)
export const options = {
  stages: [
    { duration: '10s', target: 100 }, // Ramp-up: 50 users over 10 seconds
    { duration: '10s', target: 1000 }, // Steady load: 50 users for 30 seconds
    { duration: '10s', target: 0 },  // Ramp-down: Reduce to 0 users over 10 seconds
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests must be under 500ms
    http_req_failed: ['rate<0.01'],  // Less than 1% failure rate
  },
};

// 🔗 API Endpoint (Replace with your URL)
const BASE_URL = 'https://localhost:443/api/courses/1';

export default function () {
  let res = http.get(BASE_URL);

  if (res.status !== 200) {
    console.error(`❌ Request failed with status: ${res.status}`);
  }
  check(res, {
    '✅ Status is 200': (r) => r.status === 200,
    '⚡ Response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1); // Simulates user wait time between requests
}

// 🛑 Graceful Shutdown
export function teardown() {
  console.log('✅ Load test completed!');
}

// 📊 Save test results as a formatted JSON file
export function handleSummary(data) {
  console.log('📊 Load Test Summary:', JSON.stringify(data, null, 2));

  return {
    'summary.json': JSON.stringify(data, null, 2), // Save indented JSON file
  };
}
