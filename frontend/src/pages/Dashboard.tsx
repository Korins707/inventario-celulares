import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Alert from '../components/Alert';
import ConfirmDialog from '../components/ConfirmDialog';
import Spinner from '../components/Spinner';
import StatusBadge from '../components/StatusBadge';
import { ApiRequestError, api } from '../services/api';
import { DEVICE_STATUSES } from '../types';
import type { Device, DeviceFilters, DeviceStatus, Location } from '../types';

const filtrosIniciales: DeviceFilters = {
  search: '',
  brand: '',
  status: '',
  locationId: '',
  page: 1,
  pageSize: 10
};

export default function Dashboard() {
  const [equipos, setEquipos] = useState<Device[]>([]);
  const [ubicaciones, setUbicaciones] = useState<Location[]>([]);
  const [filtros, setFiltros] = useState<DeviceFilters>(filtrosIniciales);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [exito, setExito] = useState<string | null>(null);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [totalEquipos, setTotalEquipos] = useState(0);
  const [porEliminar, setPorEliminar] = useState<Device | null>(null);

  const cargarUbicaciones = useCallback(async () => {
    try {
      setUbicaciones(await api.listarUbicaciones());
    } catch {
      setUbicaciones([]);
    }
  }, []);

  const cargarEquipos = useCallback(async (filtrosActuales: DeviceFilters) => {
    setCargando(true);
    setError(null);
    try {
      const pagina = await api.listarEquipos(filtrosActuales);
      setEquipos(pagina.items);
      setTotalPaginas(pagina.totalPages);
      setTotalEquipos(pagina.totalItems);
    } catch (excepcion) {
      setError(
        excepcion instanceof ApiRequestError
          ? excepcion.validationMessage
          : 'No se pudo cargar el inventario.'
      );
      setEquipos([]);
    } finally {
      setCargando(false);
    }
  }, []);

  useEffect(() => {
    void cargarUbicaciones();
  }, [cargarUbicaciones]);

  useEffect(() => {
    void cargarEquipos(filtros);
  }, [cargarEquipos, filtros]);

  function actualizarFiltro<K extends keyof DeviceFilters>(campo: K, valor: DeviceFilters[K]) {
    setFiltros((previos) => ({ ...previos, [campo]: valor, page: 1 }));
  }

  async function confirmarEliminacion() {
    if (!porEliminar) {
      return;
    }

    try {
      await api.eliminarEquipo(porEliminar.id);
      setExito(`Equipo ${porEliminar.brand} ${porEliminar.model} eliminado.`);
      setPorEliminar(null);
      await cargarEquipos(filtros);
    } catch (excepcion) {
      setError(
        excepcion instanceof ApiRequestError
          ? excepcion.validationMessage
          : 'No se pudo eliminar el equipo.'
      );
    }
  }

  return (
    <section className="page">
      <div className="page__head">
        <h2>Panel de equipos celulares</h2>
        <Link className="btn btn--primario" to="/equipos/nuevo">
          + Registrar equipo
        </Link>
      </div>

      <form className="buscador" onSubmit={(evento) => evento.preventDefault()}>
        <div className="buscador__campo">
          <label htmlFor="busqueda">Buscar (IMEI, marca o modelo)</label>
          <input
            id="busqueda"
            type="search"
            value={filtros.search}
            placeholder="Ej: Galaxy, 490154203237518..."
            onChange={(e) => actualizarFiltro('search', e.target.value)}
          />
        </div>
        <div className="buscador__campo">
          <label htmlFor="marca">Marca</label>
          <input
            id="marca"
            type="text"
            value={filtros.brand}
            placeholder="Todas"
            onChange={(e) => actualizarFiltro('brand', e.target.value)}
          />
        </div>
        <div className="buscador__campo">
          <label htmlFor="estado">Estado</label>
          <select
            id="estado"
            value={filtros.status}
            onChange={(e) => actualizarFiltro('status', e.target.value as DeviceStatus | '')}
          >
            <option value="">Todos</option>
            {DEVICE_STATUSES.map((estado) => (
              <option key={estado} value={estado}>
                {estado}
              </option>
            ))}
          </select>
        </div>
        <div className="buscador__campo">
          <label htmlFor="ubicacion">Ubicacion</label>
          <select
            id="ubicacion"
            value={filtros.locationId}
            onChange={(e) => actualizarFiltro('locationId', e.target.value)}
          >
            <option value="">Todas</option>
            {ubicaciones.map((ubicacion) => (
              <option key={ubicacion.id} value={ubicacion.id}>
                {ubicacion.name}
              </option>
            ))}
          </select>
        </div>
      </form>

      {error && <Alert tipo="error" mensaje={error} onClose={() => setError(null)} />}
      {exito && <Alert tipo="exito" mensaje={exito} onClose={() => setExito(null)} />}

      <p className="page__resumen">
        {cargando ? 'Buscando...' : `${totalEquipos} equipo(s) encontrado(s)`}
      </p>

      {cargando ? (
        <Spinner mensaje="Cargando inventario..." />
      ) : (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th>IMEI</th>
                <th>Marca</th>
                <th>Modelo</th>
                <th>Estado</th>
                <th>Ubicacion</th>
                <th>Ingreso</th>
                <th>Precio</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {equipos.length === 0 ? (
                <tr>
                  <td colSpan={8} className="tabla__vacia">
                    No hay equipos que coincidan con los filtros aplicados.
                  </td>
                </tr>
              ) : (
                equipos.map((equipo) => (
                  <tr key={equipo.id}>
                    <td className="mono">{equipo.imei}</td>
                    <td>{equipo.brand}</td>
                    <td>{equipo.model}</td>
                    <td>
                      <StatusBadge status={equipo.statusName} />
                    </td>
                    <td>{equipo.locationName}</td>
                    <td>{new Date(equipo.entryDate).toLocaleDateString('es-PE')}</td>
                    <td>{equipo.purchasePrice != null ? `S/ ${equipo.purchasePrice.toFixed(2)}` : '-'}</td>
                    <td className="tabla__acciones">
                      <Link className="btn btn--mini" to={`/equipos/${equipo.id}/historial`}>
                        Historial
                      </Link>
                      <Link className="btn btn--mini" to={`/equipos/${equipo.id}/editar`}>
                        Editar
                      </Link>
                      <button
                        type="button"
                        className="btn btn--mini btn--peligro"
                        onClick={() => setPorEliminar(equipo)}
                      >
                        Eliminar
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {totalPaginas > 1 && (
        <div className="paginacion">
          <button
            type="button"
            className="btn btn--secundario"
            disabled={filtros.page <= 1}
            onClick={() => setFiltros((previos) => ({ ...previos, page: previos.page - 1 }))}
          >
            Anterior
          </button>
          <span>
            Pagina {filtros.page} de {totalPaginas}
          </span>
          <button
            type="button"
            className="btn btn--secundario"
            disabled={filtros.page >= totalPaginas}
            onClick={() => setFiltros((previos) => ({ ...previos, page: previos.page + 1 }))}
          >
            Siguiente
          </button>
        </div>
      )}

      {porEliminar && (
        <ConfirmDialog
          titulo="Eliminar equipo"
          mensaje={`¿Seguro que desea eliminar ${porEliminar.brand} ${porEliminar.model} (IMEI ${porEliminar.imei})? Esta accion no se puede deshacer.`}
          onConfirm={() => void confirmarEliminacion()}
          onCancel={() => setPorEliminar(null)}
        />
      )}
    </section>
  );
}
