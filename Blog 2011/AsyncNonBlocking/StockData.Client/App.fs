namespace StockData.Client

open System.Windows

type App() as app =
  inherit Application()

  // Create the main application control
  let main = new AppControl()
  do 
    // Display the control when application starts
    app.Startup.Add(fun _ -> 
      app.RootVisual <- main)