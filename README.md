# OpenIdDictSample

This repository contains several projects that demonstrate how to use the OpenIdDict library with Client Credential Flow.

## Projects

### OpenIdServer

Project to host OpenIdDict server with Client Credential flow.

### OpenIdApi

API project that consumes the OpenIdServer using the OpenIddict client library.

### IdentityServerApi

API project that consumes the OpenIdServer using the IdentityServer4.AccessTokenValidation library.

## Running the projects

Start the docker-compose file in the root of the repository. This will start the OpenIdServer and the OpenIdApi.

```bash
docker-compose up -f docker-compose.yml -f docker-compose.override.yml
```

Start the ConsoleClient project to test the OpenIdDict client library to consume the OpenIdServer and Api's.

## References

https://github.com/openiddict/openiddict-core/
https://github.com/openiddict/openiddict-samples/
https://documentation.openiddict.com/guides/getting-started.html
