using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace SmileyGame
{
  public partial class Form1 : Form
  {
    #region Wait for mouse click

    Task<MouseEventArgs> AwaitClick()
    {
      var evt = new ManualResetEvent(false);
      var tcs = new TaskCompletionSource<MouseEventArgs>();
      MouseEventHandler dlg = null;
      dlg = new MouseEventHandler((e, a) =>
       {
         tcs.SetResult(a);
         smileyBox.MouseClick -= dlg;
       });
      smileyBox.MouseClick += dlg;
      return tcs.Task;
    }

    #endregion

		// Asynchronous method that represents a part of the 
		// behavior that is responsible for moving the smiley
		// face to a new location every 800ms
    async Task MoveSmiley()
    {
      var rnd = new Random();
      while (true)
      {
        await TaskEx.Delay(800);
        smileyBox.Location = new Point
          (rnd.Next(this.ClientSize.Width - smileyBox.Width),
           rnd.Next(this.ClientSize.Height - smileyBox.Height - 20));
      }
    }

		// Asynchronous method that implements counting of 
		// clicks - the body of a loop waits for a click using
		// an asynchronous 'AwaitClick' method.
    async Task CountClicks()
    {
      for(int count = 0; true; count++)
      {
				// TODO: Note that 'AwaitClick' returns 'MouseEventArgs'
				// of the event - you can use this to check if the click
				// was actually inside the face circle.
        await AwaitClick();
        scoreLabel.Text = String.Format("Score: {0}", count);
      }      
    }

    public Form1()
    {
      InitializeComponent();
      CountClicks();
      MoveSmiley();
    }
  }
}
