//using AutoMapper;

//using ControlWorks.Common;
//using ControlWorks.Services.PVI.Pvi;
//using ControlWorks.Services.Rest.Models;

//using LiteDB;

//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;

//namespace ControlWorks.Services.Rest.Processors
//{
//    public interface IAlarmProcessor
//    {
//        void AddAlarm(string description);
//        Task<AlarmCollection> GetActiveAlarms();
//        Task Delete(int id);
//        Task DeleteAll();

//    }

//    public class AlarmProcessor : IAlarmProcessor
//    {
//        private readonly string _alarms = "alarms";

//        private readonly IMapper _mapper;
//        private IPviApplication _pviApplication;

//        public AlarmProcessor()
//        {
//        }

//        public AlarmProcessor(IMapper mapper, IPviApplication pviApplication)
//        {
//            _pviApplication = pviApplication;
//            _mapper = mapper;
//        }

//        public void AddAlarm(string description)
//        {
//            try
//            {
//                using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
//                {
//                    var alarmsCol = db.GetCollection<Alarm>(_alarms);

//                    var alarm = new Alarm(description);
//                    alarmsCol.Insert(alarm);
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"AlarmProcessor.AddAlarm. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        public async Task<AlarmCollection> GetActiveAlarms()
//        {
//            try
//            {
//                var alarmInfoList = await Task.Run( () => _pviApplication.GetAlarms());

//                var alarmCollection = new AlarmCollection();
//                foreach (var alarmInfo in alarmInfoList)
//                {
//                    alarmCollection.AddAlarm(new Alarm(alarmInfo.AlarmId, alarmInfo.Description, alarmInfo.Timestamp));
//                }

//                return alarmCollection;
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"AlarmProcessor.GetActiveAlarms. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        public async Task Delete(int id)
//        {
//            try
//            {
//                var commandName = "DeleteAlarm";
//                await Task.Run(() => _pviApplication.SendCommand(String.Empty, commandName, id.ToString()));
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"AlarmProcessor.Delete. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        public async Task DeleteAll()
//        {
//            try
//            {
//                var commandName = "DeleteAllAlarms";
//                await Task.Run(() => _pviApplication.SendCommand(String.Empty, commandName, String.Empty));
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"AlarmProcessor.DeleteAll. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }


//    }
//}

