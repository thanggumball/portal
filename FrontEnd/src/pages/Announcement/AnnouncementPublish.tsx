import { useState } from 'react';
import { Button, Form, Input, Select, Switch, DatePicker, Card, Typography, Space, Divider } from 'antd';
import { SendOutlined, ClearOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { GlobalToast } from '../../components/Toast/GlobalToast';

import './AnnouncementPublish.scss';

const { Title, Text } = Typography;
const { TextArea } = Input;

const categoryOptions = [
  { value: 'General',  label: 'General'  },
  { value: 'Academic', label: 'Academic' },
  { value: 'Event',    label: 'Event'    },
  { value: 'Urgent',   label: 'Urgent'   },
];

const priorityOptions = [
  { value: 'Low',    label: 'Low'    },
  { value: 'Medium', label: 'Medium' },
  { value: 'High',   label: 'High'   },
];

const audienceOptions = [
  { value: 'all',      label: 'All Users'  },
  { value: 'students', label: 'Students'   },
  { value: 'admins',   label: 'Admins'     },
];

export default function AnnouncementPublish() {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [preview, setPreview] = useState(false);

  const handlePublish = (values: Record<string, unknown>) => {
    setLoading(true);

    // Simulate API call
    setTimeout(() => {
      console.log('Published announcement:', values);
      GlobalToast.success('Announcement published successfully!');
      form.resetFields();
      setPreview(false);
      setLoading(false);
    }, 1000);
  };

  const handlePreview = () => {
    form
      .validateFields()
      .then(() => setPreview(true))
      .catch(() => GlobalToast.warning('Please fill in all required fields before previewing.'));
  };

  const formValues = Form.useWatch([], form);

  return (
    <div className="announcement-publish">
      <div className="announcement-publish__grid">
        {/* ───────── Form Section ───────── */}
        <Card className="announcement-publish__form-card">
          <Title level={4} style={{ marginBottom: '1.5rem' }}>
            Create Announcement
          </Title>

          <Form
            form={form}
            layout="vertical"
            onFinish={handlePublish}
            initialValues={{
              category: 'General',
              priority: 'Medium',
              audience: 'all',
              isActive: true,
              scheduledAt: null,
            }}
          >
            <Form.Item
              label="Title"
              name="title"
              rules={[{ required: true, message: 'Please enter a title' }]}
            >
              <Input placeholder="Enter announcement title" maxLength={200} showCount />
            </Form.Item>

            <Form.Item
              label="Content"
              name="content"
              rules={[{ required: true, message: 'Please enter the content' }]}
            >
              <TextArea
                placeholder="Write the announcement content…"
                rows={6}
                maxLength={2000}
                showCount
              />
            </Form.Item>

            <div className="announcement-publish__row">
              <Form.Item label="Category" name="category" style={{ flex: 1 }}>
                <Select options={categoryOptions} />
              </Form.Item>
              <Form.Item label="Priority" name="priority" style={{ flex: 1 }}>
                <Select options={priorityOptions} />
              </Form.Item>
            </div>

            <div className="announcement-publish__row">
              <Form.Item label="Audience" name="audience" style={{ flex: 1 }}>
                <Select options={audienceOptions} />
              </Form.Item>
              <Form.Item label="Schedule" name="scheduledAt" style={{ flex: 1 }}>
                <DatePicker
                  showTime
                  format="DD-MM-YYYY HH:mm"
                  style={{ width: '100%' }}
                  placeholder="Publish immediately"
                  disabledDate={(current) => current && current < dayjs().startOf('day')}
                />
              </Form.Item>
            </div>

            <Form.Item label="Active" name="isActive" valuePropName="checked">
              <Switch checkedChildren="On" unCheckedChildren="Off" />
            </Form.Item>

            <Divider />

            <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
              <Button
                icon={<ClearOutlined />}
                onClick={() => { form.resetFields(); setPreview(false); }}
                disabled={loading}
              >
                Reset
              </Button>
              <Button onClick={handlePreview} disabled={loading}>
                Preview
              </Button>
              <Button
                type="primary"
                htmlType="submit"
                icon={<SendOutlined />}
                loading={loading}
              >
                Publish
              </Button>
            </Space>
          </Form>
        </Card>

        {/* ───────── Preview Section ───────── */}
        <Card className="announcement-publish__preview-card">
          <Title level={4} style={{ marginBottom: '1.5rem' }}>
            Preview
          </Title>

          {preview && formValues ? (
            <div className="announcement-publish__preview-content">
              <div className="announcement-publish__preview-meta">
                <span
                  className={`announcement-publish__badge announcement-publish__badge--${String(formValues.category ?? 'General').toLowerCase()}`}
                >
                  {String(formValues.category ?? 'General')}
                </span>
                <span
                  className={`announcement-publish__badge announcement-publish__badge--priority-${String(formValues.priority ?? 'Medium').toLowerCase()}`}
                >
                  {String(formValues.priority ?? 'Medium')}
                </span>
              </div>
              <Title level={3} style={{ marginTop: '0.75rem' }}>
                {String(formValues.title ?? '')}
              </Title>
              <Text type="secondary" style={{ fontSize: '0.8rem' }}>
                {formValues.scheduledAt
                  ? `Scheduled: ${dayjs(formValues.scheduledAt as string).format('DD MMM YYYY, HH:mm')}`
                  : 'Published immediately'}
                {' · '}
                Audience: {audienceOptions.find((o) => o.value === formValues.audience)?.label ?? 'All'}
              </Text>
              <Divider style={{ margin: '0.75rem 0' }} />
              <div className="announcement-publish__preview-body">
                {String(formValues.content ?? '')}
              </div>
            </div>
          ) : (
            <div className="announcement-publish__preview-empty">
              <Text type="secondary">
                Fill out the form and click <strong>Preview</strong> to see how your announcement will look.
              </Text>
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
