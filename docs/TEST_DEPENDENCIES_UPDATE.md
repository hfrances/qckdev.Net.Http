# Actualizacion de Dependencias de Tests Unitarios

## Proyecto
`qckdev.Net.Http` (topologia de tests multi-proyecto)

## Proyectos de tests involucrados

- `qckdev.Net.Http.Test` (runner principal)
- `qckdev.Net.Http.Test.Common` (codigo compartido)
- `qckdev.Net.Http.Test.Net35` (runner legacy)
- `qckdev.Net.Http.Test.Net40` (runner legacy)

## Estado actual (fuente de verdad)

### `qckdev.Net.Http.Test`

- Target frameworks: `net10.0;net9.0;net8.0;net6.0;net5.0;netcoreapp3.1;$(net461);$(net451);$(net45)`
- Stack testing moderno (netcoreapp3.1+):
  - `Microsoft.NET.Test.Sdk` `17.11.1`
  - `MSTest.*` `3.2.2`
  - `coverlet.*` `6.0.0`
- Stack testing legacy (net461/net451/net45):
  - `Microsoft.NET.Test.Sdk` `17.11.0`
  - `MSTest.*` `2.2.10`
  - `coverlet.msbuild` `3.1.2`
  - `coverlet.collector` `1.2.0`

### `qckdev.Net.Http.Test.Common`

- Debe cubrir todos los TFM del proyecto `Test`.
- Actualmente incluye ademas `$(net40);$(net35)` para soporte legacy.
- Mantiene bloques de `Microsoft.Extensions.Configuration*` por framework.

## Reglas para mantenimiento

1. No cambiar `TargetFrameworks` sin solicitud explicita.
2. Mantener doble stack de testing en `qckdev.Net.Http.Test.csproj`.
3. Si se agrega un TFM en `Test`, replicarlo en `Test.Common`.
4. Mantener `DefineConstants` legacy en `Test.Common` (`NEWTONSOFT`, `NO_ASYNC`, `NO_DYNAMIC`, `SET_SECURITY_PROTOCOL*`).
5. No eliminar `Test.Net35`/`Test.Net40` sin validar pipeline legacy.
6. El servicio de pruebas se ejecuta embebido en `Test.Common` (`LocalTestServiceManager`), sin proyecto externo.

## Verificacion

```powershell
dotnet restore qckdev.Net.Http.Test.Common\qckdev.Net.Http.Test.Common.csproj
dotnet build qckdev.Net.Http.Test.Common\qckdev.Net.Http.Test.Common.csproj

dotnet restore qckdev.Net.Http.Test\qckdev.Net.Http.Test.csproj
dotnet test qckdev.Net.Http.Test\qckdev.Net.Http.Test.csproj
dotnet test qckdev.Net.Http.Test\qckdev.Net.Http.Test.csproj --list-tests
```
