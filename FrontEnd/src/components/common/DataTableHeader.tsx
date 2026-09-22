import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Button, Input, InputNumber, Select, Switch, DatePicker, Popover, Space, Tag, Upload } from 'antd';
import type { UploadProps } from 'antd';
import dayjs from 'dayjs';
import FormModal from './FormModal';
import type { Attribute, Filter } from '../../types/components/attribute';

interface DataTableHeaderProps {
  modalTitle?: string;
  filter?: boolean;
  filters?: Filter[];
  filterAttributeMapper?: Attribute[];
  createAttributeMapper?: Attribute[];
  back?: boolean;
  updateFilterFunction: (filters: Filter[]) => void;
  filterFunction?: () => void;
  addFunction?: (values: Record<string, unknown>) => void;
  loading?: boolean;
  createLink?: string | null;
  createButton?: React.ReactElement | null;
  isUpdate?: boolean;
  updateFunction?: () => void;
  importFunction?: UploadProps['beforeUpload'];
  exportFunction?: (() => void) | null;
  uploadFiles?: UploadProps['fileList'];
  changeFileFunction?: UploadProps['onRemove'];
}

const formatFilterValue = (value: unknown, attr?: Attribute): string => {
  if (!attr) return String(value);
  if (attr.type === 'select') {
    return String(attr.options.find((o) => o.value === value)?.label ?? value);
  }
  if (attr.type === 'year' || attr.type === 'month') return String(Number(value));
  if (attr.type === 'date')   return dayjs(value as string | number).format('DD-MM-YYYY');
  if (attr.type === 'number') {
    const suffix = attr.addOnAfter ? ` ${attr.addOnAfter}` : '';
    return `${Number(value)}${suffix}`;
  }
  if (attr.type === 'boolean') return value ? 'Active' : 'Inactive';
  return String(value);
};

export default function DataTableHeader({
  modalTitle, filter = false, filters = [],
  filterAttributeMapper = [], createAttributeMapper = [],
  back = false, updateFilterFunction, filterFunction, addFunction,
  loading = false, createLink = null, createButton = null,
  isUpdate = false, updateFunction,
  importFunction, exportFunction = null, uploadFiles = [], changeFileFunction,
}: DataTableHeaderProps) {
  const navigate = useNavigate();
  const [popoverOpen, setPopoverOpen] = useState(false);
  const [selectedKey, setSelectedKey] = useState<string | undefined>(filterAttributeMapper[0]?.key);
  const [pendingValue, setPendingValue] = useState<unknown>(undefined);

  const selectedAttr = filterAttributeMapper.find((a) => a.key === selectedKey);

  const commitFilter = () => {
    if (selectedKey === undefined || pendingValue === undefined || pendingValue === '') return;
    let stored = pendingValue;
    if (dayjs.isDayjs(pendingValue)) {
      if (selectedAttr?.type === 'year')       stored = pendingValue.year();
      else if (selectedAttr?.type === 'month') stored = pendingValue.month() + 1;
      else                                     stored = pendingValue.toISOString();
    }
    const exists = filters.some((f) => f.key === selectedKey);
    const next = exists
      ? filters.map((f) => (f.key === selectedKey ? { key: selectedKey, value: stored } : f))
      : [...filters, { key: selectedKey, value: stored }];
    updateFilterFunction(next);
    setPendingValue(undefined);
    setPopoverOpen(false);
  };

  const removeFilter = (key: string) => updateFilterFunction(filters.filter((f) => f.key !== key));
  const resetFilters = () => updateFilterFunction([]);

  const renderValueInput = () => {
    if (!selectedAttr) return null;
    const set = (v: unknown) => setPendingValue(v);
    switch (selectedAttr.type) {
      case 'string':  return <Input placeholder="Enter keyword" onChange={(e) => set(e.target.value)} />;
      case 'number':  return <InputNumber style={{ width: '100%' }} placeholder={selectedAttr.placeholder ?? 'Enter a number'} addonAfter={selectedAttr.addOnAfter} onChange={set} />;
      case 'boolean': return <Switch defaultChecked checkedChildren="Active" unCheckedChildren="Inactive" onChange={set} />;
      case 'select':  return (
        <Select style={{ width: '100%' }} showSearch options={selectedAttr.options}
          filterOption={(input, option) => String(option?.label).toLowerCase().includes(input.toLowerCase())}
          onChange={set} />
      );
      case 'date':    return <DatePicker style={{ width: '100%' }} onChange={set} />;
      case 'year':    return <DatePicker style={{ width: '100%' }} picker="year"  format="YYYY"    onChange={set} />;
      case 'month':   return <DatePicker style={{ width: '100%' }} picker="month" format="MM-YYYY" onChange={set} />;
      default: return null;
    }
  };

  const popoverContent = (
    <Space direction="vertical" style={{ width: 280 }}>
      <Select style={{ width: '100%' }} placeholder="Category"
        value={selectedKey}
        options={filterAttributeMapper.map((a) => ({ label: a.title, value: a.key }))}
        onChange={(v) => { setSelectedKey(v); setPendingValue(undefined); }}
      />
      {renderValueInput()}
      <Button type="primary" block onClick={commitFilter}>Add</Button>
    </Space>
  );

  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
        {back && <Button style={{ width: '6rem' }} onClick={() => navigate(-1)}>Back</Button>}
        {filter && (
          <>
            <Popover content={popoverContent} trigger="click" placement="bottomLeft"
              open={popoverOpen} onOpenChange={setPopoverOpen}>
              <Button>+ Add filter</Button>
            </Popover>
            {filters.map((f) => {
              const attr = filterAttributeMapper.find((a) => a.key === f.key);
              return (
                <Tag key={f.key} closable onClose={() => removeFilter(f.key)}>
                  {`${attr?.title ?? f.key}: ${formatFilterValue(f.value, attr)}`}
                </Tag>
              );
            })}
            <Button type="primary" loading={loading} onClick={filterFunction}>Filter</Button>
            <Button onClick={resetFilters}>Reset</Button>
          </>
        )}
      </div>
      <Space>
        {importFunction && (
          <Upload beforeUpload={importFunction} maxCount={1} fileList={uploadFiles} onRemove={changeFileFunction}>
            <Button loading={loading}>Import</Button>
          </Upload>
        )}
        {exportFunction && <Button onClick={exportFunction} loading={loading}>Export</Button>}
        {isUpdate && updateFunction ? (
          <Button type="primary" onClick={updateFunction} loading={loading}>Update</Button>
        ) : createButton ? createButton
        : createLink ? (
          <Button type="primary" onClick={() => navigate(createLink)}>Add</Button>
        ) : createAttributeMapper.length > 0 && addFunction ? (
          <FormModal
            title={`Add ${modalTitle ?? ''}`}
            button={<Button type="primary" loading={loading}>Add</Button>}
            submitFunction={addFunction}
            attributeMapper={createAttributeMapper}
            formId="DataTableAddModalFormId"
          />
        ) : null}
      </Space>
    </div>
  );
}