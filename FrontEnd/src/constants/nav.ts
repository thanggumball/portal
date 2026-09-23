import path from './path';
import { ROLES } from './roles';
import type { NavItem } from '../types/sidebar/nav';

export const navConfig: NavItem[] = [
  {
    key: 'announcement',
    label: 'Announcement',
    children: [
      { key: 'announcement-publish', label: 'Publish', path: path.announcement.publish, roles: [ROLES.ADMIN] },
      { key: 'announcement-manage',  label: 'Manage',  path: path.announcement.manage,  roles: [ROLES.ADMIN, ROLES.STUDENT] },
    ],
  },
  {
    key: 'users',
    label: 'Users',
    children: [
      { key: 'users-manage', label: 'Manage', path: path.users.manage, roles: [ROLES.ADMIN] },
    ],
  },
  {
    key: 'audit-logs',
    label: 'Audit Logs',
    children: [
      { key: 'audit-logs-manage', label: 'Manage', path: path.auditLogs.manage, roles: [ROLES.ADMIN] },
    ],
  },
];