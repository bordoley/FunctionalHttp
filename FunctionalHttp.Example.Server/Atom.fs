namespace FunctionalHttp.Example.Server

open System
open FunctionalHttp.Core

type IAtomLink =
    // FIXME: IRI
    abstract member Href:Uri with get
    abstract member HrefLanguage:Option<LanguageTag> with get
    abstract member Length:Option<uint64> with get
    abstract member Rel:Option<String> with get
    abstract member Title:Option<String> with get
    abstract member Type:Option<MediaType> with get

type IAtomCategory =
    abstract member Label:Option<string> with get

    // FIXME: IRI
    abstract member Scheme:Option<Uri> with get
    abstract member Term:string with get

type IAtomPerson =
    //abstract member Email:Option<EmailAddress> with get
    abstract member Name:string with get

    // FIXME: IRI
    abstract member Uri:Option<Uri> with get

type IAtomGenerator =
    abstract member Content:string with get

    // FIxme: IRI
    abstract member Uri:Option<Uri> with get

    abstract member Version:Option<String> with get

type IAtomEntry<'T> =
    abstract member Authors:seq<IAtomPerson> with get
    abstract member Categories:seq<IAtomCategory> with get
    abstract member Content:'T with get
    abstract member Contributors:seq<IAtomPerson> with get

    // FIXME: IRI
    abstract member Id:Uri with get
    abstract member Links:seq<IAtomLink> with get
    abstract member Published:Option<DateTime> with get
    abstract member Rights:Option<string> with get
    abstract member Source:Option<IAtomFeed<IAtomEntry<'T>, 'T>> with get
    abstract member Summary:Option<string> with get
    abstract member Title:string with get
    abstract member Updated:DateTime with get

and IAtomFeed<'TEntry, 'TContent> when 'TEntry :> IAtomEntry<'TContent> =
    abstract member Authors:seq<IAtomPerson> with get
    abstract member Categories:seq<IAtomCategory> with get
    abstract member Contributors:seq<IAtomPerson> with get
    abstract member Entries:seq<'TEntry> with get
    abstract member Generator:Option<IAtomGenerator> with get

    // Fixme: IRI
    abstract member Icon:Option<Uri> with get

    // FIXME: IRI
    abstract member Id:Uri with get
    abstract member Links:seq<IAtomLink> with get

    // FIXME: IRI
    abstract member Logo:Option<Uri> with get
    abstract member Rights:Option<string> with get
    abstract member SubTitle: Option<string> with get
    abstract member Title:string with get
    abstract member Updated:DateTime with get

type AtomEntryBuilder<'T> () =
    let authors:List<IAtomPerson> ref = ref []
    let categories:List<IAtomCategory> ref = ref []
    let content:Option<'T> ref = ref None
    let contributors:List<IAtomPerson> ref = ref []
    let id:Option<Uri> ref = ref None
    let links:List<IAtomLink> ref = ref []
    let published:Option<DateTime> ref = ref None
    let rights:Option<string> ref = ref None
    let source:Option<IAtomFeed<IAtomEntry<'T>, 'T>> ref = ref None
    let summary:Option<string> ref = ref None
    let title:Option<string> ref = ref None
    let updated:Option<DateTime> ref = ref None

    member this.Authors with set (v:seq<IAtomPerson>) = authors := (v |> List.ofSeq)
    member this.Categories with set (v:seq<IAtomCategory>) = categories := (v |> List.ofSeq)
    member this.Content with set(v) = content := Some v
    member this.Contributors with set (v:seq<IAtomPerson>) = contributors := (v |> List.ofSeq)

    // FIXME: IRI
    member this.Id with set (v:Uri) = id := Some v

    member this.Links with set (v:seq<IAtomLink>) = links := (v |> List.ofSeq)
    member this.Published with set (v:DateTime) = published := Some v
    member this.Rights with set (v:string) = rights := Some v
    member this.Source with set (v:IAtomFeed<IAtomEntry<'T>, 'T>) = source := Some v
    member this.Summary with set (v:string) = summary := Some v
    member this.Title with set (v:string) = title := Some v
    member this.Updated with set (v:DateTime) = updated := Some v

    member this.Build() =
        let authors = !authors :> seq<IAtomPerson> 
        let categories = !categories :> seq<IAtomCategory>
        let content = (!content).Value
        let contributors = !contributors :> seq<IAtomPerson> 
        let id = (!id).Value
        let links = !links :> seq<IAtomLink>
        let published = !published
        let rights = !rights
        let source = !source
        let summary = !summary
        let title = (!title).Value
        let updated = (!updated).Value

        { new IAtomEntry<'T> with
            member this.Authors with get() = authors
            member this.Categories with get() = categories
            member this.Content with get() = content
            member this.Contributors with get() = contributors
            member this.Id with get() = id
            member this.Links with get() = links
            member this.Published with get() = published
            member this.Rights with get() = rights
            member this.Source with get() = source
            member this.Summary with get() = summary
            member this.Title with get() = title
            member this.Updated with get() = updated
        }

