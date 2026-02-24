import React from "react";
import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import Auth from "./pages/Auth";
import Auctions from "./pages/Auctions";
import AuctionDetail from "./pages/AuctionDetail";
import MyBids from "./pages/MyBids";
import Profile from "./pages/Profile";
import ProtectedRoute from "./components/ProtectedRoute";
import PublicRoute from "./components/PublicRoute";
import Navbar from "./components/Navbar";

// Layout que incluye Navbar para rutas protegidas
function ProtectedLayout() {
    return (
        <ProtectedRoute>
            <>
                <Navbar />
                <Outlet />
            </>
        </ProtectedRoute>
    );
}

// Layout público (sin Navbar)
function PublicLayout() {
    return (
        <PublicRoute>
            <Outlet />
        </PublicRoute>
    );
}

const router = createBrowserRouter([
    {
        path: "/",
        Component: () => <Navigate to="/auctions" replace />,
    },
    {
        path: "/auth",
        Component: PublicLayout,
        children: [{ index: true, Component: Auth }],
    },
    {
        path: "/",
        Component: ProtectedLayout,
        children: [
            { path: "auctions", Component: Auctions },
            { path: "auctions/:id", Component: AuctionDetail },
            { path: "my-bids", Component: MyBids },
            { path: "profile", Component: Profile },
        ],
    },
]);

export default router;