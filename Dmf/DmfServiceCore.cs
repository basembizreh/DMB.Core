using DMB.Core.Elements;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace DMB.Core.Dmf
{
    public class DmfServiceCore<DS, DF, DC>
        where DS : DatasetModelCore<DF>
        where DF : DatasetFieldModelCore, new()
        where DC : DataGridColumnModelCore
    {
        public void Save(ModuleDocumentCore document, string filePath)
        {
            var rootGrid = document.GetMainGrid();
            if (rootGrid == null) return;

            var module = new XElement("Module",
                new XAttribute("version", DmfConstants.CurrentVersion));

            module.Add(this.SaveGrid(rootGrid));
            module.Add(this.SaveDatasets(document));
            module.Add(this.SaveVariables(document));

            var doc = new XDocument(module);
            doc.Save(filePath);
        }

        protected virtual GridModelCore InitiateGridModel(ModuleDocumentCore document)
        {
            var grid = new GridModelCore(document);
            return grid;
        }

        protected virtual RowModelCore InitiateRowModel(ModuleDocumentCore document)
        {
            var row = new RowModelCore(document);
            return row;
        }

        public GridModelCore? LoadFromXml(ModuleDocumentCore document, string xml, bool isPaste)
        {
            var doc = XDocument.Parse(xml);

            var version = doc.Root?.Attribute("version")?.Value;
            if (version != DmfConstants.CurrentVersion)
                throw new Exception($"Unsupported DMF version: {version}");

            document.Clear();
            document.Globals["Language"] = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            this.LoadDatasets(document, doc.Root?.Element("Datasets"), isPaste);
            this.LoadVariables(document, doc.Root?.Element("Variables"), isPaste);

            var gridNode = doc.Root?.Element("Grid");
            if (gridNode == null)
                return null;

            var rootGrid = LoadGrid(document, gridNode, null, isPaste);
            document.SetMainGrid(rootGrid);
            document.RaiseStateChanged();

            return rootGrid;
        }

        public string SaveToXml(ModuleDocumentCore document)
        {
            var rootGrid = document.GetMainGrid();
            if (rootGrid == null)
                return "";

            var module = new XElement("Module",
                new XAttribute("version", DmfConstants.CurrentVersion));

            module.Add(this.SaveGrid(rootGrid));
            module.Add(this.SaveDatasets(document));
            module.Add(this.SaveVariables(document));

            var doc = new XDocument(module);
            return doc.ToString();
        }

        public GridModelCore? Load(ModuleDocumentCore document, string filePath, bool isPaste)
        {
            var doc = XDocument.Load(filePath);

            var version = doc.Root?.Attribute("version")?.Value;
            if (version != DmfConstants.CurrentVersion)
            {
                throw new Exception($"Unsupported DMF version: {version}");
            }

            document.Clear();

            // Keep language available as a global for expressions
            document.Globals["Language"] = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            this.LoadDatasets(document, doc.Root?.Element("Datasets"), isPaste);
            this.LoadVariables(document, doc.Root?.Element("Variables"), isPaste);

            var gridNode = doc.Root?.Element("Grid");
            if (gridNode == null)
                return null;

            var rootGrid = LoadGrid(document, gridNode, null, isPaste);
            document.SetMainGrid(rootGrid);

            document.RaiseStateChanged();
            return rootGrid;
        }

        private GridModelCore LoadGrid(ModuleDocumentCore document, XElement node, CellModelCore? parentCell
            , bool isPaste)
        {
            var grid = this.InitiateGridModel(document);
            grid.ParentCell = parentCell;

            DmfReflect.ReadAll(node, grid, isPaste);
            document.Register(grid, isPaste);

            foreach (var rowNode in node.Elements("Row"))
            {
                var row = LoadRow(document, rowNode, grid, isPaste);
                grid.Rows.Add(row);
            }

            if (parentCell != null)
                parentCell.Element = grid;

            return grid;
        }

        private RowModelCore LoadRow(ModuleDocumentCore document, XElement node, GridModelCore parentGrid
            , bool isPaste)
        {
            var row = this.InitiateRowModel(document);
            row.ParentGrid = parentGrid;

            DmfReflect.ReadAll(node, row, isPaste);
            document.Register(row, isPaste);

            foreach (var cellNode in node.Elements("Cell"))
            {
                var cell = LoadCell(document, cellNode, row, isPaste);
                row.Cells.Add(cell);
            }

            return row;
        }

        protected virtual CellModelCore InitiateCellModel(ModuleDocumentCore document, RowModelCore parentRow)
        {
            var cell = new CellModelCore(document, parentRow);
            return cell;
        }

        private CellModelCore LoadCell(ModuleDocumentCore document, XElement node, RowModelCore parentRow
            , bool isPaste)
        {
            var cell = this.InitiateCellModel(document, parentRow);

            DmfReflect.ReadAll(node, cell, isPaste);
            document.Register(cell, isPaste);

            // Load expandable properties first, then load the single child element (if any)
            ElementModel? element = null;

            foreach (var child in node.Elements())
            {
                // IMPORTANT: model = cell, node = child
                if (this.TryLoadExpandableProperty(cell, child))
                    continue;

                // First non-expandable child is the actual element inside the cell
                element = LoadElement(document, child, cell, isPaste);
                break; // cell contains ONE element only (as per current design)
            }

            cell.Element = element;
            return cell;
        }

        protected virtual ButtonModelCore InitiateButtonModel(ModuleDocumentCore document)
        {
            var button = new ButtonModelCore(document);
            return button;
        }

        protected virtual TextBlockModelCore InitiateTextBlockModel(ModuleDocumentCore document)
        {
            var tb = new TextBlockModelCore(document);
            return tb;
        }

        protected virtual TextInputModelCore InitiateTextInputModel(ModuleDocumentCore document)
        {
            var ti = new TextInputModelCore(document);
            return ti;
        }

        protected virtual SelectModelCore InitiateSelectModel(ModuleDocumentCore document)
        {
            var select = new SelectModelCore(document);
            return select;
        }

        protected virtual SwitchModelCore InitiateSwitchModel(ModuleDocumentCore document)
        {
            var sw = new SwitchModelCore(document);
            return sw;
        }

        protected virtual CheckBoxModelCore InitiateCheckBoxModel(ModuleDocumentCore document)
        {
            var cb = new CheckBoxModelCore(document);
            return cb;
        }

        protected virtual DatePickerModelCore InitiateDatePickerModel(ModuleDocumentCore document)
        {
            var dp = new DatePickerModelCore(document);
            return dp;
        }

        protected virtual TimePickerModelCore InitiateTimePickerModel(ModuleDocumentCore document)
        {
            var tp = new TimePickerModelCore(document);
            return tp;
        }

        protected virtual ImageModelCore InitiateImageModel(ModuleDocumentCore document)
        {
            var img = new ImageModelCore(document);
            return img;
        }

        protected virtual DataGridModelCore<DC> InitiateDataGridModel(ModuleDocumentCore document)
        {
            var dg = new DataGridModelCore<DC>(document);
            return dg;
        }

        protected virtual DS InitiateDatasetModel(ModuleDocumentCore document) =>
            (DS)Activator.CreateInstance(typeof(DS), document)!;

        protected virtual DF InitiateDatasetFieldModel() => new DF();

        protected virtual DatasetRowModelCore InitiateDatasetRowModel()
        {
            var row = new DatasetRowModelCore();
            return row;
        }

        protected virtual VariableModelCore InitiateVariableModel(ModuleDocumentCore document)
        {
            var variable = new VariableModelCore(document);
            return variable;
        }

        public ElementModel? LoadElement(ModuleDocumentCore document, XElement node, bool isPaste)
        {
            if (node.Name.LocalName == "Grid")
                return LoadGrid(document, node, parentCell: null, isPaste);

            ElementModel el;

            switch (node.Name.LocalName)
            {
                case "Button":
                    el = this.InitiateButtonModel(document);
                    break;
                case "TextBlock":
                    el = this.InitiateTextBlockModel(document);
                    break;
                case "TextInput":
                    el = this.InitiateTextInputModel(document);
                    break;
                case "Select":
                    el = this.InitiateSelectModel(document);
                    break;
                case "Switch":
                    el = this.InitiateSwitchModel(document);
                    break;
                case "CheckBox":
                    el = this.InitiateCheckBoxModel(document);
                    break;
                case "DatePicker":
                    el = this.InitiateDatePickerModel(document);
                    break;
                case "TimePicker":
                    el = this.InitiateTimePickerModel(document);
                    break;
                case "Image":
                    el = this.InitiateImageModel(document);
                    break;
                case "DataGrid":
                    el = this.InitiateDataGridModel(document);
                    break;
                default:
                    return null;
            }

            el.ParentCell = null;

            document.Register(el, isPaste);
            DmfReflect.ReadAll(node, el, isPaste);

            // Special handling for DataGrid toolbar
            if (el is DataGridModelCore<DC> dg)
            {
                var toolbarWrapper = node.Element(nameof(dg.ToolBarGrid));
                var toolbarGridNode = toolbarWrapper?.Element("Grid");

                if (toolbarGridNode != null)
                {
                    var toolbarGrid = LoadGrid(document, toolbarGridNode, parentCell: null, isPaste);
                    dg.ToolBarGrid = toolbarGrid;
                    dg.HasToolbar = true;
                }
            }

            return el;
        }

        public ElementModel? LoadElement(ModuleDocumentCore document, XElement node, CellModelCore parentCell
            , bool isPaste)
        {
            if (node.Name.LocalName == "Grid")
                return LoadGrid(document, node, parentCell, isPaste);

            var el = LoadElement(document, node, isPaste);
            if (el == null)
                return null;

            el.ParentCell = parentCell;
            return el;
        }

        private void LoadDatasets(ModuleDocumentCore document, XElement? datasetsNode
            , bool isPaste)
        {
            if (datasetsNode == null) return;

            foreach (var dsNode in datasetsNode.Elements("Dataset"))
            {
                var ds = this.InitiateDatasetModel(document);
                document.Register(ds);
                DmfReflect.ReadAll(dsNode, ds, isPaste);

                // Fields
                var fieldsNode = dsNode.Element("Fields");
                if (fieldsNode != null)
                {
                    foreach (var fNode in fieldsNode.Elements("Field"))
                    {
                        var f = this.InitiateDatasetFieldModel();
                        DmfReflect.ReadAll(fNode, f, isPaste);
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

        private void LoadVariables(ModuleDocumentCore document, XElement? varsNode, bool isPaste)
        {
            if (varsNode == null) return;

            foreach (var vNode in varsNode.Elements("Var"))
            {
                var v = this.InitiateVariableModel(document);
                document.Register(v);
                DmfReflect.ReadAll(vNode, v, isPaste);
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

            if (el is DataGridModelCore<DC> dg)
            {
                var node = NewNode(el);
                DmfReflect.WriteAll(node, el);

                if (dg.ToolBarGrid != null)
                {
                    var toolbarNode = new XElement(nameof(dg.ToolBarGrid));
                    toolbarNode.Add(SaveGrid(dg.ToolBarGrid));
                    node.Add(toolbarNode);
                }

                return node;
            }


            var normalNode = NewNode(el);
            DmfReflect.WriteAll(normalNode, el);
            return normalNode;
        }

        private static XElement NewNode(object obj)
        {
            var typeName = obj.GetType().Name;
            if (typeName.EndsWith("Model"))
                typeName = typeName.Substring(0, typeName.Length - 5);

            return new XElement(typeName);
        }

        private XElement SaveDatasets(ModuleDocumentCore document)
        {
            var root = new XElement("Datasets");

            var datasets = document.AllItems.OfType<DS>().ToList();
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

        private XElement SaveVariables(ModuleDocumentCore document)
        {
            var root = new XElement("Variables");

            var vars = document.AllItems.OfType<VariableModelCore>().ToList();
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