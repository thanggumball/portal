const path = {
  home: '/',
  login: '/login',
  signup: '/signup',
  announcement: {
    root: '/announcement',
    publish: '/announcement/publish',
    manage: '/announcement/manage',
  },
  users: {
    root: '/users',
    manage: '/users/manage',
  },
  auditLogs: {
    root: '/audit-logs',
    manage: '/audit-logs/manage',
    detail: '/audit-logs/:id',
  },
} as const;

export default path;