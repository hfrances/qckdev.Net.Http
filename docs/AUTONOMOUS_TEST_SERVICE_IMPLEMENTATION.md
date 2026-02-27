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

- API local de pruebas (`qckdev.Net.Http.Test.Service`) con los endpoints usados por los tests.
- `appsettings` de test apuntando a `localhost`.
- Arranque/parada del servicio en `AssemblyInitialize/AssemblyCleanup`, evitando meter infraestructura en `GetSettings`.
- Puerto por framework (TFM) para evitar colisiones cuando se ejecutan frameworks en paralelo.
- Arranque del servicio con:
  - `dotnet run --no-build --no-launch-profile`
  - `--no-build`: evita bloqueos por compilacion concurrente del mismo ejecutable.
  - `--no-launch-profile`: evita que `launchSettings.json` pise el puerto configurado por TFM.

## Beneficio final

Los tests pasan a ser:

- Deterministas
- Estables
- Portables (PC y agentes CI/CD)
- Compatibles con ejecucion paralela multi-framework sin conflictos de puertos

