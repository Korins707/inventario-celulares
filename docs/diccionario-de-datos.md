# Diccionario de datos

Aplicacion de Inventario de Equipos Celulares.

Base de datos relacional: **PostgreSQL 16** (gestionada con EF Core 8 y migraciones).

Migraciones aplicadas: 3.

---

## Tablas

### `devices`

Equipos celulares registrados en el inventario.

**Entidad C#:** `Device`

| Columna | Tipo | Clave | Nullable | Descripcion |
| --- | --- | --- | --- | --- |
| `id` | uuid | PK | NO | Identificador unico del equipo. |
| `brand` | varchar(50) | - | NO | Marca del fabricante. |
| `model` | varchar(80) | - | NO | Modelo comercial del equipo. |
| `imei` | varchar(15) | UQ | NO | IMEI de 15 digitos, unico en el inventario. |
| `status` | varchar(30) | - | NO | Estado actual: Disponible, Asignado, EnMantenimiento o DeBaja. |
| `location_id` | uuid | FK | NO | Ubicacion actual del equipo. |
| `entry_date` | timestamp | - | NO | Fecha de ingreso al inventario. |
| `observations` | varchar(500) | - | SI | Observaciones libres. |
| `purchase_price` | numeric(12,2) | - | SI | Precio de compra referencial en soles. |
| `created_at` | timestamp | - | NO | Fecha de creacion del registro. |
| `updated_at` | timestamp | - | NO | Ultima fecha de actualizacion. |

**Indices**

- ux_devices_imei (UNIQUE sobre imei)
- ix_devices_status (sobre status)
- ix_devices_brand (sobre brand)

---

### `movements`

Movimientos de inventario asociados a un equipo.

**Entidad C#:** `Movement`

| Columna | Tipo | Clave | Nullable | Descripcion |
| --- | --- | --- | --- | --- |
| `id` | uuid | PK | NO | Identificador unico del movimiento. |
| `device_id` | uuid | FK | NO | Equipo afectado por el movimiento. |
| `type` | varchar(30) | - | NO | Tipo: Ingreso, Salida, Traslado o Baja. |
| `from_location_id` | uuid | FK | SI | Ubicacion de origen. |
| `to_location_id` | uuid | FK | SI | Ubicacion de destino. |
| `occurred_at` | timestamp | - | NO | Fecha en que se ejecuto el movimiento. |
| `reason` | varchar(300) | - | SI | Motivo o detalle del movimiento. |
| `registered_by_id` | uuid | - | SI | Usuario que registro el movimiento. |
| `created_at` | timestamp | - | NO | Fecha de creacion del registro. |

**Indices**

- ix_movements_device_id (sobre device_id)
- ix_movements_occurred_at (sobre occurred_at)

---

### `locations`

Ubicaciones fisicas o logicas donde se almacenan los equipos.

**Entidad C#:** `Location`

| Columna | Tipo | Clave | Nullable | Descripcion |
| --- | --- | --- | --- | --- |
| `id` | uuid | PK | NO | Identificador unico de la ubicacion. |
| `name` | varchar(80) | - | NO | Nombre descriptivo de la ubicacion. |
| `code` | varchar(20) | UQ | NO | Codigo corto de la ubicacion. |
| `description` | varchar(200) | - | SI | Descripcion opcional. |

**Indices**

- ux_locations_code (UNIQUE sobre code)

---

### `users`

Usuarios con acceso al sistema de inventario.

**Entidad C#:** `User`

| Columna | Tipo | Clave | Nullable | Descripcion |
| --- | --- | --- | --- | --- |
| `id` | uuid | PK | NO | Identificador unico del usuario. |
| `username` | varchar(50) | UQ | NO | Nombre de usuario unico. |
| `password_hash` | varchar(200) | - | NO | Hash PBKDF2 de la contrasena. |
| `full_name` | varchar(120) | - | NO | Nombre completo. |
| `role` | varchar(30) | - | NO | Rol: Operador o Administrador. |
| `is_active` | boolean | - | NO | Indica si el usuario puede autenticarse. |
| `created_at` | timestamp | - | NO | Fecha de creacion del usuario. |

**Indices**

- ux_users_username (UNIQUE sobre username)

---

## Relaciones (clave foranea)

| Origen | Columna | Destino | Cardinalidad | Restriccion |
| --- | --- | --- | --- | --- |
| `devices` | `location_id` | `locations` | N:1 | `fk_devices_location` |
| `movements` | `device_id` | `devices` | N:1 | `fk_movements_device` |
| `movements` | `from_location_id` | `locations` | N:1 | `fk_movements_from_location` |
| `movements` | `to_location_id` | `locations` | N:1 | `fk_movements_to_location` |

## Reglas de integridad

- `devices.imei` es **unico** en todo el inventario (indice `ux_devices_imei`).
- Un equipo no puede quedar sin ubicacion: `devices.location_id` es obligatorio.
- Al eliminar un equipo se eliminan en cascada sus movimientos.
- Las contrasenas se almacenan como hash PBKDF2-HMAC-SHA256 con sal por usuario; nunca en texto plano.
