module TvMaze.Scraper.Retries

open System
open System.Threading
open System.Threading.Tasks
open Polly
open Polly.Retry
open System.Net
open System.Net.Http

let private jitterer = Random()

let createPolicy<'a when 'a :> exn> retryCount =
    Policy.Handle<'a>()
        .OrResult<HttpResponseMessage>(
            resultPredicate = fun response -> response.StatusCode = HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount = retryCount,
            sleepDurationProvider = fun retryAttempt -> TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)) )

let executeCustom<'a when 'a :> HttpResponseMessage> ct (fn: CancellationToken -> Task<'a>) (policy: AsyncRetryPolicy<'a>) =
    policy.ExecuteAsync(action = fn, cancellationToken = ct)