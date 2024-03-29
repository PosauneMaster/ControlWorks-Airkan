﻿using System;

using BR.AN.PviServices;

using ControlWorks.Services.PVI.Impl;
using ControlWorks.Services.PVI.Models;
using ControlWorks.Services.PVI.Panel;
using ControlWorks.Services.PVI.Task;
using ControlWorks.Services.PVI.Variables;

namespace ControlWorks.Services.PVI
{
    public interface IEventNotifier
    {
        event EventHandler<PviApplicationEventArgs> PviServiceConnected;
        event EventHandler<PviApplicationEventArgs> PviServiceDisconnected;
        event EventHandler<PviApplicationEventArgs> PviServiceError;
        event EventHandler<CpuConnectionArgs> CpuConnected;
        event EventHandler<PviApplicationEventArgs> CpuDisconnected;
        event EventHandler<PviApplicationEventArgs> CpuError;
        event EventHandler<PviApplicationEventArgs> VariableConnected;
        event EventHandler<PviApplicationEventArgs> VariableError;
        event EventHandler<PviApplicationEventArgs> VariableValueChanged;

        event EventHandler<EventArgs> PviManagerInitialized;
        event EventHandler<EventArgs> CpuManangerInitialized;
        event EventHandler<EventArgs> VariableManagerInitialized;
        event EventHandler<TaskLoaderEventArgs> TasksLoaded;

        void OnPviServiceConnected(object sender, PviApplicationEventArgs e);
        void OnPviServiceDisconnected(object sender, PviApplicationEventArgs e);
        void OnPviServiceError(object sender, PviApplicationEventArgs e);
        void OnCpuConnected(object sender, CpuConnectionArgs e);
        void OnCpuDisconnected(object sender, PviApplicationEventArgs e);
        void OnCpuError(object sender, PviApplicationEventArgs e);
        void OnVariableConnected(object sender, PviApplicationEventArgs e);
        void OnVariableError(object sender, PviApplicationEventArgs e);
        void OnVariableValueChanged(object sender, PviApplicationEventArgs e);
        void OnPviManagerInitialized(object sender, EventArgs e);
        void OnCpuManangerInitialized(object sender, EventArgs e);
        void OnVariableManagerInitialized(object sender, EventArgs e);
        void OnTasksLoaded(object sender, TaskLoaderEventArgs e);

    }
    public class EventNotifier : IEventNotifier
    {
        public event EventHandler<PviApplicationEventArgs> PviServiceConnected;
        public event EventHandler<PviApplicationEventArgs> PviServiceDisconnected;
        public event EventHandler<PviApplicationEventArgs> PviServiceError;
        public event EventHandler<CpuConnectionArgs> CpuConnected;
        public event EventHandler<PviApplicationEventArgs> CpuDisconnected;
        public event EventHandler<PviApplicationEventArgs> CpuError;
        public event EventHandler<PviApplicationEventArgs> VariableConnected;
        public event EventHandler<PviApplicationEventArgs> VariableError;
        public event EventHandler<PviApplicationEventArgs> VariableValueChanged;

        public event EventHandler<EventArgs> PviManagerInitialized;
        public event EventHandler<EventArgs> CpuManangerInitialized;
        public event EventHandler<EventArgs> VariableManagerInitialized;
        public event EventHandler<TaskLoaderEventArgs> TasksLoaded;



        public void OnPviServiceConnected(object sender, PviApplicationEventArgs e)
        {
            var temp = PviServiceConnected;
            temp?.Invoke(sender, e);
        }
        public void OnPviServiceDisconnected(object sender, PviApplicationEventArgs e)
        {
            var temp = PviServiceDisconnected;
            temp?.Invoke(sender, e);
        }
        public void OnPviServiceError(object sender, PviApplicationEventArgs e)
        {
            var temp = PviServiceError;
            temp?.Invoke(sender, e);
        }
        public void OnCpuConnected(object sender, CpuConnectionArgs e)
        {
            var temp = CpuConnected;
            temp?.Invoke(sender, e);
        }
        public void OnCpuDisconnected(object sender, PviApplicationEventArgs e)
        {
            var temp = CpuDisconnected;
            temp?.Invoke(sender, e);
        }
        public void OnCpuError(object sender, PviApplicationEventArgs e)
        {
            var temp = CpuError;
            temp?.Invoke(sender, e);
        }

        public void OnVariableConnected(object sender, PviApplicationEventArgs e)
        {
            var temp = VariableConnected;
            temp?.Invoke(sender, e);
        }

        public void OnVariableError(object sender, PviApplicationEventArgs e)
        {
            var temp = VariableError;
            temp?.Invoke(sender, e);
        }

        public void OnVariableValueChanged(object sender, PviApplicationEventArgs e)
        {
            var temp = VariableValueChanged;
            temp?.Invoke(sender, e);
        }

        public void OnPviManagerInitialized(object sender, EventArgs e)
        {
            var temp = PviManagerInitialized;
            temp?.Invoke(sender, e);
        }

        public void OnCpuManangerInitialized(object sender, EventArgs e)
        {
            var temp = CpuManangerInitialized;
            temp?.Invoke(sender, e);
        }
        public void OnVariableManagerInitialized(object sender, EventArgs e)
        {
            var temp = VariableManagerInitialized;
            temp?.Invoke(sender, e);
        }

        public void OnTasksLoaded(object sender, TaskLoaderEventArgs e)
        {
            var temp = TasksLoaded;
            temp?.Invoke(sender, e);
        }

    }

    public class PviApplicationEventArgs : EventArgs
    {
        public ICpuManager CpuManager { get; set; }
        public ICpuWrapper CpuWrapper { get; set; }
        public IVariableManager VariableManager { get; set; }
        public IVariableWrapper VariableWrapper { get; set; }
        public IServiceWrapper ServiceWrapper { get; set; }
        public string Message { get; set; }
    }

    public class CpuConnectionArgs : EventArgs
    {
        public Cpu Cpu { get; set; }
        public string Message { get; set; }
        public CpuConnectionArgs() : this(null, null)
        {

        }
        public CpuConnectionArgs(Cpu cpu): this(cpu, null)
        {

        }
        public CpuConnectionArgs(Cpu cpu, string message)
        {
            Cpu = cpu;
            Message = message;
        }
    }
}
