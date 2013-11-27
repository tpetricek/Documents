Understanding the World with F# (Channel 9)
-------------------------------------------

To run the samples, open the solution in the `code` folder (or download the contents of the
folder in the `code.zip` file) and build the solution (for the first time) to restore 
NuGet packages that are used in the F# scripts. Then you can open the `fsx` files and
follow the script. You can ignore the `start` folder,
which contains the initial source code used in the talk.
 
The Visual Studio plugin for displaying embedded charts used during the last sample 
is available in the `plugin` directory - however, note that this is a pre-alpha prototype
and it is not required. You can see visualizations in your web browser by calling
`Vega.Open()` or by navigating to http://localhost:8081 (after loading some chart
in F# Interactive). Building the plugin requires Visual Studio 2013 SDK.
