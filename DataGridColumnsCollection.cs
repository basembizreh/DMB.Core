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
        private readonly ModuleDocumentCore _moduleDocument;
        private string _dataGridId;

        public DataGridColumnsCollection(ModuleDocumentCore moduleDocument, string dataGridId)
        {
            this._moduleDocument = moduleDocument;
            this._dataGridId = dataGridId;
        }

        public string DataGridId
        {

            get { return this._dataGridId; }
            set { this._dataGridId = value; }
        }

        public event Action? Changed;

        public void RaiseChangedEvent()
        {
            this.Changed?.Invoke();
        }

        public bool SuspendChanged { get; set; } 

        protected override void InsertItem(int index, T item)
        {
            this.SetupItem(item);
            base.InsertItem(index, item);
            if (!SuspendChanged) Changed?.Invoke();
        }

        protected override void SetItem(int index, T item)
        {
            this.SetupItem(item);
            base.SetItem(index, item);
            if (!SuspendChanged) Changed?.Invoke();
        }

        private void SetupItem(T item)
        {
            item.ModuleDocumentCore = this._moduleDocument;
            item.DataGridId = _dataGridId;

            if (!this._moduleDocument.IsLoading &&
                item.CellTemplate is not null &&
                    item.CellTemplate.Enabled &&
                    item.CellTemplate.Content is null)
            {
                item.CellTemplate.Content = item.InstantiateAndRegisterCellContentGrid();
            }
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
