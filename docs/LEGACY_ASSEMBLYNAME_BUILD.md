# Build de Proyectos con `AssemblyName` Diferente en `net35`

## Contexto

En este repositorio, los proyectos principales usan nombre de ensamblado diferente para `net35`:

- `qckdev.Net.Http/qckdev.Net.Http.csproj`
  - `net35` -> `qckdev.Net.Http.2.dll`
  - resto -> `qckdev.Net.Http.dll`
- `qckdev.Net.Http.Exceptions/qckdev.Net.Http.Exceptions.csproj`
  - `net35` -> `qckdev.Net.Http.Exceptions.2.dll`
  - resto -> `qckdev.Net.Http.Exceptions.dll`

## Problema conocido

Este patron multi-target + frameworks legacy puede fallar en Visual Studio si no se inicializa primero con CLI.

Referencia:
- https://github.com/dotnet/sdk/issues/22469#issuecomment-1732733899

## Flujo recomendado de compilacion

```powershell
cd qckdev.Net.Http
dotnet restore
dotnet build
```

Para release/pack:

```powershell
cd qckdev.Net.Http
dotnet build --configuration Release
dotnet pack --configuration Release
```

## Verificacion rapida

Verificar que ambos patrones de salida existan:

- `bin/<Configuration>/net35/*.2.dll`
- `bin/<Configuration>/<otro-framework>/*.dll`

## Nota sobre tests

`qckdev.Net.Http.Test` y `qckdev.Net.Http.Test.Common` dependen de estos proyectos. Si no se ejecuta restore/build por CLI al inicio, el build/discovery de tests puede fallar en IDE.

