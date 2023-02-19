module DataAccess

open FSharp.Data
open Domain
open System

type TVMazeJson = JsonProvider<"""{
  "id": 1,
  "name": "Show name",
  "_embedded": {
    "cast": [
      {
        "person": {
          "id": 1,
          "name": "Cast name",
          "birthday": "1979-07-17"
        }
      },
      {
        "person": {
          "id": 2,
          "name": "Another cast",
          "birthday": null
        }
      }
    ]
  }
}""">

type CastDto = {
    Id: int
    Name: string
    Birthday: Nullable<DateTime>
}

type TvShowDto = {
    Id: int
    Name: string
    Casts: CastDto list
    Timestamp: DateTime
}

let toTvShowModel json =
    json
    |> TVMazeJson.Parse 
    |> fun show -> 
        {
            Id = show.Id;
            Name = show.Name;
            Casts = show.Embedded.Cast
            |> Array.map (fun p ->
                {
                    Id = p.Person.Id;
                    Name = p.Person.Name;
                    Birthday = p.Person.Birthday;
                })
        } : TvShow

let toDto (tvShow : TvShow) =
    {
        Id = tvShow.Id
        Name = tvShow.Name
        Casts = tvShow.Casts |> Array.map (fun c ->
        {
            Id = c.Id
            Name = c.Name
            Birthday = c.Birthday |> function | Some brth -> Nullable<DateTime> brth | None -> Nullable()
        }) |> List.ofArray
        Timestamp = DateTime.UtcNow
    }