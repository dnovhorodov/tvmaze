module TvMaze.Scraper.Data

open FSharp.Data
open System
open TvMaze.Persistence
open Domain

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

let toDbModel (tvShow : TvShow) =
    new TvShowDbModel (
        Id = tvShow.Id,
        Name = tvShow.Name,
        Timestamp = DateTime.UtcNow,
        Casts = ResizeArray(tvShow.Casts |> Array.map (fun c ->
            let brth = c.Birthday |> function | Some brth -> Nullable<DateTime> brth | None -> Nullable()
            new TvMaze.Persistence.Cast (
                Id = c.Id,
                Name = c.Name,
                Birthday = brth
            )))
        )