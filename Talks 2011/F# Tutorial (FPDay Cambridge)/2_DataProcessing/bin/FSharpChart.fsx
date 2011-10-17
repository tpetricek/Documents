#r "bin\\MSDN.FSharpChart.dll"
open System
open System.Windows.Forms
open MSDN.FSharp.Charting
open MSDN.FSharp.Charting.ChartTypes

#if INTERACTIVE
module InstallFsiAutoDisplay =
  fsi.AddPrinter(fun (ch:GenericChart) -> 
    let frm = new Form(Visible = true, TopMost = true, Width = 700, Height = 500)
    let ctl = new ChartControl(ch, Dock = DockStyle.Fill)
    frm.Text <- ChartFormUtilities.ProvideTitle ch
    frm.Controls.Add(ctl)
    frm.Show()
    ctl.Focus() |> ignore
    "(Chart)")
#endif
