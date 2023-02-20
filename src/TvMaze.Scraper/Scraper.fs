module TvMaze.Scraper.Worker

open System
open System.Net.Http
open LiteDB
open TvMaze.Persistence
open TvMaze.Scraper.Retries
open TvMaze.Scraper.Data

type ExecutionMode =
    | Sequential
    | Parallel of MaxDegreeOfParallelism : int

type Config = {
    // Path to the folder with database
    DbPath: string
    // Min TvShow id for scraping
    TvShowsMinId: int
    // Max TvShow id for scraping
    TvShowsMaxId: int
    // Execution mode
    Exec: ExecutionMode
}

type TvMaze(config: Config) =
    let config = config
    static let httpClient = new HttpClient (BaseAddress = new Uri("http://api.tvmaze.com"))

    member this.Config with get() = config

    member this.GetTvShow(ct, id: int) =
        async {
            let! response = 
                createPolicy<HttpRequestException> 5
                |> executeCustom ct (
                    fun ct -> task {
                        printfn $"[HTTP]::Scraping show with id {id}..."
                        let! response = httpClient.GetAsync($"/shows/{id}?embed=cast", ct)
                        return response
                    })
                |> Async.AwaitTask
            
            try
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return json |> toTvShowModel |> Ok
            with _ -> return Error("Error parsing response");
        }

    member this.Save(tvShow) =
        try
            use db = new LiteDatabase(connectionString = config.DbPath)
            let col = db.GetCollection<TvShowDbModel>(DbConfig.Collection)
            col.Insert(tvShow |> toDbModel) |> ignore
        with _ -> () // to keep it simple ignore all db write errors

    member this.Scrape(ct, bucket) =
        async {
            for id in bucket do
                match! this.GetTvShow(ct, id) with
                | Ok show -> this.Save(show)
                | Error _ -> () // here should be logging
        }

let worker (tvMaze: TvMaze) bucketSize =

    [tvMaze.Config.TvShowsMinId..tvMaze.Config.TvShowsMaxId] |> Seq.splitInto bucketSize // Split ids into buckets
    |> Seq.map (fun bucket -> async {
        let! ct = Async.CancellationToken
        do! tvMaze.Scrape(ct, bucket)
    })

let execute config ct =
    let tvMaze = TvMaze(config)

    match config.Exec with
    | Parallel maxDegreeOfParallelism -> 
            Async.RunSynchronously(
                worker tvMaze maxDegreeOfParallelism
                |> Async.Parallel 
                |> Async.Ignore, 1000, ct)
    | Sequential ->
            Async.RunSynchronously(
                worker tvMaze 1 
                |> Async.Sequential 
                |> Async.Ignore, 1000, ct)
