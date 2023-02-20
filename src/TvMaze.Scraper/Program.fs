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
        TvShowsMinId = 50
        TvShowsMaxId = 100
        Exec = Parallel (MaxDegreeOfParallelism = 3)
        // Exec = Sequential
    }

    try
        let cts = new Threading.CancellationTokenSource()
        Console.CancelKeyPress.Add(fun arg -> arg.Cancel <- true; cts.Cancel())

        printfn "Start scraping..."
        printfn "Press Ctrl+C to exit..."
        
        (config, cts.Token) ||> execute
        0
    with
        | Failure msg -> eprintfn $"General Failure: {msg}"; -1
        | :? OperationCanceledException -> eprintfn "Exiting from TvMaze.Scraper..."; 0