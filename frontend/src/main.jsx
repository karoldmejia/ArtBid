import React from "react";
import { createRoot } from "react-dom/client";
import { Provider, useSelector } from "react-redux"; 
import { PersistGate } from "redux-persist/integration/react";
import store, { persistor } from "./features/store";
import App from "./App.jsx";
import { useEffect } from "react";

import "./index.css";

// Componente de debug opcional
function DebugApp() {
    const token = useSelector((state) => state.user.token);
    const role = useSelector((state) => state.user.role);

    useEffect(() => {
        console.log("ArtBid App mounted");
        console.log("Token:", token ? "Presente" : "No hay token");
        console.log("Role:", role);
    }, [token, role]);

    return <App />;
}

const root = createRoot(document.getElementById("root"));

root.render(
    <Provider store={store}>
        <PersistGate loading={<div>Cargando sesión...</div>} persistor={persistor}>
            <DebugApp />
        </PersistGate>
    </Provider>
);