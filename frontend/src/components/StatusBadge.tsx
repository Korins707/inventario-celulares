interface StatusBadgeProps {
  status: string;
}

const clases: Record<string, string> = {
  Disponible: 'badge--disponible',
  Asignado: 'badge--asignado',
  EnMantenimiento: 'badge--mantenimiento',
  DeBaja: 'badge--baja',
  Ingreso: 'badge--disponible',
  Salida: 'badge--asignado',
  Traslado: 'badge--mantenimiento',
  Baja: 'badge--baja'
};

export default function StatusBadge({ status }: StatusBadgeProps) {
  return <span className={`badge ${clases[status] ?? ''}`}>{status}</span>;
}
