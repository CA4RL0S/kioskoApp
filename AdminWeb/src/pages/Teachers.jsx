import React, { useState, useEffect } from 'react';
import { Search, CheckCircle, XCircle, Trash2, User, ShieldCheck, ShieldAlert } from 'lucide-react';
import DeleteModal from '../components/DeleteModal';

export default function Teachers() {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [deleteModal, setDeleteModal] = useState({ isOpen: false, userId: null });

    const fetchUsers = async () => {
        try {
            const response = await fetch('http://localhost:5146/api/users');
            const data = await response.json();
            setUsers(data);
        } catch (error) {
            console.error('Error fetching users:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    const handleVerify = async (userId) => {
        try {
            const response = await fetch(`http://localhost:5146/api/users/${userId}/verify`, {
                method: 'PUT'
            });
            if (response.ok) {
                fetchUsers();
            } else {
                alert('Error al actualizar estado del usuario');
            }
        } catch (error) {
            console.error(error);
            alert('Error de conexión');
        }
    };

    const confirmDelete = (id) => {
        setDeleteModal({ isOpen: true, userId: id });
    };

    const handleDelete = async () => {
        const id = deleteModal.userId;
        if (!id) return;

        try {
            const response = await fetch(`http://localhost:5146/api/users/${id}`, {
                method: 'DELETE'
            });
            if (response.ok) {
                setUsers(users.filter(u => u.id !== id));
            } else {
                alert('Error al eliminar usuario');
            }
        } catch (error) {
            console.error(error);
            alert('Error de conexión');
        }
    };

    const filteredUsers = users.filter(user =>
        user.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.email?.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-12">
            <header className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                    <h2 className="text-3xl font-bold text-gray-900">Profesores / Evaluadores</h2>
                    <p className="text-gray-500">Gestiona el acceso de los evaluadores al sistema.</p>
                </div>

                <div className="relative w-full md:w-96">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
                    <input
                        type="text"
                        placeholder="Buscar por nombre o correo..."
                        className="w-full pl-10 pr-4 py-2 rounded-lg border border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all"
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                    />
                </div>
            </header>

            {loading ? (
                <div className="flex justify-center items-center h-64">
                    <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
                </div>
            ) : (
                <div className="bg-white rounded-xl shadow-xl shadow-gray-200/50 border border-gray-100 overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="w-full text-left border-collapse">
                            <thead>
                                <tr className="bg-gray-50 border-b border-gray-100 text-xs uppercase text-gray-500 font-semibold tracking-wider">
                                    <th className="p-5">Usuario</th>
                                    <th className="p-5">Departamento</th>
                                    <th className="p-5 text-center">Estado</th>
                                    <th className="p-5 text-right">Acciones</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {filteredUsers.length === 0 ? (
                                    <tr>
                                        <td colSpan="4" className="p-8 text-center text-gray-500">No se encontraron usuarios.</td>
                                    </tr>
                                ) : (
                                    filteredUsers.map((user) => (
                                        <tr key={user.id} className="hover:bg-gray-50/50 transition-colors group">
                                            <td className="p-5">
                                                <div className="flex items-center gap-3">
                                                    <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold shrink-0">
                                                        {user.profileImageUrl ? (
                                                            <img src={user.profileImageUrl} alt={user.fullName} className="w-full h-full rounded-full object-cover" />
                                                        ) : (
                                                            <User size={20} />
                                                        )}
                                                    </div>
                                                    <div>
                                                        <p className="font-semibold text-gray-900">{user.fullName || user.username}</p>
                                                        <p className="text-sm text-gray-500">{user.email}</p>
                                                    </div>
                                                </div>
                                            </td>
                                            <td className="p-5 text-sm text-gray-600">
                                                {user.department || 'N/A'}
                                            </td>
                                            <td className="p-5 text-center">
                                                {user.isVerified ? (
                                                    <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-green-50 text-green-700 border border-green-100">
                                                        <CheckCircle size={14} />
                                                        Verificado
                                                    </span>
                                                ) : (
                                                    <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-yellow-50 text-yellow-700 border border-yellow-100">
                                                        <ShieldCheck size={14} />
                                                        Pendiente
                                                    </span>
                                                )}
                                            </td>
                                            <td className="p-5 text-right">
                                                <div className="flex items-center justify-end gap-2">
                                                    <button
                                                        onClick={() => handleVerify(user.id)}
                                                        title={user.isVerified ? "Desverificar Usuario" : "Verificar Usuario"}
                                                        className={`p-2 rounded-lg transition-colors ${user.isVerified
                                                                ? 'bg-orange-50 text-orange-600 hover:bg-orange-100'
                                                                : 'bg-green-50 text-green-600 hover:bg-green-100'
                                                            }`}
                                                    >
                                                        {user.isVerified ? <ShieldAlert size={18} /> : <CheckCircle size={18} />}
                                                    </button>

                                                    <button
                                                        onClick={() => confirmDelete(user.id)}
                                                        title="Eliminar Usuario"
                                                        className="p-2 rounded-lg bg-red-50 text-red-600 hover:bg-red-100 transition-colors"
                                                    >
                                                        <Trash2 size={18} />
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}

            <DeleteModal
                isOpen={deleteModal.isOpen}
                onClose={() => setDeleteModal({ isOpen: false, userId: null })}
                onConfirm={handleDelete}
                title="¿Eliminar usuario?"
                message="Esta acción eliminará al profesor/evaluador permanentemente. Perderá el acceso al sistema."
            />
        </div>
    );
}
