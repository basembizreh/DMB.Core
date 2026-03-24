using DMB.Core.Actions;
using DMB.Core.Elements;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

            if (document.StartupActions != null && document.StartupActions.Count > 0)
            {
                var startupActionsNode = new XElement("StartupActions");

                foreach (var action in document.StartupActions)
                {
                    var itemNode = new XElement("Action");
                    DmfReflect.WriteAll(itemNode, action);
                    this.SaveObjectGraph(action, itemNode);
                    startupActionsNode.Add(itemNode);
                }

                module.Add(startupActionsNode);
            }


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
            document.IsLoading = true;
            try
            {
                var doc = XDocument.Parse(xml);

                var version = doc.Root?.Attribute("version")?.Value;
                if (version != DmfConstants.CurrentVersion)
                    throw new Exception($"Unsupported DMF version: {version}");

                document.Clear();
                document.Globals["Language"] = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                this.LoadDatasets(document, doc.Root?.Element("Datasets"), isPaste);
                this.LoadVariables(document, doc.Root?.Element("Variables"), isPaste);

                var gridNode = doc.Root?.Element("Grid");
                if (gridNode == null)
                    return null;

                var rootGrid = LoadGrid(document, gridNode, null, isPaste);

                var startupActionsNode = doc.Root?.Element("StartupActions");
                document.StartupActions = new List<ActionBinding>();

                if (startupActionsNode != null)
                {
                    foreach (var actionNode in startupActionsNode.Elements("Action"))
                    {
                        var action = new ActionBinding();
                        DmfReflect.ReadAll(actionNode, action, isPaste);
                        this.LoadObjectGraph(document, action, actionNode, isPaste);
                        document.StartupActions.Add(action);
                    }
                }

                document.SetMainGrid(rootGrid);
                document.RaiseStateChanged();

                return rootGrid;
            }
            finally
            {
                document.IsLoading = false;
            }
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

            if (document.StartupActions != null && document.StartupActions.Count > 0)
            {
                var startupActionsNode = new XElement("StartupActions");

                foreach (var action in document.StartupActions)
                {
                    var itemNode = new XElement("Action");
                    DmfReflect.WriteAll(itemNode, action);
                    this.SaveObjectGraph(action, itemNode);
                    startupActionsNode.Add(itemNode);
                }

                module.Add(startupActionsNode);
            }

            var doc = new XDocument(module);
            return doc.ToString();
        }

        public GridModelCore? Load(ModuleDocumentCore document, string filePath, bool isPaste)
        {
            var doc = XDocument.Load(filePath);

            var version = doc.Root?.Attribute("version")?.Value;
            if (version != DmfConstants.CurrentVersion)
                throw new Exception($"Unsupported DMF version: {version}");

            document.Clear();
            document.Globals["Language"] = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            this.LoadDatasets(document, doc.Root?.Element("Datasets"), isPaste);
            this.LoadVariables(document, doc.Root?.Element("Variables"), isPaste);

            var gridNode = doc.Root?.Element("Grid");
            if (gridNode == null)
                return null;

            var rootGrid = LoadGrid(document, gridNode, null, isPaste);
            document.SetMainGrid(rootGrid);

            var startupActionsNode = doc.Root?.Element("StartupActions");
            document.StartupActions = new List<ActionBinding>();

            if (startupActionsNode != null)
            {
                foreach (var actionNode in startupActionsNode.Elements("Action"))
                {
                    var action = new ActionBinding();
                    DmfReflect.ReadAll(actionNode, action, isPaste);
                    this.LoadObjectGraph(document, action, actionNode, isPaste);
                    document.StartupActions.Add(action);
                }
            }

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

            this.LoadObjectGraph(document, el, node, isPaste);

            //// Generic DmfChild (grid) loading: iterate properties with [DmfChild],
            //// find wrapper node, load inner <Grid> using LoadGrid and set property.
            //var props = el.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //foreach (var p in props)
            //{
            //    var chAttr = p.GetCustomAttributes(typeof(DmfChildAttribute), true)
            //                  .OfType<DmfChildAttribute>()
            //                  .FirstOrDefault();
            //    if (chAttr is null) continue;

            //    // Must be a GridModelCore property per DmfReflect contract
            //    if (!typeof(GridModelCore).IsAssignableFrom(p.PropertyType))
            //        throw new InvalidOperationException(
            //            $"Property '{el.GetType().Name}.{p.Name}' is marked with [DmfChild] but is not a GridModelCore.");

            //    var wrapper = node.Element(chAttr.ElementName);
            //    if (wrapper == null) continue;

            //    // Prefer explicit <Grid> child; else first element inside wrapper
            //    var gridNode = wrapper.Element("Grid") ?? wrapper.Elements().FirstOrDefault();
            //    if (gridNode == null) continue;

            //    var childGrid = LoadGrid(document, gridNode, parentCell: null, isPaste);

            //    if (p.SetMethod != null)
            //        p.SetValue(el, childGrid);
            //}

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
                var rowsNode = dsNode.Element("DataRows");
                if (rowsNode != null)
                {
                    foreach (var rNode in rowsNode.Elements("DataRow"))
                    {
                        var row = this.InitiateDatasetRowModel();
                        foreach (var c in rNode.Elements("C"))
                        {
                            var name = (string?)c.Attribute("n");
                            var val = (string?)c.Attribute("v");
                            if (!string.IsNullOrWhiteSpace(name))
                                row.Values[name] = val;
                        }
                        ds.DataRows.Add(row);
                    }
                }
            }
        }

        private void LoadVariables(ModuleDocumentCore document, XElement? varsNode, bool isPaste)
        {
            if (varsNode == null)
                return;

            foreach (var vNode in varsNode.Elements("Variable"))
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

            // Write element attributes / expandable properties / children collections
            var node = NewNode(el);
            DmfReflect.WriteAll(node, el);

            this.SaveObjectGraph(el, node);

            return node;
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
                var rowsNode = new XElement("DataRows");
                foreach (var row in ds.DataRows)
                {
                    var rNode = new XElement("DataRow");
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
                var node = new XElement("Variable");
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

        private void SaveObjectGraph(object owner, XElement ownerNode)
        {
            var props = owner.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var p in props)
            {
                if (!p.CanRead)
                    continue;

                var value = p.GetValue(owner);
                if (value == null)
                    continue;

                // 1) DmfChild (Grid only)
                var childAttr = p.GetCustomAttributes(typeof(DmfChildAttribute), true)
                                 .OfType<DmfChildAttribute>()
                                 .FirstOrDefault();

                if (childAttr != null)
                {
                    if (value is not GridModelCore grid)
                        throw new InvalidOperationException(
                            $"Property '{owner.GetType().Name}.{p.Name}' has [DmfChild] but is not GridModelCore.");

                    var wrapper = new XElement(childAttr.ElementName);
                    wrapper.Add(this.SaveGrid(grid));
                    ownerNode.Add(wrapper);
                    continue;
                }

                // 2) DmfChildren
                var childrenAttr = p.GetCustomAttributes(typeof(DmfChildrenAttribute), true)
                                    .OfType<DmfChildrenAttribute>()
                                    .FirstOrDefault();

                if (childrenAttr != null)
                {
                    if (value is not System.Collections.IEnumerable items)
                        throw new InvalidOperationException(
                            $"Property '{owner.GetType().Name}.{p.Name}' has [DmfChildren] but is not IEnumerable.");

                    var collectionNode = new XElement(childrenAttr.ContainerName);

                    foreach (var item in items)
                    {
                        if (item == null)
                            continue;

                        var itemNode = new XElement(childrenAttr.ItemName);

                        DmfReflect.WriteAll(itemNode, item);
                        this.SaveObjectGraph(item, itemNode);

                        collectionNode.Add(itemNode);
                    }

                    ownerNode.Add(collectionNode);
                    continue;
                }

                var isExpandable = p.GetCustomAttribute<ExpandablePropertyAttribute>() != null;
                var isDmf = p.GetCustomAttribute<DmfAttribute>() != null;

                // 3) Plain [Dmf] child object
                if (isDmf && !isExpandable)
                {
                    var valueType = value.GetType();

                    var isSimple =
                        valueType.IsPrimitive ||
                        valueType.IsEnum ||
                        valueType == typeof(string) ||
                        valueType == typeof(decimal) ||
                        valueType == typeof(DateTime) ||
                        valueType == typeof(Guid);

                    var isEnumerable = value is System.Collections.IEnumerable && value is not string;

                    if (!isSimple && !isEnumerable)
                    {
                        var childName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                        var childNode = new XElement(childName);

                        DmfReflect.WriteAll(childNode, value);
                        this.SaveObjectGraph(value, childNode);

                        ownerNode.Add(childNode);
                        continue;
                    }
                }

                // 4) ExpandableProperty + [Dmf]
                if (isExpandable && isDmf)
                {
                    var childName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                    var childNode = ownerNode.Element(childName);
                    if (childNode == null)
                    {
                        childNode = new XElement(childName);
                        ownerNode.Add(childNode);
                    }

                    DmfReflect.WriteAll(childNode, value);
                    this.SaveObjectGraph(value, childNode);
                }
            }
        }

        private void LoadObjectGraph(ModuleDocumentCore document, object owner, XElement ownerNode, bool isPaste)
        {
            var props = owner.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var p in props)
            {
                if (!p.CanRead)
                    continue;

                // 1) DmfChild
                var childAttr = p.GetCustomAttributes(typeof(DmfChildAttribute), true)
                                 .OfType<DmfChildAttribute>()
                                 .FirstOrDefault();

                if (childAttr != null)
                {
                    var wrapper = ownerNode.Element(childAttr.ElementName);
                    if (wrapper == null)
                        continue;

                    var gridNode = wrapper.Element("Grid") ?? wrapper.Elements().FirstOrDefault();
                    if (gridNode == null)
                        continue;

                    var childGrid = this.LoadGrid(document, gridNode, parentCell: null, isPaste);

                    if (p.SetMethod != null)
                        p.SetValue(owner, childGrid);

                    continue;
                }

                // 2) DmfChildren
                var childrenAttr = p.GetCustomAttributes(typeof(DmfChildrenAttribute), true)
                                    .OfType<DmfChildrenAttribute>()
                                    .FirstOrDefault();

                if (childrenAttr != null)
                {
                    var container = ownerNode.Element(childrenAttr.ContainerName);
                    if (container == null)
                        continue;

                    var collectionObj = p.GetValue(owner);
                    if (collectionObj == null)
                        continue;

                    collectionObj.GetType().GetMethod("Clear")?.Invoke(collectionObj, null);

                    var addMethod = collectionObj.GetType().GetMethod("Add");
                    if (addMethod == null)
                        continue;

                    var itemType = addMethod.GetParameters().FirstOrDefault()?.ParameterType;
                    if (itemType == null)
                        continue;

                    foreach (var itemNode in container.Elements(childrenAttr.ItemName))
                    {
                        object? itemObj;

                        if (typeof(GridModelCore).IsAssignableFrom(itemType))
                        {
                            itemObj = this.LoadGrid(document, itemNode, parentCell: null, isPaste);
                        }
                        else
                        {
                            itemObj = Activator.CreateInstance(itemType);
                            if (itemObj == null)
                                continue;

                            DmfReflect.ReadAll(itemNode, itemObj, isPaste);
                            this.LoadObjectGraph(document, itemObj, itemNode, isPaste);
                        }

                        addMethod.Invoke(collectionObj, new[] { itemObj });
                    }

                    continue;
                }

                var isExpandable = p.GetCustomAttribute<ExpandablePropertyAttribute>() != null;
                var isDmf = p.GetCustomAttribute<DmfAttribute>() != null;

                // 3) Plain [Dmf] child object
                if (isDmf && !isExpandable)
                {
                    var propType = p.PropertyType;

                    var isSimple =
                        propType.IsPrimitive ||
                        propType.IsEnum ||
                        propType == typeof(string) ||
                        propType == typeof(decimal) ||
                        propType == typeof(DateTime) ||
                        propType == typeof(Guid);

                    var isEnumerable = typeof(System.Collections.IEnumerable).IsAssignableFrom(propType)
                                       && propType != typeof(string);

                    if (!isSimple && !isEnumerable)
                    {
                        var childName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                        var childNode = ownerNode.Element(childName);
                        if (childNode == null)
                            continue;

                        var childObj = Activator.CreateInstance(propType);
                        if (childObj == null)
                            continue;

                        DmfReflect.ReadAll(childNode, childObj, isPaste);
                        this.LoadObjectGraph(document, childObj, childNode, isPaste);

                        if (p.SetMethod != null)
                            p.SetValue(owner, childObj);

                        continue;
                    }
                }

                // 4) ExpandableProperty + [Dmf]
                if (isExpandable && isDmf)
                {
                    var childName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                    var childNode = ownerNode.Element(childName);
                    if (childNode == null)
                        continue;

                    var currentValue = p.GetValue(owner);
                    if (currentValue == null)
                        continue;

                    DmfReflect.ReadAll(childNode, currentValue, isPaste);
                    this.LoadObjectGraph(document, currentValue, childNode, isPaste);
                }
            }
        }
    }

    public interface IXmlNodeSerializable
    {
        void ReadXml(XElement node);
    }
}