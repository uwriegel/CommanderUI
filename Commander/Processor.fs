module Processor
open System.IO
open Model
open ModelTools
open DirectoryProcessor

[<Literal>]
let ROOT = "root"

[<Literal>]
let DRIVES = "drives"

[<Literal>]
let FILE_SYSTEM = "FileSystem"

type ProcessorObject = {
    initEvents: (string->unit)->unit
    get: (string option)->(string option)->GetResult
    processItem: int->GetResult
    getCurrentPath: unit->string
}

type Type =
    Root = 0
    | Drives = 1
    | FileSystem = 2

let create id = 

    let mutable lastColumns: Type option = None
    let mutable currentPath = @"c:\" // TODO: Initial "root"
    // TODO: get always with path
    // TODO: path must be returned and then saved in CommanderView
    // TODO: Processor does not have to save left/right
    // TODO: DriveItems
    let mutable sentEvent = fun (item: string) -> ()
    let mutable requestNr = 0
    let mutable currentItems = [||]

    let initEvents hotSentEvent = sentEvent <- hotSentEvent

    let (| TypeChanged | _ |) arg = 
        match lastColumns with 
        | Some value -> if arg = value then None else Some ()
        | None -> Some ()

    let getColumns columnsType =
        match columnsType with
        | TypeChanged -> 
            lastColumns <- Some columnsType
            match columnsType with
            | Type.Root -> Some {
                    name = sprintf "%d-%s" id ROOT
                    values = [| { name = "Name"; isSortable = false; columnsType = ColumnsType.String } |]
                }
            | Type.Drives -> Some {
                    name = sprintf "%d-%s" id DRIVES
                    values = [| 
                        { name = "Name"; isSortable = false; columnsType = ColumnsType.String }
                        { name = "Bezeichnung"; isSortable = false; columnsType = ColumnsType.String }
                        { name = "Größe"; isSortable = false; columnsType = ColumnsType.String }
                    |]
                }
            | _ -> Some {
                    name = string id
                    values = DirectoryProcessor.getColumns ()
                }    
        | _ -> None

    let getRootItems () = { 
        response = { items = Some [||]; columns = getColumns Type.Root }
        continuation = None
    }
    let getDriveItems () = { 
        response = { items = Some [||]; columns = getColumns Type.Drives }
        continuation = None
    }

    let get path selectThis = 
        requestNr <- requestNr + 1
        let path = 
            match path with
            | Some path -> 
                if Directory.Exists path then
                    currentPath <- path
                    path
                else
                    currentPath
            | None -> currentPath

        let getIndexToSelect (currentItems: Item[]) = 
            match selectThis with
            | Some name -> 
                currentItems |> Array.findIndex (fun t -> t.name = name)
            | None -> 0

        match path with 
        | ROOT -> getRootItems ()
        | DRIVES -> getDriveItems () 
        | _ -> 
            currentItems <- DirectoryProcessor.getItems path id

            let indexToSelect = getIndexToSelect currentItems

            let getResponseItem index (item: Item) = { 
                itemType = item.itemType
                index = index
                icon = item.icon
                items = 
                    match item.itemType with
                    | ItemType.File -> [| getNameOnly item.name; item.extension; convertTime item.dateTime; string item.size; "" |] 
                    | ItemType.Directory -> [| item.name; ""; convertTime item.dateTime; ""; "" |] 
                    | _ -> [| item.name; ""; ""; ""; "" |] 
                isCurrent = index = indexToSelect
                isHidden = item.isHidden
            }

            let result = {
                items = Some (currentItems |> Array.mapi getResponseItem)
                columns = getColumns Type.FileSystem
            }
            let thisRequest = requestNr

            let check () = thisRequest = requestNr

            let continuation () = 
                async {
                    let updateItems = retrieveFileVersions path result.items.Value check
                    let updateItems2 = retrieveExifDates path result.items.Value check
                    match check () with
                    | true -> 
                        let commanderUpdate = {
                            id = requestNr
                            updateItems =  Array.concat [|updateItems; updateItems2|] 
                        }
                        sentEvent <| Json.serialize commanderUpdate
                    | false -> ()          
                } |> Async.Start
            
            {
                response = result
                continuation = Some continuation
            }

    let processItem index =             
        let selectedItem = currentItems.[index]
        match selectedItem.itemType with
        | ItemType.Parent ->
            let info = DirectoryInfo currentPath
            get (Some info.Parent.FullName) (Some info.Name)
        | ItemType.Directory -> 
            let path = Path.Combine (currentPath, selectedItem.name)
            get (Some path) None
        | _ -> 
            {
                response = {
                    items = None
                    columns = None
                }
                continuation = None
            }

    let getCurrentPath () = currentPath            
        
    {
        initEvents = initEvents
        get = get
        processItem = processItem
        getCurrentPath = getCurrentPath
    }