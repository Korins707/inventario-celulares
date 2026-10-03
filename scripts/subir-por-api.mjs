/**
 * Sube el contenido del repositorio a GitHub usando la API de Contents.
 *
 * Util cuando `git push` devuelve 403 por permisos y la web de GitHub
 * rechaza subir muchas carpetas de una vez.
 *
 * Crea un commit por archivo y actualiza la rama `main` del remoto.
 */

import { readFileSync, readdirSync, statSync } from 'node:fs';
import { execFileSync } from 'node:child_process';
import { join, relative, sep } from 'node:path';

const REPO = 'UPT-FAING-EPIS/si784-2026-ii-si784-2026-ii-examen-u1-korins707';
const RAIZ = process.cwd();
const RAMA = 'main';
const API = 'https://api.github.com';

/** Obtiene la credencial desde la sesion activa de GitHub CLI. */
function obtenerCredencial() {
  const desdeEntorno = process.env.GITHUB_TOKEN;
  if (desdeEntorno) {
    return desdeEntorno.trim();
  }

  try {
    const salida = execFileSync('gh', ['auth', 'token'], { encoding: 'utf8' });
    return salida.trim();
  } catch {
    return '';
  }
}

const CRED = obtenerCredencial();

if (!CRED) {
  console.error('No se encontro una credencial de GitHub. Inicia sesion con: gh auth login');
  process.exit(1);
}

const IGNORAR = new Set(['.git', 'node_modules', 'bin', 'obj', 'dist', '.terraform']);
const MAX_PETICIONES_POR_MINUTO = 28;

/** Lista recursivamente los archivos a subir. */
function listarArchivos(directorio) {
  const encontrados = [];

  for (const entrada of readdirSync(directorio)) {
    if (IGNORAR.has(entrada)) {
      continue;
    }

    const completa = join(directorio, entrada);
    if (statSync(completa).isDirectory()) {
      encontrados.push(...listarArchivos(completa));
    } else {
      encontrados.push(completa);
    }
  }

  return encontrados;
}

async function api(ruta, opciones = {}) {
  const respuesta = await fetch(`${API}${ruta}`, {
    ...opciones,
    headers: {
      Authorization: `Bearer ${CRED}`,
      Accept: 'application/vnd.github+json',
      'X-GitHub-Api-Version': '2022-11-28',
      'Content-Type': 'application/json',
      ...(opciones.headers ?? {})
    }
  });

  const texto = await respuesta.text();
  const cuerpo = texto ? JSON.parse(texto) : null;

  if (!respuesta.ok) {
    throw new Error(`${opciones.method ?? 'GET'} ${ruta} -> ${respuesta.status} ${cuerpo?.message ?? ''}`);
  }

  return cuerpo;
}

const esperar = (ms) => new Promise((resolver) => setTimeout(resolver, ms));

async function principal() {
  const archivos = listarArchivos(RAIZ);
  console.log(`Archivos a subir: ${archivos.length}\n`);

  let subidos = 0;
  let errores = 0;
  let ultimoMinuto = Date.now();

  for (const archivo of archivos) {
    const rutaRelativa = relative(RAIZ, archivo).split(sep).join('/');
    const contenido = readFileSync(archivo).toString('base64');

    try {
      await api(`/repos/${REPO}/contents/${rutaRelativa}`, {
        method: 'PUT',
        body: JSON.stringify({
          message: `chore: agregar ${rutaRelativa}`,
          content: contenido,
          branch: RAMA
        })
      });

      subidos += 1;
      if (subidos % 10 === 0) {
        console.log(`  ${subidos}/${archivos.length} subidos`);
      }
    } catch (error) {
      errores += 1;
      console.error(`  FALLO ${rutaRelativa}: ${error.message}`);
      if (errores === 1) {
        console.error('\nSi el error es 403, el token sigue sin permiso de escritura. Abortando.\n');
        break;
      }
    }

    // Respetar el limite de la API Secondary Rate Limit.
    if (Date.now() - ultimoMinuto > 60000 / MAX_PETICIONES_POR_MINUTO) {
      await esperar(60000 / MAX_PETICIONES_POR_MINUTO);
      ultimoMinuto = Date.now();
    }
  }

  console.log(`\nResultado: ${subidos} subidos, ${errores} errores.`);
  console.log(`Repositorio: https://github.com/${REPO}`);
}

principal().catch((error) => {
  console.error(error.message);
  process.exit(1);
});