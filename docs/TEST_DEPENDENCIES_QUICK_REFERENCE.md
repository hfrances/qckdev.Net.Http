# Referencia Rapida: Tests Multi-Framework

## Proyecto
`qckdev.Net.Http` (tests)

## Checklist

1. No cambiar `TargetFrameworks` sin solicitud explicita.
2. Mantener doble stack de testing (moderno + legacy) en `qckdev.Net.Http.Test`.
3. Mantener `qckdev.Net.Http.Test.Common` alineado con `qckdev.Net.Http.Test`.
4. Mantener `Test.Net35`, `Test.Net40` y `Test.Service`.
5. Ejecutar `dotnet test` y `--list-tests` en el proyecto principal.

## Frameworks activos (`qckdev.Net.Http.Test`)

`net10.0;net9.0;net8.0;net6.0;net5.0;netcoreapp3.1;net461;net451;net45`

## Paquetes de testing

### Grupo moderno (netcoreapp3.1+)

- `Microsoft.NET.Test.Sdk` `17.11.1`
- `MSTest.TestAdapter` `3.2.2`
- `MSTest.TestFramework` `3.2.2`
- `coverlet.msbuild` `6.0.0`
- `coverlet.collector` `6.0.0`

### Grupo legacy (net461/net451/net45)

- `Microsoft.NET.Test.Sdk` `17.11.0`
- `MSTest.TestAdapter` `2.2.10`
- `MSTest.TestFramework` `2.2.10`
- `coverlet.msbuild` `3.1.2`
- `coverlet.collector` `1.2.0`
