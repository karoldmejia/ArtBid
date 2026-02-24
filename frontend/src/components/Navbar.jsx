import React from "react";
import "../styles/topbar.css";
import { useDispatch } from "react-redux";
import { useNavigate } from "react-router-dom";
import { logout } from "../features/userSlice";

export default function Navbar() {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const handleLogout = () => {
        dispatch(logout());
        localStorage.removeItem("token");
        navigate("/auth");
    };

    return (
        <div className="topbar">
            {/* Sección izquierda - GALLERY y ABOUT ME */}
            <div className="left-nav">
                <span className="nav-link" onClick={() => navigate("/auctions")}>
                    GALLERY
                </span>
                <span className="nav-link" onClick={() => navigate("/profile")}>
                    ABOUT ME
                </span>
            </div>

            {/* Título central */}
            <div className="center-title">
                <h1>ARTBID</h1>
            </div>

            {/* Sección derecha - LOGOUT */}
            <div className="right-nav">
                <span className="nav-link logout" onClick={handleLogout}>
                    LOGOUT
                </span>
            </div>
        </div>
    );
}