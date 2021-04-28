using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SMEV.WCFContract;

namespace AddapterSMEVClient
{
    /// <summary>
    /// Логика взаимодействия для FolderDialog.xaml
    /// </summary>
    public partial class FolderDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }
        IWcfInterface wcf
        {
            get
            {
                return MainWindow.wcf;
            }
        }
       List<FolderDialogItem> _Items = new List<FolderDialogItem>();
       public List<FolderDialogItem> Items { get { return _Items; } set { _Items = value; RaisePropertyChanged("Items"); } }

        List<string> drives = new List<string>();
        public List<string> Drives { get { return drives; } set { drives = value; RaisePropertyChanged("Drives"); } }

        string _SelectedPath = "";
        public string SelectedPath { get { return _SelectedPath; } set { _SelectedPath = value; RaisePropertyChanged("SelectedPath"); } }

        CollectionViewSource ItemsViewSource;
        public FolderDialog(string _path = "")
        {
            InitializeComponent();
            try
            {
             
                ItemsViewSource = (CollectionViewSource)FindResource("ItemsViewSource");
                Drives = wcf.GetLocalDisk().ToList();

                SelectedPath = _path;
                if (SelectedPath == string.Empty)
                {
                    SelectedPath = Drives[0];
                }
                setlist(SelectedPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                RaisePropertyChanged("Drives");
                RaisePropertyChanged("Items");
            }
        }

        void ItemsClear()
        {
            Items.Clear();
            FolderDialogItem fdi = new FolderDialogItem(TypeFolderDialogItem.Return, "", "...");
            Items.Add(fdi);
        }
        void ItemsAddFolderRange(string[] values)
        {
            Items.AddRange(values.Select(x => new FolderDialogItem(TypeFolderDialogItem.Folder, x, Path.GetFileName(x))));
   
        }

        void setlist(string selectPath)
        {
            ItemsClear();
            ItemsAddFolderRange(wcf.GetFolderLocal(selectPath));
            ItemsViewSource.View.Refresh();
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {


                if (ItemsViewSource.View.CurrentItem != null)
                {
                    var item = ItemsViewSource.View.CurrentItem as FolderDialogItem;
                    switch (item.TFDI)
                    {
                        case TypeFolderDialogItem.Folder:
                            SelectedPath = item.Path;
                            break;
                        case TypeFolderDialogItem.Return:
                            if (Path.GetDirectoryName(SelectedPath) == null) return;
                            SelectedPath = Path.GetDirectoryName(SelectedPath);
                            break;

                    }
                    setlist(SelectedPath);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e != null)
                    SelectedPath = e.AddedItems[0].ToString();
                else
                    SelectedPath = comboBox.Text;
             
                setlist(SelectedPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Key == Key.Enter)
                comboBox_SelectionChanged(comboBox, null);
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
    }

    public enum TypeFolderDialogItem
    {
        Return = 0,
        Folder = 1
    }

    public class FolderDialogItem
    {
        public TypeFolderDialogItem TFDI { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }

        public FolderDialogItem(TypeFolderDialogItem TFDI, string Path, string Name)
        {
            this.TFDI = TFDI;
            this.Path = Path;
            this.Name = Name;
        }

    }
}
