import { useState, cloneElement } from 'react';
import type { ReactElement } from 'react';
import { Modal, Button, Form } from 'antd';
import SimpleForm from './SimpleForm';
import type { Attribute } from '../../types/components/attribute';

interface FormModalProps {
  title: string;
  button: ReactElement;
  submitFunction: (values: Record<string, unknown>) => void;
  data?: Record<string, unknown>;
  attributeMapper: Attribute[];
  formId: string;
  deletion?: boolean;
  closeOnFinish?: boolean;
  submitButtonContent?: string;
  extraSubmitData?: Record<string, unknown>;
  loading?: boolean;
}

const groupIntoRows = (attrs: Attribute[]): Attribute[][] =>
  attrs.reduce<Attribute[][]>((rows, a, i) => {
    if (i % 2 === 0) rows.push([]);
    rows[rows.length - 1].push(a);
    return rows;
  }, []);

export default function FormModal({
  title, button, submitFunction, data, attributeMapper, formId,
  deletion = false, closeOnFinish = false, submitButtonContent,
  extraSubmitData = {}, loading = false,
}: FormModalProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [form] = Form.useForm();

  const rows = groupIntoRows(attributeMapper);

  const handleOpen  = () => setIsOpen(true);
  const handleClose = () => setIsOpen(false);
  const handleReset = () => form.resetFields();
  const handleSubmit = () => form.submit();

  const handleFinish = (values: Record<string, unknown>) => {
    submitFunction(values);
    if (closeOnFinish) setIsOpen(false);
  };

  return (
    <>
      <span onClick={handleOpen} style={{ display: 'inline-block' }}>
        {cloneElement(button)}
      </span>
      <Modal
        title={<h4 style={{ textAlign: 'center', marginBottom: '2rem' }}>{title}</h4>}
        open={isOpen}
        onCancel={handleClose}
        width="50%"
        footer={[
          <Button key="cancel" style={{ marginRight: '1rem' }}
            onClick={deletion ? handleClose : handleReset} disabled={loading}>
            Cancel
          </Button>,
          <Button key="submit" type="primary" danger={deletion}
            onClick={handleSubmit} disabled={loading}>
            {submitButtonContent ?? (deletion ? 'Delete' : 'Confirm')}
          </Button>,
        ]}
      >
        <SimpleForm
          form={form}
          attributeMapper={rows}
          data={data}
          readonly={deletion}
          onFinish={handleFinish}
          extraSubmitData={extraSubmitData}
          formId={formId}
        />
      </Modal>
    </>
  );
}