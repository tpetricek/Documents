// --------------------------------------------------------------------------------------
// Composable functional 3D graphics library for education
// --------------------------------------------------------------------------------------
// (c) Tomas Petricek (tomas@tomasp.net)
// Distributed under the open-source MS-PL license
// --------------------------------------------------------------------------------------

module Functional3D

open System
open System.Drawing
open System.Windows.Forms
open System.Collections.Generic

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

// --------------------------------------------------------------------------------------
// Representing 3D objects
// --------------------------------------------------------------------------------------

/// Represents the context of the drawing (mainly color at the moment)    
type Drawing3DContext = 
  { Color : Color4 }

/// 3D object is represented as a function that draws it
type Drawing3D = DF of (Drawing3DContext -> unit)

// --------------------------------------------------------------------------------------
// Drawing form used to display the OpenGL content
// (supports rotations and zooming, works in F# interactive)
// --------------------------------------------------------------------------------------
 
type DrawingForm(?drawing:Drawing3D) as x =
  inherit Form(ClientSize=Size(800, 600), Text="Functional 3D Drawing") 

  let mutable drawing = defaultArg drawing (DF ignore)
  let mutable lighting = (fun () ->
    GL.Light(LightName.Light0, LightParameter.Ambient, [| 0.2f; 0.2f; 0.2f; 1.0f |])
    GL.Light(LightName.Light0, LightParameter.Diffuse, [| 1.0f; 1.0f; 1.0f; 1.0f |])
    GL.Light(LightName.Light0, LightParameter.Specular, [| 1.0f; 1.0f; 1.0f; 1.0f |])
    GL.Light(LightName.Light0, LightParameter.Position, [| 1.0f; 1.0f; 1.0f; 0.0f |])
    GL.Enable(EnableCap.Light0)
    GL.Enable(EnableCap.Lighting) )

  // ----------------------------------------------------------------------------------

  let mutable cameraDistance = -10.0
  let mutable currentAngles = [| 0.0; 0.0; 0.0 |]
  let mutable currentSpeeds = [| 0.0; 0.0; 0.0 |]
  let loaded = ref false

  // ----------------------------------------------------------------------------------

  let glControl = new GLControl(Dock = DockStyle.Fill)

  let redrawWindow() = 
    GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
    GL.MatrixMode(MatrixMode.Modelview)
    
    GL.LoadIdentity()
    GL.Enable(EnableCap.Normalize) // scaling issue
        
    lighting()
    GL.Translate(0., 0., cameraDistance)
    GL.Rotate(30., 1., 0., 0.)
        
    GL.Rotate(currentAngles.[0], 1.0, 0.0, 0.0)
    GL.Rotate(currentAngles.[1], 0.0, 1.0, 0.0)
    GL.Rotate(currentAngles.[2], 0.0, 0.0, 1.0)

    let clr = Color.DarkOliveGreen
    let conv n = float32 n / 255.0f
    let ctx = { Color = Color4(conv clr.R, conv clr.G, conv clr.B, conv clr.A) }
    let (DF f) = drawing

    GL.ShadeModel(ShadingModel.Smooth)
    f(ctx)

    glControl.SwapBuffers()
  
  let setupViewPort() = 
    let w, h = glControl.ClientSize.Width, glControl.ClientSize.Height
    GL.Viewport(0, 0, w, h)
    let ratio = float32 w / float32 h
    let mutable persp = Matrix4.CreatePerspectiveFieldOfView(float32 Math.PI / 4.0f, ratio, 1.0f, 64.0f)
    GL.MatrixMode(MatrixMode.Projection)
    GL.LoadMatrix(&persp)
    
  // ----------------------------------------------------------------------------------
  // Interaction and event handling - repeatedly refresh the form
  // and implement zooming & rotation using win forms events
  
  do 
    let rec timer() = async { 
      do! Async.Sleep(10)
      x.Invoke(Action(fun () ->
        for i in 0 .. 2 do 
          currentAngles.[i] <- currentAngles.[i] + currentSpeeds.[i] 
        x.Refresh() )) |> ignore
      return! timer() }
      
    x.Controls.Add(glControl)
    x.Load.Add(fun _ -> 
      loaded := true
      GL.ClearColor(Color.FromArgb(220, 225, 205))  
      GL.Enable(EnableCap.DepthTest)
      timer() |> Async.Start
      x.Resize.Add(fun _ -> setupViewPort())
      setupViewPort()  )
    
    glControl.KeyPress 
      |> Event.add (fun ke ->
          match ke.KeyChar with
          | '-' | '_' -> x.CameraDistance <- x.CameraDistance - 1.0
          | '+' | '=' -> x.CameraDistance <- x.CameraDistance + 1.0 
          | _ -> () )

    glControl.KeyPress 
      |> Event.choose (fun ke ->
          match ke.KeyChar with
          | 'q' | 'Q' -> Some(0, -0.1)
          | 'w' | 'W' -> Some(0, 0.1)
          | 'a' | 'A' -> Some(1, -0.1)
          | 's' | 'S' -> Some(1, 0.1)
          | 'z' | 'Z' -> Some(2, -0.1)
          | 'x' | 'X' -> Some(2, 0.1)
          | _ -> None )
      |> Event.add (fun (idx, ofs) ->
          currentSpeeds.[idx] <- currentSpeeds.[idx] + ofs )
          
    glControl.Paint.Add(fun _ ->
      if !loaded then  redrawWindow() )

  // ----------------------------------------------------------------------------------
  // Properties used to set displayed object & view properties
    
  member x.Drawing 
    with get() = drawing 
    and set(v) = 
      drawing <- v
      glControl.Refresh()
      
  member x.Lighting 
    with set(v) = 
      lighting <- v
      glControl.Refresh()
      
  member x.CameraDistance 
    with get() = cameraDistance 
    and set(v) = 
      cameraDistance <- v
      glControl.Refresh()
      
  member x.ResetRotation() = 
    currentAngles <- [| 0.0; 0.0; 0.0 |]
    currentSpeeds <- [| 0.0; 0.0; 0.0 |]
    glControl.Refresh()

