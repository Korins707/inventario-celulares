import { useCallback, useEffect, useState } from 'react';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import StatusBadge from '../components/StatusBadge';
import { ApiRequestError, api } from '../services/api';
import { MOVEMENT_TYPES } from '../types';
import type { Device, Location, Movement, MovementInput, MovementType } from '../types';

const hoy = () => new Date().toISOString().slice(0, 16);

export default function Movements() {
  const [movimientos, setMovimientos] = useState<Movement[]>([]);
  const [equipos, setEquipos] = useState<Device[]>([]);
  const [ubicaciones, setUbicaciones] = useState<Location[]>([]);
  const [formulario, setFormulario] = useState<MovementInput>({
    deviceId: '',
    type: 'Traslado',
    toLocationId: '',
    reason: ''
  });
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [exito, setExito] = useState<string | null>(null);

  const cargarMovimientos = useCallback(async () => {
    setCargando(true);
    try {
      setMovimientos(await api.listarMovimientos());
      setError(null);
    } catch (excepcion) {
      setError(
        excepcion instanceof ApiRequestError
          ? excepcion.validationMessage
          : 'No se pudieron cargar los movimientos.'
      );
    } finally {
      setCargando(false);
    }
  }, []);

  useEffect(() => {
    void cargarMovimientos();

    api
      .listarUbicaciones()
      .then((lista) => {
        setUbicaciones(lista);
        setFormulario((previos) => ({
          ...previos,
          toLocationId: previos.toLocationId || (lista[0]?.id ?? '')
        }));
      })
      .catch(() => setUbicaciones([]));

    api
      .listarEquipos({ search: '', brand: '', status: '', locationId: '', page: 1, pageSize: 100 })
      .then((pagina) => setEquipos(pagina.items))
      .catch(() => setEquipos([]));
  }, [cargarMovimientos]);

  async function registrar(evento: React.FormEvent) {
    evento.preventDefault();
    setError(null);
    setExito(null);

    if (!formulario.deviceId) {
      setError('Seleccione el equipo que se movera.');
      return;
    }

    if (formulario.type === 'Traslado' && !formulario.toLocationId) {
      setError('Un traslado necesita una ubicacion de destino.');
      return;
    }

    try {
      await api.registrarMovimiento({
        deviceId: formulario.deviceId,
        type: formulario.type,
        toLocationId: formulario.type === 'Traslado' ? formulario.toLocationId : undefined,
        reason: formulario.reason || undefined
      });
      setExito(`Movimiento de tipo ${formulario.type} registrado.`);
      setFormulario((previos) => ({ ...previos, reason: '' }));
      await cargarMovimientos();
    } catch (excepcion) {
      setError(
        excepcion instanceof ApiRequestError
          ? excepcion.validationMessage
          : 'No se pudo registrar el movimiento.'
      );
    }
  }

  return (
    <section className="page">
      <div className="page__head">
        <h2>Gestion de movimientos</h2>
      </div>

      {error && <Alert tipo="error" mensaje={error} onClose={() => setError(null)} />}
      {exito && <Alert tipo="exito" mensaje={exito} onClose={() => setExito(null)} />}

      <form className="formulario formulario--compacto" onSubmit={registrar} noValidate>
        <div className="formulario__grid">
          <div className="campo">
            <label htmlFor="deviceId">Equipo *</label>
            <select
              id="deviceId"
              value={formulario.deviceId}
              onChange={(e) => setFormulario((previos) => ({ ...previos, deviceId: e.target.value }))}
              className="campo__input"
            >
              <option value="">Seleccione un equipo...</option>
              {equipos.map((equipo) => (
                <option key={equipo.id} value={equipo.id}>
                  {equipo.brand} {equipo.model} - {equipo.imei}
                </option>
              ))}
            </select>
          </div>

          <div className="campo">
            <label htmlFor="tipo">Tipo de movimiento</label>
            <select
              id="tipo"
              value={formulario.type}
              onChange={(e) =>
                setFormulario((previos) => ({ ...previos, type: e.target.value as MovementType }))
              }
              className="campo__input"
            >
              {MOVEMENT_TYPES.map((tipo) => (
                <option key={tipo} value={tipo}>
                  {tipo}
                </option>
              ))}
            </select>
          </div>

          <div className="campo">
            <label htmlFor="destino">Ubicacion de destino</label>
            <select
              id="destino"
              value={formulario.toLocationId}
              onChange={(e) =>
                setFormulario((previos) => ({ ...previos, toLocationId: e.target.value }))
              }
              className="campo__input"
            >
              <option value="">Seleccione...</option>
              {ubicaciones.map((ubicacion) => (
                <option key={ubicacion.id} value={ubicacion.id}>
                  {ubicacion.name}
                </option>
              ))}
            </select>
          </div>

          <div className="campo">
            <label htmlFor="motivo">Motivo</label>
            <input
              id="motivo"
              type="text"
              maxLength={300}
              value={formulario.reason ?? ''}
              onChange={(e) => setFormulario((previos) => ({ ...previos, reason: e.target.value }))}
              className="campo__input"
            />
          </div>
        </div>
        <div className="formulario__acciones">
          <button type="submit" className="btn btn--primario">
            Registrar movimiento
          </button>
          <span className="muted">Registrado el {hoy().replace('T', ' ')}</span>
        </div>
      </form>

      <h3 className="subtitulo">Historial completo</h3>
      {cargando ? (
        <Spinner mensaje="Cargando movimientos..." />
      ) : (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Equipo</th>
                <th>IMEI</th>
                <th>Tipo</th>
                <th>Origen</th>
                <th>Destino</th>
                <th>Motivo</th>
              </tr>
            </thead>
            <tbody>
              {movimientos.length === 0 ? (
                <tr>
                  <td colSpan={7} className="tabla__vacia">
                    Todavia no hay movimientos registrados.
                  </td>
                </tr>
              ) : (
                movimientos.map((movimiento) => (
                  <tr key={movimiento.id}>
                    <td>{new Date(movimiento.occurredAt).toLocaleString('es-PE')}</td>
                    <td>{movimiento.deviceName}</td>
                    <td className="mono">{movimiento.imei}</td>
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
      )}
    </section>
  );
}
