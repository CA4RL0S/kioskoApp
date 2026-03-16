# KioskoApp

**KioskoApp** es un sistema integral diseñado para la gestión y evaluación de proyectos académicos o de innovación. La plataforma consta de un conjunto de aplicaciones que permiten a los administradores, estudiantes y evaluadores interactuar en un ecosistema robusto, respaldado por un backend potente y servicios en la nube.

---

## 📌 Arquitectura del Sistema

El ecosistema de KioskoApp está dividido en cuatro componentes principales:

1. **KioskoAPI (Backend)**
   - API RESTFUL desarrollada en **ASP.NET Core Web API (.NET 10)**.
   - Base de datos NoSQL con **MongoDB**.
   - Integración con **Cloudinary** para la gestión y almacenamiento de recursos multimedia.
   - Swagger incluido para la documentación e interacción con los endpoints.

2. **AdminWeb (Panel de Administración)**
   - Aplicación web desarrollada con **React 19** y **Vite**.
   - Interfaces estilizadas con **TailwindCSS** y **Lucide React**.
   - Permite a los administradores gestionar proyectos, profesores (evaluadores) y asignar ciclos de evaluación.

3. **StudentApp (Kiosko Alumnos)**
   - Aplicación multiplataforma construida con **.NET MAUI (.NET 10)**.
   - Diseñada para iOS, Android, MacCatalyst y Windows.
   - Autenticación segura mediante **Microsoft.Identity.Client (MSAL)**.
   - Permite a los estudiantes subir información, videos y documentos de sus proyectos.

4. **EvaluatorApp (Kiosko Evaluadores)**
   - Aplicación multiplataforma construida con **.NET MAUI (.NET 10)**.
   - Permite a los profesores/evaluadores visualizar los proyectos asignados y calificarlos en tiempo real.
   - Uso de **SQLite (sqlite-net-pcl)** para almacenamiento y caché local.
   - Los criterios de evaluación incluyen: *Problema, Innovación, Tecnología, Impacto, Presentación, Conocimiento y Resultados*.

---

## 🚀 Tecnologías Principales

- **Frameworks & Lenguajes:** C# 13, .NET 10, ASP.NET Core, .NET MAUI, React 19, JavaScript.
- **Base de Datos:** MongoDB (Nube/Local), SQLite (Caché local móvil).
- **Frontend Stack:** React.js, TailwindCSS, Vite.
- **Autenticación e Identidad:** Azure AD / MSAL (Microsoft Authentication Library).
- **Almacenamiento Multimedia:** Cloudinary.

---

## ⚙️ Estructura del Repositorio

```text
kioskoApp/
│
├── AdminWeb/            # Proyecto React/Vite (Frontend web de administración)
├── EvaluatorApp/        # Proyecto .NET MAUI (App para evaluadores)
├── KioskoAPI/           # Proyecto ASP.NET Core (Backend API)
├── StudentApp/          # Proyecto .NET MAUI (App para alumnos)
└── KioskoSolution.sln   # Solución global de Visual Studio para .NET
```

---

## 🛠️ Requisitos Previos

Para ejecutar los distintos proyectos de esta solución, asegúrate de tener instalado:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/en/) (v18 o superior recomendado para `AdminWeb`)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / [Rider](https://www.jetbrains.com/rider/) o **VS Code** con extensiones de C# y MAUI.
- Instancias o cadenas de conexión válidas para **MongoDB**, **Cloudinary** y **Azure AD** (Configuración MSAL).

---

## 📖 Instrucciones de Ejecución

### 1. Ejecutar el Backend (KioskoAPI)
Navega a la carpeta del API y ejecuta el proyecto:
```bash
cd KioskoAPI
dotnet run
```
*El API estará disponible y mostrará la interfaz de Swagger en modo desarrollo para probar los endpoints.*

### 2. Ejecutar el Panel Web (AdminWeb)
Navega a la carpeta web, instala las dependencias y corre el servidor de desarrollo:
```bash
cd AdminWeb
npm install
npm run dev
```

### 3. Ejecutar las Aplicaciones Móviles (MAUI)
Puedes compilar y desplegar `StudentApp` o `EvaluatorApp` desde Visual Studio seleccionando el proyecto de inicio deseado (Ej: Android Emulator, iOS Simulator o Mac Catalyst).
Por línea de comandos (ejemplo para Mac Catalyst):
```bash
cd EvaluatorApp
dotnet build -t:Run -f net10.0-maccatalyst
```

---

## 🔒 Notas de Configuración (Keys / Secrets)

> [!WARNING]
> Recuerda configurar correctamente tus archivos `appsettings.json` o `appsettings.Development.json` dentro de **KioskoAPI** y las aplicaciones móviles con las credenciales de tus dependencias externas (MongoDB URI, Cloudinary API Keys, MSAL Client IDs). **No expongas credenciales en control de versiones.**

---

## 👨‍💻 Contribución y Desarrollo

El código está estructurado para ser altamente mantenible.
- Para cambios de interfaz móvil, asegúrate de probar tanto en iOS como en Android.
- Para cambios en el API, actualiza los modelos tanto en el Backend como en las aplicaciones cliente que consumen los endpoints.
