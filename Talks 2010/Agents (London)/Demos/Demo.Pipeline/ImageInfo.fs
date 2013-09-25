namespace Demo.ImagePipeline

open System
open System.Diagnostics.CodeAnalysis
open System.Drawing

type ImageInfo = 
  { SequenceNumber : int
    FileName : string
    Image : Bitmap
    ClockOffset : int
    PhaseStartTick : int[]
    PhaseEndTick : int [] 
    QueueCount1 : int
    QueueCount2 : int
    QueueCount3 : int
    ImageCount : int
    FramesPerSecond : int }
  static member Create(sequenceNumber:int, fileName:string, originalImage:Bitmap, clockOffset:int) =
    { SequenceNumber = sequenceNumber
      FileName = fileName
      QueueCount1 = 0
      QueueCount2 = 0
      QueueCount3 = 0
      ImageCount = 0
      FramesPerSecond = 0
      Image = originalImage
      ClockOffset = clockOffset
      PhaseStartTick = Array.create 4 0
      PhaseEndTick = Array.create 4 0 }
  interface IDisposable with
    member x.Dispose() =
      if x.Image <> null then x.Image.Dispose()
      GC.SuppressFinalize(x)

