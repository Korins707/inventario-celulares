import { Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import AdminPanel from './pages/AdminPanel';
import Dashboard from './pages/Dashboard';
import DeviceForm from './pages/DeviceForm';
import DeviceHistory from './pages/DeviceHistory';
import Movements from './pages/Movements';
import Reports from './pages/Reports';

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/equipos" element={<Dashboard />} />
        <Route path="/equipos/nuevo" element={<DeviceForm />} />
        <Route path="/equipos/:id/editar" element={<DeviceForm />} />
        <Route path="/equipos/:id/historial" element={<DeviceHistory />} />
        <Route path="/movimientos" element={<Movements />} />
        <Route path="/reportes" element={<Reports />} />
        <Route path="/admin" element={<AdminPanel />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  );
}
