import React, { useEffect, useState } from 'react';
import { X, AlertTriangle } from 'lucide-react';

export default function DeleteModal({ isOpen, onClose, onConfirm, title, message }) {
    const [isClosing, setIsClosing] = useState(false);

    useEffect(() => {
        if (isOpen) setIsClosing(false);
    }, [isOpen]);

    const handleClose = () => {
        setIsClosing(true);
        setTimeout(onClose, 200); // Wait for animation
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            {/* Backdrop */}
            <div
                className={`absolute inset-0 bg-black/40 backdrop-blur-sm transition-opacity duration-200 ${isClosing ? 'opacity-0' : 'opacity-100'}`}
                onClick={handleClose}
            />

            {/* Modal */}
            <div className={`relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6 transform transition-all duration-200 ${isClosing ? 'scale-95 opacity-0' : 'scale-100 opacity-100'}`}>
                <button
                    onClick={handleClose}
                    className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-full p-1 transition-colors"
                >
                    <X size={20} />
                </button>

                <div className="flex flex-col items-center text-center gap-4">
                    <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center text-red-600 mb-2">
                        <AlertTriangle size={24} />
                    </div>

                    <h3 className="text-xl font-bold text-gray-900">
                        {title || '¿Eliminar proyecto?'}
                    </h3>

                    <p className="text-gray-500">
                        {message || 'Esta acción no se puede deshacer. ¿Estás seguro de que quieres continuar?'}
                    </p>

                    <div className="flex gap-3 w-full mt-4">
                        <button
                            onClick={handleClose}
                            className="flex-1 px-4 py-2.5 rounded-xl border border-gray-200 text-gray-700 font-medium hover:bg-gray-50 transition-colors"
                        >
                            Cancelar
                        </button>
                        <button
                            onClick={() => {
                                onConfirm();
                                handleClose();
                            }}
                            className="flex-1 px-4 py-2.5 rounded-xl bg-red-600 text-white font-medium hover:bg-red-700 shadow-lg shadow-red-500/30 transition-all hover:-translate-y-0.5"
                        >
                            Sí, Eliminar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
