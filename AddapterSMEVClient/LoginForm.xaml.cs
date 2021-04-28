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
using AddapterSMEVClient.Properties;
using AddapterSMEVClient.ServerBehavior;
using SMEV.WCFContract;

namespace AddapterSMEVClient
{
    /// <summary>
    /// Логика взаимодействия для LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public IWcfInterface MyWcfConnection;//{ set; get; }
        public MyServiceCallback callback;
        public string DIALOG_MESSAGE = null;
        public LoginForm()
        {
            InitializeComponent();
           passwordBoxPass.Password = ProtectStr.UnprotectString(Settings.Default.PASSWORD);
            textBoxUserName.Text = ProtectStr.UnprotectString(Settings.Default.USER_NAME);
            textBoxHOST.Text = Settings.Default.IP_CONNECT;

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
                if (passwordBoxPass.Password.Trim() == "")
                {
                    result = false;
                    passwordBoxPass.BorderBrush = new SolidColorBrush();
                    passwordBoxPass.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                }
                if (textBoxUserName.Text.Trim() == "")
                {
                    result = false;
                    textBoxUserName.BorderBrush = new SolidColorBrush();
                    textBoxUserName.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca);

                }
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
        public static List<string> SecureCard;
        public static int ID = -1;
        public static int ORG_ID = -1;
        public static int ISSMO = -1;
        public static string SMOKOD = "";
        string Log = "";
        string PASS = "";
        public void ThreadConnect()
        {
            try
            {

                Dispatcher.Invoke(() =>
                {
                    if (checkBox1.IsChecked == true)
                    {
                        Settings.Default.PASSWORD = ProtectStr.ProtectString(passwordBoxPass.Password);
                        Settings.Default.USER_NAME = ProtectStr.ProtectString(textBoxUserName.Text);
                    }
                    Log = textBoxUserName.Text;
                    PASS = passwordBoxPass.Password;
                    Settings.Default.IP_CONNECT = textBoxHOST.Text;

                });

                Connect();
                button1.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Title = "Подключение: Запрос прав";
                }));
              //  SecureCard = MyWcfConnection.Connect();

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
            

            string addr = @"net.tcp://" + Settings.Default.IP_CONNECT + ":50505/TFOMS_SMEV.svc"; // Адрес сервиса
            Uri tcpUri = new Uri(addr);
            EndpointAddress address = new EndpointAddress(tcpUri);

        //    var t = address.Identity;
            //  BasicHttpBinding basicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None); //HTTP!
            var netTcpBinding = new NetTcpBinding();
           

            // Ниже строки для того, чтоб пролазили таблицы развером побольше
            netTcpBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            netTcpBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            netTcpBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            netTcpBinding.MaxBufferPoolSize = int.MaxValue;
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;
            netTcpBinding.SendTimeout = new TimeSpan(0, 30, 0);
            netTcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0);



            /* netTcpBinding.Security.Mode = SecurityMode.TransportWithMessageCredential;
             netTcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
             netTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

     */



            callback = new MyServiceCallback();
            var instanceContext = new InstanceContext(callback);
            DuplexChannelFactory<IWcfInterface> factory = new DuplexChannelFactory<IWcfInterface>(instanceContext, netTcpBinding, address);
            factory.Endpoint.Behaviors.Add(new MessageServerBehavior());
            factory.Credentials.UserName.UserName = Log;
            factory.Credentials.UserName.Password = PASS;

            factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;

            //factory.Credentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySubjectName, "MSERVICE");
            button1.Dispatcher.BeginInvoke(new Action(() =>
            {
                Title = "Подключение: Создание канала";


            }));


            foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
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
        

        private void checkBox1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.SAVE_LOG_AND_PASS = checkBox1.IsChecked.Value;
        }

        private void textBoxHOST_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                button1_Click(null, null);
            }
        }

        private void button2_Click_2(object sender, RoutedEventArgs e)
        {
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
