# TvMaze

## TvMaze Scraper
TvMaze Scraper is a service that scrapes Tv shows from https://www.tvmaze.com/api and stores them in the database. 

TvMaze scraper targets .NET 7 and written in F#. It uses [LiteDB](https://www.litedb.org/) embedded NoSQL database. It uses [Polly](https://github.com/App-vNext/Polly) for implementing exponential backoff policy with jitter when hitting HTTP 429

### Known issues and limitations
* LiteDB is embedded database and not suited for highly concurrent write scenarios
* In parallel execution mode there is a loss of writes. So this not suited for production scenarios
* To have consistency on writes use Sequential execuition mode
* There some simplifications (e.g. no logging, status progress)

### Possible improvements
* Resumable state for scraper (e.g. check when left off by checking the last id)
* Implement scraper via [F#'s Mailbox Processor](https://en.wikibooks.org/wiki/F_Sharp_Programming/MailboxProcessor) (a.k.a actor model)
* Use server-side RDBMS or NoSQL (e.g. RavenDB)
* Better logging and satus on progress
* Nicer console with [Spectre.Console](https://spectreconsole.net/)

## TvMaze Service

TvMaze Service is a Web API written in C# that targets .NET 7. It exposes endpoints for paginated traversal of data scraped by TvMaze Scraper. As well as a scraper, it uses the same shared LiteDB database that is used for data scraping.

3 pagination strategy implemented:
* Simple pagination with a page number
* Offset pagination
* Keyset pagination

## Get started

TvMaze Scraper and Service use common database, so it is important to update database path in both projects.

### TvMaze Scraper
Navigate to the `TvMaze.Scraper` project and change `DbPath` in `Program.fs` to your path:

```fsharp
let config = {
        DbPath = Path.Combine(@"D:\My\RTL\data", DbConfig.Database)
        TvShowsMinId = 1
        TvShowsMaxId = 67022
        Exec = Parallel (MaxDegreeOfParallelism = 3)
    }
```
Please use absolute path.

You could also specify the range of ids for scraping and execution mode. Parallel execution is fast, but does not work well with LiteDB. To get more predictable and consistent results use sequential execution mode: `Exec = Sequential`. Get it fast but messy with `Exec = Parallel (MaxDegreeOfParallelism = 3)`

### TvMaze Service
Navigate to the `TvMaze.Service` project and change `DbPath` in `appsettings.json` to your path:

```json
"TvMaze": {
    "DbPath": "D:\\My\\RTL\\data",
    "PageSize": 25
  }
```
Set PageSize for simple pagination based on page number

### Database
Solution already comes with some scraped sample data. You can find db file in `data\tvmaze.db`. You can download [LiteDB.Stdio](https://github.com/mbdavid/LiteDB.Studio/releases) to open it and query content.

## Run

```sh
dotnet build
```
### Start scraper

```sh
dotnet run --project .\src\TvMaze.Scraper\TvMaze.Scraper.fsproj
```
Wait for a while for data to be scraped. You can cancel it by pressing `Ctrl+C` or start service in another terminal session (it should be able to open it in shared mode)

### Start service

```sh
dotnet run --project .\src\TvMaze.Service\TvMaze.Service.csproj
```
or hit `F5` in Visual Studio IDE

## Test it!

### Simple pagination with a page number
```sh
curl --location 'http://localhost:5042/tv-shows/pagination?page=1'
```

### Offset pagination
```sh
curl --location 'http://localhost:5042/tv-shows?limit=25&offset=25'
```

### Keyset pagination
```sh
curl --location 'http://localhost:5042/tv-shows/keyset?pageSize=25&lastId=26'
```

Have fun :)
