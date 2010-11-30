module Demo.ImagePipeline.PipelineAgent

open System
open System.IO
open System.Drawing
open System.Threading
open System.Windows.Forms
open FSharp.Control

open Demo.ImagePipeline.Extensions

// ------------------------------------------------------------------------------
// Operations for individual images
// ------------------------------------------------------------------------------

let loadImage fname count clockOffset =
    let startTick = Environment.TickCount
    let bitmap = new Bitmap(fname:string) 
    bitmap.Tag <- fname
    let info = ImageInfo.Create(count, fname, bitmap, clockOffset)
    info.PhaseStartTick.[0] <- startTick - clockOffset 
    info.PhaseEndTick.[0] <- Environment.TickCount - clockOffset
    info 


let scaleImage (info:ImageInfo) =
    let startTick = Environment.TickCount
    use orig = info.Image
    let scale = 400
    let isLandscape = (orig.Width > orig.Height)
    let newWidth = if isLandscape then scale else scale * orig.Width / orig.Height
    let newHeight = if not isLandscape then scale else scale * orig.Height / orig.Width
    let bitmap = new Bitmap(orig, newWidth, newHeight)
    let bitmap = bitmap.AddBorder(15)
    bitmap.Tag <- orig.Tag
    let info = { info with Image = bitmap }
    info.PhaseStartTick.[1] <- startTick - info.ClockOffset
    info.PhaseEndTick.[1] <- Environment.TickCount - info.ClockOffset
    info

let filterImage (info:ImageInfo) =
    let startTick = Environment.TickCount
    use sc = info.Image
    let bitmap = sc.AddNoise()
    bitmap.Tag <- sc.Tag
    let info = { info with Image = bitmap }
    info.PhaseStartTick.[2] <- startTick - info.ClockOffset 
    info.PhaseEndTick.[2] <- Environment.TickCount - info.ClockOffset
    info

let displayImage (info:ImageInfo) count displayFn duration =
    let startTick = Environment.TickCount
    let info = { info with ImageCount = count }
    info.PhaseStartTick.[3] <- startTick - info.ClockOffset
    info.PhaseEndTick.[3] <- 
      if  duration > 0 then startTick - info.ClockOffset + duration
      else Environment.TickCount - info.ClockOffset
    displayFn info

// ------------------------------------------------------------------------------
// A version using F# Mailbox Processor
// ------------------------------------------------------------------------------

let runMessagePassing displayFn (cts:CancellationTokenSource) = 

  let queueLength = 8

  let loadedImages = new BlockingQueueAgent<_>(queueLength)
  let scaledImages = new BlockingQueueAgent<_>(queueLength)    
  let filteredImages = new BlockingQueueAgent<_>(queueLength)    



  // [PHASE 1] Load images from disk and put them a queue.
  let loadImages = async {
    let clockOffset = Environment.TickCount
    let count = ref 0
    while true do 
      let images = 
        Directory.GetFiles(Application.StartupPath, "*.jpg") 
      for img in images do
        let info = loadImage img (!count) clockOffset
        incr count 
        do! loadedImages.AsyncAdd(info) }

  // [PHASE 2] Scale to thumbnail size and render picture frame.
  let scalePipelinedImages = async {
    while true do 
      let! info = loadedImages.AsyncGet()
      let info = scaleImage info
      do! scaledImages.AsyncAdd(info) }

  // [PHASE 3] Give images a speckled appearance by adding noise
  let filterPipelinedImages = async {
    while true do 
      let! info = scaledImages.AsyncGet()
      let info = filterImage info
      do! filteredImages.AsyncAdd(info) }

  // [PHASE 4]: Invoke the function (display the result in a UI)
  let displayPipelinedImages = 
    let rec loop count duration = async {
      let! info = filteredImages.AsyncGet()
      let displayStart = Environment.TickCount
      let info = 
        { info with 
            QueueCount1 = loadedImages.Count
            QueueCount2 = scaledImages.Count
            QueueCount3 = filteredImages.Count }
      displayImage info (count + 1) displayFn duration
      let time = (Environment.TickCount - displayStart)
      return! loop (count + 1) time }
    loop 0 0


  Async.Start(loadImages, cts.Token)
  Async.Start(scalePipelinedImages, cts.Token)
  Async.Start(filterPipelinedImages, cts.Token)
  try Async.RunSynchronously
        (displayPipelinedImages, cancellationToken = cts.Token)
  with :? OperationCanceledException -> () 
