module RequestState
open System.Collections.Concurrent

let private recentRequests = new ConcurrentDictionary<string, string>()

let updateRecentRequest callerId requestId =
    recentRequests.[callerId] <- requestId

let getRecentRequest callerId = recentRequests.[callerId]