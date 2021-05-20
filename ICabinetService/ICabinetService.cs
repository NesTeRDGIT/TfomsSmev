using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace CabinetContract
{
    /// <summary>
    /// Сервис
    /// </summary>
    [ServiceContract]
    public interface ICabinetService
    {
        /// <summary>
        /// Получить данные о страховании и оказанной медицинской помощи
        /// </summary>
        /// <param name="person">Данные застрахованного</param>
        /// <returns></returns>
        [OperationContract]
        Response GetData(Person person);
        /// <summary>
        /// Проверка связи
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Ping();
        /// <summary>
        /// Подлежит ли застрахованный информированию
        /// </summary>
        /// <param name="ENP">ЕНП</param>
        /// <returns></returns>
        [OperationContract]
        bool InformingValidate(string ENP);
        /// <summary>
        /// Отметка об информировании
        /// </summary>
        /// <param name="ENP">ЕНП</param>
        /// <returns></returns>
        [OperationContract]
        bool InformingInsertRecord(string ENP);
    }
    /// <summary>
    /// Расширение
    /// </summary>
    public static class EntitiesExtensions
    {
        /// <summary>
        ///  Вызов валидации произвольного класса от IDataContract
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<ValidationResult> Validate<T>(this T entity) where T : IDataContract
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(entity);
            Validator.TryValidateObject(entity, context, results, true);
            return results;
        }
    }
    /// <summary>
    /// Контракт данных для валидации
    /// </summary>
    public interface IDataContract
    {
    }



    /// <summary>
    /// Данные о страховании и оказанной медицинской помощи
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Информация о страховке и прикреплении
        /// </summary>
        [DataMember, Description("Информация о страховке и прикреплении")]
        public PersonInfo Info { get; set; }
        /// <summary>
        /// Информация об оказанной медицинской помощи
        /// </summary>
        [DataMember, Description("Информация об оказанной медицинской помощи")]
        public List<MedicalCare> Care { get; set; }
    }
    /// <summary>
    /// Данные застрахованного
    /// </summary>
    [DataContract, ServiceBehavior(IgnoreExtensionDataObject = true)]
    public class Person: IDataContract
    {
        /// <summary>
        /// ЕНП
        /// </summary>
        [DataMember(IsRequired = false), Description("ЕНП"), RegularExpression(@"(^$)|(^\d{16}$){0,1}", ErrorMessage = "Поле 'ЕНП' может быть пустым или должно состоять из 16 символов")]
        public string ENP { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        [DataMember(IsRequired = false), Description("Фамилия"), RegularExpression(@"^[а-яА-ЯёЁ\s\-]{0,50}$", ErrorMessage = "Поле 'Фамилия' может быть пустым или содержать только буквы русского алфавита, пробел или символ '-'")]
        public string FAM { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        [DataMember(IsRequired = false), Description("Имя"), RegularExpression(@"^[а-яА-ЯёЁ\s\-]{0,50}$", ErrorMessage = "Поле 'Имя' может быть пустым или содержать только буквы русского алфавита, пробел или символ '-'")]
        public string IM { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        [DataMember(IsRequired = false), Description("Отчество"), RegularExpression(@"^[а-яА-ЯёЁ\s\-]{0,50}$", ErrorMessage = "Поле 'Отчество' может быть пустым или содержать только буквы русского алфавита, пробел или символ '-'")]
        public string OT { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        [DataMember(IsRequired = true), Description("Дата рождения")]
        public DateTime DR { get; set; }
        /// <summary>
        /// Пол
        /// </summary>
        [DataMember(IsRequired = false), Description("Пол"), RegularExpression(@"^[1-2]{0,1}")]
        public int? W { get; set; }
        /// <summary>
        /// СНИЛС
        /// </summary>
        [DataMember(IsRequired = false), Description("СНИЛС"), RegularExpression(@"^(\d{3}-\d{3}-\d{3}\s\d{2}){0,1}$", ErrorMessage = "Поле 'СНИЛС' может быть пустым или соответствовать маске 'ХХХ-ХХХ-ХХХ ХХ'")]
        public string SNILS { get; set; }
        /// <summary>
        /// Серия документа, удостоверяющего личность
        /// </summary>
        [DataMember(IsRequired = false), Description("Серия документа, удостоверяющего личность")]
        public string DOCS { get; set; }
        /// <summary>
        /// Номер документа, удостоверяющего личность
        /// </summary>
        [DataMember(IsRequired = false), Description("Номер документа, удостоверяющего личность")]
        public string DOCN { get; set; }

    }

    /// <summary>
    /// Медицинская услуга
    /// </summary>
    [DataContract, ServiceBehavior(IgnoreExtensionDataObject = true)]
    public class MedicalCare : IDataContract
    {
        /// <summary>
        /// Код МО
        /// </summary>
        [DataMember, Description("Код МО")]
        public string CODE_MO { get; set; }
        /// <summary>
        /// Наименование МО
        /// </summary>
        [DataMember, Description("Наименование МО")]
        public string NAM_MOK { get; set; }
        /// <summary>
        /// Дата начала лечения
        /// </summary>
        [DataMember, Description("Дата начала лечения")]
        public DateTime? DATE_1 { get; set; }
        /// <summary>
        /// Дата окончания лечения
        /// </summary>
        [DataMember, Description("Дата окончания лечения")]
        public DateTime? DATE_2 { get; set; }
        /// <summary>
        /// Наименование услуги
        /// </summary>
        [DataMember, Description("Наименование услуги")]
        public string NAME_TARIF { get; set; }
        /// <summary>
        /// Идентификатор случая
        /// </summary>
        [DataMember, Description("Идентификатор случая")]
        public int SLUCH_ID { get; set; }
        /// <summary>
        /// Код услуги
        /// </summary>
        [DataMember, Description("Код услуги")]
        public string CODE_USL { get; set; }
        /// <summary>
        /// Дата начала услуги
        /// </summary>
        [DataMember, Description("Дата начала услуги")]
        public DateTime? DATE_IN { get; set; }
        /// <summary>
        /// Дата окончания услуги
        /// </summary>
        [DataMember, Description("Дата окончания услуги")]
        public DateTime? DATE_OUT { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        [DataMember, Description("Сумма")]
        public decimal SUMV { get; set; }
        /// <summary>
        /// Идентификатор способа оплаты
        /// </summary>
        [DataMember, Description("Идентификатор способа оплаты")]
        public int IDSP { get; set; }
        /// <summary>
        /// Наименование способа оплаты
        /// </summary>
        [DataMember, Description("Наименование способа оплаты")]
        public string SPNAME { get; set; }

    }
    /// <summary>
    /// Информация о страховании
    /// </summary>
    [DataContract, ServiceBehavior(IgnoreExtensionDataObject = true)]
    public class PersonInfo : IDataContract
    {
        /// <summary>
        /// Код МО
        /// </summary>
        [DataMember]
        public string CODE_MO { get; set; }
        /// <summary>
        /// Наименование СМО
        /// </summary>
        [DataMember]
        public string NAM_SMOK { get; set; }
        /// <summary>
        /// ЕНП
        /// </summary>
        [DataMember]
        public string ENP { get; set; }
        /// <summary>
        /// Серия полиса
        /// </summary>
        [DataMember]
        public string SPOL { get; set; }
        /// <summary>
        /// Номер полиса
        /// </summary>
        [DataMember]
        public string NPOL { get; set; }
        /// <summary>
        ///Дата начала страхования
        /// </summary>
        [DataMember]
        public DateTime? DBEG { get; set; }
        /// <summary>
        /// Дата окончания страхование
        /// </summary>
        [DataMember]
        public DateTime? DEND { get; set; }
        /// <summary>
        /// Дата прекращения страхования
        /// </summary>
        [DataMember]
        public DateTime? DSTOP { get; set; }
        /// <summary>
        /// Наименование МО
        /// </summary>
        [DataMember]
        public string NAM_MOK { get; set; }
        /// <summary>
        /// Тип прикрепления
        /// </summary>
        [DataMember]
        public string LPUAUTO { get; set; }
        /// <summary>
        /// Дата прикрепления
        /// </summary>
        [DataMember]
        public DateTime? LPUDT { get; set; }
        /// <summary>
        /// Дата открепления
        /// </summary>
        [DataMember]
        public DateTime? LPUDX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime? LPUDT_DOC { get; set; }
        /// <summary>
        /// Участок прикрепления
        /// </summary>
        [DataMember]
        public string LPUUCH { get; set; }
        /// <summary>
        /// Подразделение прикрепления
        /// </summary>
        [DataMember]
        public string SUBDIV { get; set; }
        /// <summary>
        /// Наименование подразделение прикрепления
        /// </summary>
        [DataMember]
        public string SUBDIV_NAME { get; set; }
        /// <summary>
        /// Ключи совпадения
        /// </summary>
        [DataMember]
        public string KEYS { get; set; }
        /// <summary>
        /// Получить объект из System.Data.DataRow
        /// </summary>
        /// <param name="row">System.Data.DataRow</param>
        /// <returns></returns>
        public static PersonInfo Get(System.Data.DataRow row)
        {
            try
            {
                var item = new PersonInfo();
                item.CODE_MO = Convert.ToString(row["CODE_MO"]);
                item.NAM_SMOK = Convert.ToString(row["NAM_SMOK"]);
                item.ENP = Convert.ToString(row["ENP"]);
                item.SPOL = Convert.ToString(row["SPOL"]);
                item.NPOL = Convert.ToString(row["NPOL"]);
                if(row["DBEG"] !=DBNull.Value)
                    item.DBEG = Convert.ToDateTime(row["DBEG"]);
                if (row["DEND"] != DBNull.Value)
                    item.DEND = Convert.ToDateTime(row["DEND"]);
                if (row["DSTOP"] != DBNull.Value)
                    item.DSTOP = Convert.ToDateTime(row["DSTOP"]);
                item.NAM_MOK = Convert.ToString(row["NAM_MOK"]);
                item.LPUAUTO = Convert.ToString(row["LPUAUTO"]);
                if (row["LPUDT"] != DBNull.Value)
                    item.LPUDT = Convert.ToDateTime(row["LPUDT"]);
                if (row["LPUDX"] != DBNull.Value)
                    item.LPUDX = Convert.ToDateTime(row["LPUDX"]);
                if (row["LPUDT_DOC"] != DBNull.Value)
                    item.LPUDT_DOC = Convert.ToDateTime(row["LPUDT_DOC"]);
                item.LPUUCH = Convert.ToString(row["LPUUCH"]);
                item.SUBDIV = Convert.ToString(row["SUBDIV"]);
                item.SUBDIV_NAME = Convert.ToString(row["SUBDIV_NAME"]);
                item.KEYS = Convert.ToString(row["KEYS"]);
                return item;
            }
            catch(Exception ex)
            {
                throw new Exception($"Ошбика получения PersonInfo: {ex.Message}", ex); 
            }
        }
    }
    
}
