import type {
  ApiError,
  Device,
  DeviceFilters,
  DeviceInput,
  Location,
  Movement,
  MovementInput,
  Paged,
  StockReport
} from '../types';

import { API_URL as BASE_URL } from '../config';

/** Error de la API con mensaje legible para el usuario. */
export class ApiRequestError extends Error {
  readonly status: number;
  readonly payload: ApiError;

  constructor(status: number, payload: ApiError) {
    super(payload.message ?? 'Ocurrio un error inesperado. Intentelo nuevamente.');
    this.name = 'ApiRequestError';
    this.status = status;
    this.payload = payload;
  }

  /** Convierte los errores de validacion del backend en texto legible. */
  get validationMessage(): string {
    const errores = this.payload.errors;
    if (errores && Object.keys(errores).length > 0) {
      return Object.values(errores).flat().join(' ');
    }
    return this.message;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let respuesta: Response;

  try {
    respuesta = await fetch(`${BASE_URL}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...init
    });
  } catch {
    throw new ApiRequestError(0, {
      message: 'No se pudo conectar con el servidor. Verifique que la API este disponible.'
    });
  }

  const texto = await respuesta.text();
  let cuerpo: unknown = null;
  if (texto) {
    try {
      cuerpo = JSON.parse(texto);
    } catch {
      cuerpo = null;
    }
  }

  if (!respuesta.ok) {
    throw new ApiRequestError(respuesta.status, (cuerpo ?? {}) as ApiError);
  }

  return cuerpo as T;
}

function construirQuery(filtros: DeviceFilters): string {
  const params = new URLSearchParams();
  if (filtros.search.trim()) {
    params.set('search', filtros.search.trim());
  }
  if (filtros.brand.trim()) {
    params.set('brand', filtros.brand.trim());
  }
  if (filtros.status) {
    params.set('status', filtros.status);
  }
  if (filtros.locationId) {
    params.set('locationId', filtros.locationId);
  }
  params.set('page', String(filtros.page));
  params.set('pageSize', String(filtros.pageSize));
  return params.toString();
}

export const api = {
  listarEquipos: (filtros: DeviceFilters): Promise<Paged<Device>> =>
    request<Paged<Device>>(`/devices?${construirQuery(filtros)}`),

  obtenerEquipo: (id: string): Promise<Device> => request<Device>(`/devices/${id}`),

  crearEquipo: (equipo: DeviceInput): Promise<Device> =>
    request<Device>('/devices', { method: 'POST', body: JSON.stringify(equipo) }),

  actualizarEquipo: (id: string, equipo: Partial<DeviceInput>): Promise<Device> =>
    request<Device>(`/devices/${id}`, { method: 'PUT', body: JSON.stringify(equipo) }),

  eliminarEquipo: (id: string): Promise<void> =>
    request<void>(`/devices/${id}`, { method: 'DELETE' }),

  registrarMovimiento: (movimiento: MovementInput): Promise<Movement> =>
    request<Movement>('/movements', { method: 'POST', body: JSON.stringify(movimiento) }),

  listarMovimientos: (deviceId?: string): Promise<Movement[]> =>
    request<Movement[]>(`/movements${deviceId ? `?deviceId=${deviceId}` : ''}`),

  reporteStock: (): Promise<StockReport> => request<StockReport>('/reports/stock'),

  listarUbicaciones: (): Promise<Location[]> => request<Location[]>('/locations')
};
