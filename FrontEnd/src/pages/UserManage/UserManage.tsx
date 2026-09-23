import { useMemo } from 'react';
import { DataTable, DataTableHeader, TablePagination } from '../../components/common/Common';
import type { Attribute, Filter } from '../../types/components/attribute';
import { useUsers } from '@/hooks/user.hook';
import { useRoles } from '@/hooks/role.hook';
import { USER_STATUS_OPTIONS, type UserFilterParams } from '@/types/user.type';

export default function UsersManage() {
  const { roles } = useRoles();
  const { users, total, params, setParams, loading, refetch } = useUsers();

  const roleOptions = useMemo(
    () => roles.map((r) => ({ value: r.id, label: r.name })),
    [roles]
  );

  const userFilterMapper: Attribute[] = useMemo(() => [
    { key: 'keyword',   title: 'Search',      type: 'string'   },
    { key: 'fullName',  title: 'Full Name',   type: 'string'   },
    { key: 'email',     title: 'Email',       type: 'string'   },
    { key: 'userName',  title: 'Username',    type: 'string'   },
    { key: 'userCode',  title: 'User Code',   type: 'string'   },
    { key: 'roleId',    title: 'Role',        type: 'select', options: roleOptions },
    { key: 'status',    title: 'Status',      type: 'select', options: USER_STATUS_OPTIONS },
    { key: 'createdFrom',   title: 'Created From',    type: 'date' },
    { key: 'createdTo',     title: 'Created To',      type: 'date' },
    { key: 'lastLoginFrom', title: 'Last Login From', type: 'date' },
    { key: 'lastLoginTo',   title: 'Last Login To',   type: 'date' },
  ], [roleOptions]);

  // Display mapper — columns shown in DataTable
  const userColumnMapper: Attribute[] = [
    { key: 'fullName',    title: 'Full Name',   type: 'string' },
    { key: 'email',       title: 'Email',       type: 'string' },
    { key: 'userName',    title: 'Username',    type: 'string' },
    { key: 'userCode',    title: 'User Code',   type: 'string' },
    { key: 'roleName',    title: 'Role',        type: 'string' },
    { key: 'status',      title: 'Status',      type: 'select', options: USER_STATUS_OPTIONS },
    { key: 'lastLoginAt', title: 'Last Login',  type: 'date' },
    { key: 'createdAt',   title: 'Created',     type: 'date' },
  ];

  // DataTableHeader still uses Filter[]; convert to/from UserFilterParams
  const filters: Filter[] = useMemo(
    () =>
      Object.entries(params)
        .filter(([k, v]) => v !== undefined && v !== '' && k !== 'page' && k !== 'pageSize')
        .map(([key, value]) => ({ key, value })),
    [params]
  );

  const applyFilters = (next: Filter[]) => {
    const patch: UserFilterParams = { page: params.page, pageSize: params.pageSize };
    for (const f of next) {
      (patch as Record<string, unknown>)[f.key] = f.value;
    }
    setParams(patch);
  };

  const runFilter = () => {
    setParams((p) => ({ ...p, page: 1 }));
    refetch();
  };

  const updateUser = (id: string, values: Record<string, unknown>) => {
    console.log('update', id, values); // TODO: userApi.update(id, values)
  };

  const deleteUser = (id: string) => {
    console.log('delete', id); // TODO: userApi.delete(id)
  };

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <DataTableHeader
        modalTitle="user"
        filter
        filters={filters}
        filterAttributeMapper={userFilterMapper}
        updateFilterFunction={applyFilters}
        filterFunction={runFilter}
        loading={loading}
      />
      <div style={{ flex: 1, overflow: 'hidden' }}>
        <DataTable
          name="user"
          dataSource={users}
          attributeMapper={userColumnMapper}
          updateItemFunc={updateUser}
          deleteItemFunc={deleteUser}
          pageNumber={params.page ?? 1}
          pageSize={params.pageSize ?? 10}
          loading={loading}
        />
      </div>
      <TablePagination
        totalItem={total}
        pageSize={params.pageSize ?? 10}
        pageNumber={params.page ?? 1}
        pageSizeOptions={[5, 10, 20, 50]}
        onChangeFunction={(p, s) => setParams((prev) => ({ ...prev, page: p, pageSize: s }))}
        loading={loading}
      />
    </div>
  );
}