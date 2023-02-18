open System.Net.Http
open System
open System.Threading.Tasks
open System.Threading

//module polly

System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

//#r "nuget: FsHttp"
#r "nuget: FSharp.Data"
#r "nuget: Polly, 7.2.3"
#load "./Retries.fs"

open FSharp.Data

type Cast = {
    Id: int
    Name: string
    Birthday: DateTime option
}

type TvShow = {
    Id: int
    Name: string
    Casts: Cast array
}

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

//let toTvShowModel json =
//    json
//    |> TVMazeJson.Parse 
//    |> fun show -> 
//        {
//            Id = show.Id;
//            Name = show.Name;
//            Casts = show.Embedded.Cast
//            |> Array.map (fun p ->
//                {
//                    Id = p.Person.Id;
//                    Name = p.Person.Name;
//                    Birthday = p.Person.Birthday;
//                })
//        }

type TvMaze() =
    static let httpClient = new HttpClient (BaseAddress = new Uri("http://api.tvmaze.com"))
    //static member Http = httpClient
    static let toTvShowModel json =
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
            }

    static member GetTvShow(ct, id: int) =
        async {
            let! response = 
                Retries.createPolicy<HttpRequestException> 2
                |> Retries.executeCustom ct (
                    fun ct -> task {
                        printfn $"Making HTTP request for show with id {id}..."
                        let! response = httpClient.GetAsync($"/shows/{id}?embed=cast", ct)
                        //printfn $"Request for show with id {id} successful"
                        return response
                    })
                |> Async.AwaitTask
    
            let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            // Add error handlinng here (Result type)
            // when 404 we get {"name":"Not Found","message":"","code":0,"status":404} in response
            //

            return json |> toTvShowModel
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

        let items = ResizeArray<_>()
        for tvShow in tvShows do
            printfn $"Resolving show..."
            let! result = tvShow
            printfn $"TvShow: {result.Id} - {result.Name}"
            items.Add result

        return items.ToArray()
    }

let save ct items =
    async {
        for item in items do
            //let! _ = fn2 item
            return ()
    }

let runScraper (ct : CancellationToken) =
    let maxDegreeOfParallelism = 5

    let work = 
        Seq.splitInto 5 [1..120]
        |> Seq.map (fun chunk -> async {
            printfn "1"
            let! scrapeCompletor = chunk |> scrape ct |> Async.StartChild
            printfn "2"
            let! result = scrapeCompletor
            printfn "3"
            let! saveCompletor = result |> save ct |> Async.StartChild
            printfn "4"
            let! _ = saveCompletor
            printfn "5"
            
            return ()
        }) 
        //|> Async.Parallel
        |> Async.Sequential
        |> Async.Ignore

    Async.Start(work, ct)
    //Async.RunSynchronously(work, 1000, token)

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