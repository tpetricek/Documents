module FunctionalDrawing

open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

type DrawingContext = 
  { Graphics : Graphics
    Brush : Brush
    Pen : Pen 
    Font : Font }

type Drawing = 
  | Draw of (DrawingContext -> float32 * float32 * float32 * float32)

type MainForm(drawing) = 
  inherit Form(TopMost = true)
  let mutable drawing = drawing
  member x.Drawing 
    with get() = drawing 
    and set(v) = 
      drawing <- v
      x.Refresh()
  override x.OnResize(e) = x.Invalidate()
  override x.OnMouseDown(e) = 
    if e.Button = MouseButtons.Left then 
      x.Close()
    else
      let dlg = new SaveFileDialog(Filter = "PNG files (*.png)|*.png")
      if dlg.ShowDialog() = DialogResult.OK then
        use bmp = new Bitmap(x.ClientSize.Width, x.ClientSize.Height)
        use gr = Graphics.FromImage(bmp)
        x.OnPaint(new PaintEventArgs(gr, Rectangle.Empty))
        bmp.Save(dlg.FileName)

  override x.OnPaint(e) =
    e.Graphics.Clear(Color.White)
    let m = new Matrix(1.0f, 0.0f, 0.0f, -1.0f, float32 (x.ClientSize.Width/2), float32 (x.ClientSize.Height/2))
    e.Graphics.MultiplyTransform(m)
    let (Draw f) = drawing
    use pen = new Pen(Color.Transparent, 0.0f)
    use fnt = new Font("Tahoma", 13.0f)
    f { Graphics = e.Graphics; Brush = Brushes.Black; Pen = pen; Font = fnt } |> ignore

let ($) (Draw f1) (Draw f2) = 
  Draw(fun g -> 
    let x1, y1, w1, h1 = f1 g
    let x2, y2, w2, h2 = f2 g
    let x = min x1 x2
    let y = min y1 y2 
    let w = (max (x1 + w1) (x2 + w2)) - x
    let h = (max (y1 + h1) (y2 + h2)) - y
    x, y, w, h )

