import React, { useEffect, useState } from 'react';
import { Trash2, Edit, Plus, Search, RefreshCw, FolderOpen } from 'lucide-react';
import { Link } from 'react-router-dom';
import DeleteModal from '../components/DeleteModal';

export default function Dashboard() {
    const [projects, setProjects] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [deleteModal, setDeleteModal] = useState({ isOpen: false, projectId: null });

    const fetchProjects = async () => {
        setLoading(true);
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/projects`);
            if (response.ok) {
                const data = await response.json();
                setProjects(data);
            } else {
                console.error('Failed to fetch projects');
            }
        } catch (error) {
            console.error('Error fetching projects:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchProjects();
    }, []);

    const confirmDelete = (id) => {
        setDeleteModal({ isOpen: true, projectId: id });
    };

    const handleDelete = async () => {
        const id = deleteModal.projectId;
        if (!id) return;

        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/projects/${id}`, {
                method: 'DELETE',
            });
            if (response.ok) {
                setProjects(projects.filter(p => p.id !== id));
            } else {
                alert('Error al eliminar el proyecto');
            }
        } catch (error) {
            console.error('Error deleting project:', error);
            alert('Error de conexión');
        }
    };

    const filteredProjects = projects.filter(p =>
        p.title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.description?.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                    <h2 className="text-3xl font-bold text-gray-900 tracking-tight">Proyectos</h2>
                    <p className="text-gray-500 mt-1">Gestiona los proyectos del kiosko</p>
                </div>
                <Link
                    to="/add-project"
                    className="inline-flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-lg font-medium transition-all shadow-md hover:shadow-lg hover:-translate-y-0.5"
                >
                    <Plus size={20} />
                    Nuevo Proyecto
                </Link>
            </div>

            {/* Filters */}
            <div className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex gap-4 items-center">
                <div className="relative flex-1">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
                    <input
                        type="text"
                        placeholder="Buscar proyectos..."
                        className="w-full pl-10 pr-4 py-2 rounded-lg border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                    />
                </div>
                <button
                    onClick={fetchProjects}
                    className="p-2 text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                    title="Recargar"
                >
                    <RefreshCw size={20} />
                </button>
            </div>

            {/* Table */}
            <div className="bg-white rounded-xl shadow-xl shadow-gray-200/50 border border-gray-100 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-gray-50/50 border-b border-gray-100 text-gray-500 text-sm uppercase tracking-wider">
                                <th className="p-4 font-semibold">Proyecto</th>
                                <th className="p-4 font-semibold">Ciclo</th>
                                <th className="p-4 font-semibold">Estado</th>
                                <th className="p-4 font-semibold text-center">Score</th>
                                <th className="p-4 font-semibold text-right">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-50">
                            {loading ? (
                                <tr>
                                    <td colSpan="5" className="p-8 text-center text-gray-500">
                                        <div className="flex flex-col items-center gap-3">
                                            <div className="w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin"></div>
                                            Cargando proyectos...
                                        </div>
                                    </td>
                                </tr>
                            ) : filteredProjects.length === 0 ? (
                                <tr>
                                    <td colSpan="5" className="p-12 text-center text-gray-400">
                                        <div className="flex flex-col items-center gap-4">
                                            <FolderOpen size={48} strokeWidth={1.5} />
                                            <p className="text-lg font-medium text-gray-500">No se encontraron proyectos</p>
                                        </div>
                                    </td>
                                </tr>
                            ) : (
                                filteredProjects.map((project) => (
                                    <tr key={project.id} className="hover:bg-gray-50 transition-colors group">
                                        <td className="p-4">
                                            <div className="flex items-center gap-4">
                                                <div className="w-12 h-12 rounded-lg overflow-hidden bg-gray-100 border border-gray-200 shadow-sm flex-shrink-0">
                                                    {project.imageUrl ? (
                                                        <img src={project.imageUrl} alt={project.title} className="w-full h-full object-cover" />
                                                    ) : (
                                                        <div className="w-full h-full flex items-center justify-center text-gray-400 text-xs">IMG</div>
                                                    )}
                                                </div>
                                                <div>
                                                    <h3 className="font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">{project.title}</h3>
                                                    <p className="text-sm text-gray-500 line-clamp-1 max-w-[200px]">{project.description}</p>
                                                </div>
                                            </div>
                                        </td>
                                        <td className="p-4">
                                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800 border border-gray-200">
                                                {project.cycle || 'N/A'}
                                            </span>
                                        </td>
                                        <td className="p-4">
                                            <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${project.isEvaluated
                                                ? 'bg-green-50 text-green-700 border-green-200'
                                                : 'bg-yellow-50 text-yellow-700 border-yellow-200'
                                                }`}>
                                                {project.isEvaluated ? 'Evaluado' : 'Pendiente'}
                                            </span>
                                        </td>
                                        <td className="p-4 text-center font-mono font-medium text-gray-700">
                                            {project.score || '-'}
                                        </td>
                                        <td className="p-4 text-right">
                                            <div className="flex items-center justify-end gap-2">
                                                <Link
                                                    to={`/edit-project/${project.id}`}
                                                    className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                                                    title="Editar"
                                                >
                                                    <Edit size={18} />
                                                </Link>
                                                <button
                                                    onClick={() => confirmDelete(project.id)}
                                                    className="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                                    title="Eliminar"
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
                <div className="bg-gray-50 px-4 py-3 border-t border-gray-200 text-sm text-gray-500 flex justify-between">
                    <span>{filteredProjects.length} proyectos totales</span>
                    <span>Mostrando {filteredProjects.length} resultados</span>
                </div>
            </div>

            <DeleteModal
                isOpen={deleteModal.isOpen}
                onClose={() => setDeleteModal({ isOpen: false, projectId: null })}
                onConfirm={handleDelete}
                title="¿Eliminar proyecto?"
                message="Esta acción eliminará el proyecto permanentemente. No podrás deshacer esta acción."
            />
        </div>
    );
}
