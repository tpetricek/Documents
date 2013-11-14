Effective Data & Time Series Analysis with Deedle
-------------------------------------------------

For more information, see:
 
 * [Announcing Deedle at BlueMountain tech blog](http://techblog.bluemountaincapital.com/2013/11/13/announcing-deedle/)
 * [Talk recording available on Vimeo](http://vimeo.com/79345095)
 * [Deedle documentation on GitHub](bluemountaincapital.github.io/Deedle/)

### Running the samples

To run the samples, open the solution in the `Samples` folder (or download the contents of the
folder in the `samples.zip` file) and build the solution once to 
restore NuGet packages that are used in the F# scripts. Then open the `fsx` files and
run them line-by-line in F# interactive.

### Talk abstract 

[Deedle](http://bluemountaincapital.github.io/Deedle) is a new open-source library for data and time series manipulation. It supports wide range of operations such as slicing, joining and aligning, handling of missing values, grouping and aggregation, statistics and more. Deedle is written in F#, but it also provides an easy to use C# interface.

In this talk, we'll demonstrate Deedle by analyzing time series financial data from historical Yahoo stock prices. Then we'll try to understand the US government spending with data from Freebase and WorldBank and finally, we'll explore the Titanic survivals data set.

Although the main focus of the talk is the Deedle library, we will demonstrate the entire "data science" stack for F# along the way. We will:

Use F# Data type providers to fetch data from the internet

 * Align, process and explore data using Deedle

 * Perform advanced statistical computations using the R type provider

 * Visualize data using R's ggplot and F# Charting
