open System
open System.Threading
open System.Windows.Forms
open System.Drawing

// ----------------------------------------------------------------------------
// Create WinForms GUI
    
let frm = new Form(ClientSize=Size(600,500), Visible = true)
let fnt = new Font("Arial", 9.75f, FontStyle.Bold)
let lblScore = 
  new Label
   (Location = Point(0, 0), Height = 25, Dock = DockStyle.Top,
    BackColor = SystemColors.ControlDark, Font = fnt, 
    ForeColor = SystemColors.ControlLight, Text = "Score: 0", 
    TextAlign = ContentAlignment.MiddleCenter)
let picSmiley = 
  new PictureBox
   (SizeMode = PictureBoxSizeMode.AutoSize, Location=Point(75, 75),
    Image = Bitmap.FromFile(__SOURCE_DIRECTORY__ + "\\smiley.png"))

frm.Controls.Add(lblScore)
frm.Controls.Add(picSmiley)


// ----------------------------------------------------------------------------

// Counting of clicks, implemented as a recursive asynchronous function
let rec clickCounter count = async {
  let! e = Async.AwaitEvent(picSmiley.MouseClick)
  // TODO: Use EventArgs 'e' to check if click was inside the face circle
  lblScore.Text <- sprintf "Score: %d" count
  return! clickCounter (count + 1) }

// Implements moving of smiley to a random location every 800ms
let smileyMover = async {
  let rnd = new Random()
  while true do
    let x = rnd.Next(frm.ClientSize.Width - picSmiley.Width)
    let y = 30 + rnd.Next(frm.ClientSize.Height - picSmiley.Height - 30)
    picSmiley.Location <- Point(x, y)
    do! Async.Sleep(800) }

// ----------------------------------------------------------------------------

smileyMover |> Async.StartImmediate
clickCounter 1 |> Async.StartImmediate