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
    /// Логика взаимодействия для ViewFeedBack.xaml
    /// </summary>
    public partial class ViewFeedBack : Window
    {
        public static IWcfInterface wcf => MainWindow.wcf;
        FeedBackData _Data = new FeedBackData(new List<FeedBackDataIN>());

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }


        public FeedBackData Data { get { return _Data; } set { _Data = value; RaisePropertyChanged("Data"); } }

        private CollectionViewSource DataViewSource;

        public ViewFeedBack(int ID, string OrderId, string ApplicationId)
        {
            InitializeComponent();
            textBoxApplicationId.Text = ApplicationId;
            textBoxOrderId.Text = OrderId;
            DataViewSource = (CollectionViewSource) FindResource("DataViewSource");
            try
            {
                Data = wcf.GetFeedBackData(ID);
                DataViewSource.Source = Data.IN;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
