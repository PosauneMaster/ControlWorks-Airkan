using System.Linq;
using System.Threading.Tasks;

using ControlWorks.Common;
using ControlWorks.Services.Rest.Models;
using ControlWorks.Services.Rest.Processors;

using LiteDB;

namespace ControlWorks.Services.Rest.Mocks
{
    public class AlarmProcessorMock : IAlarmProcessor
    {
        private string _connectionString = ConfigurationProvider.MocksDbConnectionString;
        private readonly string _dbName = "MockAlarms";

        public void AddAlarm(string description)
        {
            using (var db = new LiteDatabase(ConfigurationProvider.MocksDbConnectionString))
            {
                var alarmCol = db.GetCollection<Alarm>(_dbName);
                var currentAlarm = alarmCol.Find(a => a.Description == description).FirstOrDefault();
                if (currentAlarm == null)
                {
                    alarmCol.Insert(new Alarm(description));
                }
            }
        }

        public async Task Delete(int id)
        {
            await Task.Run(() => DeleteById(id));
        }

        private void DeleteById(int id)
        {
            using (var db = new LiteDatabase(ConfigurationProvider.MocksDbConnectionString))
            {
                var alarmCol = db.GetCollection<Alarm>(_dbName);
                alarmCol?.Delete(id);
            }
        }

        private void DeleteAllAlarms()
        {
            using (var db = new LiteDatabase(ConfigurationProvider.MocksDbConnectionString))
            {
                var alarmCol = db.GetCollection<Alarm>(_dbName);
                alarmCol?.DeleteAll();
            }
        }


        public async Task DeleteAll()
        {
            await Task.Run(DeleteAllAlarms);
        }

        public async Task<AlarmCollection> GetActiveAlarms()
        {
            var result = await Task.Run(GetAlarms);
            return result;
        }

        private AlarmCollection GetAlarms()
        {
            var alarmCollection = new AlarmCollection();

            using (var db = new LiteDatabase(ConfigurationProvider.MocksDbConnectionString))
            {
                var alarmCol = db.GetCollection<Alarm>(_dbName);
                foreach (var alarm in alarmCol.FindAll())
                {
                    alarmCollection.AddAlarm(alarm);
                }
            }

            return alarmCollection;
        }
    }
}
