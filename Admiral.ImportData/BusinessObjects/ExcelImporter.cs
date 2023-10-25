using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Spreadsheet;
using DevExpress.Xpo;

// 2023-08-25 - export and import function - ver 1.0.9
// 2023-10-20 - add stock count - ver 1.0.12

namespace Admiral.ImportData
{
    public interface IModelImportData
    {
        bool AllowImportData { get; set; }
    }


    public class ExcelImporter
    {
        XafApplication _application;
        IImportOption option;
        public void Setup(XafApplication app)
        {
            _application = app;

        }

        public ExcelImporter()
        {

        }

        public void showMsg(string caption, string msg, InformationType msgtype)
        {
            MessageOptions options = new MessageOptions();
            options.Duration = 3000;
            //options.Message = string.Format("{0} task(s) have been successfully updated!", e.SelectedObjects.Count);
            options.Message = string.Format("{0}", msg);
            options.Type = msgtype;
            options.Web.Position = InformationPosition.Right;
            options.Win.Caption = caption;
            options.Win.Type = WinMessageType.Flyout;
            _application.ShowViewStrategy.ShowMessage(options);
        }

        public void ProcessImportAction(IWorkbook document)
        {
            var os = _application.CreateObjectSpace() as XPObjectSpace;
            os.Session.BeginTransaction();
            bool rst = true;

            foreach (var item in document.Worksheets)
            {
                var typeName = item.Cells[1, 0].DisplayText;
                var t = option.MainTypeInfo.Application.BOModel.GetClass(ReflectionHelper.FindType(typeName));
                var success = StartImport(item, t, os);

                item.Cells[0, 7].SetValue(success ? "Import Success" : "Import Fail");

                rst &= success;
            }

            if (rst)
            {
                try
                {
                    os.Session.CommitTransaction();
                    os.CommitChanges();
                    os.Refresh();

                    SqlConnection conn = new SqlConnection(option.ConnectionString);
                    if (option.DocNum != "")
                    {
                        SqlCommand TransactionNotification = new SqlCommand("", conn);
                        TransactionNotification.CommandTimeout = 600;

                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }

                        try
                        {
                            conn.Open();

                            TransactionNotification.CommandText = "EXEC [" + conn.Database + "]..sp_ImportUpdate '" + option.DocNum + "', '" + option.Type + "'";

                            SqlDataReader reader = TransactionNotification.ExecuteReader();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            conn.Close();
                        }
                    }

                    showMsg("Successful", "Import Success.", InformationType.Success);

                }
                catch (Exception ex)
                {
                    os.Session.RollbackTransaction();
                    os.Rollback();
                    os.Refresh();
                }
            }
            else {
                os.Rollback();
                os.Refresh();
                os.Session.RollbackTransaction();

                showMsg("Error", "Import Fail.", InformationType.Error);
            }
        }

        public static Action DoApplicationEvent { get; set; }

