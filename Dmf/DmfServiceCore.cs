using System;
using System.Linq;
using System.Xml.Linq;
using DMB.Core.Elements;

namespace DMB.Core.Dmf
{
    public class DmfServiceCore<DS, DF, DC>
        where DS : DatasetModelCore<DF>
        where DF : DatasetFieldModelCore, new()
        where DC : DataGridColumnModelCore
    {
        public void Save(ModuleStateCore state, string filePath)
        {
            var rootGrid = state.GetMainGrid();
            if (rootGrid == null) return;

            var module = new XElement("Module",
                new XAttribute("version", DmfConstants.CurrentVersion));

            module.Add(SaveGrid(rootGrid));
            module.Add(SaveDatasets(state));
            module.Add(SaveVariables(state));

            var doc = new XDocument(module);
            doc.Save(filePath);
        }

        protected virtual GridModelCore InitiateGridModel(ModuleStateCore state)
        {
            var grid = new GridModelCore(state);
            return grid;
        }

        protected virtual RowModelCore InitiateRowModel(ModuleStateCore state)
        {
            var row = new RowModelCore(state);
            return row;
        }

        public GridModelCore? Load(ModuleStateCore state, string filePath)
        {
            var doc = XDocument.Load(filePath);

            var version = doc.Root?.Attribute("version")?.Value;
            if (version != DmfConstants.CurrentVersion)
            {
                throw new Exception($"Unsupported DMF version: {version}");
            }

            state.Clear();

            // Keep language available as a global for expressions
            state.Globals["Language"] = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            this.LoadDatasets(state, doc.Root?.Element("Datasets"));
            this.LoadVariables(state, doc.Root?.Element("Variables"));

            var gridNode = doc.Root?.Element("Grid");
            if (gridNode == null)
                return null;

            var rootGrid = LoadGrid(state, gridNode, null);
            state.SetMainGrid(rootGrid);

            state.RaiseStateChanged();
            return rootGrid;
        }

        private GridModelCore LoadGrid(ModuleStateCore state, XElement node, CellModelCore? parentCell)
        {
            var grid = this.InitiateGridModel(state);
            grid.ParentCell = parentCell;

            state.Register(grid);
            DmfReflect.ReadAll(node, grid);

            foreach (var rowNode in node.Elements("Row"))
            {
                var row = LoadRow(state, rowNode, grid);
                grid.Rows.Add(row);
            }

            if (parentCell != null)
                parentCell.Element = grid;

            return grid;
        }

        private RowModelCore LoadRow(ModuleStateCore state, XElement node, GridModelCore parentGrid)
        {
            var row = this.InitiateRowModel(state);
            row.ParentGrid = parentGrid;

            state.Register(row);
            DmfReflect.ReadAll(node, row);

            foreach (var cellNode in node.Elements("Cell"))
            {
                var cell = LoadCell(state, cellNode, row);
                row.Cells.Add(cell);
            }

            return row;
        }

        protected virtual CellModelCore InitiateCellModel(ModuleStateCore state, RowModelCore parentRow)
        {
            var cell = new CellModelCore(state, parentRow);
            return cell;
        }

        private CellModelCore LoadCell(ModuleStateCore state, XElement node, RowModelCore parentRow)
        {
            var cell = this.InitiateCellModel(state, parentRow);

            state.Register(cell);
            DmfReflect.ReadAll(node, cell);

            // Load expandable properties first, then load the single child element (if any)
            ElementModel? element = null;

            foreach (var child in node.Elements())
            {
                // IMPORTANT: model = cell, node = child
                if (this.TryLoadExpandableProperty(cell, child))
                    continue;

                // First non-expandable child is the actual element inside the cell
                element = LoadElement(state, child, cell);
                break; // cell contains ONE element only (as per current design)
            }

            cell.Element = element;
            return cell;
        }

        protected virtual ButtonModelCore InitiateButtonModel(ModuleStateCore state)
        {
            var button = new ButtonModelCore(state);
            return button;
        }

        protected virtual TextBlockModelCore InitiateTextBlockModel(ModuleStateCore state)
        {
            var tb = new TextBlockModelCore(state);
            return tb;
        }

        protected virtual TextInputModelCore InitiateTextInputModel(ModuleStateCore state)
        {
            var ti = new TextInputModelCore(state);
            return ti;
        }

        protected virtual SelectModelCore InitiateSelectModel(ModuleStateCore state)
        {
            var select = new SelectModelCore(state);
            return select;
        }

        protected virtual SwitchModelCore InitiateSwitchModel(ModuleStateCore state)
        {
            var sw = new SwitchModelCore(state);
            return sw;
        }

        protected virtual CheckBoxModelCore InitiateCheckBoxModel(ModuleStateCore state)
        {
            var cb = new CheckBoxModelCore(state);
            return cb;
        }

        protected virtual DatePickerModelCore InitiateDatePickerModel(ModuleStateCore state)
        {
            var dp = new DatePickerModelCore(state);
            return dp;
        }

        protected virtual TimePickerModelCore InitiateTimePickerModel(ModuleStateCore state)
        {
            var tp = new TimePickerModelCore(state);
            return tp;
        }

        protected virtual ImageModelCore InitiateImageModel(ModuleStateCore state)
        {
            var img = new ImageModelCore(state);
            return img;
        }

        protected virtual DataGridModelCore<DC> InitiateDataGridModel(ModuleStateCore state)
        {
            var dg = new DataGridModelCore<DC>(state);
            return dg;
        }

        protected virtual DS InitiateDatasetModel(ModuleStateCore state) =>
            (DS)Activator.CreateInstance(typeof(DS), state)!;

        protected virtual DF InitiateDatasetFieldModel() => new DF();

        protected virtual DatasetRowModelCore InitiateDatasetRowModel()
        {
            var row = new DatasetRowModelCore();
            return row;
        }

        protected virtual VariableModelCore InitiateVariableModel(ModuleStateCore state)
        {
            var variable = new VariableModelCore(state);
            return variable;
        }

        public ElementModel? LoadElement(ModuleStateCore state, XElement node)
        {
            if (node.Name.LocalName == "Grid")
                return LoadGrid(state, node, parentCell: null);

            ElementModel el;

            switch (node.Name.LocalName)
            {
                case "Button":
                    el = this.InitiateButtonModel(state);
                    break;

                case "TextBlock":
                    el = this.InitiateTextBlockModel(state);
                    break;

                case "TextInput":
                    el = this.InitiateTextInputModel(state);
                    break;

                case "Select":
                    el = this.InitiateSelectModel(state);
                    break;

                case "Switch":
                    el = this.InitiateSwitchModel(state);
                    break;

                case "CheckBox":
                    el = this.InitiateCheckBoxModel(state);
                    break;

                case "DatePicker":
                    el = this.InitiateDatePickerModel(state);
                    break;

                case "TimePicker":
                    el = this.InitiateTimePickerModel(state);
                    break;

                case "Image":
                    el = this.InitiateImageModel(state);
                    break;

                case "DataGrid":
                    el = this.InitiateDataGridModel(state);
                    break;

                default:
                    return null;
            }

            el.ParentCell = null;

            state.Register(el);
            DmfReflect.ReadAll(node, el);

            return el;
        }

        public ElementModel? LoadElement(ModuleStateCore state, XElement node, CellModelCore parentCell)
        {
            if (node.Name.LocalName == "Grid")
                return LoadGrid(state, node, parentCell);

            var el = LoadElement(state, node);
            if (el == null)
                return null;

            el.ParentCell = parentCell;
            return el;
        }

        private void LoadDatasets(ModuleStateCore state, XElement? datasetsNode)
        {
            if (datasetsNode == null) return;

            foreach (var dsNode in datasetsNode.Elements("Dataset"))
            {
                var ds = this.InitiateDatasetModel(state);
                state.Register(ds);
                DmfReflect.ReadAll(dsNode, ds);

                // Fields
                var fieldsNode = dsNode.Element("Fields");
                if (fieldsNode != null)
                {
                    foreach (var fNode in fieldsNode.Elements("Field"))
                    {
                        var f = this.InitiateDatasetFieldModel();
                        DmfReflect.ReadAll(fNode, f);
                        ds.Fields.Add(f);
                    }
                }

                // Rows
                var rowsNode = dsNode.Element("Rows");
                if (rowsNode != null)
                {
                    foreach (var rNode in rowsNode.Elements("Row"))
                    {
                        var row = this.InitiateDatasetRowModel();
                        foreach (var c in rNode.Elements("C"))
                        {
                            var name = (string?)c.Attribute("n");
                            var val = (string?)c.Attribute("v");
                            if (!string.IsNullOrWhiteSpace(name))
                                row.Values[name] = val;
                        }
                        ds.Rows.Add(row);
                    }
                }
            }
        }

        private void LoadVariables(ModuleStateCore state, XElement? varsNode)
        {
            if (varsNode == null) return;

            foreach (var vNode in varsNode.Elements("Var"))
            {
                var v = this.InitiateVariableModel(state);
                state.Register(v);
                DmfReflect.ReadAll(vNode, v);
            }
        }

        private XElement SaveGrid(GridModelCore grid)
        {
            var node = NewNode(grid);
            DmfReflect.WriteAll(node, grid);

            foreach (var row in grid.Rows)
                node.Add(SaveRow(row));

            return node;
        }

        private XElement SaveRow(RowModelCore row)
        {
            var node = NewNode(row);
            DmfReflect.WriteAll(node, row);

            foreach (var cell in row.Cells)
                node.Add(SaveCell(cell));

            return node;
        }

        private XElement SaveCell(CellModelCore cell)
        {
            var node = NewNode(cell);
            DmfReflect.WriteAll(node, cell);

            if (cell.Element != null)
                node.Add(this.SaveElementNode(cell.Element));

            return node;
        }

        public XElement SaveElementNode(ElementModel el)
        {
            if (el is GridModelCore g)
                return SaveGrid(g);

            var node = NewNode(el);
            DmfReflect.WriteAll(node, el);
            return node;
        }

        private static XElement NewNode(object obj)
        {
            var typeName = obj.GetType().Name;
            if (typeName.EndsWith("Model"))
                typeName = typeName.Substring(0, typeName.Length - 5);

            return new XElement(typeName);
        }

        private XElement SaveDatasets(ModuleStateCore state)
        {
            var root = new XElement("Datasets");

            var datasets = state.AllItems.OfType<DS>().ToList();
            foreach (var ds in datasets)
            {
                var dsNode = new XElement("Dataset");
                DmfReflect.WriteAll(dsNode, ds);

                // Fields
                var fieldsNode = new XElement("Fields");
                foreach (var f in ds.Fields)
                {
                    var fNode = new XElement("Field");
                    DmfReflect.WriteAll(fNode, f);
                    fieldsNode.Add(fNode);
                }
                dsNode.Add(fieldsNode);

                // Rows
                var rowsNode = new XElement("Rows");
                foreach (var row in ds.Rows)
                {
                    var rNode = new XElement("Row");
                    foreach (var kv in row.Values)
                        rNode.Add(new XElement("C",
                            new XAttribute("n", kv.Key),
                            new XAttribute("v", kv.Value?.ToString() ?? "")));
                    rowsNode.Add(rNode);
                }
                dsNode.Add(rowsNode);

                root.Add(dsNode);
            }

            return root;
        }

        private XElement SaveVariables(ModuleStateCore state)
        {
            var root = new XElement("Variables");

            var vars = state.AllItems.OfType<VariableModelCore>().ToList();
            foreach (var v in vars)
            {
                var node = new XElement("Var");
                DmfReflect.WriteAll(node, v);
                root.Add(node);
            }

            return root;
        }

        private bool TryLoadExpandableProperty(object model, XElement node)
        {
            var modelType = model.GetType();

            // Match by property name == node name
            var propInfo = modelType.GetProperty(node.Name.LocalName);
            if (propInfo is null)
                return false;

            // Must have [ExpandableProperty]
            var hasAttr = Attribute.IsDefined(propInfo, typeof(ExpandablePropertyAttribute));
            if (!hasAttr)
                return false;

            var propObj = propInfo.GetValue(model);

            // Ensure instance exists (or require constructor initialization)
            if (propObj is null)
            {
                if (propInfo.SetMethod is null)
                    throw new InvalidOperationException(
                        $"Property '{modelType.Name}.{propInfo.Name}' is null and has no setter. Initialize it in constructor.");

                propObj = Activator.CreateInstance(propInfo.PropertyType)
                         ?? throw new InvalidOperationException(
                             $"Cannot create instance of '{propInfo.PropertyType.FullName}' for '{propInfo.Name}'.");

                propInfo.SetValue(model, propObj);
            }

            // Prefer explicit XML loader
            if (propObj is IXmlNodeSerializable xmlLoadable)
            {
                xmlLoadable.ReadXml(node);
                return true;
            }

            // Fallback: ValueMode/Value attributes (common pattern)
            LoadByCommonValueModeValue(propObj, node);
            return true;
        }

        private void LoadByCommonValueModeValue(object propObj, XElement node)
        {
            var vmProp = propObj.GetType().GetProperty("ValueMode");
            var vProp = propObj.GetType().GetProperty("Value");

            var valueMode = (string?)node.Attribute("valueMode");
            var valueText = (string?)node.Attribute("value");

            // 1) ValueMode (usually enum)
            if (vmProp != null && !string.IsNullOrWhiteSpace(valueMode))
            {
                var enumVal = Enum.Parse(vmProp.PropertyType, valueMode!, ignoreCase: true);
                vmProp.SetValue(propObj, enumVal);
            }

            // 2) Value
            if (vProp == null || valueText is null)
                return;

            var targetType = vProp.PropertyType;

            // Handle Nullable<T>
            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying != null)
                targetType = underlying;

            object converted;

            if (targetType == typeof(string))
            {
                converted = valueText;
            }
            else if (targetType.IsEnum)
            {
                // IMPORTANT: parse enum names like "Start", "Center"
                converted = Enum.Parse(targetType, valueText, ignoreCase: true);
            }
            else if (targetType == typeof(bool))
            {
                converted = bool.Parse(valueText);
            }
            else if (targetType == typeof(int))
            {
                converted = int.Parse(valueText);
            }
            else if (targetType == typeof(double))
            {
                converted = double.Parse(valueText, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (targetType == typeof(float))
            {
                converted = float.Parse(valueText, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (targetType == typeof(decimal))
            {
                converted = decimal.Parse(valueText, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                // Fallback for other primitives
                converted = Convert.ChangeType(valueText, targetType, System.Globalization.CultureInfo.InvariantCulture);
            }

            vProp.SetValue(propObj, converted);
        }
    }

    public interface IXmlNodeSerializable
    {
        void ReadXml(XElement node);
    }
}