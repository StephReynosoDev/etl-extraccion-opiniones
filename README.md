# Extracción de opiniones de clientes

Proyecto de Electiva 1 (Introducción a la Big Data). Es la parte de extracción de un proceso ETL: un Worker Service en .NET 8 que saca datos de tres fuentes distintas y los guarda en una carpeta de staging como archivos JSON.

## Qué hace

Lee de estas fuentes al mismo tiempo:

- Un archivo CSV con encuestas internas (500 registros).
- Una base de datos SQL Server, tabla `dbo.ResenasWeb` (200 registros).
- Una API REST que devuelve comentarios de redes sociales (200 registros).

Cada fuente se convierte a un mismo modelo (`OpinionExtraida`) y se guarda en `staging`. Por cada una se muestra en consola cuántos registros salieron y cuánto tardó, y al final el total.

## Estructura

```
ApiComentarios/       API que sirve social_comments.csv como JSON
ExtraccionOpiniones/  Worker Service
  Dominio/            IExtractor, OpinionExtraida, ConfiguracionFuentes
  Infraestructura/    CsvExtractor, DatabaseExtractor, ApiExtractor, StagingWriter
  Aplicacion/         ExtraccionWorker
Scripts/              fuente_resenas.sql
```

## Cómo correrlo

Necesitas el SDK de .NET 8 y SQL Server (yo uso SQL Express).

1. Abre `Scripts/fuente_resenas.sql` en SSMS y ejecútalo completo. Crea la base `FuenteResenas` con la tabla `ResenasWeb` y sus 200 reseñas.
2. Si tu servidor no se llama `STEPH\SQLEXPRESS`, cambia la cadena de conexión en `ExtraccionOpiniones/appsettings.json`.
3. En una terminal, dentro de `ApiComentarios`:
   ```
   dotnet run
   ```
   Déjala abierta, la API queda en `http://localhost:5100/api/comentarios`.
4. En otra terminal, dentro de `ExtraccionOpiniones`:
   ```
   dotnet run
   ```
5. Cuando termine se cierra sola y los JSON quedan en la carpeta `staging`.

Si en tu PC solo tienes .NET 10 y sale el error de que falta el framework 8, antes del `dotnet run` ejecuta en PowerShell:

```
$env:DOTNET_ROLL_FORWARD="Major"
```

## Configuración

Todo está en `ExtraccionOpiniones/appsettings.json`: la ruta del CSV, la cadena de conexión, la URL de la API y la carpeta de staging.

## Agregar otra fuente

1. Crear una clase que implemente `IExtractor`.
2. Registrarla en `Program.cs` con `AddSingleton<IExtractor, MiNuevoExtractor>()`.

El Worker la toma solo, no hay que tocarlo.

## Tecnologías

.NET 8, Worker Service, CsvHelper, Microsoft.Data.SqlClient, IHttpClientFactory, ILogger y Stopwatch.

Stephen Reynoso, 20252009.
