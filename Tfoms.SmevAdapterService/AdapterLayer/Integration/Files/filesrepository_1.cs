using SMEV.WCFContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SmevAdapterService.AdapterLayer.Integration
{  /// <summary>
   /// Собщение репазитория
   /// </summary>
    public class MessageIntegration
    {
        public XDocument Content { get; set; }
        /// <summary>
        /// Код в БД
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Ключ запроса
        /// </summary>
        public string Key { get; set; }
    }
    /// <summary>
    /// Интеграция файлов
    /// </summary>
    public class FileIntegration : IRepository
    {
        /// <summary>
        /// Конфиг
        /// </summary>
        FileIntegrationConfig Config;
        public FileIntegration(FileIntegrationConfig config)
        {
            Config = config;
            CheckPath();
        }
        /// <summary>
        /// проверка дириктории и создания если нет их
        /// </summary>
        void CheckPath()
        {
            try
            {
                if (!Directory.Exists(Config.ArchiveFolder)) Directory.CreateDirectory(Config.ArchiveFolder);
                if (!Directory.Exists(Config.InputFolder)) Directory.CreateDirectory(Config.InputFolder);
                if (!Directory.Exists(Config.OutputFolder)) Directory.CreateDirectory(Config.OutputFolder);
                if (!Directory.Exists(Config.PoccessFolder)) Directory.CreateDirectory(Config.PoccessFolder);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка проверки каталогов файловой интеграции: {0}", ex.Message), ex);
            }
        }

        public List<MessageIntegration> GetMessage()
        {
            try
            {

                List<MessageIntegration> list = new List<MessageIntegration>();
                MoveFileInProccess();
                var files = Directory.GetFiles(Config.PoccessFolder, "{*}.xml");
                Regex r = new Regex(@"^[\{](?<key>.*)[\}]$");
               
                foreach (var file in files)
                {
                    var ma = r.Match(Path.GetFileNameWithoutExtension(file));
                    var val = ma.Groups["key"].Value;
                    if (val == "" || val == null)
                        throw new Exception("Ошибка парсинга имени файла: нет группы key");
                    list.Add(new MessageIntegration() { Key = ma.Groups["key"].Value, Content = XDocument.Load(file) });
                }
                
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в GetMessage: {0}", ex.Message), ex);
            }
        }
        /// <summary>
        /// Перенос файлов в папку обработки
        /// </summary>
        private void MoveFileInProccess()
        {
            var files = Directory.GetFiles(Config.InputFolder, "*.xml");
            foreach (var file in files)
            {
                FileManager.MoveFileTo(file, Path.Combine(Config.PoccessFolder, Path.GetFileName(file)), false);
            }
        }
        public void SendMessage(MessageIntegration mes)
        {
            try
            {
                ///Проверям путь к архиву
                string ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                string FileNameOUT = string.Format("{{{0}}}.xml",mes.Key);
                string FileNameOUTArc = string.Format("{0}_{1}.OUT", mes.ID, FileNameOUT);


                //Сохраняем исходящий в архив
                mes.Content.Save(Path.Combine(ArcPath, FileNameOUTArc));
                mes.Content.Save(Path.Combine(Config.OutputFolder, FileNameOUT));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в SendMessage: {0}", ex.Message), ex);
            }
        }
        public void EndProcessMessage(MessageIntegration mes)
        {
            try
            {
                ///Проверям путь к архиву
                string ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                string FileNameIN = string.Format("{{{0}}}.xml", mes.Key);
                string FileNameINArc = string.Format("{0}_{1}.IN", mes.ID, FileNameIN);

                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                //Входящий переносим
                FileManager.MoveFileTo(Path.Combine(Config.PoccessFolder, FileNameIN), Path.Combine(ArcPath, FileNameINArc));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в EndProcessMessage: {0}", ex.Message), ex);
            }
        }

        public void ReadMessage(MessageIntegration mes)
        {
            try
            {
                ///Проверям путь к архиву
                string ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                string FileNameIN = string.Format("{{{0}}}.xml", mes.Key);
                string FileNameINArc = string.Format("{0}_{1}.STATUS", mes.ID, FileNameIN);

                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                //Входящий переносим
                FileManager.MoveFileTo(Path.Combine(Config.PoccessFolder, FileNameIN), Path.Combine(ArcPath, FileNameINArc));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в ReadMessage: {0}", ex.Message), ex);
            }
        }

        public void ErrorMessage(MessageIntegration mes)
        {
            try
            {
                ///Проверям путь к архиву
                string ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                string FileName = string.Format("{{{0}}}.xml", mes.Key);
                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                //Входящий переносим
                FileManager.MoveFileTo(Path.Combine(Config.PoccessFolder, FileName), Path.Combine(ArcPath, FileName + ".ERR"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в ErrorMessage: {0}", ex.Message), ex);
            }
        }
    }
   
    /// <summary>
    /// Конфиг файловой интеграции
    /// </summary>
    public class FileIntegrationConfig : IConfigRepository
    {

        public static FileIntegrationConfig Get(FileIntegrationSet fis)
        {
            FileIntegrationConfig fic = new FileIntegrationConfig();
            fic.ArchiveFolder = fis.ArchiveFolder;
            fic.InputFolder = fis.InputFolder;
            fic.OutputFolder = fis.OutputFolder;
            fic.PoccessFolder = fis.PoccessFolder;
      
            return fic;
        }
        /// <summary>
        /// Папка входящих файлов
        /// </summary>
        public string InputFolder { get; set; }
        /// <summary>
        /// Папка исходящих файлов
        /// </summary>
        public string OutputFolder { get; set; }
        /// <summary>
        /// Папка обработки
        /// </summary>
        public string PoccessFolder { get; set; }
        /// <summary>
        /// Папка архива
        /// </summary>
        public string ArchiveFolder { get; set; }

    }
    public static class FileManager
    {
        public static string MoveFileTo(string From, string Dist, bool rename = true)
        {
            if (!Directory.Exists(Path.GetDirectoryName(Dist)))
                Directory.CreateDirectory(Path.GetDirectoryName(Dist));
            string newDist = Dist;
            if (rename)
            {
                int x = 1;
                while (File.Exists(newDist))
                {
                    newDist = Path.Combine(Path.GetDirectoryName(Dist), string.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(Dist), x, Path.GetExtension(Dist)));
                    x++;
                }
            }
            int tik = 0;
            while (!CheckFileAv(From))
            {
                tik++;
                if (tik > 3)
                    throw new Exception(string.Format("Файл {0} не доступен для переноса!", From));
            };
            File.Move(From, newDist);
            return newDist;
        }

        public static bool CheckFileAv(string path)
        {
            try
            {
                Stream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                stream.Close();
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

   
}