        private bool StartImport(Worksheet ws, IModelClass bo, IObjectSpace os)
        {
            if (ws.Cells[0,0].DisplayText != option.DocNum)
            {
                showMsg("Error", "Import Fail, excel document number not match with current record.", InformationType.Error);
                return false;
            }

            int recorrowstart = 4;
            //????:
            //1.??????????????
            Dictionary<int, IModelMember> fields = new Dictionary<int, IModelMember>();
            List<SheetRowObject> objs = new List<SheetRowObject>();
            //var ws = _spreadsheet.Document.Worksheets[0];
            var columnCount = ws.Columns.LastUsedIndex;

            var updateImport = bo.TypeInfo.FindAttribute<UpdateImportAttribute>();
            var findObjectProviderAttribute = bo.TypeInfo.FindAttribute<FindObjectProviderAttribute>();

            if (findObjectProviderAttribute != null)
                findObjectProviderAttribute.Reset();

            var isUpdateImport = updateImport != null;

            var keyColumn = 0;
            var keyItem = 0;
            var keyOidKey = 0;
            var headerError = false;
            IModelMember keyField = null;
            IModelMember ItemField = null;
            IModelMember OidField = null;
            for (int c = 1; c <= columnCount; c++)
            {
                if (ws.Cells[3, c].DisplayText.Replace(" ", "").ToString() != "")
                {
                    var fieldCaption = ws.Cells[3, c].DisplayText;
                    var fieldName = bo.AllMembers.SingleOrDefault(x => x.Caption == fieldCaption);
                    if (fieldName != null)
                    {
                        fields.Add(c, fieldName);
                        if (isUpdateImport && fieldName.Name == updateImport.KeyMember)
                        {
                            keyColumn = c;
                            keyField = fieldName;
                        }

                        if (fieldName.Name == "ItemCode")
                        {
                            keyItem = c;
                            ItemField = fieldName;
                        }

                        if (fieldName.Name == "OIDKey")
                        {
                            keyOidKey = c;
                            OidField = fieldName;
                        }
                    }
                    else
                    {
                        ws.Cells[3, c].FillColor = Color.Red;
                        headerError = true;
                    }
                }
            }

            var sheetContext = new SheetContext(ws, fields.ToDictionary(x => x.Value.Name, x => x.Key));

            var rowCount = ws.Rows.LastUsedIndex;
            ws.Workbook.BeginUpdate();
            //???????.
            for (int r = recorrowstart; r <= rowCount; r++)
            {
                //ws.Cells[r, 0].ClearContents();

                for (int c = 1; c <= columnCount; c++)
                {
                    var cel = ws.Cells[r, c];
                    if (cel.FillColor != Color.Empty)
                        cel.FillColor = Color.Empty;

                    if (cel.Font.Color != Color.Empty)
                        cel.Font.Color = Color.Empty;
                }

                var errorCell = ws.Cells[r, 0];
                if (!errorCell.Value.IsEmpty)
                {
                    errorCell.Clear();
                }
            }

            ws.Workbook.EndUpdate();
            if (headerError)
            {
                ws.Cells[0, 4].SetValue("Field not exist, please check red header.");
                return false;
            }

            var updateStep = rowCount/100;
            if (updateStep == 0)
                updateStep = 1;

            var numberTypes = new[]
            {
                typeof(Int16),typeof(Int32),typeof(Int64),typeof(UInt16),typeof(UInt32),typeof(UInt64),typeof(decimal),typeof(float),typeof(double),
                typeof(byte),typeof(sbyte)
            };

            ws.Workbook.BeginUpdate();
            for (int r = recorrowstart; r <= rowCount; r++)
            {
                XPBaseObject obj = null;
                if (isUpdateImport)
                {
                    if (ws.Cells[r, keyColumn].DisplayText.Replace(" ", "").ToString() != "" && ws.Cells[r, keyColumn].Value.IsNumeric)
                    {
                        var cdvalue = Convert.ChangeType(ws.Cells[r, keyColumn].Value.ToObject(), keyField.Type);
                        var cri = new BinaryOperator(updateImport.KeyMember, cdvalue);
                        //CriteriaOperator cri = CriteriaOperator.Parse(updateImport.KeyMember, cdvalue);
                        if (findObjectProviderAttribute != null)
                        {
                            var t = findObjectProviderAttribute.FindObject(os, bo.TypeInfo.Type, cri, true);
                            if (t.Count > 0)
                            {
                                obj = t[0] as XPBaseObject;
                            }
                            else
                            {
                                t = null;
                            }
                        }
                        else
                        {
                            obj = os.FindObject(bo.TypeInfo.Type, cri) as XPBaseObject;
                        }

                        //if (obj == null)
                        //{
                        //    obj = os.CreateObject(bo.TypeInfo.Type) as XPBaseObject;
                        //}
                    }
                }
                else
                {
                    obj = os.CreateObject(bo.TypeInfo.Type) as XPBaseObject;
                }

                var result = new SheetRowObject(sheetContext) {Object = obj, Row = r, RowObject = ws.Rows[r]};
               
                if (isUpdateImport && obj != null)
                {
                    if (option.Type == "GoodsReceiptPO")
                    {
                        var docnum = obj.GetMemberValue("GRN");

                        if (docnum.GetType().GetProperty("DocNum").GetValue(docnum).ToString() != option.DocNum)
                        {
                            result.AddErrorMessage(string.Format("OID Key not match with currect document number.", fields[3]), ws.Cells[r, keyOidKey]);
                        }
                    }
                    else if (option.Type == "ASN")
                    {
                        var docnum = obj.GetMemberValue("ASN");

                        if (docnum.GetType().GetProperty("DocNum").GetValue(docnum).ToString() != option.DocNum)
                        {
                            result.AddErrorMessage(string.Format("OID Key not match with currect document number.", fields[3]), ws.Cells[r, keyOidKey]);
                        }
                    }
                    var itemcode = obj.GetMemberValue("ItemCode");

                    if (itemcode.GetType().GetProperty("ItemCode").GetValue(itemcode).ToString() != ws.Cells[r, keyItem].DisplayText.ToString())
                    {
                        result.AddErrorMessage(string.Format("Item Code not match with document line number.", fields[3]), ws.Cells[r, keyItem]);
                    }
                }

                if (isUpdateImport && obj == null)
                {
                    result.AddErrorMessage(string.Format("OID Key not available in this document.", fields[3]), ws.Cells[r, keyOidKey]);
                }
                else
                {
                    //var vle = ws.Cells[r, c];
                    for (int c = 1; c <= columnCount; c++)
                    {
                        if (ws.Cells[3, c].DisplayText != "")
                        {
                            if (option.Type == "SalesQuotation")
                            {
                                if (ws.Cells[3, c].DisplayText == "Sales Quotation")
                                {
                                    ws.Cells[r, c].SetValue(option.DocNum);
                                }
                            }
                            if (option.Type == "PurchaseOrder")
                            {
                                if (ws.Cells[3, c].DisplayText == "Purchase Orders")
                                {
                                    ws.Cells[r, c].SetValue(option.DocNum);
                                }
                            }
                            // Start ver 1.0.9
                            if (option.Type == "WarehouseTransferReq")
                            {
                                if (ws.Cells[3, c].DisplayText == "Warehouse Transfer Req")
                                {
                                    ws.Cells[r, c].SetValue(option.DocNum);
                                }
                            }
                            // End ver 1.0.9

                            // Start ver 1.0.12
                            if (option.Type == "StockCountSheetTarget")
                            {
                                if (ws.Cells[3, c].DisplayText == "Stock Count Sheet")
                                {
                                    ws.Cells[r, c].SetValue(option.DocNum);
                                }
                            }

                            if (option.Type == "StockCountSheetCounted")
                            {
                                if (ws.Cells[3, c].DisplayText == "Stock Count Sheet")
                                {
                                    ws.Cells[r, c].SetValue(option.DocNum);
                                }
                            }

                            if (option.Type == "StockCountConfirm")
                            {
                                if (ws.Cells[3, c].DisplayText == "Stock Count Confirm")
                                {
                                    ws.Cells[r, c].SetValue(option.DocNum);
                                }
                            }
                            // End ver 1.0.12

                            var field = fields[c];
                            var cell = ws.Cells[r, c];

                            if (!cell.Value.IsEmpty)
                            {
                                object value = null;
                                //????
                                //??DC??
                                var memberType = field.MemberInfo.MemberType;
                                if (memberType.IsValueType && memberType.IsGenericType)
                                {
                                    if (memberType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    {
                                        memberType = memberType.GetGenericArguments()[0];
                                    }
                                }

                                if (typeof(XPBaseObject).IsAssignableFrom(memberType) || field.MemberInfo.MemberTypeInfo.IsDomainComponent)
                                {
                                    #region ????
                                    var conditionValue = cell.Value.ToObject();
                                    //???????????????
                                    var idf = field.MemberInfo.FindAttribute<ImportDefaultFilterCriteria>();
                                    var condition = idf == null ? "" : idf.Criteria;
                                    bool cont = true;

                                    #region ????

                                    if (string.IsNullOrEmpty(condition))
                                    {
                                        //?????????????????????????
                                        if (!field.MemberInfo.MemberTypeInfo.KeyMember.IsAutoGenerate)
                                        {
                                            condition = field.MemberInfo.MemberTypeInfo.KeyMember.Name + " = ?";
                                        }
                                    }

                                    if (string.IsNullOrEmpty(condition))
                                    {
                                        //??????????????
                                        var ufield =
                                            field.MemberInfo.MemberTypeInfo.Members.FirstOrDefault(
                                                x => x.FindAttribute<RuleUniqueValueAttribute>() != null
                                                );
                                        if (ufield != null)
                                            condition = ufield.Name + " = ? ";
                                    }

                                    if (string.IsNullOrEmpty(condition))
                                    {
                                        //??????defaultproperty???
                                        var ufield = field.MemberInfo.MemberTypeInfo.DefaultMember;
                                        if (ufield != null)
                                        {
                                            condition = ufield.Name + " = ? ";
                                        }
                                    }

                                    #endregion

                                    #region p

                                    if (string.IsNullOrEmpty(condition))
                                    {
                                        result.AddErrorMessage(
                                            string.Format(
                                            "??????????{0}?????????????????????????!",
                                                field.MemberInfo.Name), cell);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var @operator = CriteriaOperator.Parse(condition, new object[] { conditionValue });


                                            IList list = null;
                                            if (findObjectProviderAttribute != null)
                                            {
                                                list = findObjectProviderAttribute.FindObject(os, field.MemberInfo.MemberType,
                                                    @operator, true);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    list = os.GetObjects(field.MemberInfo.MemberType, @operator, true);
                                                }
                                                catch (Exception exception1)
                                                {
                                                    result.AddErrorMessage(string.Format("Object not found.", field.Name), cell);
                                                    cont = false;
                                                }
                                            }

                                            if (cont == true)
                                            {
                                                if (field.Caption == "???")
                                                    Debug.WriteLine(list.Count + "," + field.Caption, @operator.ToString());
                                                if (list.Count != 1)
                                                {
                                                    result.AddErrorMessage(
                                                        string.Format(
                                                            "??????“{0}”????????“{1}”??????“{3}”???????????????????!????:{2}",
                                                            field.MemberInfo.MemberType.FullName, condition,
                                                            "???" + list.Count + "???", conditionValue), cell);
                                                }
                                                else
                                                {
                                                    value = list[0];
                                                }
                                            }
                                        }
                                        catch (Exception exception1)
                                        {
                                            result.AddErrorMessage(
                                              string.Format("??????“{0}”????????“{1}”???????????????????!????:{2}",
                                                    field.MemberInfo.MemberType.FullName, condition, exception1.Message),
                                                cell);
                                        }
                                    }

                                    #endregion

                                    #endregion

                                }
                                else if (memberType == typeof(DateTime))
                                {
                                    if (!cell.Value.IsDateTime)
                                    {
                                        result.AddErrorMessage(string.Format("Please Key in Date Format.", field.Name), cell);
                                    }
                                    else
                                    {
                                        value = cell.Value.DateTimeValue;
                                    }
                                }
                                else if (numberTypes.Contains(memberType))
                                {
                                    if (!cell.Value.IsNumeric)
                                    {
                                        result.AddErrorMessage(string.Format("Please Key in Numeric.", field.Name), cell);
                                    }
                                    else
                                    {
                                        value = Convert.ChangeType(cell.Value.NumericValue, field.MemberInfo.MemberType);
                                    }
                                }
                                else if (memberType == typeof(bool))
                                {
                                    if (!cell.Value.IsBoolean)
                                    {
                                        result.AddErrorMessage(string.Format("Please Key in TRUE/FALSE.", field.Name), cell);
                                    }
                                    else
                                    {
                                        value = cell.Value.BooleanValue;
                                    }
                                }
                                else if (memberType == typeof(string))
                                {
                                    var v = cell.Value.ToObject();
                                    if (v != null)
                                        value = v.ToString();
                                }
                                else if (memberType.IsEnum)
                                {
                                    #region ??
                                    if (cell.Value.IsNumeric)
                                    {
                                        #region ??????
                                        var vle = Convert.ToInt64(cell.Value.NumericValue);
                                        var any =
                                            Enum.GetValues(field.MemberInfo.MemberType)
                                                .OfType<object>()
                                                .Any(
                                                    x =>
                                                    {
                                                        return object.Equals(Convert.ToInt64(x), vle);
                                                    }
                                                );


                                        if (any)
                                        {
                                            value = Enum.ToObject(field.MemberInfo.MemberType, vle);
                                            // cell.Value.NumericValue;    
                                        }
                                        else
                                        {
                                            result.AddErrorMessage(string.Format("??:{0},???????????????!", field.Name), cell);
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        #region ??????
                                        var names = field.MemberInfo.MemberType.GetEnumNames();
                                        if (names.Contains(cell.Value.TextValue))
                                        {
                                            value = Enum.Parse(field.MemberInfo.MemberType, cell.Value.TextValue);
                                        }
                                        else
                                        {
                                            result.AddErrorMessage(string.Format("??:{0},???????????????!", field.Name), cell);
                                        }
                                        #endregion
                                    }
                                    #endregion
                                }
                                else
                                {
                                    value = cell.Value.ToObject();
                                }
                                obj.SetMemberValue(field.Name, value);
                            }
                            else
                            {
                                result.AddErrorMessage(string.Format("Column not allow blank record.", field.Name), cell);
                            }
                        }
                    }
                }
                
                objs.Add(result);
                if ((r-2)%updateStep == 0)
                {
                    Debug.WriteLine("Process:" + r);
                    if (DoApplicationEvent != null)
                    {
                        DoApplicationEvent();

                        this.option.Progress = ((r/(decimal) rowCount)+0.01m );
                        //Debug.WriteLine(this.option.Progress);
                        //var progress = ws.Cells[r, 0];
                        //progress.SetValue("Íê³É");
                    }
                }
            }
            ws.Workbook.EndUpdate();
            if (objs.All(x => !x.HasError)){
                try
                {
                    Validator.RuleSet.ValidateAll(os, objs.Select(x => x.Object), "Save");
                    return true;
                }
                catch (ValidationException msgs)
                {
                    var rst = true;
                    ws.Workbook.BeginUpdate();
                    foreach (var item in msgs.Result.Results)
                    {
                        if (item.Rule.Properties.ResultType == ValidationResultType.Error && item.State == ValidationState.Invalid)
                        {
                            var r = objs.FirstOrDefault(x => x.Object == item.Target);
                            if (r != null)
                            {
                                r.AddErrorMessage(item.ErrorMessage, item.Rule.UsedProperties);
                            }
                            rst &= false;
                        }
                    }
                    ws.Workbook.EndUpdate();
                    return rst;
                }
            }


            return false;
        }

