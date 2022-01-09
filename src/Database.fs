module SqlHydraDemo.Database

open DemoStuff
open SqlHydra.Query

module Tables =
    let thingTable =
        table<demo_stuff.thing>
        |> inSchema (nameof demo_stuff)

module Things =
    let getThings (context: QueryContext) =
        select {
            for thing in Tables.thingTable do
                select thing
        }
        |> context.ReadAsync HydraReader.Read

    let insertThing (context: QueryContext) (thing: demo_stuff.thing) =
        insert {
            for e in Tables.thingTable do
                entity thing
                excludeColumn e.created_at
        }
        |> context.InsertAsync

    let updateThing (context: QueryContext) (thing: demo_stuff.thing) =
        update {
            for e in Tables.thingTable do
                entity thing
                where (e.id = thing.id)
                excludeColumn e.created_at
        }
        |> context.UpdateAsync

    let deleteThingById (context: QueryContext) (id: string) =
        delete {
            for e in Tables.thingTable do
                where (e.id = id)
        }
        |> context.DeleteAsync
