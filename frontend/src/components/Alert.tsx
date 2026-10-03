interface AlertProps {
  tipo: 'error' | 'exito';
  mensaje: string;
  onClose?: () => void;
}

export default function Alert({ tipo, mensaje, onClose }: AlertProps) {
  return (
    <div className={`alert alert--${tipo}`} role={tipo === 'error' ? 'alert' : 'status'}>
      <span>{mensaje}</span>
      {onClose && (
        <button type="button" className="alert__cerrar" onClick={onClose} aria-label="Cerrar mensaje">
          ×
        </button>
      )}
    </div>
  );
}