        IModelMember[] GetMembers(IModelClass cls)
        {
            return cls.AllMembers.Where(x =>
                !x.MemberInfo.IsAutoGenerate &&
                !x.IsCalculated &&
                !x.MemberInfo.IsReadOnly &&
                !x.MemberInfo.IsList
                ).ToArray().Except(cls.AllMembers.Where((x) =>
                {
                    var ia = x.MemberInfo.FindAttribute<ImportOptionsAttribute>();
                    return ia != null && !ia.NeedImport;
                }
                    )
                ).ToArray();
        }

        public void InitializeExcelSheet(IWorkbook book, IImportOption option)
        {
            this.option = option;
            //CreateSheet(book.Worksheets[0], option.MainTypeInfo);
            if (book.Worksheets.Count == 1)
            {
                var listProperties = option.MainTypeInfo.AllMembers.Where(x => x.MemberInfo.IsList && x.MemberInfo.ListElementTypeInfo.IsPersistent);
                foreach (var item in listProperties)
                {
                    if (item.Name == "SalesQuotationDetails")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add(cls.Caption);
                        CreateSheet(b, cls, "SalesQuotation", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }

                    if (item.Name == "PurchaseOrderDetails")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add(cls.Caption);
                        CreateSheet(b, cls, "PurchaseOrder", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }

                    if (item.Name == "GRNDetails")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add(cls.Caption);
                        CreateSheet(b, cls, "GoodsReceiptPO", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }

                    if (item.Name == "ASNDetails")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add(cls.Caption);
                        CreateSheet(b, cls, "ASN", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }

                    // Start ver 1.0.9
                    if (item.Name == "WarehouseTransferReqDetails")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add("Warehouse Request Item");
                        CreateSheet(b, cls, "WarehouseTransferReq", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }
                    // End ver 1.0.9

                    // Start ver 1.0.12
                    if (item.Name == "StockCountSheetTarget")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add("Stock Sheet Target");
                        CreateSheet(b, cls, "StockCountSheetTarget", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }

                    if (item.Name == "StockCountSheetCounted")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add("Stock Sheet Counted");
                        CreateSheet(b, cls, "StockCountSheetCounted", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }

                    if (item.Name == "StockCountConfirmDetails")
                    {
                        var cls = option.MainTypeInfo.Application.BOModel.GetClass(item.MemberInfo.ListElementTypeInfo.Type);
                        var b = book.Worksheets.Add("Stock Confirm Counted");
                        CreateSheet(b, cls, "StockCountConfirm", option.DocNum);

                        book.Worksheets.Remove(book.Worksheets[0]);
                    }
                    // End ver 1.0.12
                }
            }

            if (book.Worksheets.Count > 0)
                book.Worksheets.ActiveWorksheet = book.Worksheets[0];
        }

