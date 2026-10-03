interface ConfirmDialogProps {
  titulo: string;
  mensaje: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDialog({
  titulo,
  mensaje,
  onConfirm,
  onCancel
}: ConfirmDialogProps) {
  return (
    <div className="modal" role="dialog" aria-modal="true" aria-label={titulo}>
      <div className="modal__panel">
        <h3 className="modal__titulo">{titulo}</h3>
        <p className="modal__texto">{mensaje}</p>
        <div className="modal__acciones">
          <button type="button" className="btn btn--secundario" onClick={onCancel}>
            Cancelar
          </button>
          <button type="button" className="btn btn--peligro" onClick={onConfirm}>
            Confirmar
          </button>
        </div>
      </div>
    </div>
  );
}
