namespace FunctionalHttp.Example.Server

open System
open System.Collections.Concurrent

open FunctionalHttp.Core
open FunctionalHttp.Client
open FunctionalHttp.Server

open System.Reactive.Threading.Tasks
open FSharp.Control.Reactive
open SQLitePCL.pretty

type BlogEntryDao<'T> = {
        Created:DateTime
        ItemId:string
        Title:string
        Updated:DateTime
        UserId:string
        Entity:'T
    }

type IBlogStore<'T> =
    abstract member GetBlogEntries: string -> IObservable<BlogEntryDao<'T>>
    abstract member GetBlogEntry: string -> string -> Async<BlogEntryDao<'T>>
    abstract member DeleteBlogEntry: string -> string -> Async
    abstract member PutBlogEntry: BlogEntryDao<'T> -> Async

module BlogStore =
    let inMemory () =
        let db = SQLite3.Open ":memory:"
        db.Execute
            @"CREATE TABLE blogs (
              ID INT PRIMARY KEY  NOT NULL,
              CREATED        INT  NOT NULL,
              UPDATED        INT  NOT NULL,
              TITLE          TEXT NOT NULL,
              USER_ID        TEXT NOT NULL
              ENTITY         BLOB NOT NULL"
        
        let db = db.AsAsyncDatabaseConnection()
        let blogEntriesStmt = async {
            return! db.PrepareStatementAsync
                @"SELECT ID, CREATED, UPDATED, TITLE, ENTITY
                  FROM blogs
                  where USER_ID = ?" |> Async.AwaitTask
        }

        let blogEntriesStmt = Async.RunSynchronously(blogEntriesStmt)

        { new IBlogStore<'T> with 
            member this.GetBlogEntries (userid: string) = 
                blogEntriesStmt.
  
      
            member this.GetBlogEntry (userid:string) (entryid:string) = async {
            }

            member this.DeleteBlogEntry (userid:string) (entryid:string) = async {
            }
            
            member this.PutBlogEntry (entry:BlogEntryDao<_>) = async {
            }
        }

module Blog = 
    let entryResource route =
        let getBlogEntry (req:HttpRequest<_>) = async {
            return HttpResponse.create HttpStatus.successOk None
        }

        let deleteBlogEntry (req:HttpRequest<_>) = async {
            return HttpResponse.create HttpStatus.successOk ()
        }

        let updateBlogEntry (req:HttpRequest<IAtomEntry<string>>) = async {
            return HttpResponse.create HttpStatus.successOk (Some req.Entity)
        }

        let builder = UniformResourceBuilder()
        builder.Route <- route
        builder.Get <- getBlogEntry
        builder.Delete <- deleteBlogEntry
        builder.Put <- updateBlogEntry
        builder.Build()

    let toAsync (observable:IObservable<_>) = async {
        return! observable |> TaskObservableExtensions.ToTask |> Async.AwaitTask
    }

    let blogEntryToAtomEntry uri (entry:BlogEntryDao<string>) =
        let builder = Atom.entryBuilder ()
        builder.Content <- entry.Entity
        builder.Title <- entry.Title
        builder.Updated <- entry.Updated

        // FIXME: Wrong
        builder.Id <- uri
        builder.Build()

    let feedResource route (blogStore:IBlogStore<string>) =
        let getFeed (req:HttpRequest<_>) = async {
            let userId = req.Uri |> Route.getParametersFromUri route |> Map.find "userid"
            let entries = blogStore.GetBlogEntries userId

            let! entries = entries |> Observable.map(blogEntryToAtomEntry req.Uri) |> Observable.fold (fun acc v -> v::acc) [] |> toAsync
            let entries = entries |> List.rev :> seq<_> 

            let builder = Atom.feedBuilder()
            builder.Entries <- entries
            builder.Id <- req.Uri
            builder.Updated <- entries |> Seq.head |> (fun e -> e.Updated)
            let feed = builder.Build()

            return HttpResponse.create HttpStatus.successOk (Some (Choice1Of2 feed))
        }

        let createEntry (req:HttpRequest<IAtomEntry<string>>) = async {
            return HttpResponse.create HttpStatus.successOk (Some (Choice2Of2 req.Entity))
         }

        let builder = UniformResourceBuilder()
        builder.Route <- route
        builder.Get <- getFeed
        builder.Post <- createEntry
        builder.Build()