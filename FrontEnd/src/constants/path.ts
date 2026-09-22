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
} as const;

export default path;