import { useContext, useMemo, useState } from 'react';
import { Layout, Menu, Breadcrumb, Dropdown, Avatar, Switch, Space, theme } from 'antd';
import type { MenuProps } from 'antd';
import { Link, Outlet, useLocation, useMatches, useNavigate } from 'react-router';
import path from '../../constants/path';
import { navConfig } from '../../constants/nav';
import { isLeaf, type NavItem } from '../../types/sidebar/nav';
import { useThemeMode } from '../../contexts/ThemeContext';
import { AppContext } from '@/contexts/AppContext';
import { useAuth } from '@/hooks/auth.hook';

const { Header, Sider, Content } = Layout;

const SIDER_WIDTH = 200;
const SIDER_COLLAPSED_WIDTH = 80;

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
  const navigate = useNavigate();
  const { mode, toggle } = useThemeMode();
  const { token } = theme.useToken();
  const { user, resetAuth } = useContext(AppContext);
  const { logout, loading: logoutLoading } = useAuth()

  console.log("user: ", user)
  const handleLogout = async () => {
    await logout()
    resetAuth()
  }

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

  const logoWidth = collapsed ? SIDER_COLLAPSED_WIDTH : SIDER_WIDTH;

  const userMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      label: 'Profile',
      onClick: () => navigate(path.home)
    },
    {
      type: 'divider'
    },
    {
      key: 'logout',
      label: (
        <span style={{ color: token.colorError }}>
          {logoutLoading ? 'Logging out...' : 'Logout'}
        </span>
      ),
      disabled: logoutLoading,
      onClick: handleLogout
    }
  ]

  return (
    <Layout style={{ height: '100vh' }}>
      <Header
        style={{
          display: 'flex',
          alignItems: 'center',
          padding: 0,
          background: token.colorBgContainer,
          borderBottom: `1px solid ${token.colorBorderSecondary}`,
        }}
      >
        {/* Logo slot — matches sider width, collapses with it */}
        <div
          style={{
            width: logoWidth,
            flexShrink: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: collapsed ? 16 : 24,
            fontWeight: 'bold',
            color: token.colorText,
            transition: 'width 0.2s',
          }}
        >
          <Link to={path.home} style={{ color: 'inherit' }}>LOGO</Link>
        </div>

        {/* Content slot — starts where Outlet starts */}
        <div
          style={{
            flex: 1,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            padding: '0 24px',
          }}
        >
          <Breadcrumb items={breadcrumbItems} />
          <Space size="middle">
            <Switch
              checkedChildren="Dark"
              unCheckedChildren="Light"
              checked={mode === 'dark'}
              onChange={toggle}
            />
            <Dropdown menu={{ items: userMenuItems }} trigger={['click']} placement="bottomRight">
              <Avatar
                style={{
                  cursor: 'pointer',
                  backgroundColor: token.colorPrimary
                }}
              >
                {user?.userName?.charAt(0).toUpperCase()}
              </Avatar>
            </Dropdown>
          </Space>
        </div>
      </Header>

      <Layout>
        <Sider
          collapsible
          collapsed={collapsed}
          onCollapse={setCollapsed}
          width={SIDER_WIDTH}
          collapsedWidth={SIDER_COLLAPSED_WIDTH}
        >
          <Menu
            mode="inline"
            theme="dark"
            selectedKeys={[location.pathname]}
            defaultOpenKeys={defaultOpenKeys}
            items={menuItems}
          />
        </Sider>
        <Content
          style={{
            padding: 24,
            background: token.colorBgLayout,
            overflow: 'hidden',
          }}
        >
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}