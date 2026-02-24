import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useSelector } from "react-redux";
import "../styles/auctions.css";

export default function Auctions() {
    const [auctions, setAuctions] = useState([]);
    const [loading, setLoading] = useState(true);
    const token = useSelector((state) => state.user.token);
    const navigate = useNavigate();

    useEffect(() => {
        fetchActiveAuctions();
    }, []);

    const fetchActiveAuctions = async () => {
        try {
            const response = await fetch("http://localhost:5000/artbid/auctions/active", {
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                setAuctions(data);
            }
            setLoading(false);
        } catch (error) {
            console.error("Error fetching auctions:", error);
            setLoading(false);
        }
    };

    const handleAuctionClick = (auctionId) => {
        navigate(`/auctions/${auctionId}`);
    };

    if (loading) {
        return <div className="loading">Cargando subastas...</div>;
    }

    return (
        <div className="auctions-page">
            <div className="auctions-grid">
                {auctions.map((auction) => (
                    <div
                        key={auction.id}
                        className="auction-card"
                        onClick={() => handleAuctionClick(auction.id)}
                    >
                        <div className="image-container">
                            <img
                                src={auction.photo || "/default-art.jpg"}
                                alt={auction.title}
                                className="auction-image"
                            />
                            <div className="image-overlay"></div>
                            
                            <div className="hover-content">
                                <div className="hover-header">
                                    <h3 className="artwork-name">{auction.artworkName}</h3>
                                    <p className="artwork-author">{auction.artworkAuthor}</p>
                                </div>
                                
                                <div className="auction-footer">
                                    <span className="current-price">${auction.currentPrice}</span>
                                    <span className="bid-count">{auction.bidCount} pujas</span>
                                </div>
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}