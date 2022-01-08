open System.Data.Common
open DemoStuff
open Npgsql
open SqlHydra.Query
open SqlHydraDemo

let createDbConnectionString
    ({ dbUsername = username
       dbPassword = password
       dbName = dbName
       dbHost = host
       dbPort = dbPort })
    =
    $"Server=%s{host};Port=%d{dbPort};Database={dbName};User Id=%s{username};Password=%s{password};"

let createContext (config: Config) =
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
            let! context = createContext config

            let! _ =
                Database.Things.insertThing
                    context
                    ({ id = System.Guid.NewGuid().ToString()
                       name = "Thing"
                       owner = "asd"
                       age = 213 }: demo_stuff.thing)

                |> Async.AwaitTask

            let! things =
                Database.Things.getThings context
                |> Async.AwaitTask

            printfn "%A" things
        }
        |> Async.RunSynchronously

        0
