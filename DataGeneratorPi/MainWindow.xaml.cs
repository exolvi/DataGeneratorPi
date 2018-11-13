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
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Search;
using OSIsoft.AF.Time;

namespace DataGeneratorPi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PISystems systems = new PISystems();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServers();
        }

        private void LoadServers()
        {
            cmbServers.ItemsSource = systems;
            if (systems.Count == 1)
            {
                cmbServers.SelectedIndex = 0;
            }
        }

        private void cmbServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var system = (PISystem)e.AddedItems[0];
                cmbDatabases.ItemsSource = system.Databases;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDatabases.SelectedItem is AFDatabase db)
            {
                var query = @"Element:{ Name:'*' } " + $"{txtAttributeSearch.Text}*";
                using (var search = new AFAttributeSearch(db, "AttrSearch", query))
                {
                    search.CacheTimeout = TimeSpan.FromMinutes(10);

                    var attributes = search.FindAttributes();
                    dgSearchResults.Items.Clear();
                    var res = from a in attributes
                              where a.DataReference != null
                              select new SearchResult
                              {
                                  ElementName = a.Element.Name,
                                  AttributeName = a.Name,
                                  DataType = a.Type.Name,
                                  _self = a
                              };

                    dgSearchResults.ItemsSource = res.ToList();
                }
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (dgSearchResults.SelectedIndex >= 0 &&
                dtStart.Value.HasValue &&
                dtEnd.Value.HasValue)
            {
                var attr = (SearchResult)dgSearchResults.SelectedItem;
                var dtIter = dtStart.Value.Value;
                var values = new AFValues();
                var r = new Random(DateTime.Now.Millisecond);
                while (dtIter <= dtEnd.Value)
                {
                    var val = (r.NextDouble() * (udMax.Value.Value - udMin.Value.Value)) + udMin.Value.Value;

                    values.Add(new AFValue(val, new AFTime(dtIter)));

                    if ((string)cmbFrequencyUnit.SelectionBoxItem == "Minutes")
                    {
                        dtIter = dtIter.AddMinutes(udFrequency.Value.Value);
                    }
                    else if ((string)cmbFrequencyUnit.SelectionBoxItem == "Hours")
                    {
                        dtIter = dtIter.AddHours(udFrequency.Value.Value);
                    }
                    else if ((string)cmbFrequencyUnit.SelectionBoxItem == "Seconds")
                    {
                        dtIter = dtIter.AddSeconds(udFrequency.Value.Value);
                    }
                }

                attr._self.Data.UpdateValues(values, OSIsoft.AF.Data.AFUpdateOption.Replace);
            }
        }
    }
}
