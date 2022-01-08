namespace SqlHydraDemo

open System
open Thoth.Json.Net

type Config =
    { dbUsername: string
      dbPassword: string
      dbName: string
      dbHost: string
      dbPort: int }

module Env =
    let environmentDecoder: Decoder<Config> =
        Decode.object
            (fun get ->
                { dbHost = get.Required.Field "DB_HOST" Decode.string
                  dbUsername = get.Required.Field "DB_USERNAME" Decode.string
                  dbPassword = get.Required.Field "DB_PASSWORD" Decode.string
                  dbName = get.Required.Field "DB_NAME" Decode.string
                  dbPort = get.Required.Field "DB_PORT" Decode.int })

    let decodeEnvironmentVariables (decoder: Decoder<'a>) : Result<'a, exn> =
        Environment.GetEnvironmentVariables()
        |> Seq.cast<System.Collections.DictionaryEntry>
        |> Seq.map (fun d -> (d.Key :?> string, Encode.string (d.Value :?> string)))
        |> Seq.toList
        |> Encode.object
        |> Decode.fromValue "env" decoder
        |> Result.mapError (
            sprintf "Invalid environment variables:\n%s"
            >> Failure
        )
