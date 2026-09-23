import type { Role } from "@/constants/roles";

export interface User {
    id: string;
    email: string;
    userName: string;
    fullName: string;
    studentCode: string;
    avatarUrl: string;
    roleName: Role;
    lastLoginAt: string;
    createdAt: string;
}