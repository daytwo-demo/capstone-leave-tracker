# LeaveTracker.Api

API mínima en ASP.NET Core para gestionar solicitudes de vacaciones de
empleados y sus notas de aprobación.

## Requisitos

- .NET 10 SDK
- PostgreSQL 16 accesible por red

## Base de datos

Necesita un PostgreSQL local escuchando en `localhost:5432`, base
`leaves`, usuario `leaves`, password `Leaves!2026` (ver la cadena de
conexión en `Program.cs`). El esquema se crea automáticamente al arrancar
(`EnsureCreated`), no hay migraciones.

## Ejecutar

```bash
dotnet run
```

La API queda disponible en `http://localhost:8080`.

## Endpoints

### Solicitudes de vacaciones (`LeaveRequest`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/leaves` | Lista todas las solicitudes, ordenadas por fecha de creación |
| GET | `/api/leaves/{id}` | Obtiene una solicitud puntual |
| POST | `/api/leaves` | Crea una solicitud (siempre arranca en estado `Pending`) |
| PUT | `/api/leaves/{id}` | Reemplaza los campos de una solicitud, incluido `Status` |
| DELETE | `/api/leaves/{id}` | Elimina una solicitud |

Body de ejemplo para `POST /api/leaves`:

```json
{
  "employee": "Maria Sanchez",
  "startDate": "2026-09-01",
  "endDate": "2026-09-05",
  "reason": "Vacaciones familiares"
}
```

Body de ejemplo para `PUT /api/leaves/{id}`:

```json
{
  "employee": "Maria Sanchez",
  "startDate": "2026-09-01",
  "endDate": "2026-09-05",
  "reason": "Vacaciones familiares",
  "status": "Approved"
}
```

### Notas de aprobación (`ApprovalNote`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/leaves/{id}/notes` | Lista las notas de una solicitud, ordenadas por fecha de creación |
| POST | `/api/leaves/{id}/notes` | Agrega una nota a una solicitud |

Body de ejemplo para `POST /api/leaves/{id}/notes`:

```json
{
  "author": "Juan Perez",
  "note": "Aprobado, coordinar cobertura con el equipo"
}
```

## Configuración hardcodeada (a propósito)

`Program.cs` tiene dos constantes fijas en el código:

- `MaxResultsPerPage`: límite de resultados en `GET /api/leaves`.
- `ExternalApiKey`: crear una solicitud (`POST /api/leaves`) simula que
  la app necesita notificar a un sistema externo, y para eso necesita
  esta clave. Si queda vacía, `POST /api/leaves` responde `500` con
  `"Falta ExternalApiKey: no se puede notificar al sistema externo."`.

## Endpoint de carga

`GET /api/carga/{n}` calcula el n-ésimo número de Fibonacci de forma
recursiva (sin memoización), lo que consume CPU real de forma
proporcional a `n`. Útil para generar carga sostenida en pruebas.
Devuelve el resultado y el tiempo transcurrido en milisegundos.

```bash
curl http://localhost:8080/api/carga/35
```

Valores de `n` entre 38 y 42 tardan varios segundos reales en responder.

## Probar rápido con curl

```bash
# Crear una solicitud
curl -X POST http://localhost:8080/api/leaves \
  -H "Content-Type: application/json" \
  -d '{"employee":"Maria Sanchez","startDate":"2026-09-01","endDate":"2026-09-05","reason":"Vacaciones familiares"}'

# Listar solicitudes
curl http://localhost:8080/api/leaves

# Agregar una nota de aprobación
curl -X POST http://localhost:8080/api/leaves/{id}/notes \
  -H "Content-Type: application/json" \
  -d '{"author":"Juan Perez","note":"Aprobado"}'
```
