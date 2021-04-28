using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Tfoms.SmevAdapterService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller = new ServiceProcessInstaller();
            this.serviceInstaller = new ServiceInstaller();
            // 
            // serviceProcessInstaller
            // 
            this.serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;
            // 
            // serviceInstaller
            // 
            this.serviceInstaller.Description = "Сервис-прослойка между Адаптером СМЭВ3 (Ростелеком) и ТФОМС Забайкальского края";
            this.serviceInstaller.DisplayName = "TFOMS Адаптер СМЭВ";
            this.serviceInstaller.ServiceName = "TFOMSAdapterSMEV";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new Installer[] {
            this.serviceInstaller,
            this.serviceProcessInstaller});

        }

        #endregion

        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;
    }
}