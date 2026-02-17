import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Save, ArrowLeft, Image as ImageIcon, Video, UploadCloud } from 'lucide-react';

export default function AddProject() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    title: '',
    cycle: new Date().getFullYear().toString() + '-' + (new Date().getMonth() < 6 ? '1' : '2'),
    description: '',
    members: '',
    techScore: 0,
    innovationScore: 0,
    presentationScore: 0
  });

  const [imageFile, setImageFile] = useState(null);
  const [videoFile, setVideoFile] = useState(null);
  const [imagePreview, setImagePreview] = useState('');
  const [loading, setLoading] = useState(false);
  const [uploadStatus, setUploadStatus] = useState('');

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleFileChange = (e, type) => {
    const file = e.target.files[0];
    if (!file) return;

    // 100 MB Limit for Cloudinary Free Tier
    const MAX_SIZE = 100 * 1024 * 1024;
    if (file.size > MAX_SIZE) {
      alert(`El archivo excede el límite de 100MB (Tamaño actual: ${(file.size / (1024 * 1024)).toFixed(2)} MB)`);
      e.target.value = null; // Reset input
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

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Upload failed');
    }
    const data = await response.json();
    return data.url;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setUploadStatus('Subiendo archivos...');

    try {
      let imageUrl = "https://via.placeholder.com/300?text=No+Image";
      let videoUrl = "";

      if (imageFile) {
        imageUrl = await uploadFile(imageFile);
      }

      if (videoFile) {
        setUploadStatus('Subiendo video (esto puede tardar)...');
        videoUrl = await uploadFile(videoFile);
      }

      setUploadStatus('Guardando proyecto...');

      // Construct payload compatible with backend
      const projectPayload = {
        title: formData.title,
        cycle: formData.cycle,
        description: formData.description,
        imageUrl: imageUrl,
        members: formData.members.split(',').map(m => m.trim()).filter(m => m),
        statusText: "Pendiente de Evaluación",
        isPending: true,
        isEvaluated: false,
        score: "0",
        innovationScore: 0,
        techScore: 0,
        presentationScore: 0,
        evaluations: [],
        videos: videoUrl ? [{ url: videoUrl, title: "Loop Video", description: "Main Project Video" }] : [],
        documents: []
      };

      const response = await fetch('http://localhost:5146/api/projects', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(projectPayload),
      });

      if (response.ok) {
        navigate('/');
      } else {
        alert('Error al crear el proyecto');
      }
    } catch (error) {
      console.error('Error creating project:', error);
      alert('Error: ' + error.message);
    } finally {
      setLoading(false);
      setUploadStatus('');
    }
  };

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
          <h2 className="text-3xl font-bold text-gray-900">Nuevo Proyecto</h2>
          <p className="text-gray-500">Registra un nuevo proyecto en el sistema</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-xl shadow-gray-200/50 border border-gray-100 overflow-hidden">
        <div className="p-8 space-y-6">
          {/* Main Info */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-2">
              <label className="text-sm font-semibold text-gray-700">Nombre del Proyecto</label>
              <input
                type="text"
                name="title"
                required
                className="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all"
                placeholder="Ej. Sistema Solar VR"
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
                placeholder="Ej. 2024-1"
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
              placeholder="Describe brevemente el proyecto..."
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
                    <p className="text-sm text-gray-500">Arrastra o clic para subir imagen</p>
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
                    <p className="text-sm text-gray-500">Arrastra o clic para subir video</p>
                  )}
                </div>
              </div>
              {videoFile && (
                <p className="text-xs text-purple-600 font-medium mt-1 flex items-center gap-1">
                  <UploadCloud size={12} /> Listo para subir
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
              placeholder="A01234567, A09876543"
              value={formData.members}
              onChange={handleChange}
            />
            <p className="text-xs text-gray-500">Separa las matrículas con comas</p>
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
              {loading ? (
                <>
                  <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  Procesando...
                </>
              ) : (
                <>
                  <Save size={18} />
                  Guardar Proyecto
                </>
              )}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
