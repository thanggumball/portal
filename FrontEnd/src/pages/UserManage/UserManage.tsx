import { useState } from 'react';
import { DataTable, DataTableHeader, TablePagination } from '../../components/common/Common';
import type { Attribute, Filter } from '../../types/components/attribute';

interface User {
  id: string;
  name: string;
  email: string;
  role: 'Admin' | 'Student';
  age: number;
  isActive: boolean;
  joinedAt: number;     // epoch ms — DataTable formats to DD-MM-YYYY
  graduationYear: number;
  notes: string;
}

const userMapper: Attribute[] = [
  { key: 'name',   title: 'Name',   type: 'string',
    rules: [{ required: true, message: 'Name is required' }] },
  { key: 'email',  title: 'Email',  type: 'string',
    rules: [{ required: true, type: 'email', message: 'Valid email required' }] },
  { key: 'role',   title: 'Role',   type: 'select',
    options: [
      { value: 'Admin',   label: 'Admin'   },
      { value: 'Student', label: 'Student' },
    ] },
  { key: 'age',    title: 'Age',    type: 'number', addOnAfter: 'yrs' },
  { key: 'isActive',       title: 'Active',          type: 'boolean' },
  { key: 'joinedAt',       title: 'Joined',          type: 'date' },
  { key: 'graduationYear', title: 'Graduation Year', type: 'year' },
  { key: 'notes',          title: 'Notes',           type: 'textarea' },
];

const MOCK_USERS: User[] = [
  { id: '1', name: 'Alice Nguyen',    email: 'alice@example.com',   role: 'Admin',   age: 28, isActive: true,  joinedAt: new Date('2022-03-15').getTime(), graduationYear: 2020, notes: 'Team lead for platform.' },
  { id: '2', name: 'Bob Tran',        email: 'bob@example.com',     role: 'Student', age: 22, isActive: false, joinedAt: new Date('2024-08-01').getTime(), graduationYear: 2026, notes: 'On leave this semester.' },
  { id: '3', name: 'Charlie Le',      email: 'charlie@example.com', role: 'Student', age: 20, isActive: true,  joinedAt: new Date('2023-09-10').getTime(), graduationYear: 2027, notes: '' },
  { id: '4', name: 'Diana Pham',      email: 'diana@example.com',   role: 'Admin',   age: 34, isActive: true,  joinedAt: new Date('2021-01-20').getTime(), graduationYear: 2015, notes: 'Manages user access.' },
  { id: '5', name: 'Ethan Vu',        email: 'ethan@example.com',   role: 'Student', age: 24, isActive: false, joinedAt: new Date('2023-02-14').getTime(), graduationYear: 2025, notes: 'Thesis in progress.' },
  { id: '6', name: 'Fiona Do',        email: 'fiona@example.com',   role: 'Student', age: 21, isActive: true,  joinedAt: new Date('2024-01-05').getTime(), graduationYear: 2027, notes: '' },
  { id: '7', name: 'George Bui',      email: 'george@example.com',  role: 'Admin',   age: 41, isActive: true,  joinedAt: new Date('2020-06-30').getTime(), graduationYear: 2010, notes: 'Backend lead.' },
  { id: '8', name: 'Hannah Ho',       email: 'hannah@example.com',  role: 'Student', age: 23, isActive: true,  joinedAt: new Date('2023-11-11').getTime(), graduationYear: 2026, notes: '' },
  { id: '9', name: 'Ian Vo',          email: 'ian@example.com',     role: 'Student', age: 25, isActive: false, joinedAt: new Date('2022-07-22').getTime(), graduationYear: 2024, notes: 'Graduated, pending review.' },
  { id: '10', name: 'Julia Dang',     email: 'julia@example.com',   role: 'Admin',   age: 30, isActive: true,  joinedAt: new Date('2021-10-01').getTime(), graduationYear: 2018, notes: '' },
  { id: '11', name: 'Kevin Ly',       email: 'kevin@example.com',   role: 'Student', age: 19, isActive: true,  joinedAt: new Date('2024-09-01').getTime(), graduationYear: 2028, notes: 'First year.' },
  { id: '12', name: 'Linh Truong',    email: 'linh@example.com',    role: 'Student', age: 22, isActive: true,  joinedAt: new Date('2023-03-18').getTime(), graduationYear: 2026, notes: '' },
];

export default function UsersManage() {
  const [users, setUsers] = useState<User[]>(MOCK_USERS);
  const [filters, setFilters] = useState<Filter[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading] = useState(false);

  // Client-side filter for testing — real project delegates to backend
  const filteredUsers = users.filter((u) =>
    filters.every((f) => {
      const v = (u as unknown as Record<string, unknown>)[f.key];
      if (typeof v === 'string' && typeof f.value === 'string') {
        return v.toLowerCase().includes(f.value.toLowerCase());
      }
      return v === f.value;
    })
  );

  const pagedUsers = filteredUsers.slice(
    (pageNumber - 1) * pageSize,
    pageNumber * pageSize
  );

  const runFilter = () => setPageNumber(1);

  const createUser = (values: Record<string, unknown>) => {
    setUsers((prev) => [
      ...prev,
      { ...(values as unknown as Omit<User, 'id'>), id: String(prev.length + 1) },
    ]);
  };

  const updateUser = (id: User['id'], values: Record<string, unknown>) => {
    setUsers((prev) =>
      prev.map((u) => (u.id === id ? { ...u, ...(values as Partial<User>) } : u))
    );
  };

  const deleteUser = (id: User['id']) => {
    setUsers((prev) => prev.filter((u) => u.id !== id));
  };

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <DataTableHeader
        modalTitle="user"
        filter
        filters={filters}
        filterAttributeMapper={userMapper}
        createAttributeMapper={userMapper}
        updateFilterFunction={setFilters}
        filterFunction={runFilter}
        addFunction={createUser}
        loading={loading}
      />
      <div style={{ flex: 1, overflow: 'hidden' }}>
        <DataTable
          name="user"
          dataSource={pagedUsers}
          attributeMapper={userMapper}
          updateItemFunc={updateUser}
          deleteItemFunc={deleteUser}
          pageNumber={pageNumber}
          pageSize={pageSize}
          loading={loading}
        />
      </div>
      <TablePagination
        totalItem={filteredUsers.length}
        pageSize={pageSize}
        pageNumber={pageNumber}
        pageSizeOptions={[5, 10, 20, 50]}
        onChangeFunction={(p, s) => {
          setPageNumber(p);
          setPageSize(s);
        }}
        loading={loading}
      />
    </div>
  );
}