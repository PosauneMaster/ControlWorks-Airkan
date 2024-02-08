using BR.AN.PviServices;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Task = BR.AN.PviServices.Task;

namespace ControlWorks.Services.PVI.Task
{
    public class TaskLoader
    {
        private static readonly AutoResetEvent AutoReset = new AutoResetEvent(true);
        private readonly CpuDataService _dataService;
        private readonly ConcurrentDictionary<string, bool> _taskStatus;
        private readonly IEventNotifier _eventNotifier;

        public event EventHandler<TaskLoaderEventArgs> TaskVariablesLoaded;

        public TaskLoader(CpuDataService dataService, IEventNotifier eventNotifier)
        {
            _dataService = dataService;
            _taskStatus = new ConcurrentDictionary<string, bool>();
            _eventNotifier = eventNotifier;
        }
        public void LoadTasks(Cpu cpu)
        {
            AutoReset.WaitOne(2000);

            cpu.Variables.Uploaded += Cpu_variables_Uploaded;
            cpu.Variables.Upload();

        }

        private void Cpu_variables_Uploaded(object sender, PviEventArgs e)
        {
            var collection = sender as VariableCollection;

            if (collection?.Parent is Cpu cpu)
            {
                foreach (DictionaryEntry variable in collection)
                {
                    var v = variable.Value as Variable;
                    _dataService.AddVariable(cpu.Name, VariableScope.Global, v.Name, v.IECDataType.ToString());
                }

                cpu.Tasks.Uploaded += Tasks_Uploaded;
                cpu.Tasks.Upload();
            }
        }

        private void OnTaskVariablesLoaded(string cpuName)
        {
            _eventNotifier.OnTasksLoaded(this, new TaskLoaderEventArgs(cpuName));
            var temp = TaskVariablesLoaded;
            temp?.Invoke(this, new TaskLoaderEventArgs(cpuName));
            AutoReset.Set();
        }

        private void Tasks_Uploaded(object sender, PviEventArgs e)
        {
            var taskCollection = sender as TaskCollection;
            var cpu = taskCollection.Parent as Cpu;

            foreach (DictionaryEntry task in taskCollection)
            {
                if (!_taskStatus.ContainsKey(task.Key.ToString()))
                {
                    _taskStatus.TryAdd(task.Key.ToString(), false);
                }
            }

            System.Threading.Tasks.Task.Run(() => WaitForTasks(cpu));

            foreach (DictionaryEntry task in taskCollection)
            {
                var t = task.Value as BR.AN.PviServices.Task;
                t.Connected += task_Connected; ;
                t.Connect();
            }
        }

        private void task_Connected(object sender, PviEventArgs e)
        {
            var task = sender as BR.AN.PviServices.Task;

            task.Variables.Uploaded += Variables_Uploaded;
            task.Variables.Upload();
        }

        private void Variables_Uploaded(object sender, PviEventArgs e)
        {
            var variableCollection = sender as VariableCollection;
            var task = variableCollection.Parent as BR.AN.PviServices.Task;
            var cpu = task.Parent as Cpu;

            foreach (DictionaryEntry variable in variableCollection)
            {
                var v = variable.Value as Variable;
                _dataService.AddVariable(cpu.Name, VariableScope.Task, v.Name, v.IECDataType.ToString(), task.Name);
            }

            if (_taskStatus.ContainsKey(task.Name))
            {
                _taskStatus.TryUpdate(task.Name, true, false);
            }
        }

        private void WaitForTasks(Cpu cpu)
        {
            var counter = 0;
            var complete = false;
            while (!complete && counter < 50000)
            {
                complete = !(_taskStatus.Values.Contains(false));
                System.Threading.Thread.Sleep(50);
                Interlocked.Increment(ref counter);
            }

            OnTaskVariablesLoaded(cpu.Name);
        }
    }

    public class TaskLoaderEventArgs : EventArgs
    {
        public string CpuName { get; set; }

        public TaskLoaderEventArgs()
        {
        }

        public TaskLoaderEventArgs(string cpuName)
        {
            CpuName = cpuName;
        }

    }
}
