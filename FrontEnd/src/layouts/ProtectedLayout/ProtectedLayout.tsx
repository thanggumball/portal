import { useMemo, useState } from 'react';
import { Layout, Menu, Breadcrumb, Button } from 'antd';
import type { MenuProps } from 'antd';
import { Link, Outlet, useLocation, useMatches } from 'react-router';
import path from '../../constants/path';
import { navConfig } from '../../constants/nav';
import { isLeaf, type NavItem } from '../../types/sidebar/nav';

const { Header, Sider, Content } = Layout;
const MOCK_USERNAME = 'Some username';

type RouteHandle = { breadcrumb?: string };

function toMenuItems(items: NavItem[]): MenuProps['items'] {
  return items.map((item) =>
    isLeaf(item)
      ? { key: item.path, label: <Link to={item.path}>{item.label}</Link> }
      : { key: item.key, label: item.label, children: toMenuItems(item.children) }
  );
}

export default function ProtectedLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();
  const matches = useMatches();

  const menuItems = useMemo(() => toMenuItems(navConfig), []);

  const breadcrumbItems = useMemo(
    () =>
      matches
        .map((m) => (m.handle as RouteHandle | undefined)?.breadcrumb)
        .filter((label): label is string => Boolean(label))
        .map((label) => ({ title: label })),
    [matches]
  );

  const defaultOpenKeys = navConfig.filter((i) => !isLeaf(i)).map((i) => i.key);

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', gap: 32, padding: '0 24px', background: '#d9d9d9' }}>
        <Link to={path.home} style={{ fontSize: 24, fontWeight: 'bold', color: '#000' }}>LOGO</Link>
        <Breadcrumb items={breadcrumbItems} style={{ flex: 1 }} />
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span>{MOCK_USERNAME}</span>
          <Link to={path.home}><Button type="link">Logout</Button></Link>
        </div>
      </Header>
      <Layout>
        <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed}>
          <Menu mode="inline" theme="dark" selectedKeys={[location.pathname]} defaultOpenKeys={defaultOpenKeys} items={menuItems} />
        </Sider>
        <Content style={{ padding: 24, background: '#fff' }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}