// --------------------------------------------------------------------------------------
// Helper functions and extension methods
// --------------------------------------------------------------------------------------

module Array3D = 
  /// Creates a 3D array from cube (represented as nested arrays)
  /// The resulting array has indices from -x/2 to x/2
  let fromCube (data:int[][][]) =
    let length = Seq.length data
    let b = -length/2;
    let res = Array.CreateInstance(typeof<int>, [| length; length; length |], [| b; b; b |])
    data |> Seq.iteri (fun x data -> 
      data |> Seq.iteri (fun y data -> 
        data |> Seq.iteri (fun z v -> 
          res.SetValue(v, [| x+b; y+b; z+b |]) )))
    (res :?> int[,,])

type GLEx = 
  /// Add multiple vertices to GL
  static member Vertices vertices = 
    for (x:float32), y, z in vertices do
      GL.Vertex3(x, y, z)

  /// Add mesh to the GL and set the specified normal vector first
  static member Face (x:float32, y, z) vertices = 
    GL.Normal3(x, y, z)
    GLEx.Vertices vertices
       
// --------------------------------------------------------------------------------------
// Representing and constructing 3D objects
// --------------------------------------------------------------------------------------

/// Composes two 3D objects by drawing both of them
let ($) (DF a) (DF b) = DF (fun ctx -> 
  a(ctx)
  b(ctx) )

