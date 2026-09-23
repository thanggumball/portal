export const AUDIT_ACTION_OPTIONS = [
  { value: 'Create',     label: 'Create'      },
  { value: 'Update',     label: 'Update'      },
  { value: 'Delete',     label: 'Delete'      },
  { value: 'SoftDelete', label: 'Soft Delete' },
];

export const AUDIT_ENTITY_OPTIONS = [
  { value: 'User',           label: 'User'            },
  { value: 'Role',           label: 'Role'            },
  { value: 'Announcement',   label: 'Announcement'    },
  { value: 'EmailWhitelist', label: 'Email Whitelist' },
];

export interface AuditLogListItem {
  id: string;
  userId: string | null;
  userName: string | null;
  action: string;
  entityName: string;
  entityId: string | null;
  ipAddress: string | null;
  createdAt: string;
}

export interface AuditLogDetail extends AuditLogListItem {
  oldValue: string | null;
  newValue: string | null;
}

export interface AuditLogFilterParams {
  page?: number;
  pageSize?: number;
  userId?: string;
  entityName?: string;
  action?: string;
  from?: string;
  to?: string;
}
