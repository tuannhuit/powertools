using System.Collections.Generic;
using System.Windows;
using PowerTools.Core.Models;

namespace PowerTools.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static DataGridCriteriaList CriteriaList = new DataGridCriteriaList(new List<Criteria>
        {
            new Criteria("Column1", "Column ABC", (s) =>
            {

            }),
            new RangeCriteria("Column2", "Column ABC", "Column ABC 2", (s) =>
            {

            }),
        },new List<Criteria>
        {
            new Criteria("Column1", "Column 1", true, (s) =>
            {

            })
        });

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
