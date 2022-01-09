open System.Data.Common
open DemoStuff
open Npgsql
open SqlHydra.Query
open SqlHydraDemo

module QueryContext =
    let private createDbConnectionString (config: Config) =
        $"Server=%s{config.dbHost};Port=%d{config.dbPort};Database={config.dbName};User Id=%s{config.dbUsername};Password=%s{config.dbPassword};"

    let create (config: Config) =
        let connectionString = createDbConnectionString config
        let compiler = SqlKata.Compilers.PostgresCompiler()

        let dbConnection =
            new NpgsqlConnection(connectionString) :> DbConnection

        async {
            do! dbConnection.OpenAsync() |> Async.AwaitTask
            return new QueryContext(dbConnection, compiler)
        }

module Program =
    let config =
        match Env.decodeEnvironmentVariables Env.environmentDecoder with
        | Ok env -> env
        | Error err -> raise err

    [<EntryPoint>]
    let main _ =
        async {
            let! context = QueryContext.create config

            let id = System.Guid.NewGuid().ToString()

            let newThing: demo_stuff.thing =
                { id = id
                  name = "Thing"
                  owner = "asd"
                  age = 213
                  created_at = System.DateTime.UtcNow }

            do!
                Database.Things.insertThing context newThing
                |> Async.AwaitTask
                |> Async.Ignore

            do!
                Database.Things.updateThing context { newThing with name = "Göttans" }
                |> Async.AwaitTask
                |> Async.Ignore

            let! things =
                Database.Things.getThings context
                |> Async.AwaitTask

            printfn "%A" things
        }
        |> Async.RunSynchronously

        0
