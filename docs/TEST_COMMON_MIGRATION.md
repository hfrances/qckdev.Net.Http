# Migracion de `*.Test.Common` (qckdev.Net.Http)

## Objetivo

Evitar errores de compilacion por desalineacion de frameworks/paquetes entre:

- `qckdev.Net.Http.Test.csproj`
- `qckdev.Net.Http.Test.Common.csproj`

## Regla principal

`qckdev.Net.Http.Test.Common` debe cubrir todos los TFM de `qckdev.Net.Http.Test`.

Actualmente:

- `qckdev.Net.Http.Test`: `net10.0;net9.0;net8.0;net6.0;net5.0;netcoreapp3.1;net461;net451;$(net45)`
- `qckdev.Net.Http.Test.Common`: incluye los anteriores y ademas `$(net40);$(net35)`

## Patron de dependencias por framework

### 1. Testing SDK

Incluir `Microsoft.NET.Test.Sdk` para frameworks modernos y `net461` cuando el proyecto lo requiera.

### 2. Configuracion (`Microsoft.Extensions.Configuration*`)

Mantener mapeo por version de framework:

- `net10.0` -> `10.0.0`
- `net9.0` -> `9.0.0`
- `net8.0` -> `8.0.0`
- `net7.0` -> `7.0.0`
- `net6.0/net5.0/netcoreapp3.1` -> `6.0.0`
- `net461` -> `3.1.0` (+ `System.Text.Json 4.7.0` y `System.Configuration.ConfigurationManager 4.7.0`)

### 3. Referencias legacy

- `Microsoft.CSharp` para `net40/net451/net461`
- `System.Net.Http` para `net451/net461`

### 4. Constantes de compilacion

Mantener `DefineConstants` legacy (`NEWTONSOFT`, `NO_ASYNC`, `SET_SECURITY_PROTOCOL*`) para no romper comportamiento condicional del codigo.

## Checklist de migracion

1. Cambias TFM en `qckdev.Net.Http.Test` -> replica el cambio en `qckdev.Net.Http.Test.Common`.
2. Añades TFM moderno (`net9+`, `net10+`) -> añade bloque de `Microsoft.Extensions.Configuration*` correspondiente.
3. Compila primero `Test.Common`, luego `Test`.
4. Ejecuta discovery de tests con `--list-tests`.

## Comandos de verificacion

```powershell
dotnet restore qckdev.Net.Http.Test.Common\qckdev.Net.Http.Test.Common.csproj
dotnet build qckdev.Net.Http.Test.Common\qckdev.Net.Http.Test.Common.csproj

dotnet restore qckdev.Net.Http.Test\qckdev.Net.Http.Test.csproj
dotnet test qckdev.Net.Http.Test\qckdev.Net.Http.Test.csproj --list-tests
```

