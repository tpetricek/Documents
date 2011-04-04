namespace SemaphoreLight

open Utils
open System
open System.Windows 
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Controls

type Semaphore() as this =
  inherit UserControl()
  // ----------------------------------------------------------------
  // Initialize application & find known XAML elements

  let uri = new System.Uri("/SemaphoreLight;component/Semaphore.xaml", UriKind.Relative)
  do Application.LoadComponent(this, uri)

  let red : Ellipse = this?Red
  let green : Ellipse = this?Green
  let orange : Ellipse = this?Orange

  // ----------------------------------------------------------------
  // Switching semaphore states

  let display (current:Ellipse) =
    green.Visibility <- Visibility.Collapsed
    red.Visibility <- Visibility.Collapsed
    orange.Visibility <- Visibility.Collapsed
    current.Visibility <- Visibility.Visible 

  let semaphoreStates2() = async {
    while true do
      let! md = Async.AwaitObservable(this.MouseLeftButtonDown)
      display(green) 
      let! md = Async.AwaitObservable(this.MouseLeftButtonDown)
      display(orange) 
      let! md = Async.AwaitObservable(this.MouseLeftButtonDown)
      display(red)  
    }
  
  do
    semaphoreStates2() |> Async.StartImmediate

