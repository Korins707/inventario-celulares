interface SpinnerProps {
  mensaje?: string;
}

export default function Spinner({ mensaje = 'Cargando...' }: SpinnerProps) {
  return (
    <div className="spinner" role="status" aria-live="polite">
      <span className="spinner__circle" aria-hidden="true" />
      <span>{mensaje}</span>
    </div>
  );
}
