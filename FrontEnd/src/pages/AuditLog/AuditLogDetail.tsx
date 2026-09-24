import { useMemo } from 'react';
import { useNavigate, useParams } from 'react-router';
import { Alert, Button, Card, Descriptions, Empty, Spin, Table, Tag, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { useAuditLogDetail } from '@/hooks/auditLog.hook';
import type { AuditLogChange } from '@/types/auditLog.type';

const { Text } = Typography;

const ACTION_COLORS: Record<string, string> = {
  Create: 'green',
  Update: 'blue',
  Delete: 'red',
  SoftDelete: 'orange',
};

// A brand-new row always has these values, so showing them on Create tells the reader nothing
const HIDDEN_ON_CREATE = new Set(['IsDeleted']);

const FIELD_LABELS: Record<string, string> = {
  RoleId: 'Role',
  RoleReceived: 'Audience',
};

// "FullName" -> "Full Name"
const fieldLabel = (field: string) => FIELD_LABELS[field] ?? field.replace(/([a-z])([A-Z])/g, '$1 $2');

// Backend already turns ids / enum numbers into names; only empty values are left to label
const showValue = (v: string | null) => v || '(empty)';

export default function AuditLogDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { log, loading, error } = useAuditLogDetail(id);

  // Hooks must run before any early return below
  const changes = useMemo(
    () => (log?.changes ?? []).filter((c) => !(log?.action === 'Create' && HIDDEN_ON_CREATE.has(c.field))),
    [log]
  );

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '3rem' }}><Spin size="large" /></div>;
  }

  if (error) {
    return <Alert type="error" showIcon title="Failed to load audit log" />;
  }

  if (!log) {
    return <Empty description="Audit log not found" />;
  }

  // Create has no old value and Delete has no new value, so an Old/New pair would leave one column all "—"
  const fieldColumn = { key: 'field', title: 'Field', dataIndex: 'field', width: 200, render: (f: string) => fieldLabel(f) };
  const changeColumns: ColumnsType<AuditLogChange> =
    log.action === 'Create'
      ? [fieldColumn, { key: 'newValue', title: 'Value', dataIndex: 'newValue', render: showValue }]
      : log.action === 'Delete'
        ? [fieldColumn, { key: 'oldValue', title: 'Value', dataIndex: 'oldValue', render: showValue }]
        : [
            fieldColumn,
            { key: 'oldValue', title: 'Old value', dataIndex: 'oldValue', render: (v: string | null) => <Text type="secondary">{showValue(v)}</Text> },
            { key: 'newValue', title: 'New value', dataIndex: 'newValue', render: (v: string | null) => <Text strong>{showValue(v)}</Text> },
          ];

  return (
    <div style={{ height: '100%', overflow: 'auto', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <div>
        <Button onClick={() => navigate(-1)}>Back</Button>
      </div>

      <Card title="Log information">
        <Descriptions
          column={2}
          items={[
            { key: 'time',     label: 'Time',      children: dayjs(log.createdAt).format('DD-MM-YYYY HH:mm:ss') },
            { key: 'action',   label: 'Action',    children: <Tag color={ACTION_COLORS[log.action] ?? 'default'}>{log.action}</Tag> },
            { key: 'user',     label: 'User',      children: log.userName ?? '(system)' },
            { key: 'ip',       label: 'IP',        children: log.ipAddress ?? '—' },
            { key: 'entity',   label: 'Entity',    children: log.entityName },
            { key: 'entityId', label: 'Entity ID', children: log.entityId ?? '—' },
          ]}
        />
      </Card>

      <Card title="Changes">
        <Table<AuditLogChange>
          rowKey="field"
          dataSource={changes}
          pagination={false}
          locale={{ emptyText: 'No field values actually changed' }}
          columns={changeColumns}
        />
      </Card>
    </div>
  );
}
