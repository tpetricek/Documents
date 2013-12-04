Doing Data Science with F# (NDC London 2013)
--------------------------------------------

To run the samples, open the solution in the `code` folder (or download the contents of the
folder in the `code.zip` file) and build the solution (for the first time) to restore 
NuGet packages that are used in the F# scripts. Then you can open the `fsx` files and
follow the script. You might need to update paths, or copy the `data` folder to `C:\data`.

You can ignore the `start` folder,
which contains the initial source code used in the talk.

### Related materials

 * [Understanding the World with F#](http://channel9.msdn.com/posts/Understanding-the-World-with-F)
   is a recorded version of a very similar talk at Channel 9

 * [Slides from the talk](http://www.slideshare.net/tomaspfb/doing-data-science-with-f-28891405) are also available for easy viewing on SlideShare

### Vega plugin 

The Visual Studio plugin for displaying embedded charts used during the last sample 
is available in the `plugin` directory - however, note that this is a pre-alpha prototype
and it is not required. You can see visualizations in your web browser by calling
`Vega.Open()` or by navigating to http://localhost:8081 (after loading some chart
in F# Interactive). Building the plugin requires Visual Studio 2013 SDK.

