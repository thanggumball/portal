import { Button, Flex, Table, Switch } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from 'react-router';
import dayjs from 'dayjs';
import FormModal from './FormModal';
import type { Attribute } from '../../types/components/attribute';

interface TableReference {
    path: string;
    key: string | string[];
}

interface DataTableProps<T extends { id: string | number }> {
    name: string;
    dataSource: T[];
    attributeMapper: Attribute[];
    updateAttributeMapper?: Attribute[];
    updateItemFunc?: (id: T['id'], values: Record<string, unknown>) => void;
    deleteItemFunc?: (id: T['id']) => void;
    reference?: TableReference;
    pageNumber?: number;
    pageSize?: number;
    buttons?: boolean;
    writePermission?: boolean;
    deletePermission?: boolean;
    loading?: boolean;
}

// Display-only formatting. Update forms get the RAW record.
const formatForDisplay = (item: Record<string, unknown>, mapper: Attribute[]) => {
  const out: Record<string, unknown> = { ...item };
  for (const f of mapper) {
    const v = item[f.key];
    switch (f.type) {
      case 'date':
        out[f.key] = v ? dayjs(v as string | number).format('DD-MM-YYYY') : '';
        break;
      case 'year':
        out[f.key] = v ? dayjs().year(Number(v)).format('YYYY') : '';
        break;
      case 'select':
        out[f.key] = f.options.find((o) => o.value === v)?.label ?? '';
        break;
    }
  }
  return out;
};

export default function DataTable<T extends { id: string | number; isActive?: boolean }>({name, dataSource, attributeMapper, updateAttributeMapper, updateItemFunc, deleteItemFunc, reference, pageNumber = 1, pageSize = 10, buttons = true, writePermission = true, deletePermission = true, loading = false,}: DataTableProps<T>) {
    const navigate = useNavigate();
    const editMapper = updateAttributeMapper?.length ? updateAttributeMapper : attributeMapper;

    const displayData = dataSource.map((item) =>
        formatForDisplay(item as Record<string, unknown>, attributeMapper)
    );

    type Row = Record<string, unknown>;

    const columns: ColumnsType<Row> = [
    {
        key: '#',
        title: '#',
        dataIndex: '#',
        render: (_, __, i) => (pageNumber - 1) * pageSize + i + 1,
    },
    ...attributeMapper.map(({ key, title, width, align = 'left' as const }) =>
        key === 'isActive'
        ? {
            key, title, dataIndex: key, align,
            render: (isActive: boolean) => <Switch checked={isActive} disabled />,
            ...(width !== undefined ? { width } : {}),
            }
        : { key, title, dataIndex: key, align, ...(width !== undefined ? { width } : {}) }
    ),
    ...(buttons
        ? [{
            key: 'action',
            title: 'Actions',
            dataIndex: 'action',
            render: (_: unknown, _row: Row, i: number) => {
            const raw = dataSource[i];
            return (
                <Flex gap={0} wrap align="center">
                {reference && (
                    <Button
                    style={{ padding: '1.25rem' }}
                    onClick={() => {
                        let finalPath = reference.path;
                        const keys = Array.isArray(reference.key) ? reference.key : [reference.key];
                        keys.forEach((k) => {
                        finalPath = finalPath.replace(`:${k}`, String((raw as Record<string, unknown>)[k]));
                        });
                        navigate(finalPath);
                    }}
                    >
                    Details
                    </Button>
                )}
                {writePermission && updateItemFunc && (
                    <FormModal
                    title={`Update ${name}`}
                    button={<Button style={{ padding: '1.25rem' }}>Edit</Button>}
                    submitFunction={(values) => updateItemFunc(raw.id, values)}
                    data={raw as Record<string, unknown>}
                    attributeMapper={editMapper}
                    formId={`DataTableUpdateModalFormId${i}`}
                    loading={loading}
                    />
                )}
                {deletePermission && deleteItemFunc && (
                    <FormModal
                    title={`Delete ${name}`}
                    button={<Button style={{ padding: '1.25rem' }} danger>Delete</Button>}
                    submitFunction={() => deleteItemFunc(raw.id)}
                    data={raw as Record<string, unknown>}
                    attributeMapper={editMapper}
                    formId={`DataTableDeleteModalFormId${i}`}
                    deletion
                    loading={loading}
                    />
                )}
                </Flex>
            );
            },
        }]
        : []),
    ];

    return (
    <div className="datatable-scroll" style={{ height: '100%', overflow: 'auto' }}>
        <Table<Row>
            dataSource={displayData}
            columns={columns}
            rowKey="id"
            pagination={false}
        />
    </div>
    );
}