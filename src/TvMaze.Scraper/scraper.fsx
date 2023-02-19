open System.Net.Http
open System
open System.Collections.Concurrent;
open System.Threading.Tasks
open System.Threading
open System.IO

System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

//#r "nuget: FsHttp"
#r "nuget: FSharp.Data, 5.0.2"
#r "nuget: Polly, 7.2.3"
#r "nuget: LiteDB, 5.0.15"
#load "./Retries.fs"
#load "./Domain.fs"
#load "./DataAccess.fs"

open Domain
open DataAccess
// open FSharp.Data

//let workingDirectory = Environment.CurrentDirectory
//let projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
//let dbPath = Path.Combine(projectDirectory, "data", "tvmaze.db")

open LiteDB

type TvMaze() =
    static let httpClient = new HttpClient (BaseAddress = new Uri("http://api.tvmaze.com"))
    static let projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
    static let dbPath = Path.Combine(projectDirectory, "data", "tvmaze.db")

    static member GetTvShow(ct, id: int) =
        async {
            let! response = 
                Retries.createPolicy<HttpRequestException> 5
                |> Retries.executeCustom ct (
                    fun ct -> task {
                        printfn $"Making HTTP request for show with id {id}..."
                        let! response = httpClient.GetAsync($"/shows/{id}?embed=cast", ct)
                        return response
                    })
                |> Async.AwaitTask
            
            try
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return json |> toTvShowModel |> Ok
            with _ -> return Error("Error parsing response");
        }

    static member Save(tvShow) =
        try
            use db = new LiteDatabase(dbPath)
            let col = db.GetCollection<TvShowDto>("tv_shows")
            col.Insert(tvShow |> toDto) |> ignore
        with _ -> () // to keep it simple ignore all db write errors

    static member Scrape(ct, chunk) =
        async {
            for id in chunk do
                match! TvMaze.GetTvShow(ct, id) with
                | Ok show -> TvMaze.Save(show)
                | Error _ -> () // here should be logging
        }

let runScraper (ct : CancellationToken) =
    let chunkSize = 3 // maxDegreeOfParallelism
    let idRange = [1..500] // Tv show ids to scrape

    let work = 
        idRange |> Seq.splitInto chunkSize // Split ids in buckets
        |> Seq.map (fun chunk -> async { do! TvMaze.Scrape(ct, chunk) })
        |> Async.Parallel     // this will actually run buckets in parallel
        //|> Async.Sequential     // this will run without parallelization
        |> Async.Ignore

    Async.Start(work, ct)

#time
let cts = new System.Threading.CancellationTokenSource()
runScraper cts.Token
//System.Threading.Thread.Sleep(2000)
//cts.Cancel()
#time