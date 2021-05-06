using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using SMEV.WCFContract;

namespace SmevAdapterService.AdapterLayer.Integration
{  
    /// <summary>
   /// Сообщение 
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
        /// проверка директории и создания если нет их
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
                throw new Exception($"Ошибка проверки каталогов файловой интеграции: {ex.Message}", ex);
            }
        }
        public List<MessageIntegration> GetMessage()
        {
            try
            {
                var list = new List<MessageIntegration>();
                MoveFileInProccess();
                var files = Directory.GetFiles(Config.PoccessFolder, "{*}.xml");
                var r = new Regex(@"^[\{](?<key>.*)[\}]$");
               
                foreach (var file in files)
                {
                    var ma = r.Match(Path.GetFileNameWithoutExtension(file));
                    var val = ma.Groups["key"].Value;
                    if (string.IsNullOrEmpty(val))
                        throw new Exception("Ошибка разбора имени файла: нет группы key");
                    list.Add(new MessageIntegration { Key = ma.Groups["key"].Value, Content = XDocument.Load(file) });
                }
                
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в GetMessage: {ex.Message}", ex);
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
                //Проверяем путь к архиву
                var ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                var FileNameOUT = $"{{{mes.Key}}}.xml";
                var FileNameOUTArc = $"{mes.ID}_{FileNameOUT}.OUT";

                //Сохраняем исходящий в архив
                mes.Content.Save(Path.Combine(ArcPath, FileNameOUTArc));
                mes.Content.Save(Path.Combine(Config.OutputFolder, FileNameOUT));
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в SendMessage: {ex.Message}", ex);
            }
        }
        public void EndProcessMessage(MessageIntegration mes)
        {
            try
            {
                //Проверяем путь к архиву
                var ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                var FileNameIN = $"{{{mes.Key}}}.xml";
                var FileNameINArc = $"{mes.ID}_{FileNameIN}.IN";

                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                //Входящий переносим
                FileManager.MoveFileTo(Path.Combine(Config.PoccessFolder, FileNameIN), Path.Combine(ArcPath, FileNameINArc));
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в EndProcessMessage: {ex.Message}", ex);
            }
        }
        public void ReadMessage(MessageIntegration mes)
        {
            try
            {
                //Проверям путь к архиву
                var ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                var FileNameIN = $"{{{mes.Key}}}.xml";
                var FileNameINArc = $"{mes.ID}_{FileNameIN}.STATUS";

                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                //Входящий переносим
                FileManager.MoveFileTo(Path.Combine(Config.PoccessFolder, FileNameIN), Path.Combine(ArcPath, FileNameINArc));
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в ReadMessage: {ex.Message}", ex);
            }
        }
        public void ErrorMessage(MessageIntegration mes, string perfix_ext = "")
        {
            try
            {
                //Проверим путь к архиву
                var ArcPath = Path.Combine(Config.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"));
                var FileName = $"{{{mes.Key}}}.xml";
                if (!Directory.Exists(ArcPath)) Directory.CreateDirectory(ArcPath);
                //Входящий переносим
                FileManager.MoveFileTo(Path.Combine(Config.PoccessFolder, FileName), Path.Combine(ArcPath,$"{FileName}.ERR{perfix_ext}"));
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в ErrorMessage: {ex.Message}", ex);
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
            var fic = new FileIntegrationConfig
            {
                ArchiveFolder = fis.ArchiveFolder,
                InputFolder = fis.InputFolder,
                OutputFolder = fis.OutputFolder,
                PoccessFolder = fis.PoccessFolder
            };

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
            var newDist = Dist;
            if (rename)
            {
                var x = 1;
                while (File.Exists(newDist))
                {
                    newDist = Path.Combine(Path.GetDirectoryName(Dist),$"{Path.GetFileNameWithoutExtension(Dist)}({x}){Path.GetExtension(Dist)}");
                    x++;
                }
            }
            var tik = 0;
            while (!CheckFileAv(From))
            {
                tik++;
                if (tik > 3)
                    throw new Exception($"Файл {From} не доступен для переноса!");
                Thread.Sleep(5000);
            }
            File.Move(From, newDist);
            return newDist;
        }

        public static bool CheckFileAv(string path)
        {
            try
            {
                using (Stream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Close();
                    return true;
                }
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
