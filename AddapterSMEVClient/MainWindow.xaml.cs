using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AddapterSMEVClient.Class;
using ExcelManager;
using Microsoft.Win32;
using SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService;
using SMEV.WCFContract;
using Image = System.Windows.Controls.Image;

namespace AddapterSMEVClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public static IWcfInterface wcf;
       private MyServiceCallback cal;

        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();

        private bool close = false;
        public MainWindow()
        {
          
            var f = new LoginForm();
            if (f.ShowDialog() == true)
            {

                wcf = f.MyWcfConnection;
                cal = f.callback;
                cal.ePingResult += Cal_ePingResult;
                ((ICommunicationObject) wcf).Faulted += LoginForm_Faulted;
                ((ICommunicationObject) wcf).Closed += LoginForm_Faulted;
                //LoginForm.SMOKOD = t[4].ToString();
                ni.Text = this.Title;
                ni.Visible = false;
               
                ni.Icon = Properties.Resources.MainIcon;





                ni.ContextMenu = new System.Windows.Forms.ContextMenu(new[] {
                    new System.Windows.Forms.MenuItem("Развернуть",  delegate
                    {
                        this.Show();
                        ni.Visible = false;
                        this.WindowState = WindowState.Normal;

                    }){DefaultItem = true},

                    new System.Windows.Forms.MenuItem("-"),

                    new System.Windows.Forms.MenuItem("Закрыть", delegate
                    {
                        close = true;
                        System.Windows.Application.Current.Shutdown();
                    })
                });




                ni.DoubleClick +=
                    delegate
                    {
                        this.Show();
                        ni.Visible = false;
                        this.WindowState = WindowState.Normal;
                    };
            }
            else
            {
                MessageBox.Show("Отключение");
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();


        }



        CollectionViewSource ConfigViewSource;

        void LoginForm_Faulted(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                var f = new LoginForm {DIALOG_MESSAGE = "Связь с сервером потеряна!"};


                if (f.ShowDialog() == true)
                {
                    wcf = f.MyWcfConnection;
                    cal = f.callback;
                    cal.ePingResult += Cal_ePingResult;
                    ((ICommunicationObject) wcf).Faulted += LoginForm_Faulted;
                    ((ICommunicationObject) wcf).Closed += LoginForm_Faulted;

                }
                else
                {
                    MessageBox.Show("Отключение");
                    Application.Current.Shutdown();
                }

            });
        }

        private void Cal_ePingResult(PingResult PR)
        {
            var win = GetPingView();
            if (win == null)
            {
                Dispatcher.Invoke(() =>
                {
                    win = new PingView();
                    win.Show();

                });

            }
            Dispatcher.Invoke(() =>
            {
                win.AddPingResult(PR);
                win.Activate();

            });

           

          
        }


        private PingView GetPingView()
        {
            PingView r_f = null;
            Dispatcher?.Invoke(new Action(() =>
            {
                foreach (Window f in Application.Current.Windows)
                {
                    if (f is PingView)
                    {
                        r_f = f as PingView;
                    }
                }
            }));
            return r_f;
        }

        Configuration _Configuration;

        public Configuration Configuration
        {
            get { return _Configuration; }
            set
            {
                _Configuration = value;
                RaisePropertyChanged("Configuration");
            }
        }

        private void buttonSetting_Click(object sender, RoutedEventArgs e)
        {
            buttonSetting.IsEnabled = false;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var t = wcf.GetConfig();
                    Dispatcher?.Invoke(() => { Configuration = t; });

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения конфигурации: {ex.Message}");
                }
                finally
                {
                    Dispatcher?.Invoke(() => { buttonSetting.IsEnabled = true; });
                }
            });



        }



        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                wcf.SetConfig(Configuration);
                MessageBox.Show("ОК");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка передачи конфигурации на сервер: {ex.Message}");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var t = ConfigViewSource.View?.CurrentItem as Config_VS;
            if (t == null) return;
            var fbd = new FolderDialog(t.FilesConfig.ArchiveFolder);
            if (fbd.ShowDialog() == true)
            {
                t.FilesConfig.ArchiveFolder = fbd.SelectedPath;
            }
        }

        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {

            var t = (ConfigViewSource.View.CurrentItem as Config_VS);
            if (t == null) return;
            var fbd = new FolderDialog(t.FilesConfig.InputFolder);
            if (fbd.ShowDialog() == true)
            {
                t.FilesConfig.InputFolder = fbd.SelectedPath;
            }
        }

        private void button1_Copy1_Click(object sender, RoutedEventArgs e)
        {

            var t = (ConfigViewSource.View.CurrentItem as Config_VS);
            if (t == null) return;
            var fbd = new FolderDialog(t.FilesConfig.OutputFolder);
            if (fbd.ShowDialog() == true)
            {
                t.FilesConfig.OutputFolder = fbd.SelectedPath;
            }
        }

        private void button1_Copy2_Click(object sender, RoutedEventArgs e)
        {

            var t = (ConfigViewSource.View.CurrentItem as Config_VS);
            if (t != null)
            {
                var fbd = new FolderDialog(t.FilesConfig.PoccessFolder);
                if (fbd.ShowDialog() == true)
                {
                    t.FilesConfig.PoccessFolder = fbd.SelectedPath;
                }
            }
        }

        private void button1_Copy3_Click(object sender, RoutedEventArgs e)
        {
            var t = (ConfigViewSource.View.CurrentItem as Config_VS);
            if (t == null) return;
            var fbd = new FolderDialog(t.TranspotrMessage);
            if (fbd.ShowDialog() == true)
            {
                t.TranspotrMessage = fbd.SelectedPath;
            }
        }

        List<EntriesMy> _LogService = new List<EntriesMy>();

        public List<EntriesMy> LogService
        {
            get { return _LogService; }
            set
            {
                _LogService = value;
                RaisePropertyChanged("LogService");
            }
        }



        private void buttonLogRefresh_Click(object sender, RoutedEventArgs e)
        {
            buttonLogRefresh.IsEnabled = false;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var k = 50;
                    var HideWar = true;
                    Dispatcher?.Invoke(() => { k = Convert.ToInt32(textBoxCountLog.Text);
                        HideWar = checkBoxHideWarning.IsChecked == true;
                    });
                    var t = wcf.GetEventLogEntry(k, HideWar);
                    Dispatcher?.Invoke(() => { LogService = t; });

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения лога сервиса: {ex.Message}");
                }
                finally
                {
                    Dispatcher?.Invoke(() => { buttonLogRefresh.IsEnabled = true; });
                }
            });


        }

        List<VSWorkProcess> _DoWork = new List<VSWorkProcess>();

        public List<VSWorkProcess> DoWork
        {
            get { return _DoWork; }
            set
            {
                _DoWork = value;
                RaisePropertyChanged("DoWork");
            }
        }

        private void buttonStatus_Click(object sender, RoutedEventArgs e)
        {
            buttonStatus.IsEnabled = false;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var t = wcf.GetDoWork();
                    Dispatcher?.Invoke(() => { DoWork = t; });

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения процессов сервиса: {ex.Message}");
                }
                finally
                {
                    Dispatcher?.Invoke(() => { buttonStatus.IsEnabled = true; });
                }
            });


        }

        List<LogRow> _LogMessage = new List<LogRow>();

        public List<LogRow> LogMessage
        {
            get { return _LogMessage; }
            set
            {
                _LogMessage = value;
                RaisePropertyChanged("LogMessage");
            }
        }

        private void buttonLogMessage_Click(object sender, RoutedEventArgs e)
        {

            int Count;
            DateTime? date_in;
            DateTime? date_out;
            MessageLoggerVS[] vs;
            try
            {
                Count = Convert.ToInt32(textBoxLogMessageCount.Text);
                date_in = DataIN.SelectedDate;
                date_out = DataOUT.SelectedDate;
                vs = comboBoxVSMessage.SelectedItems.Cast<MessageLoggerVS>().ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка разбора кол-ва: {ex.Message}");
                return;
            }

            buttonLogMessage.IsEnabled = false;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var t = wcf.GetLog(Count, vs, date_in, date_out);
                    Dispatcher?.Invoke(() => { LogMessage = t; });

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения лога сообщений: {ex.Message}");
                }
                finally
                {
                    Dispatcher?.Invoke(() => { buttonLogMessage.IsEnabled = true; });
                }
            });

        }

        Thread PingThread;

        private void this_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                ConfigViewSource = FindResource("ConfigViewSource") as CollectionViewSource;
                DTReportE.SelectedDate = DateTime.Now;
                DTReportB.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                PingThread = new Thread(Pinging) {IsBackground = true};
                PingThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Pinging()
        {
            while (true)
            {
                try
                {
                    var state = ((ICommunicationObject) wcf).State;
                    if (state == CommunicationState.Opened)
                    {
                        wcf.Ping();
                    }

                }
                catch (Exception ex)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        MessageBox.Show("Не удалось подтвердить связь с сервером: " + ex.Message);
                    });
                    return;
                }

                Thread.Sleep(1800000);
            }
           
        }
    

    private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (LogServiceTab.IsSelected)
                    buttonLogRefresh_Click(buttonLogRefresh, new RoutedEventArgs());
                if (LogMessageTab.IsSelected)
                    buttonLogMessage_Click(buttonLogMessage, new RoutedEventArgs());
                if (StatusServiceTab.IsSelected)
                    buttonStatus_Click(buttonStatus, new RoutedEventArgs());
                if (SettingTab.IsSelected)
                    buttonSetting_Click(buttonSetting, new RoutedEventArgs());
                if(PingTab.IsSelected)
                    ButtonGetPingParameter_Click(ButtonGetPingParameter, new RoutedEventArgs());
            }

        }

        private void DataIN_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonLogMessage_Click(buttonLogMessage, new RoutedEventArgs());
        }

        private void comboBoxVSMessage_OnSelectionChange()
        {
            buttonLogMessage_Click(buttonLogMessage, new RoutedEventArgs());
        }
        public LogRow UslLogRow
        {
            get
            {
                if (DataGridMessage == null) return null;
                if (DataGridMessage.SelectedCells.Count == 0)
                    return null;
                return DataGridMessage.SelectedCells[0].Item as LogRow;
            }
        }

        public IEnumerable<LogRow> SelectedUslLogRow
        {
            get
            {
                if (DataGridMessage == null) return null;
                if (DataGridMessage.SelectedCells.Count == 0)
                    return null;
                return DataGridMessage.SelectedCells.Select(x=>(LogRow)x.Item).Distinct();
            }
        }

        private void listBox12_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UslLogRow == null) return;
            switch (UslLogRow.VS)
            {
                case MessageLoggerVS.InputData:
                    var ViewMP = new ViewMP(UslLogRow.ID);
                    ViewMP.Show();
                    break;
                case MessageLoggerVS.FeedbackOnMedicalService:
                    var ViewFeedBack = new ViewFeedBack(UslLogRow.ID,UslLogRow.OrderId,UslLogRow.ApplicationId);
                    ViewFeedBack.Show();
                    break;
            }
          
        }

        List<ReportRow> _Report = new List<ReportRow>();
        public List<ReportRow> Report
        {
            get
            {
                return _Report;
            }
            set
            {
                _Report = value;
                RaisePropertyChanged("Report");
            }
        }

        private DateTime ds;
        private DateTime de;

        private void buttonReportUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!DTReportB.SelectedDate.HasValue || !DTReportE.SelectedDate.HasValue) return;

            buttonReportUpdate.IsEnabled = false;
            ds = DTReportB.SelectedDate.Value;
            de = DTReportE.SelectedDate.Value;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var t = wcf.GetReport(ds, de);
                    Dispatcher?.Invoke(() => { Report = t; });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения отчета: {ex.Message}");
                }
                finally
                {
                    Dispatcher?.Invoke(() => { buttonReportUpdate.IsEnabled = true; });
                }
            });
        }

        SaveFileDialog sfd = new SaveFileDialog { Filter = "*.xlsx|*.xlsx" };
        private void buttonReportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Report.Count!=0)
                {
                    sfd.FileName =
                        $"Запросы медицинской помощи СМЭВ с {ds.ToShortDateString()} по {de.ToShortDateString()}";
                    if(sfd.ShowDialog() == true)
                    {
                        var excel = new ExcelOpenXML(sfd.FileName, "Данные");
                        var styleDef = excel.CreateType(new FontOpenXML(), new BorderOpenXML(), null);
                        var styleBold = excel.CreateType(new FontOpenXML { Bold = true }, new BorderOpenXML(), null);

                        var styleDt = excel.CreateType(new FontOpenXML {  Format = Convert.ToUInt32(DefaultNumFormat.F14) }, new BorderOpenXML(), null);


                        uint RowIndex = 1;
                        var row = excel.GetRow(RowIndex);
                        excel.PrintCell(row, 1, "Дата", styleBold);
                        excel.PrintCell(row, 2, "Кол-во запросов", styleBold);
                        excel.PrintCell(row, 3, "Кол-во людей", styleBold);
                        excel.PrintCell(row, 4, "Кол-во возвращенных услуг", styleBold);
                        excel.PrintCell(row, 5, "Отвечено", styleBold);
                        excel.PrintCell(row, 6, "Ошибок", styleBold);
                        excel.PrintCell(row, 7, "Без ответа", styleBold);

                        foreach(var r in Report)
                        {
                            RowIndex++;
                            row = excel.GetRow(RowIndex);

                            var curr_style = r.dt.HasValue ? styleDef : styleBold;
                            if(r.dt.HasValue)
                                excel.PrintCell(row, 1,r.dt.Value, styleDt);
                            else
                                excel.PrintCell(row, 1, "Итого", curr_style);

                            excel.PrintCell(row, 2, Convert.ToDouble(r.Count), curr_style);
                            excel.PrintCell(row, 3, Convert.ToDouble(r.People), curr_style);
                            excel.PrintCell(row, 4, Convert.ToDouble(r.USL), curr_style);
                            excel.PrintCell(row, 5, Convert.ToDouble(r.Answer), curr_style);
                            excel.PrintCell(row, 6, Convert.ToDouble(r.Error), curr_style);
                            excel.PrintCell(row, 7, Convert.ToDouble(r.noAnswer), curr_style);
                        }

                        excel.AutoSizeColumns(1, 7);


                        excel.Save();
                 
                        if (MessageBox.Show("Формирование счета завершено. Показать файл?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            ShowSelectedInExplorer.FileOrFolder(sfd.FileName);
                        }

                    }
                }
               
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonReportToExcel_Copy_Click(object sender, RoutedEventArgs e)
        {
            DTReportE.SelectedDate = DateTime.Now;
            DTReportB.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            var str = string.Join(Environment.NewLine,
                listView1.SelectedItems.Cast<EntriesMy>().Select(x =>
                    $"{x.TimeGenerated:dd.MM.yyyy_HH:mm} - {x.Message}"));
            Clipboard.SetText(str);
        }

        private void buttonUserOutMessage_Click(object sender, RoutedEventArgs e)
        {
            var t = (ConfigViewSource.View.CurrentItem as Config_VS);
            if (t == null) return;
            var fbd = new FolderDialog(t.UserOutMessage);
            if (fbd.ShowDialog() == true)
            {
                t.UserOutMessage = fbd.SelectedPath;
            }

            
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
            if (!close)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
        }

        private void this_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                ni.Visible = true;
                ni.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                ni.BalloonTipText = @"Программа свернута в трей";
                ni.BalloonTipTitle = @"Внимание";
                ni.ShowBalloonTip(1000);
            }
        }
        

        private void ButtonGetPingParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p = wcf.PingParamGet();
                textBoxPingAdress.Text = p.Adress;
                CheckBoxPingEnabled.IsChecked = p.IsEnabled;
                textBoxPingTimeout.Text = p.TimeOut.ToString();
                textBoxNameProcess.Text = string.Join(Environment.NewLine, p.Process);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonPing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p = wcf.PingAdress();
                MessageBox.Show(this,$"Узел [{p.Adress}] :{(p.Result ? "доступен!" : "НЕ доступен!")}{(string.IsNullOrEmpty(p.Text)? "" : $"({p.Text})" )}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonSavePingParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p = new PingConfig
                {
                    Adress = textBoxPingAdress.Text.Trim(),
                    IsEnabled = CheckBoxPingEnabled.IsChecked == true,
                    TimeOut = Convert.ToInt32(textBoxPingTimeout.Text),
                    Process = textBoxNameProcess.Text.Split(new[] { Environment.NewLine },StringSplitOptions.None).Where(x=>!string.IsNullOrEmpty(x)).ToArray()
                };
                wcf.PingParamSet(p);
                ButtonGetPingParameter_Click(ButtonGetPingParameter, new RoutedEventArgs());
                MessageBox.Show("ОК");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItem_OnClickView(object sender, RoutedEventArgs e)
        {
            listBox12_MouseDoubleClick(DataGridMessage, null);
        }

        private void MenuItem_OnClickDel(object sender, RoutedEventArgs e)
        {
            try
            {
                var Selected = SelectedUslLogRow.ToList();
                if (Selected.Count == 0) return;

                if (MessageBox.Show($"Вы уверены что хотите удалить {Selected.Count} записей?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    wcf.DeleteLog(Selected.Select(x=>x.ID).ToArray());
                    buttonLogMessage_Click(buttonLogMessage, new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var VS = ((sender as Image)?.DataContext as VSWorkProcess);
                if (VS != null)
                {
                    wcf.ChangeActivProcess(VS.VS, !VS.Activ);
                    buttonStatus_Click(buttonStatus, new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
          
        }
    }
}
