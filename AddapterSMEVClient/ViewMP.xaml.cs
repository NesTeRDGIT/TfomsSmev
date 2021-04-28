using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using SMEV.WCFContract;

namespace AddapterSMEVClient
{
    /// <summary>
    /// Логика взаимодействия для ViewMP.xaml
    /// </summary>
    public partial class ViewMP : Window, INotifyPropertyChanged
    {
        public static IWcfInterface wcf => MainWindow.wcf;

        MedpomData _Data = new MedpomData(new MedpomInData(), new List<MedpomOutData>());

        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }

        public MedpomData Data { get{return _Data;}set{ _Data = value; RaisePropertyChanged("Data"); } }
        public ViewMP(int ID)
        {
            InitializeComponent();
            try
            {
                Data = wcf.GetMedpomData(ID);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            
        }
    }
}
