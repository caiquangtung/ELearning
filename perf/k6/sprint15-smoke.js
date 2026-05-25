import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
  vus: Number(__ENV.VUS || 10),
  duration: __ENV.DURATION || '1m',
  thresholds: {
    'http_req_duration{type:api}': ['p(95)<200'],
    'http_req_failed{type:api}': ['rate<0.01'],
  },
};

const baseUrl = __ENV.BASE_URL || 'http://localhost:5000';
const token = __ENV.TOKEN || '';

function params() {
  return {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    tags: { type: 'api' },
  };
}

export default function () {
  group('catalog paging', () => {
    const res = http.get(`${baseUrl}/api/v1/courses?page=1&pageSize=20&sort=Newest`, params());
    check(res, {
      'courses list status is acceptable': (r) => [200, 401, 403].includes(r.status),
      'courses list responds under 200ms': (r) => r.timings.duration < 200,
    });
  });

  group('training class paging', () => {
    const res = http.get(`${baseUrl}/api/v1/training-classes?page=1&pageSize=20`, params());
    check(res, {
      'training classes status is acceptable': (r) => [200, 401, 403].includes(r.status),
      'training classes respond under 200ms': (r) => r.timings.duration < 200,
    });
  });

  sleep(1);
}
