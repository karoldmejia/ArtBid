import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useSelector } from "react-redux";
import "../styles/profile.css";

export default function Profile() {
    const [userInfo, setUserInfo] = useState(null);
    const [participatedAuctions, setParticipatedAuctions] = useState([]);
    const [publishedAuctions, setPublishedAuctions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('participated'); // 'participated' o 'published'
    
    const token = useSelector((state) => state.user.token);
    const navigate = useNavigate();

    useEffect(() => {
        fetchUserData();
        fetchParticipatedAuctions();
        fetchPublishedAuctions();
    }, []);

    const fetchUserData = async () => {
        try {
            const response = await fetch("http://localhost:5000/artbid/users/me", {
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                setUserInfo(data);
            } else if (response.status === 401) {
                navigate("/auth");
            }
        } catch (error) {
            console.error("Error fetching user data:", error);
        }
    };

    const fetchParticipatedAuctions = async () => {
        try {
            const response = await fetch("http://localhost:5000/artbid/users/me/participated", {
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                setParticipatedAuctions(data);
            }
            setLoading(false);
        } catch (error) {
            console.error("Error fetching participated auctions:", error);
            setLoading(false);
        }
    };

    const fetchPublishedAuctions = async () => {
        try {
            const response = await fetch("http://localhost:5000/artbid/users/me/published", {
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                setPublishedAuctions(data);
            }
        } catch (error) {
            console.error("Error fetching published auctions:", error);
        }
    };

    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleDateString('es-ES', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    };

    const getStatusText = (status) => {
        // Si status es número (0,1,2,3)
        if (typeof status === 'number') {
            switch(status) {
                case 0: return 'Borrador';
                case 1: return 'Activa';
                case 2: return 'Finalizada';
                case 3: return 'Cancelada';
                default: return 'Desconocido';
            }
        }
        // Si status es string
        switch(status) {
            case 'Active': return 'Activa';
            case 'Ended': return 'Finalizada';
            case 'Cancelled': return 'Cancelada';
            case 'Draft': return 'Borrador';
            default: return status;
        }
    };

    const getStatusClass = (status) => {
        // Si status es número
        if (typeof status === 'number') {
            switch(status) {
                case 1: return 'status-active';
                case 2: return 'status-ended';
                case 3: return 'status-cancelled';
                case 0: return 'status-draft';
                default: return '';
            }
        }
        // Si status es string
        switch(status) {
            case 'Active': return 'status-active';
            case 'Ended': return 'status-ended';
            case 'Cancelled': return 'status-cancelled';
            case 'Draft': return 'status-draft';
            default: return '';
        }
    };

    const handleAuctionClick = (auctionId) => {
        navigate(`/auctions/${auctionId}`);
    };

    if (loading) {
        return <div className="loading-profile">Cargando perfil...</div>;
    }

    return (
        <div className="profile-page">
            <div className="profile-header">
                <div className="profile-avatar">
                    {userInfo?.username?.charAt(0).toUpperCase()}
                </div>
                <div className="profile-info">
                    <p className="profile-email">{userInfo?.email}</p>
                    
                    <div className="profile-stats">
                            <div className="stat-item">
                            <span className="stat-label">Usuario</span>
                            <p className="stat-value">{userInfo?.username}</p>
                        </div>
                        <div className="stat-item">
                            <span className="stat-label">Estado</span>
                            <span className={`stat-value status-badge ${userInfo?.isActive ? 'inactive' : 'active'}`}>
                                {userInfo?.isActive ? 'Inactivo' : 'Activo'}
                            </span>
                        </div>
                        <div className="stat-item">
                            <span className="stat-label">Balance</span>
                            <span className="stat-value balance">${userInfo?.balance}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div className="profile-tabs">
                <button 
                    className={`tab-button ${activeTab === 'participated' ? 'active' : ''}`}
                    onClick={() => setActiveTab('participated')}
                >
                    Subastas en que participé ({participatedAuctions.length})
                </button>
                <button 
                    className={`tab-button ${activeTab === 'published' ? 'active' : ''}`}
                    onClick={() => setActiveTab('published')}
                >
                    Subastas publicadas ({publishedAuctions.length})
                </button>
            </div>

            <div className="auctions-section">
                {activeTab === 'participated' && (
                    <div className="auctions-grid-profile">
                        {participatedAuctions.length > 0 ? (
                            participatedAuctions.map((auction) => (
                                <div 
                                    key={auction.id} 
                                    className="auction-card-profile"
                                    onClick={() => handleAuctionClick(auction.id)}
                                >
                                    <div className="auction-image-container">
                                        <img 
                                            src={auction.photo || "/default-art.jpg"} 
                                            alt={auction.title}
                                            className="auction-image-profile"
                                        />
                                        <div className={`auction-status-badge ${getStatusClass(auction.status)}`}>
                                            {getStatusText(auction.status)}
                                        </div>
                                    </div>
                                    <div className="auction-details">
                                        <h3 className="auction-title">{auction.title}</h3>
                                        <p className="auction-artwork">{auction.artworkName}</p>
                                        <div className="auction-meta">
                                            <span className="current-price">${auction.currentPrice}</span>
                                            <span className="bid-count">{auction.bidCount} ofertas</span>
                                        </div>
                                        <div className="auction-footer-profile">
                                            <span className="end-date">Cierra: {formatDate(auction.endTime)}</span>
                                            {auction.winnerId && (
                                                <span className="winner-badge">¡Ganador!</span>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))
                        ) : (
                            <p className="no-auctions">No has participado en ninguna subasta aún</p>
                        )}
                    </div>
                )}

                {activeTab === 'published' && (
                    <div className="auctions-grid-profile">
                        {publishedAuctions.length > 0 ? (
                            publishedAuctions.map((auction) => (
                                <div 
                                    key={auction.id} 
                                    className="auction-card-profile"
                                    onClick={() => handleAuctionClick(auction.id)}
                                >
                                    <div className="auction-image-container">
                                        <img 
                                            src={auction.photo || "/default-art.jpg"} 
                                            alt={auction.title}
                                            className="auction-image-profile"
                                        />
                                        <div className={`auction-status-badge ${getStatusClass(auction.status)}`}>
                                            {getStatusText(auction.status)}
                                        </div>
                                    </div>
                                    <div className="auction-details">
                                        <h3 className="auction-title">{auction.title}</h3>
                                        <p className="auction-artwork">{auction.artworkName}</p>
                                        <div className="auction-meta">
                                            <span className="current-price">${auction.currentPrice}</span>
                                            <span className="bid-count">{auction.bidCount} ofertas</span>
                                        </div>
                                        <div className="auction-footer-profile">
                                            <span className="end-date">Finaliza: {formatDate(auction.endTime)}</span>
                                            {auction.winnerId && (
                                                <span className="winner-info">Ganador: {auction.winnerId.substring(0, 8)}...</span>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))
                        ) : (
                            <p className="no-auctions">No has publicado ninguna subasta aún</p>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}