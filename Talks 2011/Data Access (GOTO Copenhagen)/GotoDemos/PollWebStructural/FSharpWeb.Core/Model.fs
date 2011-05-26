namespace FSharpWeb.Core

/// Data type used for passing data to the view
type PollOption =
  { ID : int
    Votes : int
    Percentage : float
    Title : string }
          
/// Data model implemented as F# module with functions
module Model =

  let connectionString = 
    "Data Source=.\SQLEXPRESS;AttachDbFilename=\"|DataDirectory|" +
    "PollData.mdf\";Integrated Security=True;User Instance=True"
  
  /// Connect to the database & return results as PollOption sequence
  let load() : seq<PollOption> = 
    // Dynamically call 'GetOptions' stored procedure
    // The result is automatically converted to the desired type
    let db = new DynamicDatabase(connectionString)
    db?GetOptions()

  /// Connect to the database & vote for the specified option
  let vote(id:int) : unit = 
    // Since the result type is 'unit' the ? operator
    // knows that it should use the 'NonQuery' call
    let db = new DynamicDatabase(connectionString)
    db?Vote(id)