import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import { ApiRequestError, api } from '../services/api';
import { DEVICE_STATUSES } from '../types';
import type { DeviceInput, DeviceStatus, Location } from '../types';

interface ErroresFormulario {
  brand?: string;
  model?: string;
  imei?: string;
  locationId?: string;
}

/** Valida el formulario en el navegador antes de llamar a la API. */
function validar(datos: DeviceInput): ErroresFormulario {
  const errores: ErroresFormulario = {};

  if (!datos.brand.trim()) {
    errores.brand = 'La marca es obligatoria.';
  } else if (datos.brand.trim().length < 2) {
    errores.brand = 'La marca debe tener al menos 2 caracteres.';
  }

  if (!datos.model.trim()) {
    errores.model = 'El modelo es obligatorio.';
  }

  if (!/^[0-9]{15}$/.test(datos.imei.trim())) {
    errores.imei = 'El IMEI debe tener exactamente 15 digitos numericos.';
  }

  if (!datos.locationId) {
    errores.locationId = 'Seleccione una ubicacion.';
  }

  return errores;
}

const datosIniciales: DeviceInput = {
  brand: '',
  model: '',
  imei: '',
  status: 'Disponible',
  locationId: '',
  entryDate: new Date().toISOString().slice(0, 10),
  observations: '',
  purchasePrice: undefined
};

