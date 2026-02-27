# Implementacion de API local para tests autonomos

## Por que fue necesario

Los tests de `qckdev.Net.Http` dependian de servicios de terceros para validar escenarios HTTP. Eso provocaba:

- No determinismo por cambios externos en respuestas/datos.
- Fallos por red, latencia, rate limits o indisponibilidad de APIs externas.
- Dependencia de internet, incompatible con un `dotnet test` realmente autosuficiente.
- Colisiones de puerto al ejecutar varios `TargetFrameworks` en paralelo.

## Objetivo

Conseguir que los tests se ejecuten de forma autonoma y reproducible en local y en DevOps, sin depender de terceros.

## Que se implemento y por que

- Servicio HTTP embebido en `qckdev.Net.Http.Test.Common` (`LocalTestServiceManager`) con los mismos endpoints de test.
- `appsettings` de test apuntando a `localhost`.
- Arranque/parada del servicio en `AssemblyInitialize/AssemblyCleanup`.
- Puerto determinista por proceso (no por TFM), para que cada host de tests pueda levantar su servicio sin colisiones.
- Eliminacion de complejidad de bootstrap externo:
  - ya no se usa `dotnet run`
  - ya no se hace `dotnet build` desde los tests
  - ya no se busca el `.csproj` del servicio en disco

## Beneficio final

Los tests pasan a ser:

- Deterministas
- Estables
- Portables (PC y agentes CI/CD)
- Compatibles con ejecucion paralela multi-framework sin conflictos de puertos
- Menos acoplamiento al layout del repositorio y menor coste de mantenimiento