module Fun = 
  /// A constant that specifies the number of triangles in sphere or a cylinder
  let mutable quality = 40

  // ------------------------------------------------------------------------------------
  // Operations for composing and modifying 3D objects

  /// Scale the specified 3D object by the specified scales along the 3 axes
  let scale (x:float, y, z) (DF f) = DF (fun ctx ->
    GL.Scale(x, y, z)
    f(ctx)
    GL.Scale(1.0/x, 1.0/y, 1.0/z) )
  
  /// Scale the specified 3D object by the specified scales along the 3 axes
  let rotate (x:float, y, z) (DF f) = DF (fun ctx ->
    GL.Rotate(x, 1.0, 0.0, 0.0)
    GL.Rotate(y, 0.0, 1.0, 0.0)
    GL.Rotate(z, 0.0, 0.0, 1.0)
    f(ctx)
    GL.Rotate(-x, 1.0, 0.0, 0.0)
    GL.Rotate(-y, 0.0, 1.0, 0.0)
    GL.Rotate(-z, 0.0, 0.0, 1.0) )
  
  /// Move the specified object by the provided offsets
  let translate (x:float, y:float, z:float) (DF f) = DF (fun ctx ->
    GL.Translate(Vector3(float32 x, float32 y, float32 z))
    f(ctx)
    GL.Translate(Vector3(float32 -x, float32 -y, float32 -z)) )
  
  /// Set color to be used when drawing the specified 3D objects
  let color (clr:Color) (DF f) = DF ( fun ctx -> 
    let conv n = float32 n / 255.0f
    f { ctx with Color = Color4(conv clr.R, conv clr.G, conv clr.B, conv clr.A) })


  // ------------------------------------------------------------------------------------
  // Primitive 3D objects 

  /// Creates an empty 3D object that doesn't show anything
  let empty = DF ignore

  /// Creates a 3D cube of unit size using the current color
  let cube = DF (fun ctx ->
    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, ctx.Color)
    GL.Begin(BeginMode.Quads)
    GLEx.Face 
      (-1.f, 0.f, 0.f) 
      [ (-0.5f, -0.5f, -0.5f); (-0.5f, -0.5f,  0.5f); 
        (-0.5f,  0.5f,  0.5f);  (-0.5f,  0.5f, -0.5f) ] 
    GLEx.Face 
      ( 1.f, 0.f, 0.f)
      [ ( 0.5f, -0.5f, -0.5f); ( 0.5f, -0.5f,  0.5f);
        ( 0.5f,  0.5f,  0.5f); ( 0.5f,  0.5f, -0.5f) ]
    GLEx.Face 
      (0.f, -1.f, 0.f)
      [ (-0.5f, -0.5f, -0.5f); (-0.5f, -0.5f,  0.5f);
        ( 0.5f, -0.5f,  0.5f); ( 0.5f, -0.5f, -0.5f) ]
    GLEx.Face 
      (0.f, 1.f, 0.f)
      [ (-0.5f,  0.5f, -0.5f); (-0.5f,  0.5f,  0.5f);
        ( 0.5f,  0.5f,  0.5f); ( 0.5f,  0.5f, -0.5f) ]
    GLEx.Face 
      (0.f, 0.f, -1.f)
      [ (-0.5f, -0.5f, -0.5f); (-0.5f,  0.5f, -0.5f);
        ( 0.5f,  0.5f, -0.5f); ( 0.5f, -0.5f, -0.5f) ]
    GLEx.Face 
      (0.f, 0.f, 1.f)
      [ (-0.5f, -0.5f,  0.5f); (-0.5f,  0.5f,  0.5f);
        ( 0.5f,  0.5f,  0.5f); ( 0.5f, -0.5f,  0.5f) ]
    GL.End() )
  
  
  /// Generates a 3D cylinder object of a unit size
  let cylinder = DF (fun ctx ->
    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, ctx.Color)
    GL.Begin(BeginMode.Triangles)
    
    // points that will be used for generating the circle
    let q = float32 (Math.PI / (float quality / 2.0))
    let circlePoints = 
      [ for i in 0 .. quality -> 
          sin(float32 i * q) * 0.5f, cos(float32 i * q) * 0.5f ]
                   
    // generate 3D points that form the coordinates of the circle
    let borderCirlces = 
      [| for hy in [-0.5f; 0.5f] -> 
           [| for (x, y) in circlePoints -> Vector3(x, y, hy) |] |]
  
    // generate triangles forming the cylinder
    for i in 0 .. quality - 1 do
      // First triangle of the rounded part
      GL.Normal3 (borderCirlces.[0].[i].X, borderCirlces.[0].[i].Y, 0.0f)
      GL.Vertex3  borderCirlces.[0].[i]
      GL.Normal3 (borderCirlces.[0].[i+1].X, borderCirlces.[0].[i+1].Y, 0.0f)
      GL.Vertex3  borderCirlces.[0].[i+1]
      GL.Vertex3  borderCirlces.[1].[i+1]
      
      // Second triangle of the rounded part
      GL.Vertex3  borderCirlces.[1].[i+1]
      GL.Normal3 (borderCirlces.[0].[i].X, borderCirlces.[0].[i].Y, 0.0f)
      GL.Vertex3  borderCirlces.[1].[i]
      GL.Vertex3  borderCirlces.[0].[i]
  
      // Triangle to form the lower side
      GL.Normal3 (0.0, 0.0, -1.0)
      GL.Vertex3 borderCirlces.[0].[i]
      GL.Vertex3 borderCirlces.[0].[i+1]
      GL.Vertex3 (0.0, 0.0, -0.5)
      
      // Triangle to form the upper side
      GL.Normal3 (0.0, 0.0, 1.0)
      GL.Vertex3 borderCirlces.[1].[i+1]
      GL.Vertex3 borderCirlces.[1].[i]
      GL.Vertex3 (0.0, 0.0, 0.5)
    GL.End() )
    
  /// Creates a 3D sphere with unit size
  let sphere = DF (fun ctx ->
    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, ctx.Color)
    GL.Begin(BeginMode.Triangles)
  
    // points that will be used for generating the circle
    let q = float32 (Math.PI / (float quality / 2.0))
    let circlePoints = 
      [ for i in 0 .. quality -> 
          sin(float32 i * q) * 0.5f, cos(float32 i * q) * 0.5f ]
  
    // points from the top to the bottom
    let heightPoints = 
      [ for i in 0 .. quality -> 
          sin(float32 i * q) * 0.5f, cos(float32 i * q) * 0.5f ]
    
    // Array (along one dimension) of circles 
    let points = 
      [| for hx, hy in heightPoints ->
           [| for x, y in circlePoints -> 
                Vector3(x * hx * 2.0f, y * hx * 2.0f, hy) |] |]
  
    /// Generate the sphere 
    for lat in 0 .. quality - 1 do
      for i in 0 .. quality - 1 do
        GL.Normal3 points.[lat].[i]
        GL.Vertex3 points.[lat].[i]
        GL.Normal3 points.[lat].[i+1]
        GL.Vertex3 points.[lat].[i+1]
        GL.Normal3 points.[lat+1].[i+1]
        GL.Vertex3 points.[lat+1].[i+1]
  
        GL.Normal3 points.[lat+1].[i+1]
        GL.Vertex3 points.[lat+1].[i+1]
        GL.Normal3 points.[lat+1].[i]
        GL.Vertex3 points.[lat+1].[i]
        GL.Normal3 points.[lat].[i]
        GL.Vertex3 points.[lat].[i]
    GL.End() )
  
  
  /// Generates a 3D cylinder object of a unit size
  let cone = DF (fun ctx ->
    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, ctx.Color)
    GL.Begin(BeginMode.Triangles)
    
    // points that will be used for generating the circle
    let q = float32 (Math.PI / (float quality / 2.0))
    let circlePoints = 
      [| for i in 0 .. quality -> 
           Vector3(sin(float32 i * q) * 0.5f, cos(float32 i * q) * 0.5f, 0.5f) |]
                   
    // generate triangles forming the cylinder
    for i in 0 .. quality - 1 do
      // First triangle of the rounded part
      GL.Normal3 (circlePoints.[i].X, circlePoints.[i].Y, -0.25f)
      GL.Vertex3 circlePoints.[i]
      GL.Normal3 (circlePoints.[i+1].X, circlePoints.[i+1].Y, -0.25f)
      GL.Vertex3 circlePoints.[i + 1]
      GL.Normal3 (circlePoints.[i].X + circlePoints.[i+1].X / 2.0f, circlePoints.[i].Y + circlePoints.[i+1].Y / 2.0f, -0.25f)
      GL.Vertex3 (0.0, 0.0, -0.5)
  
      /// Triangle to form the lower side
      GL.Normal3 (0.0, 0.0, 1.0)
      GL.Vertex3 circlePoints.[i]
      GL.Vertex3 circlePoints.[i + 1]
      GL.Vertex3 (0.0, 0.0, 0.5)
    GL.End() )
    
  // --------------------------------------------------------------------------------------
  // Provide easy way of displaying 3D objects

  let private createForm() = 
    lazy (new DrawingForm(Visible = false))
    
  let mutable private lazyForm = createForm()
  
  /// Returns the currently displayed form
  let getForm() = lazyForm.Value
  
  /// Gets the distance of camera from the object
  let getDistance() = lazyForm.Value.CameraDistance
  /// Sets the distance of camera from the object
  let setDistance(v) = lazyForm.Value.CameraDistance <- v
  
  /// Resets the rotation properties of the view
  let resetRotation() =
    lazyForm.Value.ResetRotation()
    
  /// Display the specified 3D object on a form
  let show drawing = 
    if lazyForm.Value.IsDisposed then
      lazyForm <- createForm()
    lazyForm.Value.Drawing <- drawing
    lazyForm.Value.Visible <- true

#if INTERACTIVE
do
  fsi.AddPrinter(fun (d:Drawing3D) ->
    Fun.show d
    "(Displayed 3D object)" )
#endif

module FunEx = 
  let init() =  
    use control = new Control()
    let hWnd = control.Handle
    use windowInfo = OpenTK.Platform.Utilities.CreateWindowsWindowInfo( hWnd )
    use context = new GraphicsContext( GraphicsMode.Default, windowInfo )
    context.MakeCurrent( windowInfo )
    context.LoadAll()
