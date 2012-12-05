namespace FSharp.Web

// Initial version of the parser from http://blog.efvincent.com/parsing-json-using-f/
// Simplyfied, added AST and fixed some minor bugs

open System
open System.Text

module private JsonTokenizer =
  type Token =
    | OpenBracket | CloseBracket
    | OpenArray | CloseArray
    | Colon | Comma
    | String of string
    | Boolean of bool
    | Null
    | Number of string

  let escapedChars = 
    dict [ 'r', "\r"; 'n', "\n"; 'b', "\b"; 'f', "\f"; 't', "\t"; '/', "/"; '"', "\"" ]

  let (|EscapedChar|_|) c = 
    match escapedChars.TryGetValue(c) with
    | true, s -> Some s
    | _ -> None

  let (|UnicodeChar|_|) l =
    match l with
    | '\\' :: 'u' :: a :: b :: c :: d :: t ->
        Some((char (int (new String([| '0'; 'x'; a; b; c; d |])))).ToString(), t)
    | _ -> None

  let tokenize source=
      let rec parseString acc = function
          | UnicodeChar(s, t) ->
              parseString (acc + s) t
          | '\\' :: (EscapedChar s) :: t -> // escaped character
              parseString (acc + s) t
          | '"' :: t -> // closing quote terminates    
              acc, t
          | c :: t -> // otherwise accumulate
              parseString (acc + (c.ToString())) t
          | _ -> failwith "Malformed string."
 
      let rec token acc = function
          | (')' :: _) as t -> acc, t // closing paren terminates
          | ('}' :: _) as t -> acc, t // closing paren terminates
          | (']' :: _) as t -> acc, t // closing brackeet terminates
          | (':' :: _) as t -> acc, t // colon terminates
          | (',' :: _) as t -> acc, t // comma terminates
          | w :: t when Char.IsWhiteSpace(w) -> acc, t // whitespace terminates
          | [] -> acc, [] // end of list terminates
          | c :: t -> token (acc + (c.ToString())) t // otherwise accumulate chars

      let rec tokenize' acc = function
          | w :: t when Char.IsWhiteSpace(w) -> tokenize' acc t   // skip whitespace
          | '{' :: t -> tokenize' (OpenBracket :: acc) t
          | '}' :: t -> tokenize' (CloseBracket :: acc) t
          | '[' :: t -> tokenize' (OpenArray :: acc) t
          | ']' :: t -> tokenize' (CloseArray :: acc) t
          | ':' :: t -> tokenize' (Colon :: acc) t
          | ',' :: t -> tokenize' (Comma :: acc) t
          | 'n' :: 'u' :: 'l' :: 'l' :: t -> tokenize' (Token.Null :: acc) t
          | 't' :: 'r' :: 'u' :: 'e' :: t -> tokenize' (Boolean true :: acc) t
          | 'f' :: 'a' :: 'l' :: 's' :: 'e' :: t -> tokenize' (Boolean false :: acc) t
          | '"' :: t -> // start of string
              let s, t' = parseString "" t
              tokenize' (Token.String(s) :: acc) t'        
          | '-' :: d :: t when Char.IsDigit(d) -> // start of negative number
              let n, t' = token ("-" + d.ToString()) t
              tokenize' (Token.Number(n) :: acc) t'
          | '+' :: d :: t 
          | d :: t when Char.IsDigit(d) -> // start of positive number
              let n, t' = token (d.ToString()) t
              tokenize' (Token.Number(n) :: acc) t'
          | [] -> List.rev acc // end of list terminates
          | _ -> failwith "Tokinzation error"

      tokenize' [] [for x in source -> x]

type JSON =
  | JsonText of string
  | JsonNumber of float
  | JsonBoolean of bool
  | JsonNull
  | JsonArray of JSON list
  | JsonObject of Map<string,JSON>

  override json.ToString() =
      let sb = new StringBuilder()
      let rec writeToStringBuilder = function
      | JsonText t -> sb.AppendFormat("\"{0}\"",t)  |> ignore
      | JsonNumber n -> sb.Append n |> ignore
      | JsonBoolean true -> sb.Append "true" |> ignore
      | JsonBoolean false -> sb.Append "false" |> ignore
      | JsonNull -> sb.Append "null" |> ignore
      | JsonArray list -> 
          let isNotFirst = ref false
          sb.Append "[" |> ignore
          list
            |> List.iter 
                  (fun element -> 
                      if !isNotFirst then sb.Append "," |> ignore else isNotFirst := true
                      writeToStringBuilder element |> ignore)
          sb.Append "]"  |> ignore
      | JsonObject map -> 
          let isNotFirst = ref false
          sb.Append "{"  |> ignore
          map
            |> Map.iter 
                  (fun key value -> 
                      if !isNotFirst then sb.Append "," |> ignore else isNotFirst := true
                      sb.AppendFormat("\"{0}\":",key)  |> ignore
                      writeToStringBuilder value |> ignore)
          sb.Append "}"  |> ignore

      writeToStringBuilder json
      sb.ToString()

module JsonParser =
  let private emptyJObject = JsonObject Map.empty
  let private addProperty key value = function
  | JsonObject(properties) -> JsonObject(Map.add key value properties)
  | _ -> failwith "Malformed JSON object" 

  open System.Globalization
  open JsonTokenizer

  open FunJS

  /// Parses a JSON source text and returns an JSON AST
  let parse source =
      let map = function
      | Token.Number number -> 
          JsonNumber (Double.Parse(number, CultureInfo.InvariantCulture))
      | Token.String text -> JsonText text
      | Token.Null -> JSON.JsonNull
      | Token.Boolean(b) -> JsonBoolean b
      | v -> failwith "Syntax Error, unrecognized token in map()"
 
      let rec parseValue = function
      | OpenBracket :: t -> parseJObject t
      | OpenArray :: t ->  parseArray t
      | h :: t -> map h, t
      | _ -> 
          failwith "bad value"
 
      and parseArray = function
      | Comma :: t -> parseArray t
      | CloseArray :: t -> JsonArray [], t
      | t ->        
          let element, t' = parseValue t
          match parseArray t' with
          | JsonArray(elements),t'' -> JsonArray (element :: elements),t''
          | _ -> failwith "Malformed JSON array"
      and parseJObject = function
      | Comma :: t -> parseJObject t
      | Token.String(name) :: Colon :: t -> 
          let value,t' = parseValue t
          let jObject,t'' = parseJObject t'
          addProperty name value jObject,t''
      | CloseBracket :: t -> emptyJObject, t
      | _ -> failwith "Malformed JSON object"
    
      tokenize source 
      |> parseValue
      |> fst

module JsonUtils = 
  let (?) (jsonObject:JSON) (property:string) = 
    match jsonObject with
    | JsonObject o -> o.[property]
    | _ -> failwith "Not an object"

  type JSON with
    member x.GetEnumerator() = 
      match x with
      | JsonArray things -> (things :> seq<_>).GetEnumerator()
      | _ -> failwith "Not an array"
    member x.Text = 
      match x with
      | JsonText t -> t
      | _ -> failwith "Not a string"
    member x.ConcatenatedText = 
      match x with
      | JsonText t -> t
      | JsonArray a -> a |> List.map (fun e -> e.ConcatenatedText) |> String.concat ""
      | _ -> failwith "Non-text element"
    member x.Properties = 
      match x with
      | JsonObject map -> seq { for (KeyValue(k, v)) in map -> k, v }
      | _ -> failwith "Not an object"