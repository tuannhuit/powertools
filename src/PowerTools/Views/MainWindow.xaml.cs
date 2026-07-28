using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
        }, new List<Criteria>
        {
            new Criteria("Column1", "Column 1", true, (s) =>
            {

            })
        });

        public MainWindow()
        {
            InitializeComponent();
            ApplicationService.Instance.MainWindow = this;
            ApplicationService.Instance.DialogView = ContentControlDialogView;
        }

        public static ObservableCollection<Criteria> ColumnFilters = new ObservableCollection<Criteria>
            {
                new RangeCriteria("UTCTimestamp", "yyyy-MM-dd HH:mm:ss.fffZ", "yyyy-MM-dd HH:mm:ss.fffZ", (s) =>
                {
                    if (s.Reason != ValueChangedEvent.KeyEnter && !string.IsNullOrEmpty((string)s.ValueChanged))
                    {
                        return;
                    }

                    if (ColumnFilters == null)
                    {
                        return;
                    }

                    var criteria = (RangeCriteria) ColumnFilters.First(p=>p.Name == "UTCTimestamp");
                     
                }), 
                new Criteria("CustomerId", string.Empty, (s) =>
                {
                    if (s.Reason != ValueChangedEvent.KeyEnter && !string.IsNullOrEmpty((string)s.ValueChanged))
                    {
                        return;
                    }
                     
                }),
                new Criteria("DocumentId", string.Empty, (s) =>
                {
                    if (s.Reason != ValueChangedEvent.KeyEnter && !string.IsNullOrEmpty((string)s.ValueChanged))
                    {
                        return;
                    } 
                })
            };

        public static ObservableCollection<CustomAction> CustomActions = new ObservableCollection<CustomAction>
        {
            new CustomAction
            {
                Icon = ButtonIcons.Setting,
                Name = "Settings"
            },
            new CustomAction
            {
                Icon = ButtonIcons.Add,
                Name = "Add"
            }
        };

        public static ICommand TestButtonCommand { get; set; } = new DelegateCommand(OnShowDialog);

        private static void OnShowDialog()
        {
            var dialogActions = new List<CustomAction>
            {
                new CustomAction
                {
                    Icon = ButtonIcons.Add,
                    Name = "Add"
                }
            };
        }

        public static ObservableCollection<Criteria> ColumnSettings = new ObservableCollection<Criteria>
            {
                new Criteria("UTCTimestamp", "UTC Timestamp", true),
                new Criteria("LocalTimestamp", "Local Timestamp", false),
                new Criteria("QueueName", "Queue Name", true),
                new Criteria("CustomerId", "Customer Id", true),
                new Criteria("DocumentId", "Document Id", false),
                new Criteria("Status", "Status", true),
                new Criteria("ReasonId", "Reason Id", false),
                new Criteria("EventId", "Event Id", false),
                new Criteria("Pipeline", "Pipeline", false),
                new Criteria("InsertedDateTime", "Inserted DateTime", true),
                new Criteria("StartedDateTime", "Started DateTime", true),
                new Criteria("CompletedDateTime", "Completed DateTime", true),
                new Criteria("TimeTakenToProcess", "Time Taken To Process", false),
                new Criteria("TimeTakenToProcessSecond", "Time Taken To Process (s)", false),
                new Criteria("TimeInQueue", "Time In Queue", false),
                new Criteria("TimeInQueueSecond", "Time In Queue (s)", false),
                new Criteria("Partition", "Partition", false),
                new Criteria("Offset", "Offset", false),
                new Criteria("PartitionOffset", "Partition Offset", false),
                new Criteria("ThreadId", "Thread Id", false),
                new Criteria("ServerId", "Server Id", false),
                new Criteria("SourceMachineName", "Source Machine Name", false)

            };
    }
}
