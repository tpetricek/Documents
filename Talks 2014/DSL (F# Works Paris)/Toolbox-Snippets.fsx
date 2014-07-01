// ----------------------------------------------------------------------------
// 1. Functional 3D
// ----------------------------------------------------------------------------

let tower x z = 
  (Fun.cylinder
     |> Fun.scale (1.0, 1.0, 3.0) 
     |> Fun.translate (0.0, 0.0, 1.0)
     |> Fun.color Color.DarkGoldenrod ) $ 
  (Fun.cone 
     |> Fun.scale (1.3, 1.3, 1.3) 
     |> Fun.translate (0.0, 0.0, -1.0)
     |> Fun.color Color.Red )
  |> Fun.rotate (90.0, 0.0, 0.0)
  |> Fun.translate (x, 0.5, z)

// ----------------------------------------------------------------------------

let sizedCube height = 
  Fun.cube 
  |> Fun.scale (0.5, height, 1.0) 
  |> Fun.translate (-0.5, height/2.0 - 1.0, 0.0)

let twoCubes =
  sizedCube 0.8 $ (sizedCube 1.0 |> Fun.translate (0.5, 0.0, 0.0))

// ----------------------------------------------------------------------------

let block = 
  [ for offset in -4.0 .. +4.0 ->
      twoCubes |> Fun.translate (offset, 0.0, 0.0) ]
  |> Seq.reduce ($)
  |> Fun.scale (0.5, 2.0, 0.3)
  |> Fun.color Color.DarkGray

// ----------------------------------------------------------------------------

let wall offs rotate = 
  let rotationArg = if rotate then (0.0, 90.0, 0.0) else (0.0, 0.0, 0.0)
  let translationArg = if rotate then (offs, 0.0, 0.0) else (0.0, 0.0, offs)
  block |> Fun.rotate rotationArg |> Fun.translate translationArg

// ----------------------------------------------------------------------------

tower -2.0 -2.0 $ tower 2.0 -2.0 $ 
  tower -2.0 2.0 $ tower 2.0 2.0 $
  wall -2.0 true $ wall 2.0 true $
  wall -2.0 false $ wall 2.0 false

// ----------------------------------------------------------------------------
// 2. Markdown
// ----------------------------------------------------------------------------

let list items =
  let lis = [ for i in items -> sprintf "<li>%s</li>" i ]
  "<ul>" + (String.concat "" lis) + "</ul>"

// ----------------------------------------------------------------------------

(heading 1 "Creating DSLs with F#") + 
(p "Key components of a DSL:") + 
(list [
  (strong "Model") + " describes the structure of the domain that we are modelling";
  (strong "Syntax") + " provides an easy way for solving problems using the DSL" ])
|> showHtml

// ----------------------------------------------------------------------------

let doc =
  Sequence 
    [ Heading(1, Literal "Creating DSLs with F#")
      Paragraph(Literal "Key components of a DSL:")
      List(
        [ Sequence
            [ Strong (Literal "Model")
              Literal " describes the structure of the domain that we are modelling"]
          Sequence 
            [ Strong (Literal "Syntax")
              Literal " provides an easy way for solving problems using the DSL" ]
      ]) ]

// ----------------------------------------------------------------------------

  | Heading(n, p) -> sprintf "<h%d>%s</h%d>" n (formatNode p) n
  | List items -> 
      let lis = [ for li in items -> sprintf "<li>%s</li>" (formatNode li) ]
      "<ul>" + (String.concat "" lis) + "</ul>"
  | Sequence nodes -> nodes |> List.map formatNode |> String.concat ""

// ----------------------------------------------------------------------------

let translate (phrase:string) = 
  printfn "Translating: '%s'" phrase
  if String.IsNullOrWhiteSpace(phrase) then "" else
  let phrase = phrase.Replace("F#", "fsharp")
  let doc = Translate.Load("http://mymemory.translated.net/api/get?langpair=en|no&de=tomas@tomasp.net&q=" + phrase)
  let phrase = doc.Matches.[0].Translation
  phrase.Replace("fsharp", "F#")

// ----------------------------------------------------------------------------

  | List li -> List (List.map translateNode li)
  | Paragraph p -> Paragraph(translateNode p)
  | Sequence p -> Sequence (List.map translateNode p)

// ----------------------------------------------------------------------------
// 3. Classifiers
// ----------------------------------------------------------------------------

// BACKUP: 
win.Add("Always up", Price.rising)
win.Add("Mostly up", Price.regression Price.rising)
win.Add("Mostly down", Price.regression Price.declining)
win.Add("Average", Price.average)


// ----------------------------------------------------------------------------

// Pattern that detects when price is going up and is above given limit
let highAndRising limit = 
  Price.both Price.average mostlyUp
  |> Price.map (fun (avg, up) ->
      avg > limit && up)

win.Add("High & rising", highAndRising 30.0)
