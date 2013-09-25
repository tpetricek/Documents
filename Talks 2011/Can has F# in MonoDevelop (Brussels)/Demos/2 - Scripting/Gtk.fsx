#load "/home/tomas/Programs/fsharp2/bin/load-gtk.fsx"
open Gtk

let win = new Window("Hello from F#!", Visible=true)
let btn = new Button("Click here!", Visible=true)
win.Add(btn)

btn.Clicked.Add(fun e -> printfn "Clicked!")
