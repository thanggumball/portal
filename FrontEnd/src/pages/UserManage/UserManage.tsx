import { useEffect, useState } from 'react';
import { DataTable, DataTableHeader, TablePagination } from '../../components/common/Common';
import type { Attribute, Filter } from '../../types/components/attribute';

interface User { id: string; name: string; email: string; isActive: boolean; }

const userMapper: Attribute[] = [
  { key: 'name',     title: 'Name',     type: 'string' },
  { key: 'email',    title: 'Email',    type: 'string' },
  { key: 'isActive', title: 'Active',   type: 'boolean' },
];

export default function UsersManage() {
  const [users, setUsers] = useState<User[]>([]);
  const [filters, setFilters] = useState<Filter[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);

  // TODO: replace with real http call
  useEffect(() => { /* fetch users(pageNumber, pageSize, filters) */ }, [pageNumber, pageSize]);

  const runFilter = () => { setPageNumber(1); /* fetch */ };
  const updateUser = (id: User['id'], values: Record<string, unknown>) => { console.log('update', id, values); };
  const deleteUser = (id: User['id']) => { console.log('delete', id); };

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <DataTableHeader
        modalTitle="user"
        filter
        filters={filters}
        filterAttributeMapper={userMapper}
        updateFilterFunction={setFilters}
        filterFunction={runFilter}
        loading={loading}
      />
      <div style={{ flex: 1, overflow: 'hidden' }}>
        <DataTable
          name="user"
          dataSource={users}
          attributeMapper={userMapper}
          updateItemFunc={updateUser}
          deleteItemFunc={deleteUser}
          pageNumber={pageNumber}
          pageSize={pageSize}
          loading={loading}
        />
      </div>
      <TablePagination
        totalItem={total}
        pageSize={pageSize}
        pageNumber={pageNumber}
        pageSizeOptions={[10, 20, 50]}
        onChangeFunction={(p, s) => { setPageNumber(p); setPageSize(s); }}
        loading={loading}
      />
    </div>
  );
}