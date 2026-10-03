/**
 * Configuracion de tiempo de ejecucion de la aplicacion.
 *
 * En desarrollo se leen las variables de entorno de Vite (VITE_API_URL).
 * En produccion se puede sobreescribir sin recompilar mediante el archivo
 * public/config.js, lo que permite cambiar la URL de la API desde el hosting
 * sin tocar el codigo.
 */

interface ConfigGlobal {
  VITE_API_URL?: string;
}

const globalConfig = (window as unknown as { __APP_CONFIG__?: ConfigGlobal }).__APP_CONFIG__ ?? {};

export const API_URL: string = (
  globalConfig.VITE_API_URL ??
  import.meta.env.VITE_API_URL ??
  'http://localhost:8080'
).replace(/\/+$/, '');