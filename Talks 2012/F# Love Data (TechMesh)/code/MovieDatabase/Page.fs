[<ReflectedDefinition>]
module Program

open FunJS
open FSharp.Apiary

// ------------------------------------------------------------------
// Initializataion

type MovieDb = ApiaryProvider<"themoviedb">

type j = TypeScript.Api<"Types/jquery.d.ts">
let jQuery (command:string) = j.jQuery.Invoke(command)
let (?) jq name = jQuery("#" + name)


// ------------------------------------------------------------------
// Main function

let main() = 

  let db = new MovieDb("http://api.themoviedb.org")
  db.AddQueryParam("api_key", "6ce0ef5b176501f8c07c634dfa933cff")
  let root = "http://cf2.imgobject.com/t/p/w92/"

  // ----------------------------------------------------------------
  // Show details

  let showDetails (id:int) = async {
    let! movie = db.Movie.AsyncGetMovie(id.ToString())
    jQuery?dialogOverview.text(movie.Overview) |> ignore 
    jQuery?dialogTitle.text(movie.Title) |> ignore
    jQuery?dialogImage.attr("src", root + movie.PosterPath) |> ignore
  
    let! casts = movie.AsyncCasts()
    let sorted = casts.Cast |> Array.sortBy (fun c -> c.Order)    
    let sorted = 
      if sorted.Length <= 10 then sorted |> Seq.ofArray 
      else sorted |> Seq.ofArray |> Seq.take 10

    jQuery?dialogCast.html("") |> ignore
    for cast in casts.Cast do 
      let html = "<strong>" + cast.Name + "</strong> (" + cast.Character + ")"
      jQuery("<li>").html(html).appendTo(jQuery?dialogCast) |> ignore }

  // ----------------------------------------------------------------
  // Movie search

  let search term = async {
    let! res = db.Search.AsyncMovie(query=["query", term])
    jQuery?results.html("") |> ignore

    res.Results 
    |> Seq.ofArray
    |> Seq.iteri (fun index item ->
        let link = 
          jQuery("<a>").attr("data-toggle", "modal").attr("href", "#detailsDialog")
            .text(item.Title).click(fun _ -> 
                showDetails item.Id |> Async.StartImmediate )
        let details =
          [| jQuery("<li>").html("<strong>Released:</strong> " + item.ReleaseDate.Value)
             jQuery("<li>").html("<strong>Average vote:</strong> " + item.VoteAverage.ToString())
             jQuery("<li>").html("<strong>Popularity:</strong> " + item.Popularity.ToString()) |]
        let body = 
          [| jQuery("<h3>").append(link)
             jQuery("<img>").attr("src", root + item.PosterPath.Value) 
             jQuery("<ul>").append(details)
             jQuery("<div>").addClass("clearer") |]    
        jQuery("<div>").addClass("searchResult")
         .append(body).appendTo(jQuery?results) |> ignore )  }

  // ----------------------------------------------------------------
  // Movie search

  jQuery?searchButton.click(fun () ->
    let id = jQuery?searchInput.``val``() :?> string
    search id |> Async.StartImmediate )

// ------------------------------------------------------------------
do Runtime.run()