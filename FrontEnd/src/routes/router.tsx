import { createBrowserRouter } from 'react-router';
import { lazy } from 'react';
import path from '../constants/path';
import MainLayout from '../layouts/MainLayout';
import ProtectedLayout from '../layouts/ProtectedLayout';

const Home  = lazy(() => import('../pages/Home'));
const Login = lazy(() => import('../pages/Login'));
const SignUp = lazy(() => import('../pages/SignUp'));

const AnnouncementPublish = () => <div>Announcement / Publish</div>;
const AnnouncementManage  = () => <div>Announcement / Manage</div>;
const UsersManage = lazy(() => import('../pages/UserManage'));

export const router = createBrowserRouter([
  {
    element: <MainLayout />,
    children: [
      { path: path.home, index: true, element: <Home /> },
      { path: path.login, element: <Login /> },
      { path: path.signup, element: <SignUp /> },
    ],
  },
  {
    element: <ProtectedLayout />,
    children: [
      {
        path: path.announcement.root,
        handle: { breadcrumb: 'Announcement' },
        children: [
          { path: path.announcement.publish, handle: { breadcrumb: 'Publish' }, element: <AnnouncementPublish /> },
          { path: path.announcement.manage,  handle: { breadcrumb: 'Manage'  }, element: <AnnouncementManage  /> },
        ],
      },
      {
        path: path.users.root,
        handle: { breadcrumb: 'Users' },
        children: [
          { path: path.users.manage, handle: { breadcrumb: 'Manage' }, element: <UsersManage /> },
        ],
      },
    ],
  },
]);
