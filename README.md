# waste-batteries-reg-backend

Core delivery C# ASP.NET backend template.

* [Install MongoDB](#install-mongodb)
* [Inspect MongoDB](#inspect-mongodb)
* [Testing](#testing)
* [Running](#running)
* [Dependabot](#dependabot)


### Docker Compose

A Docker Compose template is in [compose.yml](compose.yml).

A local environment with:

- Localstack for AWS services (S3, SQS)
- Redis
- MongoDB
- This service.
- A commented out frontend example.

```bash
docker compose up --build -d
```

A more extensive setup is available in [github.com/DEFRA/cdp-local-environment](https://github.com/DEFRA/cdp-local-environment)

### MongoDB

#### MongoDB via Docker

See above.

```
docker compose up -d --wait mongodb
```

The container starts a single-node replica set named `rs0`. The healthcheck initiates it and waits until the node is primary. `docker compose up` without `--wait` can return before that.

#### MongoDB locally

Alternatively install MongoDB locally as a single-node replica set, matching the Compose service.

- Install [MongoDB](https://www.mongodb.com/docs/manual/tutorial/#installation) on your local machine
- Start MongoDB:

```bash
sudo mongod --replSet rs0 --bind_ip_all --dbpath ~/mongodb-cdp
```

- Initiate the set once. An existing standalone data directory can be initiated the same way:

```javascript
rs.initiate({_id: "rs0", members: [{_id: 0, host: "localhost:27017"}]})
```

Connect with `directConnection=true`. The development and Compose connection strings do. That keeps the driver on the server you named. A client that discovers the set would follow the member host, which inside Compose is not the host published to your machine.

#### Retries

Retryable reads and retryable writes stay at the MongoDB .NET driver default, which is on. The connection strings in this repo do not turn them off. A transient network error on a single-document read or write is retried by the driver.

#### MongoDB in CDP environments

In CDP environments a MongoDB instance is already set up
and the credentials exposed as environment variables.
That instance is a replica set. The deployed connection string is supplied by the platform. Do not add `directConnection=true` there: the driver should discover the set.


### Inspect MongoDB

To inspect the Database and Collections locally:
```bash
mongosh "mongodb://127.0.0.1:27017/?directConnection=true"
```

You can use the CDP Terminal to access the environments' MongoDB.

### Testing

```bash
dotnet test
```

`dotnet test` runs both suites. The pull request check and the SonarCloud check both use it. The pull request check fails when line coverage of `WasteBatteriesRegBackend` is below 90%.

`WasteBatteriesRegBackend.Tests.Unit` covers validators, configuration gating, and handlers without hosting the API.

`WasteBatteriesRegBackend.Tests.Integration` hosts the API in process with `WebApplicationFactory`. Repositories and other collaborators are substituted with NSubstitute. The tests do not start MongoDB.

### Running

Run CDP-Deployments application:
```bash
dotnet run --project src/WasteBatteriesRegBackend --launch-profile Development
```

The backend requires a Defra ID access token in the `Authorization: Bearer <token>`
header for every endpoint except `/health`, `/openapi/v1.json`, and `/swagger`.
The token is validated against the configured OIDC metadata and audience.

Outside Production, Swagger UI is available at `/swagger` and the OpenAPI
document is available at `/openapi/v1.json`. Set `Swagger:Enabled` to `true`
to turn the explorer on in Production.

Local development uses the Defra ID stub values in `appsettings.Development.json`.
Deployed environments must provide `Jwt__MetadataAddress` and `Jwt__Audience`
configuration for the real Defra ID/API audience.

### SonarCloud

SonarCloud analysis runs from the pull request, publish and publish-hotfix GitHub Action workflows.

To run the same scan locally:

```bash
SONAR_TOKEN=your-token ./sonarCloudLocal.sh
```

The script writes unresolved issues to `sonar-issues.json` and, when `python3`
is available, a copy/paste friendly `sonar-issues.md`.

To match the SonarCloud pull request summary view, pass the pull request key:

```bash
SONAR_TOKEN=your-token SONAR_PULL_REQUEST=1 ./sonarCloudLocal.sh
```

### Security scanning (ZAP)

Every pull request runs a **passive** [OWASP ZAP](https://www.zaproxy.org/)
scan as a `zap` job. The compose API is started from
[compose-github.override-zap.yml](./compose-github.override-zap.yml) and
`/health` is requested through the ZAP proxy so the daemon inspects the
response. There is no Playwright suite in this repo.

The pull request fails if ZAP reports any **High** alerts against this API
(port 8085). The assert script waits until ZAP's passive scan queue is empty
before it reads those alerts. Medium and Low findings stay in the report;
they do not fail the check. An empty scan (ZAP recorded no traffic on 8085)
also fails.

The HTML and JSON reports are uploaded as the `zap-test-report` artefact and
linked from a comment on the pull request.

ZAP is CI/test-only — it does not change app configuration.

To run the same scan locally:

```bash
docker compose -f compose.yml -f compose-github.override-zap.yml \
  up -d --wait floci mongodb your-backend
docker compose -f compose.yml -f compose-github.override-zap.yml up -d zap
# wait until http://127.0.0.1:8080 and http://localhost:8085/health answer
curl -fsS -x http://127.0.0.1:8080 http://localhost:8085/health
bash scripts/zap-assert.sh
docker compose -f compose.yml -f compose-github.override-zap.yml down
```

On PowerShell, `curl` is an alias for `Invoke-WebRequest` and does not accept
`-x`. After the same `docker compose` commands above, use `curl.exe` for the
proxied health check and `scripts/zap-assert.ps1` in place of the bash script.
Always `down` the overlay stack afterwards, including when the scan fails.

```powershell
curl.exe -fsS -x http://127.0.0.1:8080 http://localhost:8085/health
.\scripts\zap-assert.ps1
docker compose -f compose.yml -f compose-github.override-zap.yml down
```

### Dependabot

We have added an example dependabot configuration file to the repository. You can enable it by renaming
the [.github/example.dependabot.yml](.github/example.dependabot.yml) to `.github/dependabot.yml`


### About the licence

The Open Government Licence (OGL) was developed by the Controller of Her Majesty's Stationery Office (HMSO) to enable
information providers in the public sector to license the use and re-use of their information under a common open
licence.

It is designed to encourage use and re-use of information freely and flexibly, with only a few conditions.
