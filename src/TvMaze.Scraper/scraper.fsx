System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

#r "nuget: FSharp.Data, 5.0.2"
#r "nuget: Polly, 7.2.3"
#r "nuget: LiteDB, 5.0.15"
#r @"D:\My\RTL\src\TvMaze.Scraper\bin\Debug\net7.0\TvMaze.Persistence.dll"
#load "./Domain.fs"
#load "./Retries.fs"
#load "./Data.fs"
#load "./Scraper.fs"

open System.Net.Http
open System
open TvMaze.Persistence
open TvMaze.Scraper
open TvMaze.Scraper.Data
open TvMaze.Scraper.Worker
// open FSharp.Data

open LiteDB

//type TvMaze() =
//    static let httpClient = new HttpClient (BaseAddress = new Uri("http://api.tvmaze.com"))
//    static member GetTvShow(ct, id: int) =
//        async {
//            let! response = 
//                Retries.createPolicy<HttpRequestException> 5
//                |> Retries.executeCustom ct (
//                    fun ct -> task {
//                        printfn $"Making HTTP request for show with id {id}..."
//                        let! response = httpClient.GetAsync($"/shows/{id}?embed=cast", ct)
//                        return response
//                    })
//                |> Async.AwaitTask
            
//            try
//                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
//                return json |> toTvShowModel |> Ok
//            with _ -> return Error("Error parsing response");
//        }

//    static member Save(tvShow: Domain.TvShow) =
//        try
//            printfn $"Saving tv show {tvShow.Name}..."
//            use db = new LiteDatabase(DbConfig.DbPath)
//            let col = db.GetCollection<TvShowDbModel>(DbConfig.Collection)
//            col.Insert(tvShow |> toDbModel) |> ignore
//            printfn $"Tv Show {tvShow.Name} succesfully saved"
//        with _ -> () // to keep it simple ignore all db write errors

//    static member Scrape(ct, chunk) =
//        async {
//            for id in chunk do
//                match! TvMaze.GetTvShow(ct, id) with
//                | Ok show -> TvMaze.Save(show)
//                | Error _ -> () // here should be logging
//        }

//let scraperTask =
//    let chunkSize = 3 // maxDegreeOfParallelism
//    let idRange = [1..200] // Tv show ids to scrape
    
//    idRange |> Seq.splitInto chunkSize // Split ids in buckets
//    |> Seq.map (fun chunk -> async {
//        let! ct = Async.CancellationToken
//        do! TvMaze.Scrape(ct, chunk)
//    })

#time
let cts = new System.Threading.CancellationTokenSource()
let ct = cts.Token

let config = {
    DbPath = @"D:\My\RTL\data"
    TvShowsMinId = 1
    TvShowsMaxId = 20
    // Exec = Parallel (MaxDegreeOfParallelism = 3)
    Exec = Sequential
}

execute config ct
#time
//Async.Start(scraperTask |> Async.Parallel |> Async.Ignore, ct)

//scraperTask
//|> Async.Parallel
//|> Async.Ignore
//|> Async.RunSynchronously

//|> Async.Start(runScraper, ct)
//Async.RunSynchronously(runScraper, 1000, cts.Token)
//cts.Cancel()
