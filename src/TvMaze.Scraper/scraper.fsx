open System.Net.Http
open System
open System.Threading.Tasks
open System.Threading
open System.IO

//module polly

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

let workingDirectory = Environment.CurrentDirectory
let projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
let dbPath = Path.Combine(projectDirectory, "data", "tvmaze.db")

open LiteDB

//let testSave tvShow =
//    use db = new LiteDatabase(dbPath)
//    let col = db.GetCollection<TvShowDto>("tv_shows")
//    col.Insert(tvShow |> toDto) |> ignore
//    col.EnsureIndex(fun s -> s.Name) |> ignore
//    ()

//let tvshow = 
//    {
//        TvShow.Id = 3
//        Name = "test"
//        Casts = [|{
//            Id = 2
//            Name = "TestCast"
//            Birthday = DateTime.Today |> Some
//        }|]
//    }

//testSave tvshow

type TvMaze() =
    static let httpClient = new HttpClient (BaseAddress = new Uri("http://api.tvmaze.com"))
    static member GetTvShow(ct, id: int) =
        async {
            let! response = 
                Retries.createPolicy<HttpRequestException> 5
                |> Retries.executeCustom ct (
                    fun ct -> task {
                        printfn $"Making HTTP request for show with id {id}..."
                        let! response = httpClient.GetAsync($"/shows/{id}?embed=cast", ct)
                        //printfn $"Request for show with id {id} successful"
                        return response
                    })
                |> Async.AwaitTask
            
            try
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                // Add error handlinng here (Result type)
                // when 404 we get {"name":"Not Found","message":"","code":0,"status":404} in response
                //
                return json |> toTvShowModel |> Ok
            with _ -> return Error("Error parsing response");
        }

//let getTvShow ct id =
//  Retries.createPolicy<HttpRequestException> 3
//  |> Retries.executeCustom ct (fun ct -> task {
       
//       printfn $"Making HTTP request for show with id {id}..."
//       let! response = TvMaze.Http.GetAsync($"/shows/{id}?embed=cast", ct)
//       //let! content = response.Content.ReadAsStringAsync()
//       printfn $"Request for show with id {id} successful"
       
//       //return content
//       return response
//     })

//let getTvShow ct id = async {
//    let! response = 
//        Retries.createPolicy<HttpRequestException> 3
//        |> Retries.executeCustom ct (
//            fun ct -> task {
//                printfn $"Making HTTP request for show with id {id}..."
//                let! response = TvMaze.Http.GetAsync($"/shows/{id}?embed=cast", ct)
//                printfn $"Request for show with id {id} successful"
//                return response
//            })
//        |> Async.AwaitTask
    
//    let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
//    return json |> toTvShowModel
//}

let scrape ct chunk =
    async {
        let tvShows = 
            chunk |> Array.map (fun id -> TvMaze.GetTvShow(ct, id))

        let results = ResizeArray<_>()
        for tvShow in tvShows do
            //let! result = tvShow
            match! tvShow with
            | Ok show -> results.Add show
            | Error _ -> ()

        return results |> Seq.toList
    }

let save tvShows =
    async {
        for tvShow in tvShows do
            try
                use db = new LiteDatabase(dbPath)
                let col = db.GetCollection<TvShowDto>("tv_shows")
                col.Insert(tvShow |> toDto) |> ignore
            with _ -> () // to keep it simple ignore all db write errors
    }

let runScraper (ct : CancellationToken) =
    let chunkSize = 3 // maxDegreeOfParallelism
    let idRange = [1..100] // Tv show ids to scrape

    let work = 
        idRange |> Seq.splitInto chunkSize // Split ids in buckets
        |> Seq.map (fun chunk -> async {
            let! scrapeCompletor = chunk |> scrape ct |> Async.StartChild
            let! result = scrapeCompletor
            printfn "Start writing in db..."
            let! saveCompletor = result |> save |> Async.StartChild
            let! _ = saveCompletor
            //result |> save |> Async.StartImmediate
            printfn "Finished writing in db..."
            
            return ()
        }) 
        //|> Async.Parallel     // this will actually run buckets in parallel
        |> Async.Sequential     // this will run without parallelization
        |> Async.Ignore

    //Async.Start(work, ct)
    Async.RunSynchronously(work, 2000, ct)

#time
let cts = new System.Threading.CancellationTokenSource()
runScraper cts.Token
//System.Threading.Thread.Sleep(5000)
//cts.Cancel()
#time

//let cts = new System.Threading.CancellationTokenSource()
//let token = cts.Token
//let tvShow = TvMaze.GetTvShow(token, 17) |> Async.RunSynchronously

//let getTvShowWithCancellation id = getTvShow token
//getTvShowWithCancellation 1
//Async.Start(main, cts.Token)
//System.Threading.Thread.Sleep(1000)
//cts.Cancel()