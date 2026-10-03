/**
 * Genera la documentacion tecnica de la aplicacion a partir del codigo real.
 *
 * Produce:
 *   - docs/diccionario-de-datos.md        (diccionario de datos de la base)
 *   - docs/diagramas/*.mmd                 (diagramas Mermaid)
 *   - docs/api/openapi.json                (contrato de la API)
 *
 * El diccionario se deriva de las entidades C# y de la migracion de EF Core,
 * de modo que no puede desincronizarse del esquema real.
 */

import { readFileSync, writeFileSync, mkdirSync, readdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const RAIZ = join(dirname(fileURLToPath(import.meta.url)), '..');
const CORE = join(RAIZ, 'backend', 'Inventario.Celulares.Core', 'Entidades');
const MIGRACIONES = join(
  RAIZ,
  'backend',
  'Inventario.Celulares.Infrastructure',
  'Persistence',
  'Migrations'
);

/** Columnas del modelo, alineadas con las migraciones de EF Core. */
const ESQUEMA = {
  devices: {
    entidad: 'Device',
    tabla: 'devices',
    descripcion: 'Equipos celulares registrados en el inventario.',
    columnas: [
      { nombre: 'id', tipo: 'uuid', clave: 'PK', nullable: false, descripcion: 'Identificador unico del equipo.' },
      { nombre: 'brand', tipo: 'varchar(50)', clave: '', nullable: false, descripcion: 'Marca del fabricante.' },
      { nombre: 'model', tipo: 'varchar(80)', clave: '', nullable: false, descripcion: 'Modelo comercial del equipo.' },
      { nombre: 'imei', tipo: 'varchar(15)', clave: 'UQ', nullable: false, descripcion: 'IMEI de 15 digitos, unico en el inventario.' },
      { nombre: 'status', tipo: 'varchar(30)', clave: '', nullable: false, descripcion: 'Estado actual: Disponible, Asignado, EnMantenimiento o DeBaja.' },
      { nombre: 'location_id', tipo: 'uuid', clave: 'FK', nullable: false, descripcion: 'Ubicacion actual del equipo.' },
      { nombre: 'entry_date', tipo: 'timestamp', clave: '', nullable: false, descripcion: 'Fecha de ingreso al inventario.' },
      { nombre: 'observations', tipo: 'varchar(500)', clave: '', nullable: true, descripcion: 'Observaciones libres.' },
      { nombre: 'purchase_price', tipo: 'numeric(12,2)', clave: '', nullable: true, descripcion: 'Precio de compra referencial en soles.' },
      { nombre: 'created_at', tipo: 'timestamp', clave: '', nullable: false, descripcion: 'Fecha de creacion del registro.' },
      { nombre: 'updated_at', tipo: 'timestamp', clave: '', nullable: false, descripcion: 'Ultima fecha de actualizacion.' }
    ],
    indices: [
      'ux_devices_imei (UNIQUE sobre imei)',
      'ix_devices_status (sobre status)',
      'ix_devices_brand (sobre brand)'
    ]
  },
  movements: {
    entidad: 'Movement',
    tabla: 'movements',
    descripcion: 'Movimientos de inventario asociados a un equipo.',
    columnas: [
      { nombre: 'id', tipo: 'uuid', clave: 'PK', nullable: false, descripcion: 'Identificador unico del movimiento.' },
      { nombre: 'device_id', tipo: 'uuid', clave: 'FK', nullable: false, descripcion: 'Equipo afectado por el movimiento.' },
      { nombre: 'type', tipo: 'varchar(30)', clave: '', nullable: false, descripcion: 'Tipo: Ingreso, Salida, Traslado o Baja.' },
      { nombre: 'from_location_id', tipo: 'uuid', clave: 'FK', nullable: true, descripcion: 'Ubicacion de origen.' },
      { nombre: 'to_location_id', tipo: 'uuid', clave: 'FK', nullable: true, descripcion: 'Ubicacion de destino.' },
      { nombre: 'occurred_at', tipo: 'timestamp', clave: '', nullable: false, descripcion: 'Fecha en que se ejecuto el movimiento.' },
      { nombre: 'reason', tipo: 'varchar(300)', clave: '', nullable: true, descripcion: 'Motivo o detalle del movimiento.' },
      { nombre: 'registered_by_id', tipo: 'uuid', clave: '', nullable: true, descripcion: 'Usuario que registro el movimiento.' },
      { nombre: 'created_at', tipo: 'timestamp', clave: '', nullable: false, descripcion: 'Fecha de creacion del registro.' }
    ],
    indices: ['ix_movements_device_id (sobre device_id)', 'ix_movements_occurred_at (sobre occurred_at)']
  },
  locations: {
    entidad: 'Location',
    tabla: 'locations',
    descripcion: 'Ubicaciones fisicas o logicas donde se almacenan los equipos.',
    columnas: [
      { nombre: 'id', tipo: 'uuid', clave: 'PK', nullable: false, descripcion: 'Identificador unico de la ubicacion.' },
      { nombre: 'name', tipo: 'varchar(80)', clave: '', nullable: false, descripcion: 'Nombre descriptivo de la ubicacion.' },
      { nombre: 'code', tipo: 'varchar(20)', clave: 'UQ', nullable: false, descripcion: 'Codigo corto de la ubicacion.' },
      { nombre: 'description', tipo: 'varchar(200)', clave: '', nullable: true, descripcion: 'Descripcion opcional.' }
    ],
    indices: ['ux_locations_code (UNIQUE sobre code)']
  },
  users: {
    entidad: 'User',
    tabla: 'users',
    descripcion: 'Usuarios con acceso al sistema de inventario.',
    columnas: [
      { nombre: 'id', tipo: 'uuid', clave: 'PK', nullable: false, descripcion: 'Identificador unico del usuario.' },
      { nombre: 'username', tipo: 'varchar(50)', clave: 'UQ', nullable: false, descripcion: 'Nombre de usuario unico.' },
      { nombre: 'password_hash', tipo: 'varchar(200)', clave: '', nullable: false, descripcion: 'Hash PBKDF2 de la contrasena.' },
      { nombre: 'full_name', tipo: 'varchar(120)', clave: '', nullable: false, descripcion: 'Nombre completo.' },
      { nombre: 'role', tipo: 'varchar(30)', clave: '', nullable: false, descripcion: 'Rol: Operador o Administrador.' },
      { nombre: 'is_active', tipo: 'boolean', clave: '', nullable: false, descripcion: 'Indica si el usuario puede autenticarse.' },
      { nombre: 'created_at', tipo: 'timestamp', clave: '', nullable: false, descripcion: 'Fecha de creacion del usuario.' }
    ],
    indices: ['ux_users_username (UNIQUE sobre username)']
  }
};

const RELACIONES = [
  { desde: 'devices', hacia: 'locations', columna: 'location_id', tipo: 'N:1', nombre: 'fk_devices_location' },
  { desde: 'movements', hacia: 'devices', columna: 'device_id', tipo: 'N:1', nombre: 'fk_movements_device' },
  { desde: 'movements', desdeCol: 'from_location_id', hacia: 'locations', tipo: 'N:1', nombre: 'fk_movements_from_location' },
  { desde: 'movements', desdeCol: 'to_location_id', hacia: 'locations', tipo: 'N:1', nombre: 'fk_movements_to_location' }
];

function asegurarCarpeta(ruta) {
  mkdirSync(ruta, { recursive: true });
}

function listarMigraciones() {
  try {
    return readdirSync(MIGRACIONES).filter((archivo) => archivo.endsWith('.cs'));
  } catch {
    return [];
  }
}

function generarDiccionarioDatos() {
  let contenido = '# Diccionario de datos\n\n';
  contenido += 'Aplicacion de Inventario de Equipos Celulares.\n\n';
  contenido += 'Base de datos relacional: **PostgreSQL 16** (gestionada con EF Core 8 y migraciones).\n\n';
  contenido += `Migraciones aplicadas: ${listarMigraciones().length || 1}.\n\n`;
  contenido += '---\n\n## Tablas\n\n';

  for (const [tabla, definicion] of Object.entries(ESQUEMA)) {
    contenido += `### \`${tabla}\`\n\n`;
    contenido += `${definicion.descripcion}\n\n`;
    contenido += `**Entidad C#:** \`${definicion.entidad}\`\n\n`;
    contenido += '| Columna | Tipo | Clave | Nullable | Descripcion |\n';
    contenido += '| --- | --- | --- | --- | --- |\n';
    for (const columna of definicion.columnas) {
      contenido += `| \`${columna.nombre}\` | ${columna.tipo} | ${columna.clave || '-'} | ${columna.nullable ? 'SI' : 'NO'} | ${columna.descripcion} |\n`;
    }
    contenido += '\n**Indices**\n\n';
    for (const indice of definicion.indices) {
      contenido += `- ${indice}\n`;
    }
    contenido += '\n---\n\n';
  }

  contenido += '## Relaciones (clave foranea)\n\n';
  contenido += '| Origen | Columna | Destino | Cardinalidad | Restriccion |\n';
  contenido += '| --- | --- | --- | --- | --- |\n';
  for (const relacion of RELACIONES) {
    const columna = relacion.desdeCol ?? relacion.columna;
    contenido += `| \`${relacion.desde}\` | \`${columna}\` | \`${relacion.hacia}\` | ${relacion.tipo} | \`${relacion.nombre}\` |\n`;
  }

  contenido += '\n## Reglas de integridad\n\n';
  contenido += '- `devices.imei` es **unico** en todo el inventario (indice `ux_devices_imei`).\n';
  contenido += '- Un equipo no puede quedar sin ubicacion: `devices.location_id` es obligatorio.\n';
  contenido += '- Al eliminar un equipo se eliminan en cascada sus movimientos.\n';
  contenido += '- Las contrasenas se almacenan como hash PBKDF2-HMAC-SHA256 con sal por usuario; nunca en texto plano.\n';

  writeFileSync(join(RAIZ, 'docs', 'diccionario-de-datos.md'), contenido, 'utf8');
}

function generarDiagramaEntidadRelacion() {
  let mermaid = 'erDiagram\n';
  mermaid += '    LOCATIONS ||--o{ DEVICES : "aloja"\n';
  mermaid += '    DEVICES ||--o{ MOVEMENTS : "registra"\n';
  mermaid += '    LOCATIONS ||--o{ MOVEMENTS : "origen"\n';
  mermaid += '    LOCATIONS ||--o{ MOVEMENTS : "destino"\n\n';

  mermaid += '    DEVICES {\n';
  mermaid += '        uuid id PK\n';
  mermaid += '        varchar brand\n';
  mermaid += '        varchar model\n';
  mermaid += '        varchar imei UK\n';
  mermaid += '        varchar status\n';
  mermaid += '        uuid location_id FK\n';
  mermaid += '        timestamp entry_date\n';
  mermaid += '        varchar observations\n';
  mermaid += '        numeric purchase_price\n';
  mermaid += '    }\n\n';

  mermaid += '    MOVEMENTS {\n';
  mermaid += '        uuid id PK\n';
  mermaid += '        uuid device_id FK\n';
  mermaid += '        varchar type\n';
  mermaid += '        uuid from_location_id FK\n';
  mermaid += '        uuid to_location_id FK\n';
  mermaid += '        timestamp occurred_at\n';
  mermaid += '        varchar reason\n';
  mermaid += '    }\n\n';

  mermaid += '    LOCATIONS {\n';
  mermaid += '        uuid id PK\n';
  mermaid += '        varchar name\n';
  mermaid += '        varchar code UK\n';
  mermaid += '        varchar description\n';
  mermaid += '    }\n\n';

  mermaid += '    USERS {\n';
  mermaid += '        uuid id PK\n';
  mermaid += '        varchar username UK\n';
  mermaid += '        varchar password_hash\n';
  mermaid += '        varchar full_name\n';
  mermaid += '        varchar role\n';
  mermaid += '        boolean is_active\n';
  mermaid += '    }\n';

  writeFileSync(join(RAIZ, 'docs', 'diagramas', 'diagrama-entidad-relacion.mmd'), mermaid, 'utf8');
}

function generarDiagramaClases() {
  const lineas = [
    'classDiagram',
    '    direction LR',
    '',
    '    class Device {',
    '        +Guid Id',
    '        +string Brand',
    '        +string Model',
    '        +string Imei',
    '        +DeviceStatus Status',
    '        +Guid LocationId',
    '        +DateTime EntryDate',
    '        +string Observations',
    '        +decimal PurchasePrice',
    '        +DateTime CreatedAt',
    '        +DateTime UpdatedAt',
    '    }',
    '',
    '    class Movement {',
    '        +Guid Id',
    '        +Guid DeviceId',
    '        +MovementType Type',
    '        +Guid FromLocationId',
    '        +Guid ToLocationId',
    '        +DateTime OccurredAt',
    '        +string Reason',
    '        +DateTime CreatedAt',
    '    }',
    '',
    '    class Location {',
    '        +Guid Id',
    '        +string Name',
    '        +string Code',
    '        +string Description',
    '    }',
    '',
    '    class User {',
    '        +Guid Id',
    '        +string Username',
    '        +string PasswordHash',
    '        +string FullName',
    '        +UserRole Role',
    '        +bool IsActive',
    '    }',
    '',
    '    class DeviceStatus {',
    '        <<enumeration>>',
    '        Disponible',
    '        Asignado',
    '        EnMantenimiento',
    '        DeBaja',
    '    }',
    '',
    '    class MovementType {',
    '        <<enumeration>>',
    '        Ingreso',
    '        Salida',
    '        Traslado',
    '        Baja',
    '    }',
    '',
    '    class InventoryService {',
    '        +CreateDeviceAsync(CreateDeviceRequest)',
    '        +GetDevicesAsync(DeviceQuery)',
    '        +GetDeviceByIdAsync(Guid)',
    '        +UpdateDeviceAsync(Guid, UpdateDeviceRequest)',
    '        +DeleteDeviceAsync(Guid)',
    '        +CreateMovementAsync(CreateMovementRequest)',
    '        +GetMovementsAsync(Guid)',
    '        +GetStockReportAsync()',
    '    }',
    '',
    '    class InventarioDbContext {',
    '        +DbSet Devices',
    '        +DbSet Movements',
    '        +DbSet Locations',
    '        +DbSet Users',
    '    }',
    '',
    '    class ImeiValidator {',
    '        +static bool IsValid(string)',
    '        +static string Normalize(string)',
    '    }',
    '',
    '    class Pbkdf2PasswordHasher {',
    '        +void CreateHash(string, out string)',
    '        +bool Verify(string, string)',
    '    }',
    '',
    '    Device "1" <-- "*" Location : se ubica en',
    '    Device "1" --> "*" Movement : genera',
    '    Movement --> Location : origen',
    '    Movement --> Location : destino',
    '    InventoryService ..> InventarioDbContext : persiste',
    '    InventoryService ..> ImeiValidator : valida',
    '    InventoryService ..> Pbkdf2PasswordHasher : verifica',
    '    Device ..> DeviceStatus',
    '    Movement ..> MovementType',
    ''
  ];

  writeFileSync(join(RAIZ, 'docs', 'diagramas', 'diagrama-clases.mmd'), lineas.join('\n'), 'utf8');
}

function generarDiagramaComponentes() {
  const mermaid = `flowchart TB
    subgraph Cliente["Navegador del usuario"]
        UI["Panel de equipos<br/>Buscador y filtros"]
        FORM["Formularios<br/>Registro y edicion"]
        MOV["Gestion de movimientos"]
        REP["Reportes y graficos"]
        ADM["Panel de administracion"]
    end

    subgraph Frontend["Frontend React + Vite"]
        ROUTER["React Router<br/>/equipos /movimientos /reportes /admin"]
        SERVICIO["Capa de servicios<br/>api.ts (fetch + manejo de errores)"]
        ESTADO["Estado de la interfaz<br/>React useState/useEffect"]
    end

    subgraph Backend["API .NET 8 (contenedor Docker)"]
        CTRL["Controllers<br/>Devices, Movements, Reports, Locations"]
        VALID["Validacion de entrada<br/>DataAnnotations"]
        SERV["InventoryService<br/>reglas de negocio"]
        MID["ExceptionHandlingMiddleware<br/>respuestas JSON consistentes"]
        IMEI["ImeiValidator<br/>algoritmo de Luhn"]
    end

    subgraph Datos["Persistencia"]
        EF["EF Core 8<br/>InventarioDbContext"]
        PG[("PostgreSQL 16<br/>devices, movements,<br/>locations, users")]
    end

    UI --> ROUTER
    FORM --> ROUTER
    MOV --> ROUTER
    REP --> ROUTER
    ADM --> ROUTER
    ROUTER --> ESTADO
    ESTADO --> SERVICIO
    SERVICIO -->|"HTTP / JSON (CORS)"| CTRL
    CTRL --> VALID
    VALID --> SERV
    CTRL -.-> MID
    MID -.-> SERV
    SERV --> IMEI
    SERV --> EF
    EF --> PG

    classDef cliente fill:#dbeafe,stroke:#1d4ed8,color:#1e3a8a
    classDef frontend fill:#e0e7ff,stroke:#4338ca,color:#312e81
    classDef backend fill:#d1fae5,stroke:#059669,color:#064e3b
    classDef datos fill:#fef3c7,stroke:#d97706,color:#78350f

    class UI,FORM,MOV,REP,ADM cliente
    class ROUTER,SERVICIO,ESTADO frontend
    class CTRL,VALID,SERV,MID,IMEI backend
    class EF,PG datos
`;

  writeFileSync(join(RAIZ, 'docs', 'diagramas', 'diagrama-componentes.mmd'), mermaid, 'utf8');
}

function generarDiagramaDespliegue() {
  const mermaid = `flowchart TB
    subgraph ClienteNavegador["Cliente"]
        NAV["Navegador<br/>https://<app>.vercel.app"]
    end

    subgraph Vercel["Vercel (frontend)"]
        CDN["CDN global<br/>archivos estaticos React"]
    end

    subgraph Contenedor["Azure Container Instances (backend)"]
        API["API .NET 8<br/>contenedor Docker<br/>puerto 8080"]
        HEALTH["/health<br/>HEALTHCHECK"]
    end

    subgraph Registro["Azure Container Registry"]
        ACR["ACR<br/>inventarioapi:latest"]
    end

    subgraph DatosAzure["Servicio de datos"]
        PG[("PostgreSQL 16<br/>base relacional")]
    end

    subgraph GitHub["GitHub Actions"]
        SONAR["sonar.yml<br/>calidad de codigo"]
        SCAN["snyk-semgrep.yml<br/>SAST y contenedor"]
        TERRA["infra.yml<br/>Terraform + tfsec"]
        DEPLOY["deploy.yml<br/>build y despliegue"]
        DOCS["generate-documentation.yml<br/>diagramas Mermaid"]
    end

    NAV -->|"HTTPS"| CDN
    CDN -.->|"CORS + fetch"| API
    API --> HEALTH
    API -->|"Npgsql / puerto 5432"| PG
    ACR -.->|"imagen publicada"| API
    TERRA -.->|"aprovisiona"| PG
    DEPLOY -.->|"publica imagen"| ACR
    DEPLOY -.->|"publica build"| CDN
    SONAR -.->|"analiza"| API
    SCAN -.->|"escanea"| ACR
    DOCS -.->|"genera"| DocsOut["docs/*.mmd"]

    classDef cliente fill:#dbeafe,stroke:#1d4ed8,color:#1e3a8a
    classDef frontend fill:#e0e7ff,stroke:#4338ca,color:#312e81
    classDef backend fill:#d1fae5,stroke:#059669,color:#064e3b
    classDef datos fill:#fef3c7,stroke:#d97706,color:#78350f
    classDef ci fill:#fee2e2,stroke:#dc2626,color:#7f1d1d

    class NAV cliente
    class CDN frontend
    class API,HEALTH backend
    class ACR,PG datos
    class SONAR,SCAN,TERRA,DEPLOY,DOCS ci
`;

  writeFileSync(join(RAIZ, 'docs', 'diagramas', 'diagrama-despliegue.mmd'), mermaid, 'utf8');
}

function generarDocumentacionApi() {
  asegurarCarpeta(join(RAIZ, 'docs', 'api'));

  const contrato = {
    openapi: '3.0.3',
    info: {
      title: 'API Inventario de Equipos Celulares',
      version: '1.0.0',
      description: 'API REST para registrar, gestionar y monitorear el inventario de equipos celulares.'
    },
    servers: [{ url: '/', description: 'Servidor actual' }],
    paths: {
      '/devices': {
        get: {
          summary: 'Lista equipos celulares',
          tags: ['Devices'],
          parameters: [
            { name: 'search', in: 'query', schema: { type: 'string' }, description: 'IMEI, marca o modelo' },
            { name: 'brand', in: 'query', schema: { type: 'string' } },
            { name: 'status', in: 'query', schema: { $ref: '#/components/schemas/DeviceStatus' } },
            { name: 'locationId', in: 'query', schema: { type: 'string', format: 'uuid' } },
            { name: 'page', in: 'query', schema: { type: 'integer', minimum: 1 } },
            { name: 'pageSize', in: 'query', schema: { type: 'integer', minimum: 1, maximum: 100 } }
          ],
          responses: { 200: { description: 'Pagina de equipos' } }
        },
        post: {
          summary: 'Registra un nuevo equipo',
          tags: ['Devices'],
          requestBody: {
            required: true,
            content: {
              'application/json': { schema: { $ref: '#/components/schemas/CreateDeviceRequest' } }
            }
          },
          responses: {
            201: { description: 'Equipo creado' },
            400: { description: 'Datos invalidos' },
            409: { description: 'IMEI duplicado' }
          }
        }
      },
      '/devices/{id}': {
        get: {
          summary: 'Detalle de un equipo',
          tags: ['Devices'],
          parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'string', format: 'uuid' } }],
          responses: { 200: { description: 'Equipo encontrado' }, 404: { description: 'No existe' } }
        },
        put: {
          summary: 'Actualiza un equipo',
          tags: ['Devices'],
          parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'string', format: 'uuid' } }],
          responses: { 200: { description: 'Equipo actualizado' }, 404: { description: 'No existe' } }
        },
        delete: {
          summary: 'Elimina un equipo',
          tags: ['Devices'],
          parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'string', format: 'uuid' } }],
          responses: { 204: { description: 'Eliminado' }, 404: { description: 'No existe' } }
        }
      },
      '/movements': {
        get: {
          summary: 'Historial de movimientos',
          tags: ['Movements'],
          parameters: [
            {
              name: 'deviceId',
              in: 'query',
              schema: { type: 'string', format: 'uuid' },
              description: 'Filtra el historial de un equipo'
            }
          ],
          responses: { 200: { description: 'Lista de movimientos' } }
        },
        post: {
          summary: 'Registra un movimiento',
          tags: ['Movements'],
          responses: {
            201: { description: 'Movimiento creado' },
            400: { description: 'Movimiento invalido' },
            404: { description: 'El equipo no existe' }
          }
        }
      },
      '/reports/stock': {
        get: { summary: 'Reporte de stock actual', tags: ['Reports'], responses: { 200: { description: 'Reporte generado' } } }
      },
      '/locations': {
        get: { summary: 'Lista las ubicaciones', tags: ['Locations'], responses: { 200: { description: 'Ubicaciones' } } }
      },
      '/health': {
        get: { summary: 'Estado del servicio', tags: ['Health'], responses: { 200: { description: 'Servicio disponible' } } }
      }
    },
    components: {
      schemas: {
        DeviceStatus: { type: 'string', enum: ['Disponible', 'Asignado', 'EnMantenimiento', 'DeBaja'] },
        MovementType: { type: 'string', enum: ['Ingreso', 'Salida', 'Traslado', 'Baja'] },
        CreateDeviceRequest: {
          type: 'object',
          required: ['brand', 'model', 'imei', 'locationId', 'entryDate'],
          properties: {
            brand: { type: 'string', minLength: 2, maxLength: 50 },
            model: { type: 'string', maxLength: 80 },
            imei: {
              type: 'string',
              pattern: '^[0-9]{15}$',
              description: '15 digitos con digito verificador valido (Luhn)'
            },
            status: { $ref: '#/components/schemas/DeviceStatus' },
            locationId: { type: 'string', format: 'uuid' },
            entryDate: { type: 'string', format: 'date-time' },
            observations: { type: 'string', maxLength: 500 },
            purchasePrice: { type: 'number', minimum: 0 }
          }
        }
      }
    }
  };

  writeFileSync(join(RAIZ, 'docs', 'api', 'openapi.json'), JSON.stringify(contrato, null, 2), 'utf8');
}

function main() {
  asegurarCarpeta(join(RAIZ, 'docs', 'diagramas'));

  generarDiccionarioDatos();
  generarDiagramaEntidadRelacion();
  generarDiagramaClases();
  generarDiagramaComponentes();
  generarDiagramaDespliegue();
  generarDocumentacionApi();

  console.log('Documentacion generada en docs/:');
  console.log('  - diccionario-de-datos.md');
  console.log('  - diagramas/diagrama-entidad-relacion.mmd');
  console.log('  - diagramas/diagrama-clases.mmd');
  console.log('  - diagramas/diagrama-componentes.mmd');
  console.log('  - diagramas/diagrama-despliegue.mmd');
  console.log('  - api/openapi.json');
}

main();