import { useEffect, useState } from 'react';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import { ApiRequestError, api } from '../services/api';
import type { GroupCount, StockReport } from '../types';

interface BarraProps {
  grupo: GroupCount;
  maximo: number;
}

function Barra({ grupo, maximo }: BarraProps) {
  const porcentaje = maximo > 0 ? Math.round((grupo.count / maximo) * 100) : 0;

  return (
    <li className="barra">
      <span className="barra__etiqueta">{grupo.label}</span>
      <span className="barra__pista">
        <span className="barra__relleno" style={{ width: `${porcentaje}%` }} />
      </span>
      <span className="barra__valor">{grupo.count}</span>
    </li>
  );
}

interface BloqueProps {
  titulo: string;
  grupos: GroupCount[];
}

function Bloque({ titulo, grupos }: BloqueProps) {
  const maximo = grupos.reduce((mayor, grupo) => Math.max(mayor, grupo.count), 0);

  return (
    <div className="reporte-bloque">
      <h4 className="reporte-bloque__titulo">{titulo}</h4>
      {grupos.length === 0 ? (
        <p className="muted">Sin datos para mostrar.</p>
      ) : (
        <ul className="barras">
          {grupos.map((grupo) => (
            <Barra key={grupo.label} grupo={grupo} maximo={maximo} />
          ))}
        </ul>
      )}
    </div>
  );
}

export default function Reports() {
  const [reporte, setReporte] = useState<StockReport | null>(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .reporteStock()
      .then(setReporte)
      .catch((excepcion) =>
        setError(
          excepcion instanceof ApiRequestError
            ? excepcion.validationMessage
            : 'No se pudo generar el reporte.'
        )
      )
      .finally(() => setCargando(false));
  }, []);

  function exportarCsv() {
    if (!reporte) {
      return;
    }

    const filas = [
      ['Reporte de stock', ''],
      ['Total de equipos', String(reporte.totalDevices)],
      ['Valor total', `S/ ${reporte.totalValue.toFixed(2)}`],
      [],
      ['Por estado', 'Cantidad'],
      ...reporte.byStatus.map((g) => [g.label, String(g.count)]),
      [],
      ['Por ubicacion', 'Cantidad'],
      ...reporte.byLocation.map((g) => [g.label, String(g.count)]),
      [],
      ['Por marca', 'Cantidad'],
      ...reporte.byBrand.map((g) => [g.label, String(g.count)])
    ];

    const csv = filas.map((fila) => fila.map((celda) => `"${celda}"`).join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = 'reporte-inventario.csv';
    enlace.click();
    URL.revokeObjectURL(url);
  }

  if (cargando) {
    return <Spinner mensaje="Generando reporte..." />;
  }

  return (
    <section className="page">
      <div className="page__head">
        <h2>Reportes de inventario</h2>
        <button type="button" className="btn btn--primario" onClick={exportarCsv} disabled={!reporte}>
          Exportar CSV
        </button>
      </div>

      {error && <Alert tipo="error" mensaje={error} />}
      {reporte && (
        <>
          <div className="tarjetas">
            <div className="tarjeta">
              <span className="tarjeta__titulo">Equipos registrados</span>
              <strong className="tarjeta__valor">{reporte.totalDevices}</strong>
            </div>
            <div className="tarjeta">
              <span className="tarjeta__titulo">Valor del inventario</span>
              <strong className="tarjeta__valor">S/ {reporte.totalValue.toFixed(2)}</strong>
            </div>
            <div className="tarjeta">
              <span className="tarjeta__titulo">Marcas distintas</span>
              <strong className="tarjeta__valor">{reporte.byBrand.length}</strong>
            </div>
            <div className="tarjeta">
              <span className="tarjeta__titulo">Ubicaciones</span>
              <strong className="tarjeta__valor">{reporte.byLocation.length}</strong>
            </div>
          </div>

          <div className="reportes-grid">
            <Bloque titulo="Equipos por estado" grupos={reporte.byStatus} />
            <Bloque titulo="Equipos por ubicacion" grupos={reporte.byLocation} />
            <Bloque titulo="Equipos por marca" grupos={reporte.byBrand} />
            <Bloque titulo="Movimientos por tipo" grupos={reporte.byMovementType} />
          </div>
        </>
      )}
    </section>
  );
}
