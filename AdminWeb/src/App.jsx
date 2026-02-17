import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import AddProject from './pages/AddProject';
import Teachers from './pages/Teachers';
import EditProject from './pages/EditProject';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="teachers" element={<Teachers />} />
          <Route path="add-project" element={<AddProject />} />
          <Route path="edit-project/:id" element={<EditProject />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
