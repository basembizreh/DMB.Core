using DMB.Core.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core
{
    public sealed class DataGridColumnsCollection<T> : Collection<T>
    where T : DataGridColumnModelCore
    {
        private readonly ModuleStateCore _moduleStatus;
        private string _dataGridId;

        public DataGridColumnsCollection(ModuleStateCore moduleState, string dataGridId)
        {
            _moduleStatus = moduleState;
            _dataGridId = dataGridId;
        }

        public ModuleStateCore? ModuleState => _moduleStatus;

        public event Action? Changed;

        public void RaiseChangedEvent()
        {
            this.Changed?.Invoke();
        }

        public bool SuspendChanged { get; set; } // NEW

        protected override void InsertItem(int index, T item)
        {
            item.ModuleStateCore = _moduleStatus;
            item.DataGridId = _dataGridId;
            base.InsertItem(index, item);
            if (!SuspendChanged) Changed?.Invoke();
        }

        protected override void SetItem(int index, T item)
        {
            item.ModuleStateCore = _moduleStatus;
            item.DataGridId = _dataGridId;
            base.SetItem(index, item);
            if (!SuspendChanged) Changed?.Invoke();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            if (!SuspendChanged) Changed?.Invoke();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            if (!SuspendChanged) Changed?.Invoke();
        }
    }
}
