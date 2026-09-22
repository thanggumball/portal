import type { Role } from '../../constants/roles';

export interface NavLeaf {
  key: string;
  label: string;
  path: string;
  roles: Role[];
}

export interface NavGroup {
  key: string;
  label: string;
  children: NavItem[];
}

export type NavItem = NavLeaf | NavGroup;

export const isLeaf = (item: NavItem): item is NavLeaf => 'path' in item;