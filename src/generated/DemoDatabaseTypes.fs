// This code was generated by `SqlHydra.Npgsql` -- v0.610.1.0.
namespace DemoStuff

type Column(reader: System.Data.IDataReader, getOrdinal: string -> int, column) =
        member __.Name = column
        member __.IsNull() = getOrdinal column |> reader.IsDBNull
        override __.ToString() = __.Name

type RequiredColumn<'T, 'Reader when 'Reader :> System.Data.IDataReader>(reader: 'Reader, getOrdinal, getter: int -> 'T, column) =
        inherit Column(reader, getOrdinal, column)
        member __.Read(?alias) = alias |> Option.defaultValue __.Name |> getOrdinal |> getter

type OptionalColumn<'T, 'Reader when 'Reader :> System.Data.IDataReader>(reader: 'Reader, getOrdinal, getter: int -> 'T, column) =
        inherit Column(reader, getOrdinal, column)
        member __.Read(?alias) = 
            match alias |> Option.defaultValue __.Name |> getOrdinal with
            | o when reader.IsDBNull o -> None
            | o -> Some (getter o)

type RequiredBinaryColumn<'T, 'Reader when 'Reader :> System.Data.IDataReader>(reader: 'Reader, getOrdinal, getValue: int -> obj, column) =
        inherit Column(reader, getOrdinal, column)
        member __.Read(?alias) = alias |> Option.defaultValue __.Name |> getOrdinal |> getValue :?> byte[]

type OptionalBinaryColumn<'T, 'Reader when 'Reader :> System.Data.IDataReader>(reader: 'Reader, getOrdinal, getValue: int -> obj, column) =
        inherit Column(reader, getOrdinal, column)
        member __.Read(?alias) = 
            match alias |> Option.defaultValue __.Name |> getOrdinal with
            | o when reader.IsDBNull o -> None
            | o -> Some (getValue o :?> byte[])
        
module demo_stuff =
    [<CLIMutable>]
    type thing =
        { id: string
          name: string
          owner: string
          age: int
          created_at: System.DateTime }

    type thingReader(reader: Npgsql.NpgsqlDataReader, getOrdinal) =
        member __.id = RequiredColumn(reader, getOrdinal, reader.GetString, "id")
        member __.name = RequiredColumn(reader, getOrdinal, reader.GetString, "name")
        member __.owner = RequiredColumn(reader, getOrdinal, reader.GetString, "owner")
        member __.age = RequiredColumn(reader, getOrdinal, reader.GetInt32, "age")
        member __.created_at = RequiredColumn(reader, getOrdinal, reader.GetDateTime, "created_at")
        member __.Read() =
            { id = __.id.Read()
              name = __.name.Read()
              owner = __.owner.Read()
              age = __.age.Read()
              created_at = __.created_at.Read() }

        member __.ReadIfNotNull() =
            if __.id.IsNull() then None else Some(__.Read())

type HydraReader(reader: Npgsql.NpgsqlDataReader) =
    let mutable accFieldCount = 0
    let buildGetOrdinal fieldCount =
        let dictionary = 
            [0..reader.FieldCount-1] 
            |> List.map (fun i -> reader.GetName(i), i)
            |> List.sortBy snd
            |> List.skip accFieldCount
            |> List.take fieldCount
            |> dict
        accFieldCount <- accFieldCount + fieldCount
        fun col -> dictionary.Item col
        
    let lazydemo_stuffthing = lazy (demo_stuff.thingReader (reader, buildGetOrdinal 5))
    member __.``demo_stuff.thing`` = lazydemo_stuffthing.Value
    member private __.AccFieldCount with get () = accFieldCount and set (value) = accFieldCount <- value
    member private __.GetReaderByName(entity: string, isOption: bool) =
        match entity, isOption with
        | "demo_stuff.thing", false -> __.``demo_stuff.thing``.Read >> box
        | "demo_stuff.thing", true -> __.``demo_stuff.thing``.ReadIfNotNull >> box
        | _ -> failwith $"Could not read type '{entity}' because no generated reader exists."

    static member private GetPrimitiveReader(t: System.Type, reader: Npgsql.NpgsqlDataReader, isOpt: bool) =
        let wrap get (ord: int) = 
            if isOpt 
            then (if reader.IsDBNull ord then None else get ord |> Some) |> box 
            else get ord |> box 
        
        if t = typedefof<bool> then Some(wrap reader.GetBoolean)
        else if t = typedefof<int16> then Some(wrap reader.GetInt16)
        else if t = typedefof<int> then Some(wrap reader.GetInt32)
        else if t = typedefof<int64> then Some(wrap reader.GetInt64)
        else if t = typedefof<double> then Some(wrap reader.GetDouble)
        else if t = typedefof<decimal> then Some(wrap reader.GetDecimal)
        else if t = typedefof<string> then Some(wrap reader.GetString)
        else if t = typedefof<System.Guid> then Some(wrap reader.GetGuid)
        else if t = typedefof<System.DateTime> then Some(wrap reader.GetDateTime)
        else if t = typedefof<System.TimeSpan> then Some(wrap reader.GetTimeSpan)
        else if t = typedefof<byte []> then Some(wrap reader.GetValue)
        else if t = typedefof<char> then Some(wrap reader.GetChar)
        else None

    static member Read(reader: Npgsql.NpgsqlDataReader) = 
            let hydra = HydraReader(reader)
            
            let getOrdinalAndIncrement() = 
                let ordinal = hydra.AccFieldCount
                hydra.AccFieldCount <- hydra.AccFieldCount + 1
                ordinal
            
            let buildEntityReadFn (t: System.Type) = 
                let t, isOpt = 
                    if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Option<_>> 
                    then t.GenericTypeArguments.[0], true
                    else t, false
            
                match HydraReader.GetPrimitiveReader(t, reader, isOpt) with
                | Some primitiveReader -> 
                    let ord = getOrdinalAndIncrement()
                    fun () -> primitiveReader ord
                | None ->
                    let nameParts = t.FullName.Split([| '.'; '+' |])
                    let schemaAndType = nameParts |> Array.skip (nameParts.Length - 2) |> fun parts -> System.String.Join(".", parts)
                    hydra.GetReaderByName(schemaAndType, isOpt)
            
            // Return a fn that will hydrate 'T (which may be a tuple)
            // This fn will be called once per each record returned by the data reader.
            let t = typeof<'T>
            if FSharp.Reflection.FSharpType.IsTuple(t) then
                let readEntityFns = FSharp.Reflection.FSharpType.GetTupleElements(t) |> Array.map buildEntityReadFn
                fun () ->
                    let entities = readEntityFns |> Array.map (fun read -> read())
                    Microsoft.FSharp.Reflection.FSharpValue.MakeTuple(entities, t) :?> 'T
            else
                let readEntityFn = t |> buildEntityReadFn
                fun () -> 
                    readEntityFn() :?> 'T
        
