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
  let load() = 
    // Dynamically call 'GetOptions' stored procedure
    let db = new DynamicDatabase(connectionString)
    let options = db.Query?GetOptions()
    // Use the ? operator to get columns of the record
    // (columns are automatically converted to desired types)
    [ for row in options do
        yield { ID = row?ID; Votes = row?Votes;
                Title = row?Title; Percentage = row?Percentage } ]


  /// Connect to the database & vote for the specified option
  let vote(id:int) = 
    let db = new DynamicDatabase(connectionString)
    db.NonQuery?Vote(id)