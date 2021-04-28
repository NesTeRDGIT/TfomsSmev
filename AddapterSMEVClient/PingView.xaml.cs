using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using SMEV.WCFContract;

namespace AddapterSMEVClient
{
    /// <summary>
    /// Логика взаимодействия для PingView.xaml
    /// </summary>
    public partial class PingView : Window
    {
        public BindingList<PingResult> Pings { get; set; } = new BindingList<PingResult>();
        public PingView()
        {
            InitializeComponent();
        }


        public void AddPingResult(PingResult r)
        {
            this.Dispatcher.Invoke(() => {  Pings.Insert(0, r); });
        }
    }
}
