module Demo.ImagePipeline.Pipeline

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Diagnostics.CodeAnalysis
open System.Drawing
open System.IO
open System.Linq
open System.Threading
open System.Threading.Tasks

open FSharp.Control
open Demo.ImagePipeline
open Demo.ImagePipeline.Extensions

// ------------------------------------------------------------------------------
// Image Pipeline Top Level Loop
// ------------------------------------------------------------------------------

let LoadBalancingDegreeOfConcurrency = 2
let MaxNumberOfImages = 500

/// Runs the image pipeline example. The program goes through the jpg images located in the SourceDir
/// directory and performs a series of steps: it resizes each image and adds a black border and then applies
/// a Gaussian noise filter operation to give the image a grainy effect. Finally, the program invokes 
/// a user-provided delegate to the image (for example, to display the image on the user interface).
/// 
/// Images are processed in sequential order. That is, the display delegate will be 
/// invoked in exactly the same order as the images appear in the file system.
let imagePipelineMainLoop displayFn token errorFn =
    try
        let sourceDir = Directory.GetCurrentDirectory()
        // Ensure that frames are presented in sequence before invoking the user-provided display function.
        let imagesSoFar = ref 0
        let safeDisplayFn (info:ImageInfo) =
            if info.SequenceNumber <> !imagesSoFar then
                failwithf "Images processed out of order. Saw %d, expected %d" info.SequenceNumber (!imagesSoFar)
            displayFn info
            incr imagesSoFar

        // Create a cancellation handle for inter-task signaling of exceptions. This cancellation
        // handle is also triggered by the incoming token that indicates user-requested
        // cancellation.
        use cts = CancellationTokenSource.CreateLinkedTokenSource([| token |]) 
        PipelineAgent.runMessagePassing safeDisplayFn cts
    with 
    | :? AggregateException as ae when ae.InnerExceptions.Count = 1 ->
        errorFn (ae.InnerExceptions.[0])
    | :? OperationCanceledException as e -> reraise()
    | e -> errorFn e
