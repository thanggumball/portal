import { useMemo } from 'react';
import { useNavigate, useParams } from 'react-router';
import { Alert, Button, Card, Descriptions, Empty, Spin, Table, Tag, Typography } from 'antd';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { useAuditLogDetail } from '@/hooks/auditLog.hook';

dayjs.extend(utc);

const { Text } = Typography;

const ACTION_COLORS: Record<string, string> = {
  Create: 'green',
  Update: 'blue',
  Delete: 'red',
  SoftDelete: 'orange',
};

// OldValue / NewValue are JSON strings like {"FullName":"An","Status":1}
const parseJson = (raw: string | null): Record<string, unknown> => {
  if (!raw) return {};
  try {
    return JSON.parse(raw) as Record<string, unknown>;
  } catch {
    return {};
  }
};

const showValue = (v: unknown): string => {
  if (v === undefined) return '—';
  if (v === null) return 'null';
  if (typeof v === 'object') return JSON.stringify(v);
  return String(v);
};

interface ChangeRow {
  field: string;
  oldValue: string;
  newValue: string;
}

export default function AuditLogDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { log, loading, error } = useAuditLogDetail(id);

  // Hooks must run before any early return below
  const changes = useMemo<ChangeRow[]>(() => {
    if (!log) return [];
    const oldObj = parseJson(log.oldValue);
    const newObj = parseJson(log.newValue);
    const fields = Array.from(new Set([...Object.keys(oldObj), ...Object.keys(newObj)]));
    return fields.map((field) => ({
      field,
      oldValue: showValue(oldObj[field]),
      newValue: showValue(newObj[field]),
    }));
  }, [log]);

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '3rem' }}><Spin size="large" /></div>;
  }

  if (error) {
    return <Alert type="error" showIcon title="Failed to load audit log" />;
  }

  if (!log) {
    return <Empty description="Audit log not found" />;
  }

  return (
    <div style={{ height: '100%', overflow: 'auto', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <div>
        <Button onClick={() => navigate(-1)}>Back</Button>
      </div>

      <Card title="Log information">
        <Descriptions
          column={2}
          items={[
            { key: 'time',     label: 'Time',      children: dayjs.utc(log.createdAt).local().format('DD-MM-YYYY HH:mm:ss') },
            { key: 'action',   label: 'Action',    children: <Tag color={ACTION_COLORS[log.action] ?? 'default'}>{log.action}</Tag> },
            { key: 'user',     label: 'User',      children: log.userName ?? '(system)' },
            { key: 'ip',       label: 'IP',        children: log.ipAddress ?? '—' },
            { key: 'entity',   label: 'Entity',    children: log.entityName },
            { key: 'entityId', label: 'Entity ID', children: log.entityId ?? '—' },
          ]}
        />
      </Card>

      <Card title="Changes">
        <Table<ChangeRow>
          rowKey="field"
          dataSource={changes}
          pagination={false}
          locale={{ emptyText: 'No field changes recorded' }}
          columns={[
            { key: 'field',    title: 'Field',     dataIndex: 'field' },
            { key: 'oldValue', title: 'Old value', dataIndex: 'oldValue', render: (v: string) => <Text type="secondary">{v}</Text> },
            { key: 'newValue', title: 'New value', dataIndex: 'newValue', render: (v: string) => <Text strong>{v}</Text> },
          ]}
        />
      </Card>
    </div>
  );
}
