import { Form, Input, Switch, Select, DatePicker, Checkbox } from 'antd';
import type { FormInstance } from 'antd';
import dayjs from 'dayjs';
import type { Attribute } from '../../types/components/attribute';

const { TextArea } = Input;
const DEFAULT_TEXT_AREA_ROW_SIZE = 3;

interface SimpleFormProps {
  form: FormInstance;
  attributeMapper: Attribute[][];   // rows of attributes
  data?: Record<string, unknown>;
  readonly?: boolean;
  onFinish: (values: Record<string, unknown>) => void;
  extraSubmitData?: Record<string, unknown>;
  formId?: string;
}

export default function SimpleForm({
  form, attributeMapper, data, readonly = false, onFinish, extraSubmitData = {}, formId,
}: SimpleFormProps) {
  const renderField = (attr: Attribute) => {
    const disabled = readonly || attr.readonly;
    switch (attr.type) {
      case 'string':   return <Input disabled={disabled} addonAfter={attr.addOnAfter} placeholder={attr.placeholder} />;
      case 'number':   return <Input type="number" disabled={disabled} addonAfter={attr.addOnAfter} placeholder={attr.placeholder} />;
      case 'boolean':  return <Switch disabled={disabled} />;
      case 'checkbox': return <Checkbox disabled={disabled} style={{ transform: 'scale(1.25)' }} />;
      case 'select':
        return (
          <Select
            showSearch allowClear disabled={disabled}
            options={attr.options}
            filterOption={(input, option) =>
              String(option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
            defaultValue={attr.defaultValue}
          />
        );
      case 'date':     return <DatePicker disabled={disabled} format="DD-MM-YYYY" style={{ width: '100%' }} />;
      case 'year':     return <DatePicker picker="year"  disabled={disabled} format="YYYY"    style={{ width: '100%' }} />;
      case 'month':    return <DatePicker picker="month" disabled={disabled} format="MM-YYYY" style={{ width: '100%' }} />;
      case 'textarea': return <TextArea disabled={disabled} rows={attr.rowSize ?? DEFAULT_TEXT_AREA_ROW_SIZE} />;
    }
  };

  const extraProps = (attr: Attribute) => {
    switch (attr.type) {
      case 'date':
        return {
          getValueProps: (v: unknown) => ({ value: v ? dayjs(v as string | number) : null }),
          normalize:     (v: unknown) => (v ? dayjs(v as string).toISOString() : null),
        };
      case 'year':
        return {
          getValueProps: (v: unknown) => ({ value: v ? dayjs().year(Number(v)) : null }),
          normalize:     (v: unknown) => (v ? dayjs(v as string).year() : null),
        };
      case 'month':
        return {
          getValueProps: (v: unknown) => ({ value: v ? dayjs(v as string, 'MM-YYYY') : null }),
          normalize:     (v: unknown) => (v ? dayjs(v as string).format('MM-YYYY') : null),
        };
      case 'boolean':
      case 'checkbox':
        return { valuePropName: 'checked' };
      default:
        return {};
    }
  };

  const initialValue = (attr: Attribute) => {
    const raw = data?.[attr.key];
    if (attr.type === 'boolean' || attr.type === 'checkbox') return raw ?? false;
    return raw;
  };

  return (
    <Form id={formId} form={form} layout="vertical"
      onFinish={(values) => onFinish({ ...values, ...extraSubmitData })}
    >
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        {attributeMapper.map((row, rowIndex) => (
          <div key={`row-${rowIndex}`} style={{
            display: 'grid',
            gridTemplateColumns: `repeat(${row.length}, 1fr)`,
            gap: '1.5rem',
          }}>
            {row.map((attr, colIndex) => (
              <Form.Item
                key={`field-${rowIndex}-${colIndex}`}
                label={attr.title}
                name={attr.key}
                initialValue={initialValue(attr)}
                rules={readonly ? [] : attr.rules}
                style={{ marginBottom: '0.75rem' }}
                {...extraProps(attr)}
              >
                {renderField(attr)}
              </Form.Item>
            ))}
          </div>
        ))}
      </div>
    </Form>
  );
}