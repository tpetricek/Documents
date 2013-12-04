MVP Showcase (November 2013)
-------------------------------------------

To run the samples, open the solution in the `code` folder
and build the solution (for the first time) to restore 
NuGet packages that are used in the F# scripts. Then you can open the `fsx` files and
follow the script.
 
The Visual Studio plugin for displaying embedded charts used in some of the samples 
is available in the `plugin` directory - however, note that this is a pre-alpha prototype
and it is not required. You can see visualizations in your web browser by calling
`Vega.Open()` or by navigating to http://localhost:8081 (after loading some chart
in F# Interactive). Building the plugin requires Visual Studio 2013 SDK.
