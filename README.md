# core-transactions-service — Fintech Platform

Microservicio de escritura (Write Side CQRS) de la plataforma Fintech.  
Stack: **.NET Core 9 · Hexagonal + DDD · CQRS (MediatR) · MySQL 8 · Kafka (EDA) · Docker**

---

## Tabla de contenido

1. [Pre-requisitos](#1-pre-requisitos)
2. [Estructura del proyecto](#2-estructura-del-proyecto)
3. [Infraestructura Docker — servicios compartidos](#3-infraestructura-docker--servicios-compartidos)
   - 3.1 [Levantar los contenedores](#31-levantar-los-contenedores)
   - 3.2 [Verificar que todos los servicios están healthy](#32-verificar-que-todos-los-servicios-están-healthy)
   - 3.3 [Validar cada servicio individualmente](#33-validar-cada-servicio-individualmente)
   - 3.4 [Kafka UI](#34-kafka-ui)
   - 3.5 [Detener la infraestructura](#35-detener-la-infraestructura)
4. [Variables de conexión (appsettings.Development.json)](#4-variables-de-conexión-appsettingsdevelopmentjson)
5. [Ejecutar el microservicio (.NET)](#5-ejecutar-el-microservicio-net)
6. [Build con Docker (imagen del microservicio)](#6-build-con-docker-imagen-del-microservicio)
7. [Puertos y credenciales de referencia](#7-puertos-y-credenciales-de-referencia)
8. [Solución de problemas comunes](#8-solución-de-problemas-comunes)

---

## 1. Pre-requisitos

| Herramienta | Versión mínima | Descarga |
|---|---|---|
| Docker Desktop | 26.x o superior | https://www.docker.com/products/docker-desktop/ |
| Docker Compose | v2.x (incluido en Docker Desktop) | — |
| .NET SDK | 9.0 | https://dotnet.microsoft.com/download/dotnet/9.0 |

> **Windows:** Docker Desktop debe estar iniciado (ícono en la barra de tareas) antes de ejecutar cualquier comando `docker`.

Verificar instalación:

```bash
docker --version          # Docker version 26.x.x o superior
docker compose version    # Docker Compose version v2.x.x o superior
dotnet --version          # 9.0.x
```

---

## 2. Estructura del proyecto

```
PruebaNetCoreProject/
├── docker/
│   └── docker-compose.infra.yml   ← Infraestructura compartida (MySQL, MongoDB, Redis, Kafka)
├── src/
│   ├── Core/
│   │   ├── Domain/                ← Entidades, Value Objects, Agregados, Interfaces de dominio
│   │   └── Application/           ← Commands, Queries, Ports, CQRS handlers, Result pattern
│   ├── Infrastructure/
│   │   ├── Persistence/           ← EF Core (MySQL), MongoDB, Redis, Outbox
│   │   └── DependencyInjection/   ← Registro de servicios y configuración
│   └── Presentation/
│       └── API/                   ← Controllers, Program.cs, middlewares
├── tests/
│   ├── Domain.Tests/
│   ├── Application.Tests/
│   └── Infrastructure.Tests/
├── Dockerfile                     ← Build multi-stage de la imagen del microservicio
├── .dockerignore
├── .editorconfig                  ← Reglas SonarLint C#
└── PruebaNetCoreProject.sln
```

---

## 3. Infraestructura Docker — servicios compartidos

El archivo `docker/docker-compose.infra.yml` levanta **5 contenedores** en la red `fintech-network`:

| Contenedor | Imagen | Puerto | Descripción |
|---|---|---|---|
| `mysql-fintech` | `mysql:8.0` | `3306` | Base de datos Write (ACID, EF Core) |
| `mongodb-fintech` | `mongo:7.0` | `27017` | Base de datos Read (modelos de consulta) |
| `redis-fintech` | `redis:7-alpine` | `6379` | Caché (Cache-Aside, TTL) |
| `kafka-fintech` | `apache/kafka:latest` | `9092` | Broker de eventos EDA (KRaft, sin Zookeeper) |
| `kafka-ui-fintech` | `provectuslabs/kafka-ui` | `8090` | Panel web de administración de Kafka |

### 3.1 Levantar los contenedores

Ejecutar **desde la raíz del repositorio** (`PruebaNetCoreProject/`):

```bash
docker compose -f docker/docker-compose.infra.yml up -d
```

> La primera vez descarga las imágenes (~1–2 GB en total). Las siguientes ejecuciones son inmediatas.

Salida esperada al finalizar:

```
Container mysql-fintech      Started
Container mongodb-fintech    Started
Container redis-fintech      Started
Container kafka-fintech      Started
Container kafka-ui-fintech   Started
```

### 3.2 Verificar que todos los servicios están healthy

```bash
docker compose -f docker/docker-compose.infra.yml ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
```

Resultado esperado (todos deben mostrar `healthy` o `running`):

```
NAME               STATUS                   PORTS
kafka-fintech      Up X minutes (healthy)   0.0.0.0:9092->9092/tcp
kafka-ui-fintech   Up X minutes             0.0.0.0:8090->8080/tcp
mongodb-fintech    Up X minutes (healthy)   0.0.0.0:27017->27017/tcp
mysql-fintech      Up X minutes (healthy)   0.0.0.0:3306->3306/tcp
redis-fintech      Up X minutes (healthy)   0.0.0.0:6379->6379/tcp
```

> Kafka puede tardar hasta 30 segundos en pasar a `healthy`. Si aún está en `starting`, esperar unos segundos y volver a ejecutar el comando.

### 3.3 Validar cada servicio individualmente

**MySQL** — respuesta esperada: `mysqld is alive`

```bash
docker exec mysql-fintech mysqladmin ping -h localhost -u root -pfintechroot2024
```

**MongoDB** — respuesta esperada: `{ ok: 1 }`

```bash
docker exec mongodb-fintech mongosh --eval "db.adminCommand('ping')" --quiet
```

**Redis** — respuesta esperada: `PONG`

```bash
docker exec redis-fintech redis-cli -a fintech2024 ping
```

**Kafka** — respuesta esperada: lista de topics (vacía al inicio es correcto)

```bash
docker exec kafka-fintech /opt/kafka/bin/kafka-topics.sh --list --bootstrap-server localhost:9092
```

**Verificar base de datos MySQL creada:**

```bash
docker exec mysql-fintech mysql -u root -pfintechroot2024 -e "SHOW DATABASES;"
# Debe incluir: fintech_db
```

### 3.4 Kafka UI

Panel web disponible en: **http://localhost:8090**

Permite visualizar topics, consumer groups, mensajes y el estado del broker sin necesidad de CLI.

### 3.5 Detener la infraestructura

```bash
# Detener contenedores (conserva los volúmenes / datos)
docker compose -f docker/docker-compose.infra.yml down

# Detener y eliminar todos los datos (reset total)
docker compose -f docker/docker-compose.infra.yml down -v
```

---

## 4. Variables de conexión (appsettings.Development.json)

Las cadenas de conexión ya están configuradas en `src/Presentation/API/appsettings.Development.json` apuntando a los contenedores Docker:

```json
{
  "ConnectionStrings": {
    "MySQL":   "Server=127.0.0.1;Port=3306;Database=fintech_db;Uid=fintech;Pwd=fintech2024;",
    "MongoDB": "mongodb://localhost:27017",
    "Redis":   "localhost:6379,password=fintech2024"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

> No modificar estas cadenas para entornos locales. Para staging/producción usar variables de entorno o secretos de Kubernetes.

---

## 5. Ejecutar el microservicio (.NET)

Con la infraestructura Docker corriendo, ejecutar desde la raíz del repositorio:

```bash
# Restaurar dependencias
dotnet restore PruebaNetCoreProject.sln

# Compilar (con TreatWarningsAsErrors activo)
dotnet build PruebaNetCoreProject.sln

# Ejecutar la API
dotnet run --project src/Presentation/API/API.csproj
```

La API estará disponible en:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger UI:** https://localhost:5001/swagger

Ejecutar los tests:

```bash
dotnet test PruebaNetCoreProject.sln
```

---

## 6. Build con Docker (imagen del microservicio)

Para construir y ejecutar el microservicio como contenedor Docker:

```bash
# Build de la imagen multi-stage
docker build -t core-transactions-service:latest .

# Ejecutar el contenedor conectado a la red de infraestructura
docker run -d \
  --name core-transactions-service \
  --network fintech-network \
  -p 8080:8080 \
  -e ConnectionStrings__MySQL="Server=mysql-fintech;Port=3306;Database=fintech_db;Uid=fintech;Pwd=fintech2024;" \
  -e ConnectionStrings__MongoDB="mongodb://mongodb-fintech:27017" \
  -e ConnectionStrings__Redis="redis-fintech:6379,password=fintech2024" \
  core-transactions-service:latest
```

> El `Dockerfile` usa build multi-stage (SDK 9.0 → aspnet:9.0) con usuario non-root para cumplir estándares de seguridad ISO 27001.

---

## 7. Puertos y credenciales de referencia

| Servicio | Puerto | Usuario | Contraseña | Base de datos |
|---|---|---|---|---|
| MySQL | `3306` | `root` / `fintech` | `fintechroot2024` / `fintech2024` | `fintech_db` |
| MongoDB | `27017` | — (sin auth en dev) | — | `fintech_db` |
| Redis | `6379` | — | `fintech2024` | — |
| Kafka Broker | `9092` | — | — | — |
| Kafka UI | `8090` | — | — | — |
| API (.NET) | `5000/5001` | — | JWT Bearer | — |

> **Seguridad:** Estas credenciales son exclusivas para el entorno de desarrollo local. Nunca usar en staging o producción.

---

## 8. Solución de problemas comunes

**`docker: command not found`**  
→ Docker Desktop no está instalado o no está iniciado. Abrir Docker Desktop y esperar a que el daemon arranque.

**`Error: port is already allocated`**  
→ Algún puerto (3306, 27017, 6379, 9092, 8090) está ocupado por otro proceso.  
Identificar y detener el proceso: `netstat -ano | findstr :<PUERTO>` (Windows).

**`kafka-fintech` se queda en `starting` indefinidamente**  
→ Kafka KRaft necesita unos segundos extra para inicializarse. Esperar 30s y volver a ejecutar `docker compose ps`.

**`Connection refused` al ejecutar el .NET**  
→ Verificar que los contenedores están en estado `healthy` antes de levantar la API.  
Ejecutar el paso [3.2](#32-verificar-que-todos-los-servicios-están-healthy) primero.

**`bitnami/kafka: not found`**  
→ La imagen `bitnami/kafka` dejó de ser gratuita en Docker Hub (2025). Este proyecto usa `apache/kafka:latest` (imagen oficial Apache). Si ves este error en algún fork o copia anterior, actualizar la imagen en `docker-compose.infra.yml`.

**Reset total del entorno (borrar todos los datos)**  
```bash
docker compose -f docker/docker-compose.infra.yml down -v
docker compose -f docker/docker-compose.infra.yml up -d
```
