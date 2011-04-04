using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ColorSelector.App
{
  public partial class MainPage : UserControl
  {
    public MainPage()
    {
      InitializeComponent();

      clrSelect.ColorChanged += (sender, color) => 
        mainCanvas.Background = new SolidColorBrush(color);
    }
  }
}
