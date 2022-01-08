# SqlHydra demo

This is a demo-project showing how to work with databases in a type-safe way using `F#` with automatic database type generation and type-safe queries using several `NuGet` packages from [`SqlHydra`](https://github.com/JordanMarr/SqlHydra/).

## Why

What problems are we solving here? Well working with databases can be quite a pain:

- Writing `sql` queries in strings is tedious and you won't get errors when the database schema changes,
- Your DTOs aren't in sync when your database schema changes,
- Using full blown ORM might be overkill and might bring more pain than pleasure with obscure errors or unintended consequences,
- Writing custom data-access framework might be fun but tricky and might take a lot of time (and you might not get paid for that).

`SqlHydra` will automatically generate a light-weight and type-safe data-access layer for an app:

- It translates the database schema and metadata into `F#` types, meaning your DTOs will be updated as soon as you update your db and re-rerun the type generator,
- Your `sql` queries won't be out of sync since you will get compile-time errors as soon as you re-run the type generator after the schema is updated,
- It is light-weight and doesn't support any relationship mapping, so it is much more transparent (not a real ORM).

## Overview

This demo project is using a `dotnet` tool [`SqlHydra.Npgsql`](https://github.com/JordanMarr/SqlHydra/#sqlhydranpgsql-) for type generation from a `postgres` database and a `NuGet` package [`SqlHydra.Query`](https://github.com/JordanMarr/SqlHydra/#sqlhydraquery-) for building `sql` queries using `F#` computation expressions and the generated types. The two packages are working in synergy and provide a light-weight data-access layer.

Apart from `SqlHydra`, the project includes a full-blown setup for running database migrations with `rambler`, `Make` commands for building and running other nifty tasks and running everything in `docker` containers to show how to integrate `SqlHydra` in a real-world scenario.

### Type generation

In a fresh project, you need to install `SqlHydra.Npgsql` as a `dotnet` tool:

```
dotnet new tool-manifest
dotnet tool install SqlHydra.Npgsql
```

This will create a new file `dotnet-tools.json` under `.config`. To create a config file for the type generator run `dotnet sqlhydra-npgsql` and it will guide you through the process, see [docs](https://github.com/JordanMarr/SqlHydra/#sqlhydranpgsql-) for more info. The config is located under `sqlhydra-npgsql.toml` and points to the database schema to generate types from.

The type generator is run immediately after running database migrations to make sure all changes to the database schema are reflected in code:

```sh
make migrations
```

This command will first apply `rambler` migrations and then run `sqlhydra-npgsql` generator on the freshly updated database.

## Running locally

### Spin up docker containers

Make sure `docker` is running locally and run:

```sh
docker-compose up -d
```

### Run outside docker

For local development you can run the demo application outside docker

### Makefile

Following `make` commands are available:

- `make migrations` - will pull down `rambler`, run migrations and re-generate database types.
