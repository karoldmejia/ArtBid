import React from "react";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { jwtDecode } from "jwt-decode";
import { setUserData } from "../features/userSlice";
import "../styles/auth.css";
import FloatingInput from "../components/FloatingInput";
import VerticalCarousel from "../components/VerticalCarousel";

export default function Auth() {
    // Estados para alertas
    const [signupAlert, setSignupAlert] = useState({
        type: "",     // "success" o "error"
        message: "",
        show: false
    });
    const [loginAlert, setLoginAlert] = useState({
        type: "",     // "success" o "error"
        message: "",
        show: false
    });

    // Auto-cerrar alertas después de 9 segundos
    useEffect(() => {
        if (signupAlert.show) {
            const timer = setTimeout(() => {
                setSignupAlert({ ...signupAlert, show: false });
            }, 9000);
            return () => clearTimeout(timer);
        }
    }, [signupAlert]);

    useEffect(() => {
        if (loginAlert.show) {
            const timer = setTimeout(() => {
                setLoginAlert({ ...loginAlert, show: false });
            }, 9000);
            return () => clearTimeout(timer);
        }
    }, [loginAlert]);

    // Estados para el formulario activo (signup/signin)
    const [isActive, setIsActive] = useState(false);
    const handleRegisterClick = () => setIsActive(true);
    const handleLoginClick = () => setIsActive(false);

    // Estados para los datos de los formularios
    const [signUpData, setSignUpData] = useState({
        username: "",
        email: "",
        password: "",
    });

    const [loginData, setLoginData] = useState({
        email: "",
        password: ""
    });

    const dispatch = useDispatch();
    const navigate = useNavigate();

    // Manejador para registro
    const handleSignup = async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("http://localhost:5000/artbid/auth/signup", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify({
                    username: signUpData.username,
                    email: signUpData.email,
                    password: signUpData.password,
                })
            });

            if (response.ok) {
                const data = await response.json();
                
                // Decodificar el token para obtener información del usuario
                const decoded = jwtDecode(data.token);
                
                // Guardar datos del usuario en Redux
                dispatch(setUserData({
                    token: data.token,
                    id: decoded.nameidentifier, // ClaimTypes.NameIdentifier
                    email: decoded.email,
                    username: decoded.name,
                    role: "USER" // Por defecto, puedes ajustar según tu backend
                }));
                
                setSignupAlert({
                    type: "success",
                    message: "¡Cuenta creada con éxito!",
                    show: true
                });
                
                // Redirigir al home después de registro exitoso
                setTimeout(() => navigate("/home"), 1500);
            } else {
                const errorText = await response.text();
                setSignupAlert({
                    type: "error",
                    message: errorText || "Error al crear la cuenta",
                    show: true
                });
            }
        } catch (error) {
            setSignupAlert({
                type: "error",
                message: "¡Algo salió mal! Vuelve a intentarlo más tarde",
                show: true
            });
        }
    };

    // Manejador para inicio de sesión
    const handleLogin = async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("http://localhost:5000/artbid/auth/signin", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify({
                    email: loginData.email,
                    password: loginData.password,
                })
            });

if (response.ok) {
    const data = await response.json();
    const decoded = jwtDecode(data.token);
    console.log("Token decodificado:", decoded);
    
    // Usar los nombres correctos según el token
    dispatch(setUserData({
        token: data.token,
        id: decoded.nameid || decoded.nameidentifier, // Cambiado aquí también
        email: decoded.email,
        username: decoded.unique_name || decoded.name, // Cambiado aquí también
        role: "USER"
    }));
    
                
                setLoginAlert({
                    type: "success",
                    message: "¡Inicio de sesión exitoso!",
                    show: true
                });
                
                // Redirigir al home
                navigate("/home");
            } else {
                const errorText = await response.text();
                setLoginAlert({
                    type: "error",
                    message: errorText || "Credenciales inválidas",
                    show: true
                });
            }
        } catch (error) {
            setLoginAlert({
                type: "error",
                message: "¡Algo salió mal! Vuelve a intentarlo más tarde",
                show: true
            });
        }
    };

    return (
        <div className={`container ${isActive ? "active" : ""}`} id="container">
            {/* SIGN UP */}
            <div className="form-container sign-up">
                <form onSubmit={handleSignup}>
                    <h1>Únete al mercado artístico</h1>

                    <FloatingInput
                        type="text"
                        id="username"
                        name="username"
                        label="Nombre de usuario"
                        value={signUpData.username}
                        onChange={(e) => setSignUpData({...signUpData, username: e.target.value})}
                        required
                    />

                    <FloatingInput
                        type="email"
                        id="email"
                        name="email"
                        label="Correo electrónico"
                        value={signUpData.email}
                        onChange={(e) => setSignUpData({...signUpData, email: e.target.value})}
                        required
                    />

                    <FloatingInput
                        type="password"
                        id="password"
                        name="password"
                        label="Contraseña"
                        value={signUpData.password}
                        onChange={(e) => setSignUpData({...signUpData, password: e.target.value})}
                        required
                    />

                    {signupAlert.show && (
                        <div className={`form-alert ${signupAlert.type}`}>
                            {signupAlert.message}
                        </div>
                    )}

                    <button type="submit">Regístrate</button>
                    <a href="#" onClick={(e) => {e.preventDefault(); handleLoginClick();}}>
                        ¿Ya tienes una cuenta? Inicia sesión
                    </a>
                </form>
            </div>

            {/* SIGN IN */}
            <div className="form-container sign-in">
                <form onSubmit={handleLogin}>
                    <h1>Adéntrate en la cultura</h1>
                    
                    <FloatingInput
                        type="email"
                        id="email"
                        name="email"
                        label="Correo electrónico"
                        value={loginData.email}
                        onChange={(e) => setLoginData({...loginData, email: e.target.value})}
                        required
                    />

                    <FloatingInput
                        type="password"
                        id="password"
                        name="password"
                        label="Contraseña"
                        value={loginData.password}
                        onChange={(e) => setLoginData({...loginData, password: e.target.value})}
                        required
                    />

                    {loginAlert.show && (
                        <div className={`form-alert ${loginAlert.type}`}>
                            {loginAlert.message}
                        </div>
                    )}

                    <button type="submit">Ingresar</button>
                    <a href="#" onClick={(e) => {e.preventDefault(); handleRegisterClick();}}>
                        ¿No tienes una cuenta? Regístrate
                    </a>
                </form>
            </div>

            {/* TOGGLE */}
            <div className="toggle-container">
                <div className="toggle">
                    <VerticalCarousel />
                </div>
            </div>
        </div>
    );
}