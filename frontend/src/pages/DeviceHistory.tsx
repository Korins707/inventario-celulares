import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import StatusBadge from '../components/StatusBadge';
import { ApiRequestError, api } from '../services/api';
import type { Device, Movement } from '../types';

export default function DeviceHistory() {
  const { id } = useParams<{ id: string }>();
  const [equipo, setEquipo] = useState<Device | null>(null);
  const [movimientos, setMovimientos] = useState<Movement[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }

    Promise.all([api.obtenerEquipo(id), api.listarMovimientos(id)])
      .then(([detalle, historial]) => {
        setEquipo(detalle);
        setMovimientos(historial);
      })
      .catch((excepcion) =>
        setError(
          excepcion instanceof ApiRequestError ? excepcion.validationMessage : 'No se pudo cargar el historial.'
        )
      )
      .finally(() => setCargando(false));
  }, [id]);

  if (cargando) {
    return <Spinner mensaje="Cargando historial..." />;
  }

  return (
    <section className="page">
      <div className="page__head">
        <h2>Historial del equipo</h2>
        <Link className="btn btn--secundario" to="/equipos">
          Volver al panel
        </Link>
      </div>

      {error && <Alert tipo="error" mensaje={error} />}

      {equipo && (
        <div className="tarjeta-equipo">
          <div>
            <strong>{equipo.brand} {equipo.model}</strong>
            <p className="mono">IMEI: {equipo.imei}</p>
          </div>
          <div className="tarjeta-equipo__datos">
            <StatusBadge status={equipo.statusName} />
            <span>Ubicacion: {equipo.locationName}</span>
            <span>Ingreso: {new Date(equipo.entryDate).toLocaleDateString('es-PE')}</span>
          </div>
        </div>
      )}

      <div className="tabla-contenedor">
        <table className="tabla">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Tipo</th>
              <th>Origen</th>
              <th>Destino</th>
              <th>Motivo</th>
            </tr>
          </thead>
          <tbody>
            {movimientos.length === 0 ? (
              <tr>
                <td colSpan={5} className="tabla__vacia">
                  Este equipo no tiene movimientos registrados.
                </td>
              </tr>
            ) : (
              movimientos.map((movimiento) => (
                <tr key={movimiento.id}>
                  <td>{new Date(movimiento.occurredAt).toLocaleString('es-PE')}</td>
                  <td>
                    <StatusBadge status={movimiento.typeName} />
                  </td>
                  <td>{movimiento.fromLocationName ?? '-'}</td>
                  <td>{movimiento.toLocationName ?? '-'}</td>
                  <td>{movimiento.reason ?? '-'}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}
