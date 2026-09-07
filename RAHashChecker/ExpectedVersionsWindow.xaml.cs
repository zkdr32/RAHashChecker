using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RAHashChecker
{
	/// <summary>
	/// Interaction logic for ExpectedVersionsWindow.xaml
	/// </summary>
	public partial class ExpectedVersionsWindow : Window
	{
		public ObservableCollection<ExpectedVersion> ExpectedVersions { get; }

		public ExpectedVersionsWindow(ObservableCollection<ExpectedVersion> expectedVersions)
		{
			ExpectedVersions = expectedVersions;
			InitializeComponent();
		}

		private void DataGrid_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
			e.Handled = true;
		}
	}
}