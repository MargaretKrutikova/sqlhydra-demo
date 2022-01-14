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

The ability to instantly keep in sync your database schema and the source code is especially crucial in the beginning of the project when the names change often, fields are added and tables are removed.

## Overview

This demo project is using a `dotnet` tool [`SqlHydra.Npgsql`](https://github.com/JordanMarr/SqlHydra/#sqlhydranpgsql-) for type generation from a `postgres` database and a `NuGet` package [`SqlHydra.Query`](https://github.com/JordanMarr/SqlHydra/#sqlhydraquery-) for building `sql` queries using `F#` computation expressions and the generated types. The two packages are working in synergy and provide a light-weight data-access layer.

Apart from `SqlHydra`, the project includes a full-blown setup for:

- Running database migrations with `rambler`,
- `Make` commands for building and running other nifty tasks,
- Running everything in `docker` containers,
- Running migrations and type generation in a `github` workflow,

to show how to integrate `SqlHydra` in a real-world scenario. The database and schema are created when the `docker` container is spun up locally, see [running locally](#running-locally).

The app itself is a console app consisting of the following files:

- [`generated/DemoDatabaseTypes`](./src/generated/DemoDatabaseTypes.fs) - generated `F#` types from the tables in the database `sqlhydra_demo` in schema `demo_stuff`,
- [`Database.fs`](./src/Database.fs) - light-weight data-access layer for CRUD operations against the db with the generated types and computation expressions from `SqlHydra.Query`,
- [`Env.fs`](./src/Env.fs) - reading env variables with database config values, can safely skip this,
- [`Program.fs`](./src/Program.fs) - example creating database connection with `SqlHydra.Query` and using the data access layer to insert and read database rows.

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

### Docker setup

`Dockerfile` builds the console app and runs `docker-entrypoint.sh` which in turn runs `rambler` migrations and runs the app. `docker-compose` will spin up a instance of `postgres` and create the necessary database and run the console app.

## Running locally

### Spin up docker containers

Make sure `docker` is running locally and run:

```sh
docker-compose up -d
```

A new `postgres` instance, running on port 7432 (you might already have `postgres` on 5432, and even 6432) is created with a database `sqlhydra_demo` and a schema `demo_stuff` with one table `thing`.

### Run outside docker

For local development you can run the demo application outside docker with `make run` or `make run-watch`.

### Example workflow

Let's make some changes to the db schema and see how to re-generate types:

- make changes to [`20211218-1400_create_things_table.sql`](./migrations/20211218-1400_create_things_table.sql) and change the name or type of any field,
- revert migrations with `make reverse-migrations` to undo all migrations,
- run `make migrations`,
- run `make run`,
- observe compile-time errors 💥

OBS: alternatively run `make bounce-migrations` to both revert all migrations and apply them again.

## Github workflow

There is a github workflow that makes sure the migrations are run without errors and the newly generated code compiles. The workflow will effectively fail if you add non-backwards compatible migrations or make non-backwards compatible changes and forget to re-generate the types.

The workflow will setup [PostgreSQL service container](https://docs.github.com/en/actions/using-containerized-services/creating-postgresql-service-containers) with the appropriate port, database and account. Steps that follow will run migrations and build the code.

Note the usage of [`.env.github`](./env.github) under `Run migrations` in the workflow to set `OS` env variable to `Linux` - this is necessary to download the right version of `rambler` on the github ci server.
