using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using AddapterSMEVClient.Annotations;
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
        public MedpomData Data
        {
            get => _Data;
            set
            {
                _Data = value;
                RaisePropertyChanged();
            }
        }

        public ViewMP(int ID)
        {
            InitializeComponent();
            try
            {
                Data = wcf.GetMedpomData(ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
