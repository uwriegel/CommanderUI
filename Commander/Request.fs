module Request
open WebServer
open Commander
open Str

let requestOK (headers: WebServer.RequestHeaders) = headers.path.StartsWith "/request"

let run request = 
    async {
        let method = request.data.header.path |> substring (1 + indexOfStartPos '/' 1 request.data.header.path)
        match method with
        | "close" -> 
            close ()
            do! Response.asyncSendJson request Seq.empty
            shutdown ()
        | _ -> failwith "Unknown command"
    }