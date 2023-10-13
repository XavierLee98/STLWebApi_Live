using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.XtraEditors.Filtering;
using DevExpress.XtraPrinting;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

// 2023-04-09 fix speed issue ver 1.0.8.1
// 2023-09-25 bring SO remark to DO ver 1.0.10
// 2023-09-25 copy warehouse ver 1.0.10

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class LoadControllers : ViewController
    {
        GeneralControllers genCon;
        public LoadControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            this.LCopyFromPAL.Active.SetItemValue("Enabled", false);
            this.SubmitL.Active.SetItemValue("Enabled", false);
            this.CancelL.Active.SetItemValue("Enabled", false);
            this.LGenerateDO.Active.SetItemValue("Enabled", false);
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GeneralControllers>();

            if (View.Id == "Load_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.LCopyFromPAL.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.LCopyFromPAL.Active.SetItemValue("Enabled", false);
                }

                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    this.SubmitL.Active.SetItemValue("Enabled", true);
                    this.CancelL.Active.SetItemValue("Enabled", true);
                    //this.LGenerateDO.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.SubmitL.Active.SetItemValue("Enabled", false);
                    this.CancelL.Active.SetItemValue("Enabled", false);
                    this.LGenerateDO.Active.SetItemValue("Enabled", false);
                }
            }
            else
            {
                this.LCopyFromPAL.Active.SetItemValue("Enabled", false);
                this.SubmitL.Active.SetItemValue("Enabled", false);
                this.CancelL.Active.SetItemValue("Enabled", false);
                this.LGenerateDO.Active.SetItemValue("Enabled", false);
            }
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        public void openNewView(IObjectSpace os, object target, ViewEditMode viewmode)
        {
            ShowViewParameters svp = new ShowViewParameters();
            DetailView dv = Application.CreateDetailView(os, target);
            dv.ViewEditMode = viewmode;
            dv.IsRoot = true;
            svp.CreatedView = dv;

            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));

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
            Application.ShowViewStrategy.ShowMessage(options);
        }

        private void LCopyFromPAL_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    Load load = (Load)View.CurrentObject;

                    //if (load.IsNew == true)
                    //{
                    //    IObjectSpace os = Application.CreateObjectSpace();
                    //    Load newload = os.CreateObject<Load>();

                    //    foreach (vwPackList dtl in e.PopupWindowViewSelectedObjects)
                    //    {
                    //        LoadDetails newloaditem = os.CreateObject<LoadDetails>();

                    //        newloaditem.PackList = dtl.DocNum;
                    //        if (dtl.Bundle != null)
                    //        {
                    //            newloaditem.Bundle = newloaditem.Session.GetObjectByKey<BundleType>(dtl.Bundle.Oid);
                    //        }
                    //        newloaditem.BaseDoc = dtl.DocNum;
                    //        newload.LoadDetails.Add(newloaditem);
                    //    }

                    //    ShowViewParameters svp = new ShowViewParameters();
                    //    DetailView dv = Application.CreateDetailView(os, newload);
                    //    dv.ViewEditMode = ViewEditMode.Edit;
                    //    dv.IsRoot = true;
                    //    svp.CreatedView = dv;

                    //    Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                    //    showMsg("Success", "Copy Success.", InformationType.Success);
                    //}
                    //else
                    //{
                    foreach (vwPackList dtl in e.PopupWindowViewSelectedObjects)
                    {
                        LoadDetails newloaditem = ObjectSpace.CreateObject<LoadDetails>();

                        newloaditem.PackList = dtl.DocNum;
                        if (dtl.Bundle != null)
                        {
                            newloaditem.Bundle = newloaditem.Session.GetObjectByKey<BundleType>(dtl.Bundle.Oid);
                        }
                        newloaditem.BaseDoc = dtl.DocNum;

                        // Start ver 1.0.8.1
                        PackList packlist = ObjectSpace.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtl.DocNum));

                        foreach (PackListDetails detail in packlist.PackListDetails)
                        {
                            if (dtl.Bundle.Oid == detail.Bundle.Oid)
                            {
                                PickList picklist = ObjectSpace.FindObject<PickList>(CriteriaOperator.Parse("DocNum = ?", detail.PickListNo));

                                foreach(PickListDetailsActual actual in picklist.PickListDetailsActual)
                                {
                                    if (actual.Oid.ToString() == detail.BaseId)
                                    {
                                        newloaditem.Bin = newloaditem.Session.GetObjectByKey<vwBin>(actual.ToBin.BinCode);
                                    }
                                }
                                if (picklist.Transporter != null)
                                {
                                    newloaditem.Transporter = picklist.Transporter.TransporterName;
                                }
                            }
                        }
                        // End ver 1.0.8.1

                        load.LoadDetails.Add(newloaditem);

                        // Start ver 1.0.10
                        if (load.Warehouse == null)
                        {
                             load.Warehouse = load.Session.GetObjectByKey<vwWarehouse>(packlist.Warehouse.WarehouseCode);
                        }
                        // End ver 1.0.10

                        if (load.DocNum == null)
                        {
                            string docprefix = genCon.GetDocPrefix();
                            load.DocNum = genCon.GenerateDocNum(DocTypeList.Load, ObjectSpace, TransferType.NA, 0, docprefix);
                        }

                        showMsg("Success", "Copy Success.", InformationType.Success);
                    }

                    ObjectSpace.CommitChanges();
                    ObjectSpace.Refresh();

                    // Start ver 1.0.8.1
                    IObjectSpace os = Application.CreateObjectSpace();
                    Load loadobj = os.GetObjectByKey<Load>(load.Oid);

                    string duppack = null;
                    foreach (LoadDetails dtl in loadobj.LoadDetails)
                    {
                        if (duppack != dtl.BaseDoc)
                        {
                            if (loadobj.PackListNo == null)
                            {
                                loadobj.PackListNo = dtl.BaseDoc;
                            }
                            else
                            {
                                loadobj.PackListNo = loadobj.PackListNo + ", " + dtl.BaseDoc;
                            }

                            duppack = dtl.BaseDoc;
                        }

                        PackList pack = os.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtl.PackList));

                        if (pack != null)
                        {
                            if (loadobj.SONumber == null)
                            {
                                loadobj.SONumber = pack.SONumber;
                            }

                            if (loadobj.Priority == null)
                            {
                                loadobj.Priority = pack.Priority;
                            }
                        }
                    }

                    os.CommitChanges();
                    // End ver 1.0.8.1
                    //}
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
        }

        private void LCopyFromPAL_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            Load load = (Load)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(vwPackList));
            var cs = Application.CreateCollectionSource(os, typeof(vwPackList), viewId);
            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }

        private void SubmitL_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            Load selectedObject = (Load)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;
            SqlConnection conn = new SqlConnection(genCon.getConnectionString());

            if (selectedObject.IsValid == true)
            {
                selectedObject.Status = DocStatus.Submitted;
                LoadDocTrail ds = ObjectSpace.CreateObject<LoadDocTrail>();
                ds.DocStatus = DocStatus.Submitted;
                ds.DocRemarks = p.ParamString;
                selectedObject.LoadDocTrail.Add(ds);

                //Create DO
                string getpack = "EXEC GenerateDO '" + selectedObject.DocNum + "'";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmd = new SqlCommand(getpack, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SalesOrder so = ObjectSpace.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", reader.GetString(0)));

                    if (so != null)
                    {
                        IObjectSpace loados = Application.CreateObjectSpace();
                        Load currload = loados.FindObject<Load>(CriteriaOperator.Parse("DocNum = ?", selectedObject.DocNum));

                        string picklistnum = null;
                        IObjectSpace deiveryos = Application.CreateObjectSpace();
                        DeliveryOrder newdelivery = deiveryos.CreateObject<DeliveryOrder>();

                        string docprefix = genCon.GetDocPrefix();
                        newdelivery.DocNum = genCon.GenerateDocNum(DocTypeList.DO, deiveryos, TransferType.NA, 0, docprefix);
                        newdelivery.Customer = newdelivery.Session.GetObjectByKey<vwBusniessPartner>(so.Customer.BPCode);
                        newdelivery.CustomerName = so.CustomerName;
                        newdelivery.Status = DocStatus.Submitted;
                        // Start ver 1.0.8.1
                        newdelivery.Priority = newdelivery.Session.GetObjectByKey<PriorityType>(so.Priority.Oid);
                        // End ver 1.0.8.1
                        // Start ver 1.0.10
                        newdelivery.Remarks = so.Remarks;
                        // End ver 1.0.10

                        string[] packlistnum = currload.PackListNo.Replace(" ", "").Split(',');
                        foreach (string dtlpack in packlistnum)
                        {
                            if (dtlpack != null)
                            {
                                foreach (LoadDetails dtlload in currload.LoadDetails)
                                {
                                    if (dtlload.PackList == dtlpack)
                                    {
                                        PackList pl = deiveryos.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtlpack));

                                        newdelivery.CustomerGroup = pl.CustomerGroup;

                                        foreach (PackListDetails dtlpackdetail in pl.PackListDetails)
                                        {
                                            if (dtlload.Bundle.BundleID == dtlpackdetail.Bundle.BundleID)
                                            {
                                                string picklistoid = null;
                                                bool pickitem = false;

                                                PickList picklist = deiveryos.FindObject<PickList>(CriteriaOperator.Parse("DocNum = ?", dtlpackdetail.PickListNo));

                                                foreach (PickListDetailsActual dtlactual in picklist.PickListDetailsActual)
                                                {
                                                    if (dtlpackdetail.BaseId == dtlactual.Oid.ToString())
                                                    {
                                                        picklistoid = dtlactual.PickListDetailOid.ToString();

                                                        if (dtlactual.SOBaseDoc == reader.GetString(0))
                                                        {
                                                            pickitem = true;
                                                        }
                                                        break;
                                                    }
                                                }

                                                foreach (DeliveryOrderDetails dtldelivery in newdelivery.DeliveryOrderDetails)
                                                {
                                                    if (dtldelivery.PackListLine == picklistoid)
                                                    {
                                                        dtldelivery.Quantity = dtldelivery.Quantity + dtlpackdetail.Quantity;
                                                        pickitem = false;
                                                        break;
                                                    }
                                                }

                                                if (pickitem == true)
                                                {
                                                    if (dtlpackdetail.Quantity > 0)
                                                    {
                                                        DeliveryOrderDetails newdeliveryitem = deiveryos.CreateObject<DeliveryOrderDetails>();

                                                        newdeliveryitem.ItemCode = newdeliveryitem.Session.GetObjectByKey<vwItemMasters>(dtlpackdetail.ItemCode.ItemCode);
                                                        newdeliveryitem.Quantity = dtlpackdetail.Quantity;
                                                        newdeliveryitem.PackListLine = picklistoid;

                                                        if (dtlload.Bin != null)
                                                        {
                                                            newdeliveryitem.Warehouse = newdeliveryitem.Session.GetObjectByKey<vwWarehouse>(dtlload.Bin.Warehouse);
                                                            newdeliveryitem.Bin = newdeliveryitem.Session.GetObjectByKey<vwBin>(dtlload.Bin.BinCode);
                                                        }

                                                        //GET SO
                                                        foreach (PickListDetails dtlpick in picklist.PickListDetails)
                                                        {
                                                            if (dtlpick.Oid.ToString() == picklistoid)
                                                            {
                                                                foreach (SalesOrderDetails dtlsales in so.SalesOrderDetails)
                                                                {
                                                                    if (dtlsales.ItemCode.ItemCode == dtlpackdetail.ItemCode.ItemCode &&
                                                                        dtlsales.Oid.ToString() == dtlpick.SOBaseId)
                                                                    {
                                                                        newdeliveryitem.Price = dtlsales.AdjustedPrice;
                                                                    }
                                                                }

                                                                newdeliveryitem.SOBaseID = dtlpick.SOBaseId;
                                                                newdelivery.CustomerGroup = picklist.CustomerGroup;
                                                            }
                                                        }

                                                        newdeliveryitem.BaseDoc = selectedObject.DocNum.ToString();
                                                        newdeliveryitem.BaseId = dtlload.Oid.ToString();
                                                        newdeliveryitem.SODocNum = reader.GetString(0);
                                                        //newdeliveryitem.SOBaseID = dtlpick.SOBaseId;
                                                        newdeliveryitem.PickListDocNum = dtlpackdetail.PickListNo;

                                                        newdelivery.DeliveryOrderDetails.Add(newdeliveryitem);
                                                    }

                                                    picklistnum = dtlpackdetail.PickListNo;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Start ver 1.0.8.1
                        string dupno = null;
                        string dupso = null;
                        foreach (DeliveryOrderDetails dtl in newdelivery.DeliveryOrderDetails)
                        {
                            if (dupno != dtl.BaseDoc)
                            {
                                if (newdelivery.LoadingNo == null)
                                {
                                    newdelivery.LoadingNo = dtl.BaseDoc;
                                }
                                else
                                {
                                    newdelivery.LoadingNo = newdelivery.LoadingNo + ", " + dtl.BaseDoc;
                                }

                                dupno = dtl.BaseDoc;
                            }

                            if (dupso != dtl.SODocNum)
                            {
                                if (newdelivery.SONo == null)
                                {
                                    newdelivery.SONo = dtl.SODocNum;
                                }
                                else
                                {
                                    newdelivery.SONo = newdelivery.SONo + ", " + dtl.SODocNum;
                                }

                                dupso = dtl.SODocNum;
                            }

                            // Start ver 1.0.10
                            if (newdelivery.Warehouse == null)
                            {
                                newdelivery.Warehouse = newdelivery.Session.GetObjectByKey<vwWarehouse>(dtl.Warehouse.WarehouseCode);
                            }
                            // End ver 1.0.10
                        }
                        // End ver 1.0.8.1

                        deiveryos.CommitChanges();
                    }
                }
                conn.Close();

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                Load trx = os.FindObject<Load>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, trx, ViewEditMode.View);
                showMsg("Successful", "Submit Done.", InformationType.Success);
            }
            else
            {
                showMsg("Error", "No Content.", InformationType.Error);
            }
        }

        private void SubmitL_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CancelL_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            Load selectedObject = (Load)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;

            selectedObject.Status = DocStatus.Cancelled;
            LoadDocTrail ds = ObjectSpace.CreateObject<LoadDocTrail>();
            ds.DocStatus = DocStatus.Cancelled;
            ds.DocRemarks = p.ParamString;
            selectedObject.LoadDocTrail.Add(ds);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            Load trx = os.FindObject<Load>(new BinaryOperator("Oid", selectedObject.Oid));
            openNewView(os, trx, ViewEditMode.View);
            showMsg("Successful", "Cancel Done.", InformationType.Success);
        }

        private void CancelL_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void LGenerateDO_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

        }
    }
}
