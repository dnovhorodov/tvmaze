open System
open System.IO
open TvMaze.Persistence
open TvMaze.Scraper.Worker

[<EntryPoint>]
let main _ =
    
    printfn @"
    +-+-+-+-+-+-+ +-+-+-+-+-+-+-+
    |T|v|M|a|z|e| |S|c|r|a|p|e|r|
    +-+-+-+-+-+-+ +-+-+-+-+-+-+-+
    "

    let config = {
        DbPath = Path.Combine(@"D:\My\RTL\data", DbConfig.Database)
        TvShowsMinId = 1
        TvShowsMaxId = 67022
        Exec = Parallel (MaxDegreeOfParallelism = 3)
        // Exec = Sequential
    }

    try
        let cts = new Threading.CancellationTokenSource()
        Console.CancelKeyPress.Add(fun arg -> arg.Cancel <- true; cts.Cancel())

        printfn "Start scraping..."
        printfn "Relax and give it a couple of minutes to scrape some data :)"
        printfn ""
        printfn "Press Ctrl+C to exit if you tired waiting"

        (config, cts.Token) ||> execute
        0
    with
        | Failure msg -> eprintfn $"General Failure: {msg}"; -1
        | :? OperationCanceledException -> eprintfn "Exiting from TvMaze.Scraper..."; 0