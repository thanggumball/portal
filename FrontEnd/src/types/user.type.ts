export const UserStatus {
  Active = 1,
  Inactive = 2,
  Locked = 3,
} as const;

export type UserStatus = typeof UserStatus[keyof typeof UserStatus];

export const USER_STATUS_OPTIONS = [
  { value: UserStatus.Active,   label: 'Active'   },
  { value: UserStatus.Inactive, label: 'Inactive' },
  { value: UserStatus.Locked,   label: 'Locked'   },
];

export interface UserResponse {
  id: string;
  email: string;
  userName: string;
  fullName: string;
  userCode: string | null;
  roleName: string;
  status: UserStatus;
  avatarUrl: string | null;
  lastLoginAt: string | null;
  createdAt: string;
}

export interface UserFilterParams {
  page?: number;
  pageSize?: number;
  roleId?: string;
  status?: UserStatus;
  email?: string;
  userName?: string;
  fullName?: string;
  userCode?: string;
  keyword?: string;
  createdFrom?: string;
  createdTo?: string;
  lastLoginFrom?: string;
  lastLoginTo?: string;
}