import { useEffect, useState } from 'react';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import StatusBadge from '../components/StatusBadge';
import { ApiRequestError, api } from '../services/api';
import type { Location, StockReport } from '../types';

export default function AdminPanel() {
  const [reporte, setReporte] = useState<StockReport | null>(null);
  const [ubicaciones, setUbicaciones] = useState<Location[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([api.reporteStock(), api.listarUbicaciones()])
      .then(([datos, lista]) => {
        setReporte(datos);
        setUbicaciones(lista);
      })
      .catch((excepcion) =>
        setError(
          excepcion instanceof ApiRequestError
            ? excepcion.validationMessage
            : 'No se pudo cargar la informacion administrativa.'
        )
      )
      .finally(() => setCargando(false));
  }, []);

  if (cargando) {
    return <Spinner mensaje="Cargando panel de administracion..." />;
  }

  return (
    <section className="page">
      <div className="page__head">
        <h2>Panel de administracion global</h2>
      </div>

      {error && <Alert tipo="error" mensaje={error} />}

      <div className="tarjetas">
        <div className="tarjeta">
          <span className="tarjeta__titulo">Total de equipos</span>
          <strong className="tarjeta__valor">{reporte?.totalDevices ?? 0}</strong>
        </div>
        <div className="tarjeta">
          <span className="tarjeta__titulo">Ubicaciones activas</span>
          <strong className="tarjeta__valor">{ubicaciones.length}</strong>
        </div>
        <div className="tarjeta">
          <span className="tarjeta__titulo">Movimientos totales</span>
          <strong className="tarjeta__valor">
            {reporte?.byMovementType.reduce((suma, grupo) => suma + grupo.count, 0) ?? 0}
          </strong>
        </div>
      </div>

      <h3 className="subtitulo">Ubicaciones registradas</h3>
      <div className="tabla-contenedor">
        <table className="tabla">
          <thead>
            <tr>
              <th>Codigo</th>
              <th>Nombre</th>
              <th>Descripcion</th>
              <th>Equipos</th>
            </tr>
          </thead>
          <tbody>
            {ubicaciones.map((ubicacion) => {
              const conteo = reporte?.byLocation.find((g) => g.label === ubicacion.name)?.count ?? 0;
              return (
                <tr key={ubicacion.id}>
                  <td className="mono">{ubicacion.code}</td>
                  <td>{ubicacion.name}</td>
                  <td>{ubicacion.description ?? '-'}</td>
                  <td>{conteo}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <h3 className="subtitulo">Equipos dados de baja</h3>
      <div className="tarjetas">
        {reporte?.byStatus
          .filter((grupo) => grupo.label === 'DeBaja')
          .map((grupo) => (
            <div key={grupo.label} className="tarjeta">
              <StatusBadge status={grupo.label} />
              <strong className="tarjeta__valor">{grupo.count}</strong>
            </div>
          )) ?? <p className="muted">Sin equipos dados de baja.</p>}
      </div>
    </section>
  );
}
