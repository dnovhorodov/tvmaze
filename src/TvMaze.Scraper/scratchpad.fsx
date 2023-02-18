open System.Threading
open System.Threading.Tasks


System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)
#r "nuget: Newtonsoft.Json"
#r "nuget: FSharp.Data"
#r "nuget: FsHttp"

open System
open FSharp.Data
open FsHttp

type TVMaze = JsonProvider<"""{
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

//let getTvShowA n =
//    TVMaze.AsyncLoad($"http://api.tvmaze.com/shows/{n}?embed=cast")

let getTvShow n = 
    async {
        return! http {
            GET $"http://api.tvmaze.com/shows/{n}?embed=cast"
        } 
        |> Request.send
        |> Response.toFormattedTextAsync
      }

//let (name, casts) = response |> fun json -> json?name.GetString(), json?_embedded?cast
let response = getTvShow 1 |> Async.RunSynchronously

//let res = getTvShowA 67022 |> Async.RunSynchronously
//let show = 
//    res |> fun show -> 
//        {|
//            Id = show.Id;
//            Name = show.Name;
//            Casts = show.Embedded.Cast
//            |> Array.map (fun p ->
//                {|
//                    Id = p.Person.Id;
//                    Name = p.Person.Name;
//                    Birthday = p.Person.Birthday |> Option.map (fun d -> d.ToString "yyyy/MM/dd");
//                |})
//        |}

TVMaze.Parse response
    |> fun show -> 
        {|
            Id = show.Id;
            Name = show.Name;
            Casts = show.Embedded.Cast
            |> Array.map (fun p ->
                {|
                    Id = p.Person.Id;
                    Name = p.Person.Name;
                    Birthday = p.Person.Birthday |> Option.map (fun d -> d.ToString "yyyy/MM/dd");
                |})
        |}
    
 //JsonValue.Parse (response.ToString())

let fn1 i = async {
    //printfn $"fn1 working with {i}..."
    do! Async.Sleep 500
    //printfn $"fn1 {i} finished"

    return i*2
}

let fn2 (s: int) =  async {
    printfn $"Writing {s} in DB..."
    do! Async.Sleep 500
    printfn $"Successfully saved {s}"
    ()
}

let a2 = [9..12] |> List.map (fun x -> fn1 x |> Async.RunSynchronously) |> List.sum

Seq.initInfinite (fun x -> x + 1) |> Seq.pairwise |> Seq.take 10


//let downloadTest chunk = 
//    (async {
//        let downloadTasks = chunk |> Array.map fn1
//        let mutable final = 0
//        for dwnld in downloadTasks do
//            let! result = dwnld
//            final <- final + result

//        return final
//    })

let downloadTest chunk =
    async {
        let downloadTasks = chunk |> Array.map fn1
        let items = ResizeArray<_>()
        for dwnld in downloadTasks do
            let! result = dwnld
            items.Add result

        return items.ToArray()
    }

let saveTest items =
    async {
        for item in items do
            if item = 26 then failwith "Boom"
            let! _ = fn2 item
            ()
    }

let runTest () =
    printfn "Starting workflow at %O" DateTime.Now.TimeOfDay
    let maxDegreeOfParallelism = 10

    let job = 
        Seq.splitInto 5 [1..20]
        |> Seq.map (fun chunk -> async {
            let! completor = chunk |> downloadTest |> Async.StartChild
            let! result = completor
            //async { do! saveTest result } |> Async.StartImmediate
            let! completor2 = result |> saveTest |> Async.StartChild
            let! _ = completor2
            //printfn $"Rezult: {result}"
            //return result
            //return ((), maxDegreeOfParallelism)
            return ()
        })
        //|> Async.Sequential
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously

    printfn "Finished workflow at %O" DateTime.Now.TimeOfDay

runTest ()

let runTest2 (token : CancellationToken) =
    let maxDegreeOfParallelism = 5

    let work = 
        Seq.splitInto 5 [1..20]
        |> Seq.map (fun chunk -> async {
            let! completor = chunk |> downloadTest |> Async.StartChild
            let! result = completor
            //async { do! saveTest result } |> Async.StartImmediate
            //let! _ = result |> saveTest |> Async.StartChild |> Async.Ignore
            let! completor2 = result |> saveTest |> Async.StartChild
            let! _ = completor2
            return ((), maxDegreeOfParallelism, 2)
            //return ()
        }) 
        //|> Async.Parallel
        |> Async.Sequential
        |> Async.Ignore

    Async.Start(work, token)
    //Async.RunSynchronously(work, 1000, token)

#time
let cts = new System.Threading.CancellationTokenSource()
runTest2 cts.Token
System.Threading.Thread.Sleep(5000)
cts.Cancel()
#time

//[getTvShow 1; getTvShow 2] |> Async.RunSynchronously

//let [<Literal>] private BatchSize = 100
//let someFunc count =
//    seq { 
//        for i in 1..count do 
//            if i % BatchSize = 0 then yield BatchSize
//            if i = count && count % BatchSize > 0 then yield count % BatchSize
//    } 
//    |> Seq.iter (fun n -> printfn "%i" n)

//someFunc 10000

let tttry2 = async {
    printfn "Starting sleep workflow at %O" DateTime.Now.TimeOfDay
    //return! [1..10] |> List.map (fun x -> fn1 x) |> Async.StartChild
    //let tasks = new List<Async<int>>()
    let mutable final = 0
    for i in 1..20 do
        let! res = fn1 i
        final <- final + res
    //let! a = tasks |> Task.WhenAll
    //return tasks
    printfn $"Result: {final}"
    printfn "Finished sleep workflow at %O" DateTime.Now.TimeOfDay
}

let pmap f l =
    seq { for a in l -> async { return f a } }
    |> Async.Parallel
    |> Async.RunSynchronously


// Wait 2 seconds and then print 'finished'
let work i = async {
  do! Async.Sleep(2000)
  printfn "work finished %d" i }

let main = async { 
    for i in 0 .. 5 do
      // (1) Start an independent async workflow:
      //work i |> Async.Start
      // (2) Start the workflow as a child computation:
      do! work i |> Async.StartChild |> Async.Ignore 
  }

// Start the computation, wait 1 second and than cancel it
//let cts = new System.Threading.CancellationTokenSource()
//Async.Start(main, cts.Token)
//System.Threading.Thread.Sleep(1000)    
//cts.Cancel()
