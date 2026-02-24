import React, { useEffect, useState, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useSelector } from "react-redux";
import "../styles/auctionDetail.css";

export default function AuctionDetail() {
    const { id } = useParams();
    const navigate = useNavigate();
    const token = useSelector((state) => state.user.token);
    const pollingRef = useRef(null);
    
    const [auction, setAuction] = useState(null);
    const [loading, setLoading] = useState(true);
    const [bidAmount, setBidAmount] = useState("");
    const [bidError, setBidError] = useState("");
    const [bidSuccess, setBidSuccess] = useState("");
    const [winMessage, setWinMessage] = useState(""); // Nuevo estado para mensaje de victoria
    const [timeLeft, setTimeLeft] = useState("");
    const [showBidHistory, setShowBidHistory] = useState(false);
    const [isParticipant, setIsParticipant] = useState(false);
    const [currentUserId, setCurrentUserId] = useState(null); // Estado para el ID del usuario actual

    // Función para obtener detalles de la subasta
    const fetchAuctionDetail = async () => {
        try {
            const response = await fetch(`http://localhost:5000/artbid/auctions/${id}`, {
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                
                // Obtener userId del token
                try {
                    const payload = JSON.parse(atob(token.split('.')[1]));
                    const userId = payload.sub;
                    setCurrentUserId(userId);

                    if (userId && data.participants) {
                        setIsParticipant(data.participants.some(p => p.id === userId));
                    }

                    // Verificar si el usuario actual es el ganador
                    if (data.status !== 1 && data.winnerId === userId) {
                        setWinMessage("¡Felicidades! Has ganado esta subasta");
                        // El mensaje se limpia automáticamente después de 10 segundos
                    } else if (data.status !== 1 && data.winnerId === userId && !winMessage) {
                        // Si ya estaba ganada pero no se mostró el mensaje
                        setWinMessage("¡Felicidades! Has ganado esta subasta");
                    }
                } catch (e) {
                    console.error("Error decodificando token:", e);
                }
                
                setAuction(data);
            } else if (response.status === 401) {
                console.log("Token no válido, redirigiendo...");
                navigate("/auth");
            }
            setLoading(false);
        } catch (error) {
            console.error("Error fetching auction detail:", error);
            setLoading(false);
        }
    };

    // Efecto para polling
    useEffect(() => {
        // Primera carga
        fetchAuctionDetail();
        
        // Configurar polling cada 3 segundos
        pollingRef.current = setInterval(() => {
            fetchAuctionDetail();
        }, 3000);

        // Limpiar polling al desmontar o cambiar id
        return () => {
            if (pollingRef.current) {
                clearInterval(pollingRef.current);
            }
        };
    }, [id, token, navigate]);

    // Efecto para el tiempo restante (se ejecuta cada segundo)
    useEffect(() => {
        if (auction) {
            calculateTimeLeft();
            const timer = setInterval(() => {
                calculateTimeLeft();
            }, 1000);

            return () => clearInterval(timer);
        }
    }, [auction]);

    const calculateTimeLeft = () => {
        if (!auction) return;

        const endTime = new Date(auction.endTime);
        const now = new Date();
        const diff = endTime - now;

        if (diff <= 0) {
            setTimeLeft("Subasta finalizada");
            return;
        }

        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);

        if (days > 0) {
            setTimeLeft(`${days}d ${hours}h ${minutes}m ${seconds}s`);
        } else if (hours > 0) {
            setTimeLeft(`${hours}h ${minutes}m ${seconds}s`);
        } else if (minutes > 0) {
            setTimeLeft(`${minutes}m ${seconds}s`);
        } else {
            setTimeLeft(`${seconds}s`);
        }
    };

    const handleBidSubmit = async (e) => {
        e.preventDefault();
        setBidError("");
        setBidSuccess("");

        const amount = parseFloat(bidAmount);
        if (isNaN(amount) || amount <= 0) {
            setBidError("Por favor ingresa un monto válido");
            return;
        }

        if (amount <= auction.currentPrice) {
            setBidError(`El monto debe ser mayor a $${auction.currentPrice}`);
            return;
        }

        try {
            const response = await fetch(`http://localhost:5000/artbid/auctions/${id}/bid`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify(amount)
            });

            if (response.ok) {
                setBidSuccess("¡Oferta realizada con éxito!");
                setBidAmount("");
                // Actualizar inmediatamente después de ofertar
                await fetchAuctionDetail();
                
                setTimeout(() => setBidSuccess(""), 3000);
            } else {
                const errorText = await response.text();
                setBidError(errorText);
            }
        } catch (error) {
            setBidError("Error al realizar la oferta. Intenta nuevamente.");
        }
    };

    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleString('es-ES', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    };

    const getStatusClass = (status) => {
        switch (status) {
            case 'Active': return 'status-active';
            case 'Ended': return 'status-ended';
            case 'Cancelled': return 'status-cancelled';
            default: return '';
        }
    };

    const getStatusText = (status) => {
        switch (status) {
            case 1: return 'Activa';
            case 2: return 'Finalizada';
            case 3: return 'Cancelada';
            default: return status;
        }
    };

    if (loading) {
        return <div className="loading-detail">Cargando detalles de la subasta...</div>;
    }

    if (!auction) {
        return <div className="error-detail">No se encontró la subasta</div>;
    }

    return (
        <div className="auction-detail-page">
            {/* Lado izquierdo - Imagen fija */}
            <div className="detail-left">
                <img
                    src={auction.photo || "/default-art.jpg"}
                    alt={auction.artworkName}
                    className="detail-image"
                />
            </div>

            {/* Lado derecho - Contenido scrolleable */}
            <div className="detail-right">
                <div className="detail-content">
                    {/* Título pequeño de subasta */}
                    <div className="auction-subtitle">{auction.title}</div>

                    {/* Título grande de obra */}
                    <h1 className="artwork-title">{auction.artworkName}</h1>

                    {/* Nombre del autor */}
                    <h2 className="artwork-author-detail">{auction.artworkAuthor}</h2>

                    {/* Descripción */}
                    <p className="artwork-description">{auction.description}</p>

                    {/* Precio inicial */}
                    <div className="starting-price">
                        <span className="label">Precio inicial:</span>
                        <span className="value">${auction.startingPrice}</span>
                    </div>

                    {/* Info de subasta en vivo */}
                    <div className="live-auction-info">
                        <div className="price-section">
                            <span className="label">Precio actual</span>
                            <span className="current-price-large">${auction.currentPrice}</span>
                        </div>

                        <div className="time-section">
                            <span className="label">Tiempo restante</span>
                            <span className="time-value">{timeLeft}</span>
                        </div>

                        <div className="bids-section">
                            <span className="label">Ofertas</span>
                            <span className="bids-count">{auction.bids?.length || 0}</span>
                        </div>
                    </div>

                    {/* Mensaje de ganador */}
                    {winMessage && (
                        <div className="winner-message">
                            {winMessage}
                        </div>
                    )}

                    {/* Panel de ofertas */}
                    {auction.status === 1 && (
                        <div className="bid-panel">
                            <form onSubmit={handleBidSubmit} className="bid-form">
                                <div className="bid-input-group">
                                    <input
                                        type="number"
                                        step="0.01"
                                        min={auction.currentPrice + 0.01}
                                        value={bidAmount}
                                        onChange={(e) => setBidAmount(e.target.value)}
                                        placeholder="Ingresa tu oferta"
                                        className="bid-input"
                                        required
                                    />
                                    <button type="submit" className="bid-button">
                                        Ofertar
                                    </button>
                                </div>
                            </form>

                            {bidError && (
                                <div className="bid-message error">
                                    {bidError}
                                </div>
                            )}

                            {bidSuccess && (
                                <div className="bid-message success">
                                    {bidSuccess}
                                </div>
                            )}

                            {bidError && bidError.includes("superada") && (
                                <div className="bid-message warning">
                                    Has sido superado. ¡Anímate a ofertar más!
                                </div>
                            )}
                        </div>
                    )}

                    {/* Historial de ofertas */}
                    {auction.bids && auction.bids.length > 0 && (
                        <div className="bids-history">
                            <div
                                className="history-header"
                                onClick={() => setShowBidHistory(!showBidHistory)}
                            >
                                <h3>Historial de ofertas</h3>
                                <span className={`toggle-icon ${showBidHistory ? 'expanded' : ''}`}>
                                    ▼
                                </span>
                            </div>

                            {showBidHistory && (
                                <div className="bids-table">
                                    <table>
                                        <thead>
                                            <tr>
                                                <th>Monto</th>
                                                <th>Fecha y hora</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {[...auction.bids]
                                                .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
                                                .map((bid, index) => (
                                                    <tr key={bid.id || index} className="bid-row">
                                                        <td className="bid-amount">${bid.amount}</td>
                                                        <td>{formatDate(bid.timestamp)}</td>
                                                    </tr>
                                                ))}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                        </div>
                    )}

                    {/* Participantes (solo visible si el usuario participó) */}
                    {isParticipant && auction.participants && auction.participants.length > 0 && (
                        <div className="participants-section">
                            <h3>Participantes en esta subasta</h3>
                            <div className="participants-list">
                                {auction.participants.map((participant, index) => (
                                    <div key={index} className="participant-item">
                                        <span className="participant-name">
                                            {participant.username || participant.id.substring(0, 8)}
                                        </span>
                                        <span className="participant-joined">
                                            Se unió: {formatDate(participant.joinedAt)}
                                        </span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}