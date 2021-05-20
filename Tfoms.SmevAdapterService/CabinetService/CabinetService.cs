using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CabinetContract;
using SMEV.WCFContract;

namespace SmevAdapterService.CabinetService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CabinetService : ICabinetService
    {
        IMessageLogger messageLogger;
        IMPAnswer mPAnswer;
        ILogger logger;
        IInforming informing;
        IRegister register;


        public CabinetService(IMessageLogger messageLogger, IMPAnswer mPAnswer, IInforming informing, IRegister register, ILogger logger)
        {
            this.messageLogger = messageLogger;
            this.mPAnswer = mPAnswer;
            this.informing = informing;
            this.register = register;
            this.logger = logger;
        }


        public Response GetData(Person person)
        {
            try
            {
                var response = new Response();
                var from_dt = new DateTime(2008, 1, 1);
                var to_dt = DateTime.Now.Date;
                var id_ms = messageLogger.AddInputMessage(MessageLoggerVS.InputDataSiteTFOMS, "", MessageLoggerStatus.SUCCESS, "", "","");
                messageLogger.SetMedpomDataIn(id_ms, person.FAM, person.IM, person.OT, person.DR, from_dt, to_dt, person.ENP,"");

                if (string.IsNullOrEmpty(person.SNILS) && string.IsNullOrEmpty(person.ENP) && (string.IsNullOrEmpty(person.DOCS) || string.IsNullOrEmpty(person.DOCN)))
                {
                    messageLogger.InsertStatusOut(id_ms, MessageLoggerStatus.SUCCESS, $"Одно из ключевых значений отсутствует (ЕНП,СНИЛС,УДЛ)");
                    throw ThrowException("EmptyKeyData", "Одно из ключевых значений отсутствует (ЕНП,СНИЛС,УДЛ)");
                }
                var validateResults = person.Validate().ToList();
                List<V_MEDPOM_SMEV3Row> out_date;
                if (validateResults.Any())
                {
                    var message = string.Join(";", validateResults.Select(x => x.ErrorMessage));
                    messageLogger.InsertStatusOut(id_ms, MessageLoggerStatus.SUCCESS, $"Ошибка валидации данных информации о гражданине {message}");
                    throw ThrowException("PersonValidationError",  message );
                }
                response.Info = GetPersonInfo(person);
                out_date = GetMedicalCare(person, from_dt, to_dt);
                response.Care = Convert(out_date);
                if(out_date!=null)
                    messageLogger.SetMedpomDataOut(id_ms, out_date.Select(x=> new SLUCH_REF(x.isMTR, x.SLUCH_Z_ID,x.SLUCH_ID,x.USL_ID)).ToList());
                messageLogger.InsertStatusOut(id_ms, MessageLoggerStatus.SUCCESS, response.Info!=null? "Ответ отправлен" : "Гражданин не найден в базе данных застрахованных");
                return response;
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.AddLog($"CabinetService: {ex.Message}  {ex.StackTrace}", LogType.Error);
                throw ThrowException("ServiceError", "Ошибка GetData");
            }
        }



        /// <summary>
        /// Возвращает информацию о страховке и т.п. 
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private PersonInfo GetPersonInfo(Person person)
        {
            return register.GetPersonInfo(person).FirstOrDefault();
        }

        /// <summary>
        /// Возвращает список услуг
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private List<V_MEDPOM_SMEV3Row> GetMedicalCare(Person person,DateTime From, DateTime To)
        {
            var rows = mPAnswer.GetData(person.FAM, person.IM, person.OT, person.DR, person.ENP, From, To);
            return rows;
        }

        private List<MedicalCare> Convert(List<V_MEDPOM_SMEV3Row> rows)
        {
                return rows.Select(x => new MedicalCare
                {
                    CODE_USL = x.CODE_USL,
                    DATE_1 = x.DATE_1,
                    DATE_2 = x.DATE_2,
                    DATE_IN = x.DATE_IN,
                    DATE_OUT = x.DATE_OUT,
                    IDSP = x.IDSP,
                    NAME_TARIF = x.NAME_USL,
                    NAM_MOK = x.NAM_MOK,
                    SLUCH_ID = x.SLUCH_ID,
                    SPNAME = x.IDSP_NAME,
                    SUMV = x.SUMP_USL,
                    CODE_MO = x.CODE_MO
                }).ToList();
        }

        private FaultException ThrowException(string errorCode, string errorMessage)
        {
            return new FaultException(errorMessage, new FaultCode(errorCode));
        }

        public bool InformingInsertRecord(string ENP)
        {
            try
            {
                if (informing != null)
                {
                    informing.AddInforming(ENP);
                    return true;
                }
                throw new Exception("Сервис информирование не доступен");
            }
            catch(Exception ex)
            {
                logger.AddLog($"Ошибка вставки информирования: {ex.Message}", LogType.Error);
                throw ThrowException("ServiceError", "Ошибка InformingInsertRecord");
            }
        }

        public bool InformingValidate(string ENP)
        {
            try
            {
                if (informing != null)
                {
                    return informing.Validate(ENP);
                }
                throw new Exception("Сервис информирование не доступен");
            }
            catch (Exception ex)
            {
                logger.AddLog($"Ошибка вставки информирования: {ex.Message}", LogType.Error);
                throw ThrowException("ServiceError", "Ошибка InformingValidate");
            }
        }

        public bool Ping()
        {
            return true;
        }
    }
}