export default function DeviceForm() {
  const { id } = useParams<{ id: string }>();
  const esEdicion = Boolean(id);
  const navegar = useNavigate();

  const [datos, setDatos] = useState<DeviceInput>(datosIniciales);
  const [ubicaciones, setUbicaciones] = useState<Location[]>([]);
  const [errores, setErrores] = useState<ErroresFormulario>({});
  const [error, setError] = useState<string | null>(null);
  const [exito, setExito] = useState<string | null>(null);
  const [cargando, setCargando] = useState(esEdicion);

  useEffect(() => {
    api
      .listarUbicaciones()
      .then((lista) => {
        setUbicaciones(lista);
        if (lista.length > 0) {
          setDatos((previos) => ({ ...previos, locationId: previos.locationId || lista[0].id }));
        }
      })
      .catch(() => setUbicaciones([]));
  }, []);

  useEffect(() => {
    if (!id) {
      return;
    }

    api
      .obtenerEquipo(id)
      .then((equipo) => {
        setDatos({
          brand: equipo.brand,
          model: equipo.model,
          imei: equipo.imei,
          status: equipo.status,
          locationId: equipo.locationId,
          entryDate: equipo.entryDate.slice(0, 10),
          observations: equipo.observations ?? '',
          purchasePrice: equipo.purchasePrice ?? undefined
        });
      })
      .catch((excepcion) =>
        setError(
          excepcion instanceof ApiRequestError ? excepcion.validationMessage : 'No se pudo cargar el equipo.'
        )
      )
      .finally(() => setCargando(false));
  }, [id]);

  function actualizar<K extends keyof DeviceInput>(campo: K, valor: DeviceInput[K]) {
    setDatos((previos) => ({ ...previos, [campo]: valor }));
    setErrores((previos) => ({ ...previos, [campo]: undefined }));
  }

  async function enviar(evento: React.FormEvent) {
    evento.preventDefault();
    setError(null);
    setExito(null);

    const erroresLocales = validar(datos);
    if (Object.keys(erroresLocales).length > 0) {
      setErrores(erroresLocales);
      return;
    }

    const payload: DeviceInput = {
      ...datos,
      brand: datos.brand.trim(),
      model: datos.model.trim(),
      imei: datos.imei.trim(),
      entryDate: new Date(datos.entryDate).toISOString()
    };

    try {
      if (esEdicion && id) {
        await api.actualizarEquipo(id, payload);
        setExito('Equipo actualizado correctamente.');
      } else {
        await api.crearEquipo(payload);
        setExito('Equipo registrado correctamente.');
      }
      setTimeout(() => navegar('/equipos'), 700);
    } catch (excepcion) {
      setError(
        excepcion instanceof ApiRequestError
          ? excepcion.validationMessage
          : 'No se pudo guardar el equipo.'
      );
    }
  }

  if (cargando) {
    return <Spinner mensaje="Cargando equipo..." />;
  }

  return (
    <section className="page">
      <div className="page__head">
        <h2>{esEdicion ? 'Editar equipo' : 'Registrar nuevo equipo'}</h2>
        <Link className="btn btn--secundario" to="/equipos">
          Volver al panel
        </Link>
      </div>

      {error && <Alert tipo="error" mensaje={error} onClose={() => setError(null)} />}
      {exito && <Alert tipo="exito" mensaje={exito} />}

      <form className="formulario" onSubmit={enviar} noValidate>
        <div className="formulario__grid">
          <div className="campo">
            <label htmlFor="brand">Marca *</label>
            <input
              id="brand"
              value={datos.brand}
              onChange={(e) => actualizar('brand', e.target.value)}
              className={errores.brand ? 'campo__input campo__input--error' : 'campo__input'}
            />
            {errores.brand && <span className="campo__error">{errores.brand}</span>}
          </div>

          <div className="campo">
            <label htmlFor="model">Modelo *</label>
            <input
              id="model"
              value={datos.model}
              onChange={(e) => actualizar('model', e.target.value)}
              className={errores.model ? 'campo__input campo__input--error' : 'campo__input'}
            />
            {errores.model && <span className="campo__error">{errores.model}</span>}
          </div>

          <div className="campo">
            <label htmlFor="imei">IMEI (15 digitos) *</label>
            <input
              id="imei"
              inputMode="numeric"
              maxLength={15}
              value={datos.imei}
              onChange={(e) => actualizar('imei', e.target.value.replace(/\D/g, ''))}
              className={errores.imei ? 'campo__input campo__input--error' : 'campo__input'}
            />
            {errores.imei && <span className="campo__error">{errores.imei}</span>}
          </div>

          <div className="campo">
            <label htmlFor="status">Estado</label>
            <select
              id="status"
              value={datos.status}
              onChange={(e) => actualizar('status', e.target.value as DeviceStatus)}
              className="campo__input"
            >
              {DEVICE_STATUSES.map((estado) => (
                <option key={estado} value={estado}>
                  {estado}
                </option>
              ))}
            </select>
          </div>

          <div className="campo">
            <label htmlFor="locationId">Ubicacion *</label>
            <select
              id="locationId"
              value={datos.locationId}
              onChange={(e) => actualizar('locationId', e.target.value)}
              className={errores.locationId ? 'campo__input campo__input--error' : 'campo__input'}
            >
              <option value="">Seleccione...</option>
              {ubicaciones.map((ubicacion) => (
                <option key={ubicacion.id} value={ubicacion.id}>
                  {ubicacion.name} ({ubicacion.code})
                </option>
              ))}
            </select>
            {errores.locationId && <span className="campo__error">{errores.locationId}</span>}
          </div>

          <div className="campo">
            <label htmlFor="entryDate">Fecha de ingreso</label>
            <input
              id="entryDate"
              type="date"
              value={datos.entryDate}
              onChange={(e) => actualizar('entryDate', e.target.value)}
              className="campo__input"
            />
          </div>

          <div className="campo">
            <label htmlFor="purchasePrice">Precio de compra (S/)</label>
            <input
              id="purchasePrice"
              type="number"
              min="0"
              step="0.01"
              value={datos.purchasePrice ?? ''}
              onChange={(e) =>
                actualizar('purchasePrice', e.target.value === '' ? undefined : Number(e.target.value))
              }
              className="campo__input"
            />
          </div>

          <div className="campo campo--ancho">
            <label htmlFor="observations">Observaciones</label>
            <textarea
              id="observations"
              rows={3}
              maxLength={500}
              value={datos.observations ?? ''}
              onChange={(e) => actualizar('observations', e.target.value)}
              className="campo__input"
            />
          </div>
        </div>

        <div className="formulario__acciones">
          <button type="submit" className="btn btn--primario">
            {esEdicion ? 'Guardar cambios' : 'Registrar equipo'}
          </button>
          <Link className="btn btn--secundario" to="/equipos">
            Cancelar
          </Link>
        </div>
      </form>
    </section>
  );
}
