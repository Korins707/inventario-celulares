import type { ReactNode } from 'react';
import { NavLink } from 'react-router-dom';

const enlaces = [
  { to: '/equipos', label: 'Equipos' },
  { to: '/movimientos', label: 'Movimientos' },
  { to: '/reportes', label: 'Reportes' },
  { to: '/admin', label: 'Administracion' }
];

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  return (
    <div className="layout">
      <header className="layout__header">
        <div className="layout__brand">
          <span className="layout__logo" aria-hidden="true">📱</span>
          <div>
            <h1>Inventario de Equipos Celulares</h1>
            <p>Control de stock, movimientos y reportes</p>
          </div>
        </div>
        <nav className="layout__nav" aria-label="Navegacion principal">
          {enlaces.map((enlace) => (
            <NavLink
              key={enlace.to}
              to={enlace.to}
              className={({ isActive }) => `layout__link${isActive ? ' layout__link--active' : ''}`}
            >
              {enlace.label}
            </NavLink>
          ))}
        </nav>
      </header>
      <main className="layout__main">{children}</main>
      <footer className="layout__footer">
        <span>Proyecto de coursework - Quality y Testing / Base de Datos</span>
      </footer>
    </div>
  );
}
