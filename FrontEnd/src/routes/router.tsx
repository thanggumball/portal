import { createBrowserRouter } from "react-router";
import path from "../constants/path";
import MainLayout from "../layouts/MainLayout";
import { lazy } from "react";

const Home = lazy(() => import('../pages/Home'));
const Login = lazy(() => import('../pages/Login'));
const AnnounceMent = lazy(() => import('../pages/Announcement'))


export const router = createBrowserRouter([
    {
        path: path.home,
        element: <MainLayout />,
        children: [
            {
                index: true,
                element: <Home />
            },
            {
                path: path.announcement,
                element: <AnnounceMent />
            }
        ]
    }
])