module Fun =
  let space offs (Draw f) = 
    Draw(fun args ->
      let x, y, w, h = f args
      x - offs, y - offs, w + 2.0f*offs, h + 2.0f*offs) 

  let text s = 
    Draw(fun { Graphics = g; Font = fnt; Brush = br } ->
      let m = new Matrix(1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f)
      g.MultiplyTransform(m)
      let size = g.MeasureString(s, fnt)
      printfn "%f %f" size.Width size.Height
      let m = new Matrix(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f)
      g.MultiplyTransform(m)
      g.DrawString(s, fnt, br, RectangleF(-size.Width/2.f, -size.Height/2.f, size.Width, size.Height))
      let m = new Matrix(1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f)
      g.MultiplyTransform(m) 
      -size.Width/2.f, -size.Height/2.f, size.Width, size.Height )

  let rectangle w h = 
    Draw(fun { Graphics = g; Pen = pn; Brush = br } -> 
      g.FillRectangle(br, -w/2.0f, -h/2.0f, w, h) 
      g.DrawRectangle(pn, -w/2.0f, -h/2.0f, w, h)
      -w/2.0f, -h/2.0f, w, h)

  let lineto x y =
    Draw(fun { Graphics = g; Pen = pn; Brush = br } ->
      g.DrawLine(pn, 0.0f, 0.0f, x, y)
      0.0f, 0.0f, x, y )

  let empty =
    Draw(fun _ -> 0.0f, 0.0f, 0.0f, 0.0f)

  let circle r =
    Draw(fun { Graphics = g; Pen = pn; Brush = br } -> 
      g.FillEllipse(br, -r/2.0f, -r/2.0f, r, r) 
      g.DrawEllipse(pn, -r/2.0f, -r/2.0f, r, r) 
      -r/2.0f, -r/2.0f, r, r )

  let ellipse w h =
    Draw(fun { Graphics = g; Pen = pn; Brush = br } -> 
      g.FillEllipse(br, -w/2.0f, -h/2.0f, w, h) 
      g.DrawEllipse(pn, -w/2.0f, -h/2.0f, w, h) 
      -w/2.0f, -h/2.0f, w, h )

  let fillColor clr (Draw f) = 
    Draw(fun ctx -> 
      use br = new SolidBrush(clr)
      f { ctx with Brush = br })

  let font (name:string) size (Draw f) = 
    Draw(fun ctx -> 
      use font = new Font(name, size)
      f { ctx with Font = font })

  let lineStyle w (clr:Color) (Draw f) = 
    Draw(fun ctx -> 
      use pen = new Pen(clr, w)
      f { ctx with Pen = pen })

  let info (Draw f) = 
    Draw(fun args -> 
      let x, y, w, h = f args
      printfn "[%f, %f] [%f, %f]" x y w h
      x, y, w, h) 

  let move dx dy (Draw f) = 
    Draw(fun ({ Graphics = g; Pen = pn; Brush = br } as ctx) -> 
      g.TranslateTransform(dx, dy)
      let x, y, w, h = f ctx
      g.TranslateTransform(-dx, -dy)
      x + dx, y + dy, w, h )

  let line x1 y1 x2 y2 = 
    lineto (x2 - x1) (y2 - y1) |> move x1 y1

  let scale sx sy (Draw f) = 
    Draw(fun ({ Graphics = g; Pen = pn; Brush = br } as ctx) -> 
      g.ScaleTransform(sx, sy)
      let x, y, w, h = f ctx
      g.ScaleTransform(1.0f/sx, 1.0f/sy)
      x * sx, y * sy, w * sx, h * sy)

  let private rotatePoint a = 
    let a = a * System.Math.PI / 180.0
    let cosa, sina = cos a, sin a
    fun (x, y) -> (x * cosa - y * sina, x * sina + y * cosa)

  let rotate angle (Draw f) = 
    Draw(fun ({ Graphics = g; Pen = pn; Brush = br } as ctx) -> 
      g.RotateTransform(angle)
      let x, y, w, h = f ctx
      g.RotateTransform(-angle)
      let x, y, w, h = float x, float y, float w, float h
      let points = 
        [ (x, y); (x + w, y + h); (x, y + h); (x + w, y) ]
        |> List.map (rotatePoint (float angle))
      let x1, y1 = (points |> List.map fst |> List.min), (points |> List.map snd |> List.min)
      let x2, y2 = (points |> List.map fst |> List.max), (points |> List.map snd |> List.max)
      float32 x1, float32 y1, float32 (x2 - x1), float32 (y2 - y1) )

  let private showOn (mf:MainForm) d =
    use bmp = new Bitmap(10, 10)
    use gr = Graphics.FromImage(bmp)
    let (Draw f) = d
    let x, y, w, h = f { Graphics = gr; Pen = Pens.Aqua; Brush = Brushes.Beige; Font = SystemFonts.CaptionFont }
    mf.Drawing <- d |> move (-x - w/2.0f) (-y - h/2.0f)
    mf.ClientSize <- Size(int (w*1.5f), int (h*1.5f))
    mf.Show()
    mf.Focus() |> ignore
    (int w, int h)

  let show title d = 
    let mf = new MainForm(empty, Text=title)
    showOn mf d |> ignore

  let run title d = 
    let mf = new MainForm(empty, Text=title)
    showOn mf d |> ignore
    Application.Run(mf)

  #if INTERACTIVE
  let getForm = 
    let createLazy() = lazy new MainForm(empty)
    let current = ref (createLazy())
    ( fun () -> 
        if (!current).Value.IsDisposed then current := createLazy()
        (!current).Value )

  do fsi.AddPrinter(fun d ->
    let w, h = showOn (getForm()) d
    sprintf "(Drawing %dx%d)" w h )
  #endif

let ( <||> ) ((Draw f1) as d1) ((Draw f2) as d2) =
  Draw(fun ({ Graphics = g; Pen = pn; Brush = br } as ctx) -> 
    use bmp = new Bitmap(10, 10)
    use gr = Graphics.FromImage(bmp)
    let x1, y1, w1, h1 = f1 ctx
    let x2, y2, w2, h2 = f2 ctx
    let (Draw f) = d1 $ (Fun.move w1 0.0f d2)
    f ctx )

let ( <--> ) ((Draw f1) as d1) ((Draw f2) as d2) =
  Draw(fun ({ Graphics = g; Pen = pn; Brush = br } as ctx) -> 
    use bmp = new Bitmap(10, 10)
    use gr = Graphics.FromImage(bmp)
    let x1, y1, w1, h1 = f1 ctx
    let x2, y2, w2, h2 = f2 ctx
    let (Draw f) = d1 $ (Fun.move 0.0f h1 d2)
    let x, y, w, h = f ctx 
    x, y, w, h )
