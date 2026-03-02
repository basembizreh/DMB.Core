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
            this._moduleStatus = moduleState;
            this._dataGridId = dataGridId;
            if (this.ModuleState != null)
            {
                this.ModuleState.ItemIdChanged -= this.ModuleState_ItemIdChanged;
                this.ModuleState.ItemIdChanged += this.ModuleState_ItemIdChanged;
            }
        }

        private void ModuleState_ItemIdChanged(IModuleItem item, string oldId, string newId)
        {
            if (oldId != this._dataGridId)
                return;

            this._dataGridId = newId;
            foreach (var col in this)
            {
                col.DataGridId = newId;
            }
        }

        public ModuleStateCore? ModuleState => this._moduleStatus;

        public event Action? Changed;

        public bool UniqueIgnoreCase { get; set; } = true;

        public Func<DataGridColumnModelCore, (bool ok, string error)>? Validator { get; set; }

        protected override void InsertItem(int index, T item)
        {
            item.ModuleStateCore = this._moduleStatus;
            item.DataGridId = this._dataGridId;
            base.InsertItem(index, item);
            this.Changed?.Invoke();
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            this.Changed?.Invoke();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            this.Changed?.Invoke();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            this.Changed?.Invoke();
        }
    }
}
