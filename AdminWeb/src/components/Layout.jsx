import React from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { LayoutDashboard, PlusCircle, LogOut, Users } from 'lucide-react';

const SidebarItem = ({ to, icon: Icon, label }) => {
    const location = useLocation();
    const isActive = location.pathname === to;

    return (
        <Link
            to={to}
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${isActive
                ? 'bg-blue-600 text-white shadow-lg shadow-blue-500/30'
                : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                }`}
        >
            <Icon size={20} />
            <span className="font-medium">{label}</span>
            {isActive && <div className="ml-auto w-1.5 h-1.5 rounded-full bg-white animate-pulse" />}
        </Link>
    );
};

export default function Layout() {
    return (
        <div className="flex h-screen bg-gray-50 text-gray-900 font-sans">
            {/* Sidebar */}
            <aside className="w-64 bg-gray-900 border-r border-gray-800 flex flex-col shadow-2xl z-10">
                <div className="p-6 border-b border-gray-800">
                    <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center text-white font-bold text-lg shadow-lg">
                            K
                        </div>
                        <h1 className="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-gray-400">
                            Kiosko Admin
                        </h1>
                    </div>
                    <p className="text-xs text-gray-500 mt-2 font-medium tracking-wide uppercase">Panel de Control</p>
                </div>

                <nav className="flex-1 p-4 space-y-2 mt-4">
                    <SidebarItem to="/" icon={LayoutDashboard} label="Dashboard" />
                    <SidebarItem to="/teachers" icon={Users} label="Profesores" />
                    <SidebarItem to="/add-project" icon={PlusCircle} label="Nuevo Proyecto" />
                </nav>

                <div className="p-4 border-t border-gray-800">
                    <button className="flex items-center gap-3 px-4 py-3 w-full rounded-lg text-red-400 hover:bg-red-500/10 hover:text-red-300 transition-all group">
                        <LogOut size={20} className="group-hover:-translate-x-1 transition-transform" />
                        <span className="font-medium">Cerrar Sesión</span>
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 overflow-auto bg-gray-50 relative">
                <div className="absolute inset-0 bg-grid-slate-200 [mask-image:linear-gradient(0deg,white,rgba(255,255,255,0.6))] pointer-events-none" />
                <div className="relative p-8 max-w-7xl mx-auto">
                    <Outlet />
                </div>
            </main>
        </div>
    );
}
