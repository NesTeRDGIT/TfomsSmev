using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using System.Xml.Linq;
using AddapterSMEVClient.Properties;
using AddapterSMEVClient.ServerBehavior;
using SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided;
using SMEV.WCFContract;
using SmevAdapterService.VS;

namespace AddapterSMEVClient
{
   
    /// <summary>
    /// Логика взаимодействия для LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public IWcfInterface MyWcfConnection;//{ set; get; }
        public MyServiceCallback callback;
        public string DIALOG_MESSAGE { get; set; } = null;

        public LoginForm()
        {
            InitializeComponent();
            textBoxHOST.Text = Settings.Default.IP_CONNECT;
            /*
            SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.OutputData t = new OutputData();
            var doc = t.SerializeToX();
            doc.Save("C:\\TEMP\\1.xml");
            doc.SaveXML("C:\\TEMP\\2.xml");*/
        }

        void StartAnimateButton1()
        {
            var s = (DoubleAnimation)FindResource("DA");
            var rgb = (RadialGradientBrush)button1.Background;
            s.RepeatBehavior = RepeatBehavior.Forever;
            rgb.GradientStops[0].BeginAnimation(GradientStop.OffsetProperty, s);
        }


        void StopAnimateButton1()
        {
            var s = (DoubleAnimation)FindResource("DA");

            var rgb = (RadialGradientBrush)button1.Background;
            s.RepeatBehavior = new RepeatBehavior(0);

            rgb.GradientStops[0].BeginAnimation(GradientStop.OffsetProperty, s);
           

        }

        private bool ChekData()
        {
            try
            {
                ColorAnimation ca = ((ColorAnimation)(FindResource("CA")));
                bool result = true;
              
                if (textBoxHOST.Text.Trim() == "")
                {
                    result = false;
                    textBoxHOST.BorderBrush = new SolidColorBrush();
                    textBoxHOST.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca);

                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChekData())
                {

                    if (!button1.IsHitTestVisible) return;
                    button1.IsHitTestVisible = false;
                    button1.Focusable = false;
                    StartAnimateButton1();
                    var th = new Thread(ThreadConnect);
                    th.Start();
                }

            }
            catch (Exception ex)
            {
                button1.IsHitTestVisible = true;
                button1.Focusable = true; ;
                MessageBox.Show(ex.Message);
                StopAnimateButton1();
            }
        }
       
        public void ThreadConnect()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    Settings.Default.IP_CONNECT = textBoxHOST.Text;
                });

                Connect();
                button1.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Title = "Подключение: Запрос прав";
                }));

                button1.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Title = "Подключение: Сохранение параметров";
                }));
                Settings.Default.Save();
                Dispatcher.Invoke(() =>
                {
                    Title = "Подключение";
                    StopAnimateButton1();
                    DialogResult = true;
                });


            }

            catch (Exception ex)
            {
                string errr = "";
                Exception ex1 = ex;
                errr = ex.Message;
                while (ex1.InnerException != null)
                {
                    ex1 = ex1.InnerException;
                    errr += Environment.NewLine + ex1.Source + ": " + ex1.Message + ";";

                }
                MessageBox.Show(ex1.Message + Environment.NewLine + "Полный текст ошибки: " + errr);


                Dispatcher.Invoke(() =>
                {
                    button1.IsHitTestVisible = true;
                    button1.Focusable = true;
                    StopAnimateButton1();
                });
            }
        }


        private void Connect()
        {
            var addr = $@"net.tcp://{Settings.Default.IP_CONNECT}:50505/TFOMS_SMEV.svc"; // Адрес сервиса
            var tcpUri = new Uri(addr);
            var address = new EndpointAddress(tcpUri);

            var netTcpBinding = new NetTcpBinding
            {
                ReaderQuotas = {MaxArrayLength = int.MaxValue, MaxBytesPerRead = int.MaxValue, MaxStringContentLength = int.MaxValue},
                MaxBufferPoolSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                SendTimeout = new TimeSpan(0, 30, 0),
                ReceiveTimeout = new TimeSpan(24, 0, 0)
            };


            // Ниже строки для того, чтоб пролазили таблицы развером побольше



            callback = new MyServiceCallback();
            var instanceContext = new InstanceContext(callback);
            var factory = new DuplexChannelFactory<IWcfInterface>(instanceContext, netTcpBinding, address);
            factory.Endpoint.Behaviors.Add(new MessageServerBehavior());

            button1.Dispatcher.Invoke(() =>
            {
                Title = "Подключение: Создание канала";
            });


            foreach (var op in factory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
            MyWcfConnection = factory.CreateChannel(); // Создаём само подключение   
            MyWcfConnection.Register();
        }




        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (DIALOG_MESSAGE != null)
            {
                MessageBox.Show(DIALOG_MESSAGE);
            }
        }
        

    
        private void textBoxHOST_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                button1_Click(null, null);
            }
        }
    }
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class MyServiceCallback : IWcfInterfaceCallback
    {
        public delegate void dPingResult(PingResult PR);
        public event dPingResult ePingResult;
        public void PingResult(PingResult PR)
        {
            ePingResult?.Invoke(PR);
        }

        public void Ping()
        {
           
        }
    }
    static class ProtectStr
    {
        static byte[] s_aditionalEntropy = { 10, 5, 8, 4, 9 };
        public static string ProtectString(string str)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(str);
                return Convert.ToBase64String(ProtectedData.Protect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception )
            {
                return "";
            }
        }


        public static string UnprotectString(string str)
        {
            try
            {
                byte[] data = Convert.FromBase64String(str);
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception )
            {
                return "";
            }
        }
    }

}
