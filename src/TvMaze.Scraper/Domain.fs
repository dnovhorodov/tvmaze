module TvMaze.Scraper.Domain

open System

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