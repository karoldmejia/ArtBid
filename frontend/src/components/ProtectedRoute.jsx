import React from "react";

import { Navigate } from "react-router-dom";
import { useSelector } from "react-redux";

export default function ProtectedRoute({ children }) {
    const token = useSelector((state) => state.user.token);

    if (!token) {
        // Si no está autenticado, redirige a login
        return <Navigate to="/auth" replace />;
    }

    // Si está autenticado, muestra el contenido
    return children;
}
