import { Layout, Button } from 'antd';
import { Link, Outlet } from 'react-router';
import path from '../../constants/path';

const { Header, Content } = Layout;

export default function MainLayout() {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', padding: '0 24px', background: '#d9d9d9' }}>
        <Link to={path.home} style={{ fontSize: 24, fontWeight: 'bold', color: '#000', marginRight: 'auto' }}>
          LOGO
        </Link>
        <Link to={path.login}>
          <Button type="primary">Login</Button>
        </Link>
      </Header>
      <Content style={{ padding: 24 }}>
        <Outlet />
      </Content>
    </Layout>
  );
}