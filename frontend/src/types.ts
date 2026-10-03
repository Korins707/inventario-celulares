export type DeviceStatus = 'Disponible' | 'Asignado' | 'EnMantenimiento' | 'DeBaja';
export type MovementType = 'Ingreso' | 'Salida' | 'Traslado' | 'Baja';

export interface Device {
  id: string;
  brand: string;
  model: string;
  imei: string;
  status: DeviceStatus;
  statusName: string;
  locationId: string;
  locationName: string;
  entryDate: string;
  observations: string | null;
  purchasePrice: number | null;
  createdAt: string;
}

export interface DeviceInput {
  brand: string;
  model: string;
  imei: string;
  status: DeviceStatus;
  locationId: string;
  entryDate: string;
  observations?: string;
  purchasePrice?: number;
}

export interface Location {
  id: string;
  name: string;
  code: string;
  description: string | null;
}

export interface Movement {
  id: string;
  deviceId: string;
  deviceName: string;
  imei: string;
  type: MovementType;
  typeName: string;
  fromLocationName: string | null;
  toLocationName: string | null;
  occurredAt: string;
  reason: string | null;
  registeredBy: string | null;
}

export interface MovementInput {
  deviceId: string;
  type: MovementType;
  toLocationId?: string;
  reason?: string;
}

export interface Paged<T> {
  items: T[];
  totalItems: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface GroupCount {
  label: string;
  count: number;
}

export interface StockReport {
  totalDevices: number;
  totalValue: number;
  byStatus: GroupCount[];
  byLocation: GroupCount[];
  byBrand: GroupCount[];
  byMovementType: GroupCount[];
}

export interface DeviceFilters {
  search: string;
  brand: string;
  status: DeviceStatus | '';
  locationId: string;
  page: number;
  pageSize: number;
}

export interface ApiError {
  code?: string;
  message?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

export const DEVICE_STATUSES: DeviceStatus[] = ['Disponible', 'Asignado', 'EnMantenimiento', 'DeBaja'];
export const MOVEMENT_TYPES: MovementType[] = ['Ingreso', 'Salida', 'Traslado', 'Baja'];
