import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Save, ArrowLeft, Image as ImageIcon, Video, UploadCloud, Trash2 } from 'lucide-react';

export default function EditProject() {
    const navigate = useNavigate();
    const { id } = useParams();
    const [formData, setFormData] = useState({
        title: '',
        cycle: '',
        description: '',
        members: '',
        imageUrl: '',
        videoUrl: ''
    });

    const [imageFile, setImageFile] = useState(null);
    const [videoFile, setVideoFile] = useState(null);
    const [imagePreview, setImagePreview] = useState('');
    const [loading, setLoading] = useState(true);
    const [uploadStatus, setUploadStatus] = useState('');

    useEffect(() => {
        fetchProject();
    }, [id]);

    const fetchProject = async () => {
        try {
            const response = await fetch(`http://localhost:5146/api/projects/${id}`);
            if (response.ok) {
                const data = await response.json();
                setFormData({
                    ...data,
                    members: data.members ? data.members.join(', ') : '',
                    imageUrl: data.imageUrl,
                    videoUrl: data.videos && data.videos.length > 0 ? data.videos[0].url : ''
                });
                setImagePreview(data.imageUrl);
            } else {
                alert('Error al cargar el proyecto');
                navigate('/');
            }
        } catch (error) {
            console.error('Error fetching project:', error);
            navigate('/');
        } finally {
            setLoading(false);
        }
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleFileChange = (e, type) => {
        const file = e.target.files[0];
        if (!file) return;

        const MAX_SIZE = 100 * 1024 * 1024;
        if (file.size > MAX_SIZE) {
            alert(`El archivo excede el límite de 100MB`);
            e.target.value = null;
            return;
        }

        if (type === 'image') {
            setImageFile(file);
            setImagePreview(URL.createObjectURL(file));
        } else {
            setVideoFile(file);
        }
    };

    const uploadFile = async (file) => {
        const formData = new FormData();
        formData.append('file', file);

        const response = await fetch('http://localhost:5146/api/upload', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) throw new Error('Upload failed');
        const data = await response.json();
        return data.url;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setUploadStatus('Guardando cambios...');

        try {
            let finalImageUrl = formData.imageUrl;
            let finalVideoUrl = formData.videoUrl;

            if (imageFile) {
                setUploadStatus('Subiendo nueva imagen...');
                finalImageUrl = await uploadFile(imageFile);
            }

            if (videoFile) {
                setUploadStatus('Subiendo nuevo video...');
                finalVideoUrl = await uploadFile(videoFile);
            }

            setUploadStatus('Actualizando proyecto...');

            const projectPayload = {
                id: id,
                title: formData.title,
                cycle: formData.cycle,
                description: formData.description,
                imageUrl: finalImageUrl,
                members: formData.members.split(',').map(m => m.trim()).filter(m => m),
                statusText: formData.statusText || "Pendiente de Evaluación",
                isPending: formData.isPending,
                isEvaluated: formData.isEvaluated,
                score: formData.score,
                innovationScore: formData.innovationScore,
                techScore: formData.techScore,
                presentationScore: formData.presentationScore,
                videos: finalVideoUrl ? [{ url: finalVideoUrl, title: "Loop Video", description: "Main Project Video" }] : []
            };

            const response = await fetch(`http://localhost:5146/api/projects/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(projectPayload),
            });

            if (response.ok) {
                navigate('/');
            } else {
                alert('Error al actualizar el proyecto');
            }
        } catch (error) {
            console.error('Error updating project:', error);
            alert('Error: ' + error.message);
        } finally {
            setLoading(false);
            setUploadStatus('');
        }
    };

    if (loading && !formData.title) {
        return (
            <div className="flex h-screen items-center justify-center">
                <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
            </div>
        );
    }

    return (
        <div className="max-w-3xl mx-auto space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-12">
            <div className="flex items-center gap-4 mb-8">
                <button
                    onClick={() => navigate('/')}
                    className="p-2 rounded-lg hover:bg-gray-200 text-gray-500 hover:text-gray-900 transition-colors"
                >
                    <ArrowLeft size={24} />
                </button>
                <div>
                    <h2 className="text-3xl font-bold text-gray-900">Editar Proyecto</h2>
                    <p className="text-gray-500">Modifica los detalles del proyecto</p>
                </div>
            </div>

            <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-xl shadow-gray-200/50 border border-gray-100 overflow-hidden">
                <div className="p-8 space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-700">Nombre del Proyecto</label>
                            <input
                                type="text"
                                name="title"
                                required
                                className="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all"
                                value={formData.title}
                                onChange={handleChange}
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-700">Ciclo Escolar</label>
                            <input
                                type="text"
                                name="cycle"
                                required
                                className="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all"
                                value={formData.cycle}
                                onChange={handleChange}
                            />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-semibold text-gray-700">Descripción</label>
                        <textarea
                            name="description"
                            rows="4"
                            className="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all resize-none"
                            value={formData.description}
                            onChange={handleChange}
                        />
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {/* Image Upload */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-700">Imagen de Portada</label>
                            <div className={`relative border-2 border-dashed rounded-xl p-6 transition-all ${imageFile ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-gray-400'}`}>
                                <input
                                    type="file"
                                    accept="image/*"
                                    onChange={(e) => handleFileChange(e, 'image')}
                                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                />
                                <div className="flex flex-col items-center gap-2 text-center pointer-events-none">
                                    <ImageIcon size={32} className={imageFile ? 'text-blue-500' : 'text-gray-300'} />
                                    {imageFile ? (
                                        <p className="text-sm font-medium text-blue-700 truncate max-w-full px-2">{imageFile.name}</p>
                                    ) : (
                                        <p className="text-sm text-gray-500">Cambiar imagen</p>
                                    )}
                                </div>
                            </div>
                            {imagePreview && (
                                <div className="mt-2 h-32 w-full rounded-lg overflow-hidden bg-gray-100 border border-gray-200">
                                    <img src={imagePreview} alt="Preview" className="w-full h-full object-cover" />
                                </div>
                            )}
                        </div>

                        {/* Video Upload */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-700">Video del Proyecto</label>
                            <div className={`relative border-2 border-dashed rounded-xl p-6 transition-all ${videoFile ? 'border-purple-500 bg-purple-50' : 'border-gray-300 hover:border-gray-400'}`}>
                                <input
                                    type="file"
                                    accept="video/*"
                                    onChange={(e) => handleFileChange(e, 'video')}
                                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                />
                                <div className="flex flex-col items-center gap-2 text-center pointer-events-none">
                                    <Video size={32} className={videoFile ? 'text-purple-500' : 'text-gray-300'} />
                                    {videoFile ? (
                                        <p className="text-sm font-medium text-purple-700 truncate max-w-full px-2">{videoFile.name}</p>
                                    ) : (
                                        <p className="text-sm text-gray-500">Cambiar video</p>
                                    )}
                                </div>
                            </div>
                            {(videoFile || formData.videoUrl) && (
                                <p className="text-xs text-purple-600 font-medium mt-1 flex items-center gap-1">
                                    <UploadCloud size={12} /> {videoFile ? 'Video nuevo seleccionado' : 'Video actual cargado'}
                                </p>
                            )}
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-semibold text-gray-700">Integrantes (Matrículas)</label>
                        <input
                            type="text"
                            name="members"
                            className="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all"
                            value={formData.members}
                            onChange={handleChange}
                        />
                    </div>
                </div>

                <div className="bg-gray-50 px-8 py-5 border-t border-gray-100 flex items-center justify-between gap-3">
                    <span className="text-sm font-medium text-blue-600 animate-pulse">
                        {uploadStatus}
                    </span>
                    <div className="flex gap-3">
                        <button
                            type="button"
                            onClick={() => navigate('/')}
                            className="px-5 py-2.5 rounded-lg text-gray-700 hover:bg-gray-200 font-medium transition-colors"
                            disabled={loading}
                        >
                            Cancelar
                        </button>
                        <button
                            type="submit"
                            disabled={loading}
                            className="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5 rounded-lg font-medium shadow-md shadow-blue-500/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {loading ? 'Guardando...' : <> <Save size={18} /> Guardar Cambios </>}
                        </button>
                    </div>
                </div>
            </form>
        </div>
    );
}
