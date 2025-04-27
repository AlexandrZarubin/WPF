using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ListView
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			if (lvEntries.Items.Contains(txtEntry.Text))
			{
				return;
			}
			lvEntries.Items.Add(txtEntry.Text);
		}

		private void btnClear_Click(object sender, RoutedEventArgs e)
		{
			lvEntries.Items.Clear();
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (lvEntries.SelectedIndex==-1)
			{
				return;
			}
			lvEntries.Items.RemoveAt(lvEntries.SelectedIndex);
		}
	}
}
