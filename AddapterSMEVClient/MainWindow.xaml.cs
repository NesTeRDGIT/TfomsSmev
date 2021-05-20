using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AddapterSMEVClient.Annotations;
using AddapterSMEVClient.Class;
using SMEV.WCFContract;

namespace AddapterSMEVClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public MainWindowVM VM { get; set; } = new MainWindowVM(new ReportExport());
        public static IWcfInterface wcf { get; set; }
        private MyServiceCallback cal { get; set; }
        private bool close { get; set; }
        public MainWindow()
        {
            VM.SetDispatcher(this.Dispatcher);
            if (ConnectWCF())
                CreateNotifyIcon();
            InitializeComponent();

        }
        private void CloseAPP(bool Shutdown = true)
        {
            close = true;
            ni.Visible = false;
            ni.Dispose();
            CTSPinging?.Cancel();
            if (Shutdown)
                Application.Current.Shutdown();
        }

        private bool ConnectWCF(string DIALOG_MESSAGE = null)
        {
            var f = new LoginForm() { DIALOG_MESSAGE = DIALOG_MESSAGE };
            if (f.ShowDialog() == true)
            {
                wcf = f.MyWcfConnection;
                cal = f.callback;
                cal.ePingResult += Cal_ePingResult;
                ((ICommunicationObject)wcf).Faulted += LoginForm_Faulted;
                ((ICommunicationObject)wcf).Closed += LoginForm_Faulted;
                VM.SetWCF(wcf);
                return true;
            }
            MessageBox.Show("Отключение");
            CloseAPP();
            return false;
        }

        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        private void CreateNotifyIcon()
        {
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
        void LoginForm_Faulted(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                ConnectWCF("Связь с сервером потеряна!");
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
            Dispatcher?.Invoke(() =>
            {
                foreach (Window f in Application.Current.Windows)
                {
                    if (f is PingView view)
                    {
                        r_f = view;
                    }
                }
            });
            return r_f;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private CancellationTokenSource CTSPinging = new CancellationTokenSource();
        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DTReportE.SelectedDate = DateTime.Now;
                DTReportB.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                Task.Run(() => Pinging(CTSPinging.Token));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Pinging(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    var state = ((ICommunicationObject)wcf).State;
                    if (state == CommunicationState.Opened)
                    {
                        wcf.Ping();
                    }
                    Delay(1800000, cancel);
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        MessageBox.Show($"Не удалось подтвердить связь с сервером: {ex.Message}");
                    });
                }
            }
        }

        private void Delay(int MS, CancellationToken cancel)
        {
            var task = Task.Delay(MS, cancel);
            task.Wait(cancel);
        }




        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var str = string.Join(Environment.NewLine, listView1.SelectedItems.Cast<EntriesMyVM>().Select(x => $"{x.TimeGenerated:dd.MM.yyyy_HH:mm} - {x.Message}"));
                Clipboard.SetText(str);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }



        private void this_Closing(object sender, CancelEventArgs e)
        {
            if (!close)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
            else
            {
                CloseAPP(false);
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

        public MessageLoggerVS[] SelectedMessageLoggerVS => comboBoxVSMessage?.SelectedItems.Cast<MessageLoggerVS>().ToArray();

        private void ComboBoxVSMessage_OnDropDownClosed(object sender, RoutedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectedMessageLoggerVS));
            VM.RefreshLogMessageParam.VS = SelectedMessageLoggerVS;
        }

        public LogRow[] SelectedLogItems => DataGridMessage.SelectedCells.Select(x => (LogRow)x.Item).Distinct().ToArray();
        private void DataGridMessage_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectedLogItems));
        }

        private void ButtonSetCurrentPeriod_OnClick(object sender, RoutedEventArgs e)
        {
            DTReportE.SelectedDate = DateTime.Now;
            DTReportB.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }

        public ICommand CloseAppCommand => new Command(obj =>
        {
            this.CloseAPP();
        });

    }

    public class MainWindowVM : INotifyPropertyChanged
    {
        private IReportExport reportExport { get; set; }
        private IWcfInterface wcf { get; set; }
        private Dispatcher dispatcher { get; set; }
        public MainWindowVM(IReportExport reportExport)
        {
            this.reportExport = reportExport;
        }

        public MainWindowVM SetWCF(IWcfInterface wcf)
        {
            this.wcf = wcf;
            RefreshLogServiceCommand.Execute(null);
            return this;
        }
        public MainWindowVM SetDispatcher(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            return this;
        }

        #region LogService
        private ObservableCollection<EntriesMyVM> _LogService = new ObservableCollection<EntriesMyVM>();
        public ObservableCollection<EntriesMyVM> LogService
        {
            get => _LogService;
            set
            {
                _LogService = value;
                RaisePropertyChanged();
            }
        }
        private bool _RefreshingLogService;
        public bool RefreshingLogService
        {
            get => _RefreshingLogService;
            set
            {
                _RefreshingLogService = value;
                RaisePropertyChanged();
            }
        }

        private int _LimitLog = 50;
        public int LimitLog
        {
            get => _LimitLog;
            set
            {
                _LimitLog = value;
                RaisePropertyChanged();
            }
        }
        private bool _HideWarningLog = true;
        public bool HideWarningLog
        {
            get => _HideWarningLog;
            set
            {
                _HideWarningLog = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RefreshLogServiceCommand => new Command(obj =>
        {
            try
            {
                RefreshingLogService = true;
                Task.Run(() =>
                {
                    try
                    {
                        var t = wcf.GetEventLogEntry(LimitLog, HideWarningLog);
                        dispatcher.Invoke(() =>
                        {
                            LogService.Clear();
                           // LogService.Add(new EntriesMyVM(new EntriesMy(){Message = "asd \n dasda" }));
                            foreach (var item in t)
                            {
                                LogService.Add(new EntriesMyVM(item));
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка получения лога сервиса: {ex.Message}");
                    }
                    finally
                    {
                        dispatcher.Invoke(() => { RefreshingLogService = false; });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения лога сервиса: {ex.Message}");
                RefreshingLogService = false;
            }
        }, obj => !RefreshingLogService);
        #endregion
        #region DoWork
        ObservableCollection<VSWorkProcess> _DoWork = new ObservableCollection<VSWorkProcess>();
        public ObservableCollection<VSWorkProcess> DoWork
        {
            get => _DoWork;
            set
            {
                _DoWork = value;
                RaisePropertyChanged();
            }
        }
        private bool _RefreshingDoWork;
        public bool RefreshingDoWork
        {
            get => _RefreshingDoWork;
            set
            {
                _RefreshingDoWork = value;
                RaisePropertyChanged();
            }
        }
        public ICommand RefreshDoWorkCommand => new Command(obj =>
        {
            try
            {
                RefreshingDoWork = true;
                Task.Run(() =>
                {
                    try
                    {
                        var t = wcf.GetDoWork();
                        dispatcher.Invoke(() =>
                        {
                            DoWork.Clear();
                            foreach (var item in t)
                            {
                                DoWork.Add(item);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка получения процессов сервиса: {ex.Message}");
                    }
                    finally
                    {
                        dispatcher.Invoke(() => { RefreshingDoWork = false; });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения процессов сервиса: {ex.Message}");
                RefreshingDoWork = false;
            }
        }, obj => !RefreshingDoWork);
        public ICommand ChangeActiveCommand => new Command(obj =>
        {
            try
            {
                var VS = (VSWorkProcess)obj;
                wcf.ChangeActivProcess(VS.VS, !VS.Activ);
                RefreshDoWorkCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });
        #endregion
        #region LogMessage
        ObservableCollection<LogRow> _LogMessage = new ObservableCollection<LogRow>();
        public ObservableCollection<LogRow> LogMessage
        {
            get => _LogMessage;
            set
            {
                _LogMessage = value;
                RaisePropertyChanged();
            }
        }

        private bool _RefreshingLogMessage;
        public bool RefreshingLogMessage
        {
            get => _RefreshingLogMessage;
            set
            {
                _RefreshingLogMessage = value;
                RaisePropertyChanged();
            }
        }
        public class RefreshLogMessageP
        {
            public int Count { get; set; } = 50;
            public DateTime? DATE_B { get; set; }
            public DateTime? DATE_E { get; set; }
            public MessageLoggerVS[] VS { get; set; }
        }

        public RefreshLogMessageP RefreshLogMessageParam { get; set; } = new RefreshLogMessageP();
        public ICommand RefreshLogMessageCommand => new Command(obj =>
        {
            try
            {
                RefreshingLogMessage = true;
                Task.Run(() =>
                {
                    try
                    {
                        var t = wcf.GetLog(RefreshLogMessageParam.Count, RefreshLogMessageParam.VS?.Length != 0 ? RefreshLogMessageParam.VS : null, RefreshLogMessageParam.DATE_B, RefreshLogMessageParam.DATE_E);
                        dispatcher?.Invoke(() =>
                        {
                            LogMessage.Clear();
                            foreach (var item in t)
                            {
                                LogMessage.Add(item);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка получения лога сообщений: {ex.Message}");
                    }
                    finally
                    {
                        dispatcher?.Invoke(() => { RefreshingLogMessage = false; });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения лога сообщений: {ex.Message}");
            }
        }, obj => !RefreshingLogMessage);

        public ICommand ShowLogMessageDetailCommand => new Command(obj =>
        {
            try
            {
                var items = (LogRow[])obj;
                var item = items.First();
                switch (item.VS)
                {
                    case MessageLoggerVS.InputData:
                    case MessageLoggerVS.InputDataSiteTFOMS:
                        var ViewMP = new ViewMP(item.ID);
                        ViewMP.Show();
                        break;
                    case MessageLoggerVS.FeedbackOnMedicalService:
                        var ViewFeedBack = new ViewFeedBack(item.ID, item.OrderId, item.ApplicationId);
                        ViewFeedBack.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии: {ex.Message}");
            }
        });

        public ICommand ShowStatusHistoryCommand => new Command(obj =>
        {
            try
            {
                var items = (LogRow[])obj;
                var item = items.First();
                var win = new StatusHistory(item.ID);
                win.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии: {ex.Message}");
            }
        });



        public ICommand DeleteMessageDetailCommand => new Command(obj =>
        {
            try
            {
                var items = (LogRow[])obj;
                if (MessageBox.Show($"Вы уверены что хотите удалить {items.Length} сообщений", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    wcf.DeleteLog(items.Select(x => x.ID).ToArray());
                    MessageBox.Show("Успешно");
                    RefreshLogMessageCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления сообщения: {ex.Message}");
            }
        });

        public ICommand ReSentCommand => new Command(obj =>
        {
            try
            {
                var items = (LogRow[])obj;
                var item = items.First();
                if (MessageBox.Show($"Вы уверены что хотите еще раз отправить сообщение", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            wcf.Resent(item.ID);
                            MessageBox.Show("Успешно");
                        }
                        catch (Exception ex)
                        { MessageBox.Show(ex.Message); }
                    });

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления сообщения: {ex.Message}");
            }
        });
        #endregion
        #region Report
        ObservableCollection<ReportRow> _Report = new ObservableCollection<ReportRow>();
        public ObservableCollection<ReportRow> Report
        {
            get => _Report;
            set
            {
                _Report = value;
                RaisePropertyChanged();
            }
        }

        class ReportParam
        {
            public DateTime DATE_START { get; set; }
            public DateTime DATE_END { get; set; }
        }

        private ReportParam ReportP { get; set; } = new ReportParam();
        public ICommand GetReportCommand => new Command(obj =>
        {
            try
            {
                var items = (object[])obj;
                var ds = (DateTime?)items[0];
                var de = (DateTime?)items[1];
                if (!ds.HasValue || !de.HasValue)
                    throw new Exception("Не указан период выборки");

                ReportP.DATE_START = ds.Value;
                ReportP.DATE_END = de.Value;
                Task.Run(() =>
                {
                    try
                    {
                        var t = wcf.GetReport(ds.Value, de.Value);
                        dispatcher?.Invoke(() =>
                        {
                            Report.Clear();
                            foreach (var item in t)
                            {
                                Report.Add(item);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при запросе отчета: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запросе отчета: {ex.Message}");
            }
        });


        public ICommand ExportReportCommand => new Command(obj =>
        {
            try
            {
                if (Report.Count != 0)
                    reportExport.Export(Report.ToList(), ReportP.DATE_START, ReportP.DATE_END);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запросе отчета: {ex.Message}");
            }
        });

        #endregion
        #region PingConfig
        private string _PingHost;
        public string PingHost
        {
            get => _PingHost;
            set { _PingHost = value; RaisePropertyChanged(); }
        }
        private bool _PingIsEnabled;
        public bool PingIsEnabled
        {
            get => _PingIsEnabled;
            set { _PingIsEnabled = value; RaisePropertyChanged(); }
        }
        private int _PingTimeOut;
        public int PingTimeOut
        {
            get => _PingTimeOut;
            set { _PingTimeOut = value; RaisePropertyChanged(); }
        }
        private string[] _PingNameProcess = { };
        public string[] PingNameProcess
        {
            get => _PingNameProcess;
            set { _PingNameProcess = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(PingNameProcessString));
            }
        }

        public string PingNameProcessString
        {
            get => string.Join(Environment.NewLine, _PingNameProcess);
            set { _PingNameProcess = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(x => !string.IsNullOrEmpty(x)).ToArray(); RaisePropertyChanged(); RaisePropertyChanged(nameof(PingNameProcess)); }
        }

        public ICommand SavePingParamCommand => new Command(obj =>
        {
            try
            {
                var p = new PingConfig
                {
                    Adress = PingHost,
                    IsEnabled = PingIsEnabled,
                    TimeOut = PingTimeOut,
                    Process = PingNameProcess
                };
                wcf.PingParamSet(p);
                LoadPingParamCommand.Execute(null);
                MessageBox.Show("ОК");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        });

        public ICommand LoadPingParamCommand => new Command(obj =>
        {
            try
            {
                var p = wcf.PingParamGet();
                PingHost = p.Adress;
                PingIsEnabled = p.IsEnabled;
                PingTimeOut = p.TimeOut;
                PingNameProcess = p.Process;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });

        public ICommand CheckPingCommand => new Command(obj =>
        {
            try
            {
                var p = wcf.PingAdress();
                MessageBox.Show($"Узел [{p.Adress}] :{(p.Result ? "доступен!" : "НЕ доступен!")}{(string.IsNullOrEmpty(p.Text) ? "" : $"({p.Text})")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });




        #endregion
        #region Setting
        Configuration _Configuration = new Configuration();
        public Configuration Configuration
        {
            get => _Configuration;
            set
            {
                _Configuration = value;
                RaisePropertyChanged();
            }
        }

        Config_VS _currentConfig;
        public Config_VS currentConfig
        {
            get => _currentConfig;
            set
            {
                _currentConfig = value;
                RaisePropertyChanged();
            }
        }

        public ICommand GetConfigCommand => new Command(obj =>
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var t = wcf.GetConfig();
                        dispatcher?.Invoke(() => { Configuration = t; });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка получения конфигурации: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });

        public ICommand SetConfigCommand => new Command(obj =>
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
        });

        public ICommand SelectTransportMessageCommand => new Command(obj =>
        {
            try
            {
                if (currentConfig == null) return;
                var fbd = new FolderDialog(currentConfig.TranspotrMessage);
                if (fbd.ShowDialog() == true)
                {
                    currentConfig.TranspotrMessage = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });
        public ICommand SelectUserOutMessageCommand => new Command(obj =>
        {
            try
            {
                if (currentConfig == null) return;
                var fbd = new FolderDialog(currentConfig.UserOutMessage);
                if (fbd.ShowDialog() == true)
                {
                    currentConfig.UserOutMessage = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });
        public ICommand SelectArchiveFolderCommand => new Command(obj =>
        {
            try
            {
                if (currentConfig == null) return;
                var fbd = new FolderDialog(currentConfig.FilesConfig.ArchiveFolder);
                if (fbd.ShowDialog() == true)
                {
                    currentConfig.FilesConfig.ArchiveFolder = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });

        public ICommand SelectInputFolderCommand => new Command(obj =>
        {
            try
            {
                if (currentConfig == null) return;
                var fbd = new FolderDialog(currentConfig.FilesConfig.InputFolder);
                if (fbd.ShowDialog() == true)
                {
                    currentConfig.FilesConfig.InputFolder = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });
        public ICommand SelectOutputFolderCommand => new Command(obj =>
        {
            try
            {
                if (currentConfig == null) return;
                var fbd = new FolderDialog(currentConfig.FilesConfig.OutputFolder);
                if (fbd.ShowDialog() == true)
                {
                    currentConfig.FilesConfig.OutputFolder = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });
        public ICommand SelectPoccessFolderCommand => new Command(obj =>
        {
            try
            {
                if (currentConfig == null) return;
                var fbd = new FolderDialog(currentConfig.FilesConfig.PoccessFolder);
                if (fbd.ShowDialog() == true)
                {
                    currentConfig.FilesConfig.PoccessFolder = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });









        #endregion

        private int _TabIndex;
        public int TabIndex
        {
            get => _TabIndex;
            set
            {
                _TabIndex = value;
                AutoRaiseCommand(TabIndex);
            }
        }

        private void AutoRaiseCommand(int index)
        {
            try
            {
                switch (index)
                {
                    case 0: RefreshLogServiceCommand.Execute(null); break;
                    case 1: RefreshDoWorkCommand.Execute(null); break;
                    case 2: RefreshLogMessageCommand.Execute(null); break;
                    case 4: LoadPingParamCommand.Execute(null); break;
                    case 5: GetConfigCommand.Execute(null); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


    }


    public class EntriesMyVM:INotifyPropertyChanged
    {
        private EntriesMy EntriesMy;
        public EntriesMyVM(EntriesMy e)
        {
            EntriesMy = e;
        }

        private bool _ShowFull;
        public bool ShowFull
        {
            get => _ShowFull;
            set
            {
                _ShowFull = value;
                RaisePropertyChanged();
            }
        }
        public string Message => EntriesMy.Message;
        public DateTime TimeGenerated => EntriesMy.TimeGenerated;
        public TypeEntries Type => EntriesMy.Type;

        public bool IsMultiLine => Message.Contains(Environment.NewLine) || Message.Contains('\n');

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
