using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using AddapterSMEVClient.Annotations;
using AddapterSMEVClient.Class;
using SMEV.WCFContract;

namespace AddapterSMEVClient
{
    /// <summary>
    /// Логика взаимодействия для StatusHistory.xaml
    /// </summary>
    public partial class StatusHistory : Window
    {
        public StatusHistoryVM VM { get; set; } = new StatusHistoryVM(MainWindow.wcf, Dispatcher.CurrentDispatcher);
        public StatusHistory(int ID)
        {
            InitializeComponent();
            VM.ID = ID;
            VM.StatusRefreshCommand.Execute(null);
        }
    }

    public class StatusHistoryVM : INotifyPropertyChanged
    {
        private IWcfInterface wcf;
        private Dispatcher dispatcher;

        public StatusHistoryVM(IWcfInterface wcf, Dispatcher dispatcher)
        {
            this.wcf = wcf;
            this.dispatcher = dispatcher;
        }
        public int ID { get; set; }
        public ObservableCollection<STATUS_OUT> ListSTATUS_OUT { get; set; } = new ObservableCollection<STATUS_OUT>();

        public ICommand StatusRefreshCommand => new Command(obj =>
        {
            Task.Run(() =>
            {
                var list = wcf.GetStatusOut(ID);
                dispatcher.Invoke(() =>
                {
                    ListSTATUS_OUT.Clear();
                    foreach (var statusOut in list)
                    {
                        ListSTATUS_OUT.Add(statusOut);
                    }
                });
            });
        });
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