type AtomFeedBuilder<'TEntry, 'TContent> when 'TEntry :> IAtomEntry<'TContent> () =
    let authors:List<IAtomPerson> ref = ref []
    let categories:List<IAtomCategory> ref = ref []
    let contributors:List<IAtomPerson> ref = ref []
    let entries:List<'TEntry> ref = ref []
    let generator:Option<IAtomGenerator> ref = ref None
    let icon:Option<Uri> ref = ref None
    let id:Option<Uri> ref = ref None
    let links:List<IAtomLink> ref = ref []
    let logo:Option<Uri> ref = ref None
    let rights:Option<string> ref = ref None
    let subtitle:Option<string> ref = ref None
    let title:Option<string> ref = ref None
    let updated:Option<DateTime> ref = ref None

    member this.Authors with set (v:seq<IAtomPerson>) = authors := (v |> List.ofSeq)

    member this.Categories with set (v:seq<IAtomCategory>) = categories := (v |> List.ofSeq)

    member this.Contributors with set (v:seq<IAtomPerson>) = contributors := (v |> List.ofSeq)

    member this.Entries with set (v:seq<'TEntry>) = entries := (v |> List.ofSeq)

    member this.Generator with set (v:IAtomGenerator) = generator := Some v
     
    // Fixme: IRI
    member this.Icon with set (v:Uri) = icon := Some v

    // FIXME: IRI
    member this.Id with set (v:Uri) = id := Some v

    member this.Links with set (v:seq<IAtomLink>) = links := (v |> List.ofSeq)

    // FIXME: IRI
    member this.Logo with set (v:Uri) = logo := Some v
    member this.Rights with set (v:string) = rights := Some v
    member this.SubTitle with set (v:string) = subtitle := Some v
    member this.Title with set (v:string) = title := Some v
    member this.Updated with set (v:DateTime) = updated := Some v
    
    member this.Build () =
        let authors = !authors :> seq<IAtomPerson>
        let categories = !categories :> seq<IAtomCategory>
        let contributors = !contributors :> seq<IAtomPerson>
        let entries = !entries :> seq<'TEntry>
        let generator = !generator
        let icon = !icon
        let id = (!id).Value
        let links = !links :> seq<IAtomLink>
        let logo = !logo
        let rights = !rights
        let subtitle = !subtitle
        let title = (!title).Value
        let updated = (!updated).Value

        { new IAtomFeed<'TEntry, 'TContent> with
            member this.Authors with get () = authors 
            member this.Categories with get() = categories 
            member this.Contributors with get() = contributors 
            member this.Entries with get() = entries 
            member this.Generator with get() = generator

            // Fixme: IRI
            member this.Icon with get() = icon

            // FIXME: IRI
            member this.Id with get() = id
            member this.Links with get() = links

            // FIXME: IRI
            member this.Logo with get() = logo
            member this.Rights with get() = rights
            member this.SubTitle with get() = subtitle
            member this.Title with get() = title
            member this.Updated with get() = updated
        }

module Atom = 
    let entryBuilder () = AtomEntryBuilder<_>()  
    let feedBuilder () = AtomFeedBuilder<_,_>()

module LinkRelationships =
    [<Literal>]
    let alternate = "alternate"

    [<Literal>]
    let appendix = "appendix"
    
    [<Literal>]
    let archives = "archives"
    
    [<Literal>]
    let author = "author"
    
    [<Literal>]
    let bookmark = "bookmark"
    
    [<Literal>]
    let canonical = "canonical"
    
    [<Literal>]
    let chapter = "chapter"
    
    [<Literal>]
    let collection = "collection"
    
    [<Literal>]
    let contents = "contents"
    
    [<Literal>]
    let copyright = "copyright"
    
    [<Literal>]
    let current = "current"
    
    [<Literal>]
    let describedby = "describedby"
    
    [<Literal>]
    let disclosure = "disclosure"
    
    [<Literal>]
    let duplicate = "duplicate"
    
    [<Literal>]
    let edit = "edit"
    
    [<Literal>]
    let editMedia = "edit-media"
    
    [<Literal>]
    let enclosure = "enclosure"
    
    [<Literal>]
    let first = "first"
    
    [<Literal>]
    let glossary = "glossary"
    
    [<Literal>]
    let help = "help"
    
    [<Literal>]
    let host = "host"
    
    [<Literal>]
    let hub = "hub"
    
    [<Literal>]
    let icon = "icon"
    
    [<Literal>]
    let index = "index"
    
    [<Literal>]
    let item = "item"
    
    [<Literal>]
    let last = "last"
    
    [<Literal>]
    let latestVersion = "latest-version"
    
    [<Literal>]
    let license = "license"
    
    [<Literal>]
    let lrdd = "lrdd"
    
    [<Literal>]
    let monitor = "monitor"
    
    [<Literal>]
    let monitorGroup = "monitor-group"
    
    [<Literal>]
    let next = "next"
    
    [<Literal>]
    let nextArchive = "next-archive"
    
    [<Literal>]
    let nofollow= "nofollow"
    
    [<Literal>]
    let noreferrer = "noreferrer"
    
    [<Literal>]
    let payment = "payment"
    
    [<Literal>]
    let predecessorVersion = "predecessor-version"
    
    [<Literal>]
    let prefetch = "prefetch"
    
    [<Literal>]
    let prev = "prev"
    
    [<Literal>]
    let previous = "previous"
    
    [<Literal>]
    let previousArchive = "prev-archive"
    
    [<Literal>]
    let related = "related"
    
    [<Literal>]
    let replies = "replies"
    
    [<Literal>]
    let result = "results"
    
    [<Literal>]
    let search = "search"
    
    [<Literal>]
    let section = "section"
    
    [<Literal>]
    let self = "self"
    
    [<Literal>]
    let service = "service"
    
    [<Literal>]
    let start = "start"
    
    [<Literal>]
    let stylesheet = "stylesheet"
    
    [<Literal>]
    let subsection = "subsection"
    
    [<Literal>]
    let successorVersion = "successor-version"
    
    [<Literal>]
    let suggestions = "suggestions"
    
    [<Literal>]
    let tag = "tag"
    
    [<Literal>]
    let up = "up"
    
    [<Literal>]
    let versionHistory = "version-history"
    
    [<Literal>]
    let via = "via"
    
    [<Literal>]
    let workingCopy = "working-copy"
    
    [<Literal>]
    let workingCopyOf = "working-copy-of"