        private void CreateSheet(Worksheet book, IModelClass boInfo, string module, string docnum)
        {
            if (boInfo.Caption.Length > 31)
            {
                book.Name = boInfo.Caption.Substring(0, 30).ToString();
            }
            else
            {
                book.Name = boInfo.Caption;
            }
            //book.Cells[0, 0].Value = "System Type :";
            //book.Cells[0, 2].Value = "DocNum :";
            book.Cells[0, 0].Value = docnum;
            book.Cells[1, 0].Value = boInfo.TypeInfo.FullName;
            book.Cells[2, 0].Value = "This information corresponds to the system business information at the time of import, please do not delete!";
            //1.??????????.
            //2.???????????????
            var i = 1;
            #region main
            var cells = book.Cells;
            var members = GetMembers(boInfo);
            foreach (var item in members)
            {
                if (module == "SalesQuotation")
                {
                    if (item.Name == "ItemCode" || item.Name == "Location" || item.Name == "Quantity" 
                        || item.Name == "SalesQuotation")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }

                if (module == "PurchaseOrder")
                {
                    if (item.Name == "ItemCode" || item.Name == "Quantity" || item.Name == "PurchaseOrders" || item.Name == "AdjustedPrice")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }

                if (module == "GoodsReceiptPO")
                {
                    if (item.Name == "OIDKey" || item.Name == "ItemCode" || item.Name == "Received" || item.Name == "DiscrepancyReason")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }

                if (module == "ASN")
                {
                    if (item.Name == "OIDKey" || item.Name == "ItemCode" || item.Name == "UnloadQty")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }

                // Start ver 1.0.9
                if (module == "WarehouseTransferReq")
                {
                    if (item.Name == "ItemCode" || item.Name == "Quantity" || item.Name == "WarehouseTransferReq")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }
                // End ver 1.0.9

                // Start ver 1.0.12
                if (module == "StockCountSheetTarget")
                {
                    if (item.Name == "ItemCode" || item.Name == "Bin" || item.Name == "Quantity" ||
                        item.Name == "StockCountSheet")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }

                if (module == "StockCountSheetCounted")
                {
                    if (item.Name == "ItemCode" || item.Name == "Bin" || item.Name == "Quantity" ||
                        item.Name == "StockCountSheet")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }

                if (module == "StockCountConfirm")
                {
                    if (item.Name == "ItemCode" || item.Name == "Bin" || item.Name == "Quantity" ||
                        item.Name == "StockCountConfirm")
                    {
                        var c = cells[3, i];
                        c.Value = item.Caption;
                        c.FillColor = Color.FromArgb(255, 153, 0);
                        c.Font.Color = Color.White;
                        var isRequiredField = IsRequiredField(item);

                        var range = book.Range.FromLTRB(i, 2, i, 20000);

                        //DataValidation dv = null;

                        if (isRequiredField)
                        {
                            c.Font.Bold = true;
                        }
                        i++;
                    }
                }
                // End ver 1.0.12
            }
            #endregion
        }

        IRule[] _rules;
        IRule[] Rules
        {
            get
            {
                if (_rules == null)
                {
                    _rules = Validator.RuleSet.GetRules().ToArray();
                }
                return _rules;
            }
        }

        public bool IsRequiredField(IModelMember member)
        {
            //Rules.Where(x=>x.t)
            return Rules.Any(x => x.Properties is IRuleRequiredFieldProperties && x.Properties.TargetType == member.ModelClass.TypeInfo.Type && x.UsedProperties.IndexOf(member.Name) > -1);

        }
    }
}