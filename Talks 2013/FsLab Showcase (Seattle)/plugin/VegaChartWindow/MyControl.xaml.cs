using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DualNotion.VegaChartWindow
{
	/// <summary>
	/// Interaction logic for MyControl.xaml
	/// </summary>
	public partial class MyControl : UserControl
	{
		public MyControl()
		{
			InitializeComponent();
			LoadChart();
		}

		async void LoadChart()
		{
			while (true)
			{
				var wc = new WebClient();
				await Task.Delay(500);
				try
				{
					var s = await wc.DownloadStringTaskAsync("http://localhost:8081");
					this.Dispatcher.Invoke(() => browser.Navigate("http://localhost:8081"));
					return;
				}
				catch (Exception ex)
				{
				}
			}
		}
	}
}