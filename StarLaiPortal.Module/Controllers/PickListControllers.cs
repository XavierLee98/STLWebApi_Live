﻿using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Web.Internal.XmlProcessor;
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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

// 2023-07-28 add print button and do not add count in preview ver 1.0.7
// 2023-08-16 preview multiple picklist ver 1.0.8
// 2023-08-25 add picklistactual validation ver 1.0.9
// 2023-04-09 fix speed issue ver 1.0.8.1
// 2023-09-25 bring SO remark to DO ver 1.0.10

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class PickListControllers : ViewController
    {
        GeneralControllers genCon;
        public PickListControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.PLCopyFromSOCG.Active.SetItemValue("Enabled", false);
            this.PLCopyFromSOC.Active.SetItemValue("Enabled", false);
            this.SubmitPL.Active.SetItemValue("Enabled", false);
            this.CancelPL.Active.SetItemValue("Enabled", false);
            this.PreviewPL.Active.SetItemValue("Enabled", false);
            this.PLCopyFromPLDetail.Active.SetItemValue("Enabled", false);
            // Start ver 1.0.7
            this.PrintPL.Active.SetItemValue("Enabled", false);
            // End ver 1.0.7
            // Start ver 1.0.8
            this.PrintPLByZone.Active.SetItemValue("Enabled", false);
            // End ver 1.0.8
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.

            genCon = Frame.GetController<GeneralControllers>();
            if (View.Id == "PickList_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.PLCopyFromSOCG.Active.SetItemValue("Enabled", true);
                    this.PLCopyFromSOC.Active.SetItemValue("Enabled", true);
                    this.PLCopyFromPLDetail.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.PLCopyFromSOCG.Active.SetItemValue("Enabled", false);
                    this.PLCopyFromSOC.Active.SetItemValue("Enabled", false);
                    this.PLCopyFromPLDetail.Active.SetItemValue("Enabled", false);
                }

                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    this.SubmitPL.Active.SetItemValue("Enabled", true);
                    this.CancelPL.Active.SetItemValue("Enabled", true);
                    this.PreviewPL.Active.SetItemValue("Enabled", true);
                    // Start ver 1.0.7
                    this.PrintPL.Active.SetItemValue("Enabled", true);
                    // End ver 1.0.7
                    // Start ver 1.0.8
                    this.PrintPLByZone.Active.SetItemValue("Enabled", true);
                    // End ver 1.0.8
                }
                else
                {
                    this.SubmitPL.Active.SetItemValue("Enabled", false);
                    this.CancelPL.Active.SetItemValue("Enabled", false);
                    this.PreviewPL.Active.SetItemValue("Enabled", false);
                    // Start ver 1.0.7
                    this.PrintPL.Active.SetItemValue("Enabled", false);
                    // End ver 1.0.7
                    // Start ver 1.0.8
                    this.PrintPLByZone.Active.SetItemValue("Enabled", false);
                    // End ver 1.0.8
                }
            }
            // Start ver 1.0.7
            else if (View.Id == "PickList_ListView")
            {
                this.PrintPL.Active.SetItemValue("Enabled", true);
                // Start ver 1.0.8
                this.PrintPLByZone.Active.SetItemValue("Enabled", true);
                // End ver 1.0.8
            }
            // End ver 1.0.7
            else
            {
                this.PLCopyFromSOCG.Active.SetItemValue("Enabled", false);
                this.PLCopyFromSOC.Active.SetItemValue("Enabled", false);
                this.SubmitPL.Active.SetItemValue("Enabled", false);
                this.CancelPL.Active.SetItemValue("Enabled", false);
                this.PreviewPL.Active.SetItemValue("Enabled", false);
                this.PLCopyFromPLDetail.Active.SetItemValue("Enabled", false);
                // Start ver 1.0.7
                this.PrintPL.Active.SetItemValue("Enabled", false);
                // End ver 1.0.7
                // Start ver 1.0.8
                this.PrintPLByZone.Active.SetItemValue("Enabled", false);
                // End ver 1.0.8
            }

            if (View.Id == "PickList_PickListDetails_ListView")
            {
                ((ASPxGridListEditor)((ListView)View).Editor).Grid.RowUpdating += new DevExpress.Web.Data.ASPxDataUpdatingEventHandler(Grid_RowUpdating);
            }

            // Start ver 1.0.9
            if (View.Id == "PickList_PickListDetailsActual_ListView")
            {
                ((ASPxGridListEditor)((ListView)View).Editor).Grid.RowUpdating += new DevExpress.Web.Data.ASPxDataUpdatingEventHandler(Grid_RowUpdating_Actual);
            }
            // End ver 1.0.9
        }

        private void Grid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            ASPxGridListEditor listEditor = ((ListView)View).Editor as ASPxGridListEditor;
            if (listEditor != null)
            {
                object currentObject = listEditor.Grid.GetRow(listEditor.Grid.EditingRowVisibleIndex);
                if (currentObject != null)
                {
                    object validation = currentObject.GetType().GetProperty("IsValid").GetValue(currentObject);

                    if ((bool)validation == true)
                    {
                        showMsg("Error", "Plan Quantity cannot 0.", InformationType.Error);
                    }
                }
            }
        }

        // Start ver 1.0.9
        private void Grid_RowUpdating_Actual(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            ASPxGridListEditor listEditor = ((ListView)View).Editor as ASPxGridListEditor;
            if (listEditor != null)
            {
                object currentObject = listEditor.Grid.GetRow(listEditor.Grid.EditingRowVisibleIndex);
                if (currentObject != null)
                {
                    object picklist = currentObject.GetType().GetProperty("PickList").GetValue(currentObject);
                    bool over = false;
                    string overitem = null;

                    foreach (PickListDetails dtl in (picklist as PickList).PickListDetails)
                    {
                        int pickqty = 0;
                        if (currentObject.GetType().GetProperty("PickListDetailOid").GetValue(currentObject).ToString() == dtl.Oid.ToString())
                        {
                            pickqty = pickqty + Convert.ToInt32(currentObject.GetType().GetProperty("PickQty").GetValue(currentObject));
                        }

                        if (pickqty > dtl.PlanQty)
                        {
                            over = true;
                            overitem = dtl.ItemCode.ItemCode;
                        }
                    }

                    if (over == true)
                    {
                        showMsg("Error", "Pick qty more than plan qty. Item : " + overitem, InformationType.Error);
                    }
                }
            }
        }
        // End ver 1.0.9

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

        private void PLCopyFromSOC_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    PickList pl = (PickList)View.CurrentObject;

                    //if (pl.IsNew == true)
                    //{
                    //    IObjectSpace os = Application.CreateObjectSpace();
                    //    PickList newpl = os.CreateObject<PickList>();

                    //    foreach (vwPaymentSO dtl in e.PopupWindowViewSelectedObjects)
                    //    {
                    //        PickListDetails newplitem = os.CreateObject<PickListDetails>();

                    //        newplitem.ItemCode = dtl.ItemCode;
                    //        newplitem.ItemDesc = dtl.ItemDesc;
                    //        newplitem.CatalogNo = dtl.CatalogNo;
                    //        if (dtl.Warehouse != null)
                    //        {
                    //            newplitem.Warehouse = newplitem.Session.GetObjectByKey<vwWarehouse>(dtl.Warehouse);
                    //        }
                    //        newplitem.PlanQty = dtl.Quantity;
                    //        newplitem.Customer = dtl.Customer;
                    //        newplitem.SOBaseDoc = dtl.DocNum;
                    //        newplitem.SOBaseId = dtl.Oid;
                    //        newplitem.SOCreateDate = dtl.CreateDate;
                    //        newplitem.SOExpectedDate = dtl.PostingDate;
                    //        newplitem.SORemarks = dtl.Remarks;
                    //        newplitem.SOSalesperson = dtl.Salesperson;
                    //        newplitem.SOTransporter = dtl.Transporter.TransporterName.ToString();

                    //        newpl.PickListDetails.Add(newplitem);
                    //    }

                    //    ShowViewParameters svp = new ShowViewParameters();
                    //    DetailView dv = Application.CreateDetailView(os, newpl);
                    //    dv.ViewEditMode = ViewEditMode.Edit;
                    //    dv.IsRoot = true;
                    //    svp.CreatedView = dv;

                    //    Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                    //    showMsg("Success", "Copy Success.", InformationType.Success);
                    //}
                    //else
                    //{
                        string dupcustomer = null;
                        foreach (vwPaymentSO customer in e.PopupWindowViewSelectedObjects)
                        {
                            if (dupcustomer != null)
                            {
                                if (customer.Customer != dupcustomer)
                                {
                                    showMsg("Error", "Copy fail, duplicate customer found.", InformationType.Error);
                                    return;
                                }
                            }
                            if (dupcustomer == null)
                            {
                                dupcustomer = customer.Customer;
                            }

                            foreach (PickListDetails pllist in pl.PickListDetails)
                            {
                                if (customer.Customer != pllist.Customer.BPCode)
                                {
                                    showMsg("Error", "Copy fail, duplicate customer found.", InformationType.Error);
                                    return;
                                }
                            }
                        }

                        foreach (vwPaymentSO dtl in e.PopupWindowViewSelectedObjects)
                        {
                            PickListDetails newplitem = ObjectSpace.CreateObject<PickListDetails>();

                            vwBusniessPartner customer = ObjectSpace.FindObject<vwBusniessPartner>(CriteriaOperator.Parse("BPCode = ?", dtl.Customer));
                            if (customer != null)
                            {
                                pl.CustomerGroup = customer.GroupName;

                                // Start ver 1.0.8.1
                                if (pl.Customer == null)
                                {
                                    pl.Customer = customer.BPCode;
                                }
                                if (pl.CustomerName == null)
                                {
                                    pl.CustomerName = customer.BPName;
                                }
                                // End ver 1.0.8.1
                            }
                            if (dtl.Transporter != null)
                            {
                                //pl.Transporter = newplitem.Session.GetObjectByKey<vwTransporter>(dtl.Transporter.TransporterID);
                                pl.Transporter = newplitem.Session.FindObject<vwTransporter>(CriteriaOperator.Parse("TransporterName = ?", dtl.Transporter));
                            }

                            newplitem.ItemCode = newplitem.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode);
                            newplitem.ItemDesc = dtl.ItemDesc;
                            newplitem.CatalogNo = dtl.CatalogNo;
                            if (dtl.Warehouse != null)
                            {
                                newplitem.Warehouse = newplitem.Session.GetObjectByKey<vwWarehouse>(dtl.Warehouse);
                            }
                            newplitem.PlanQty = dtl.Quantity;
                            if (dtl.Customer != null)
                            {
                                newplitem.Customer = newplitem.Session.GetObjectByKey<vwBusniessPartner>(dtl.Customer);
                            }
                            newplitem.SOBaseDoc = dtl.DocNum;
                            newplitem.SOBaseId = dtl.Oid;
                            newplitem.SOCreateDate = dtl.CreateDate;
                            newplitem.SOExpectedDate = dtl.PostingDate;
                            newplitem.SORemarks = dtl.Remarks;
                            newplitem.SOSalesperson = dtl.Salesperson;
                            //newplitem.SOTransporter = dtl.Transporter.TransporterName.ToString();
                            newplitem.SOTransporter = dtl.Transporter.ToString();
                            newplitem.SODeliveryDate = dtl.DeliveryDate;
                            if (dtl.Priority != null)
                            {
                                newplitem.Priority = newplitem.Session.GetObjectByKey<PriorityType>(dtl.Priority.Oid);
                                // Start ver 1.0.8.1
                                if (pl.Priority == null)
                                {
                                    pl.Priority = pl.Session.GetObjectByKey<PriorityType>(dtl.Priority.Oid);
                                }
                                // End ver 1.0.8.1
                            }

                            IObjectSpace os = Application.CreateObjectSpace();
                            vwPaymentSO so = os.FindObject<vwPaymentSO>(CriteriaOperator.Parse("Oid = ? and DocNum = ?",
                                dtl.Oid, dtl.DocNum));

                            if (so == null)
                            {
                                showMsg("Error", "SO already created pick list, please refresh data.", InformationType.Error);
                                return;
                            }

                            pl.PickListDetails.Add(newplitem);

                            showMsg("Success", "Copy Success.", InformationType.Success);
                        }

                        // Start ver 1.0.8.1
                        string dupso = null;
                        pl.SONumber = null;
                        foreach (PickListDetails dtlsonum in pl.PickListDetails)
                        {
                            if (dupso != dtlsonum.SOBaseDoc)
                            {
                                if (pl.SONumber == null)
                                {
                                    pl.SONumber = dtlsonum.SOBaseDoc;
                                }
                                else
                                {
                                    pl.SONumber = pl.SONumber + ", " + dtlsonum.SOBaseDoc;
                                }

                                dupso = dtlsonum.SOBaseDoc;
                            }
                        }

                        string deliverydate = pl.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.SODeliveryDate).Min().SODeliveryDate.Date.ToString();
                        pl.SODeliveryDate = deliverydate.Substring(0, 10);
                        // End ver 1.0.8.1

                        if (pl.DocNum == null)
                        {
                            string docprefix = genCon.GetDocPrefix();
                            pl.DocNum = genCon.GenerateDocNum(DocTypeList.PL, ObjectSpace, TransferType.NA, 0, docprefix);
                        }

                        ObjectSpace.CommitChanges();
                        ObjectSpace.Refresh();
                    //}
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
        }

        private void PLCopyFromSOC_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            PickList pl = (PickList)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(vwPaymentSO));
            var cs = Application.CreateCollectionSource(os, typeof(vwPaymentSO), viewId);
            if (pl.Warehouse != null)
            {
                cs.Criteria["Warehouse"] = new BinaryOperator("Warehouse", pl.Warehouse.WarehouseCode);
            }
            else
            {
                cs.Criteria["Warehouse"] = new BinaryOperator("Warehouse", "");
            }

            if (pl.Transporter != null)
            {
                cs.Criteria["Transporter"] = new BinaryOperator("Transporter", pl.Transporter.TransporterName);
            }

            if (pl.DeliveryDate != null)
            {
                cs.Criteria["DeliveryDate"] = new BinaryOperator("DeliveryDate", pl.DeliveryDate.Year + "-" + pl.DeliveryDate.Month.ToString("00") + "-" + pl.DeliveryDate.Day.ToString("00"),BinaryOperatorType.LessOrEqual);
            }

            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }

        private void SubmitPL_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            PickList selectedObject = (PickList)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;
            SqlConnection conn = new SqlConnection(genCon.getConnectionString());
            bool insuffstock = false;
            string insuff = null;

            foreach (PickListDetailsActual dtl in selectedObject.PickListDetailsActual)
            {
                vwBinStockBalance binbalance = ObjectSpace.FindObject<vwBinStockBalance>(CriteriaOperator.Parse("BinAbs = ? and ItemCode = ?", 
                    dtl.FromBin.AbsEntry, dtl.ItemCode.ItemCode));

                if (binbalance != null)
                {
                    if (binbalance.InStock < dtl.PickQty)
                    {
                        insuffstock = true;

                        if (insuff != null)
                        {
                            insuff = insuff + ", " + dtl.ItemCode.ItemCode;
                        }
                        else
                        {
                            insuff = dtl.ItemCode.ItemCode;
                        }
                    }
                }
                else
                {
                    insuffstock = true;

                    if (insuff != null)
                    {
                        insuff = insuff + ", " + dtl.ItemCode.ItemCode;
                    }
                    else
                    {
                        insuff = dtl.ItemCode.ItemCode;
                    }
                }
            }

            if (insuffstock == true)
            {
                showMsg("Error", "Bin not enough stock. Item : " + insuff, InformationType.Error);
                return;
            }

            if (selectedObject.IsValid4 == true)
            {
                showMsg("Error", "Duplicate customer in pick list.", InformationType.Error);
                return;
            }

            // Start ver 1.0.9
            if (selectedObject.IsValid5 == true)
            {
                showMsg("Error", "Pick qty more than plan qty.", InformationType.Error);
                return;
            }
            // End ver 1.0.9

            if (selectedObject.IsValid == true)
            {
                if (selectedObject.IsValid1 == false)
                {
                    if (selectedObject.IsValid3 == true && (selectedObject.Vehicle == null || selectedObject.Driver == null))
                    {
                        showMsg("Error", "Back to back order pls fill in driver and vehicle.", InformationType.Error);
                        return;
                    }
                    else
                    {
                        selectedObject.Status = DocStatus.Submitted;
                        PickListDocTrail ds = ObjectSpace.CreateObject<PickListDocTrail>();
                        ds.DocStatus = DocStatus.Submitted;
                        ds.DocRemarks = p.ParamString;
                        selectedObject.PickListDocTrail.Add(ds);

                        if (selectedObject.Transporter != null)
                        {
                            if (selectedObject.Transporter.U_Type == "OC" || selectedObject.Transporter.U_Type == "OS" || selectedObject.IsValid3 == true)
                            {
                                string docprefix = genCon.GetDocPrefix();

                                #region Add Pack List
                                string gettobin = "SELECT ToBin FROM PickListDetailsActual WHERE PickList = " + selectedObject.Oid + " GROUP BY ToBin";
                                if (conn.State == ConnectionState.Open)
                                {
                                    conn.Close();
                                }
                                conn.Open();
                                SqlCommand cmd = new SqlCommand(gettobin, conn);
                                SqlDataReader reader = cmd.ExecuteReader();

                                IObjectSpace packos = Application.CreateObjectSpace();
                                PackList newpack = packos.CreateObject<PackList>();

                                IObjectSpace loados = Application.CreateObjectSpace();
                                Load newload = loados.CreateObject<Load>();

                                newload.DocNum = genCon.GenerateDocNum(DocTypeList.Load, loados, TransferType.NA, 0, docprefix);
                                newload.Status = DocStatus.Submitted;
                                if (selectedObject.Driver != null)
                                {
                                    newload.Driver = newload.Session.GetObjectByKey<vwDriver>(selectedObject.Driver.DriverCode);
                                }
                                if (selectedObject.Vehicle != null)
                                {
                                    newload.Vehicle = newload.Session.GetObjectByKey<vwVehicle>(selectedObject.Vehicle.VehicleCode);
                                }

                                while (reader.Read())
                                {
                                    newpack.DocNum = genCon.GenerateDocNum(DocTypeList.PAL, packos, TransferType.NA, 0, docprefix);

                                    newpack.PackingLocation = newpack.Session.GetObjectByKey<vwBin>(reader.GetString(0));
                                    newpack.Status = DocStatus.Submitted;
                                    newpack.CustomerGroup = selectedObject.CustomerGroup;

                                    foreach (PickListDetailsActual dtl in selectedObject.PickListDetailsActual)
                                    {
                                        if (dtl.ToBin.BinCode == reader.GetString(0))
                                        {
                                            PackListDetails newpackdetails = packos.CreateObject<PackListDetails>();

                                            newpackdetails.ItemCode = newpackdetails.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode.ItemCode);
                                            newpackdetails.ItemDesc = dtl.ItemDesc;
                                            newpackdetails.Bundle = newpackdetails.Session.GetObjectByKey<BundleType>(1);
                                            newpackdetails.Quantity = dtl.PickQty;
                                            newpackdetails.PickListNo = selectedObject.DocNum;
                                            if (dtl.SOTransporter != null)
                                            {
                                                newpackdetails.Transporter = packos.FindObject<vwTransporter>
                                                    (CriteriaOperator.Parse("TransporterName = ?", dtl.SOTransporter));
                                            }
                                            newpackdetails.BaseDoc = selectedObject.DocNum;

                                            //foreach (PickListDetails dtl2 in selectedObject.PickListDetails)
                                            //{
                                            //    if (dtl2.ItemCode.ItemCode == dtl.ItemCode.ItemCode && dtl2.SOBaseDoc == dtl.SOBaseDoc)
                                            //    {
                                            //        newpackdetails.BaseId = dtl2.Oid.ToString();
                                            //    }
                                            //}

                                            newpackdetails.BaseId = dtl.Oid.ToString();

                                            newpack.PackListDetails.Add(newpackdetails);
                                        }
                                    }

                                    // Start ver 1.0.8.1
                                    string duppl = null;
                                    string dupso = null;
                                    string dupcustomer = null;
                                    foreach (PackListDetails dtl in newpack.PackListDetails)
                                    {
                                        if (duppl != dtl.PickListNo)
                                        {
                                            PickList picklist = packos.FindObject<PickList>(CriteriaOperator.Parse("DocNum = ?", dtl.PickListNo));

                                            if (picklist != null)
                                            {
                                                foreach (PickListDetails dtl2 in picklist.PickListDetails)
                                                {
                                                    if (dupso != dtl2.SOBaseDoc)
                                                    {
                                                        if (newpack.SONumber == null)
                                                        {
                                                            newpack.SONumber = dtl2.SOBaseDoc;
                                                        }
                                                        else
                                                        {
                                                            newpack.SONumber = newpack.SONumber + ", " + dtl2.SOBaseDoc;
                                                        }

                                                        SalesOrder salesorder = packos.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", dtl2.SOBaseDoc));

                                                        if (salesorder != null)
                                                        {
                                                            if (newpack.SAPSONo == null)
                                                            {
                                                                newpack.SAPSONo = salesorder.SAPDocNum;
                                                            }
                                                            else
                                                            {
                                                                newpack.SAPSONo = newpack.SAPSONo + ", " + salesorder.SAPDocNum;
                                                            }
                                                        }

                                                        dupso = dtl2.SOBaseDoc;
                                                    }

                                                    if (dupcustomer != dtl2.Customer.BPName)
                                                    {
                                                        if (newpack.Customer == null)
                                                        {
                                                            newpack.Customer = dtl2.Customer.BPName;
                                                        }
                                                        else
                                                        {
                                                            newpack.Customer = newpack.Customer + ", " + dtl2.Customer.BPName;
                                                        }

                                                        dupcustomer = dtl2.Customer.BPName;
                                                    }
                                                }

                                                if (picklist != null)
                                                {
                                                    if (newpack.Priority == null)
                                                    {
                                                        newpack.Priority = picklist.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.Priority).Max().Priority;
                                                    }
                                                }
                                            }

                                            if (newpack.PickListNo == null)
                                            {
                                                newpack.PickListNo = dtl.PickListNo;
                                            }
                                            else
                                            {
                                                newpack.PickListNo = newpack.PickListNo + ", " + dtl.PickListNo;
                                            }

                                            duppl = dtl.PickListNo;
                                        }
                                    }
                                    // End ver 1.0.8.1
                                    packos.CommitChanges();

                                    #region Add Load
                                    LoadDetails newloaddetails = loados.CreateObject<LoadDetails>();

                                    newloaddetails.PackList = newpack.DocNum;
                                    newloaddetails.Bundle = newloaddetails.Session.GetObjectByKey<BundleType>(1);
                                    newloaddetails.Bin = newloaddetails.Session.GetObjectByKey<vwBin>(reader.GetString(0));
                                    if (selectedObject.Transporter != null)
                                    {
                                        newloaddetails.Transporter = selectedObject.Transporter.TransporterName;
                                    }
                                    newloaddetails.BaseDoc = newpack.DocNum;
                                    newloaddetails.BaseId = newpack.Oid.ToString();

                                    newload.LoadDetails.Add(newloaddetails);
                                    #endregion
                                }
                                conn.Close();
                                #endregion

                                // Start ver 1.0.8.1
                                string duppack = null;
                                foreach (LoadDetails dtl in newload.LoadDetails)
                                {
                                    if (duppack != dtl.BaseDoc)
                                    {
                                        if (newload.PackListNo == null)
                                        {
                                            newload.PackListNo = dtl.BaseDoc;
                                        }
                                        else
                                        {
                                            newload.PackListNo = newload.PackListNo + ", " + dtl.BaseDoc;
                                        }

                                        duppack = dtl.BaseDoc;
                                    }

                                    PackList pack = loados.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtl.PackList));

                                    if (pack != null)
                                    {
                                        if (newload.SONumber == null)
                                        {
                                            newload.SONumber = pack.SONumber;
                                        }

                                        if (newload.Priority == null)
                                        {
                                            newload.Priority = pack.Priority;
                                        }
                                    }
                                }
                                // End ver 1.0.8.1
                                
                                loados.CommitChanges();

                                #region Add Delivery Order
                                string getso = "SELECT SOBaseDoc FROM PickListDetailsActual WHERE PickList = " + selectedObject.Oid + " GROUP BY SOBaseDoc";
                                if (conn.State == ConnectionState.Open)
                                {
                                    conn.Close();
                                }
                                conn.Open();
                                SqlCommand cmd1 = new SqlCommand(getso, conn);
                                SqlDataReader reader1 = cmd1.ExecuteReader();
                                while (reader1.Read())
                                {
                                    SalesOrder so = ObjectSpace.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", reader1.GetString(0)));

                                    if (so != null)
                                    {
                                        IObjectSpace deiveryos = Application.CreateObjectSpace();
                                        DeliveryOrder newdelivery = deiveryos.CreateObject<DeliveryOrder>();

                                        newdelivery.DocNum = genCon.GenerateDocNum(DocTypeList.DO, deiveryos, TransferType.NA, 0, docprefix);
                                        newdelivery.Customer = newdelivery.Session.GetObjectByKey<vwBusniessPartner>(so.Customer.BPCode);
                                        newdelivery.CustomerName = so.CustomerName;
                                        newdelivery.Status = DocStatus.Submitted;
                                        newdelivery.CustomerGroup = selectedObject.CustomerGroup;
                                        // Start ver 1.0.8.1
                                        newdelivery.Priority = newdelivery.Session.GetObjectByKey<PriorityType>(so.Priority.Oid);
                                        // End ver 1.0.8.1
                                        // Start ver 1.0.10
                                        newdelivery.Remarks = so.Remarks;
                                        // End ver 1.0.10

                                        foreach (LoadDetails dtlload in newload.LoadDetails)
                                        {
                                            //PackList pl = os.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtlload.PackList));
                                            string picklistdone = null;
                                            foreach (PackListDetails dtlpack in newpack.PackListDetails)
                                            {
                                                if (dtlpack.Quantity > 0)
                                                {
                                                    int picklistoid = 0;
                                                    bool pickitem = false;
                                                    if (dtlpack.Bundle.BundleID == dtlload.Bundle.BundleID)
                                                    {
                                                        foreach (PickListDetailsActual dtlactual in selectedObject.PickListDetailsActual)
                                                        {
                                                            if (dtlpack.BaseId == dtlactual.Oid.ToString())
                                                            {
                                                                picklistoid = dtlactual.PickListDetailOid;
                                                                break;
                                                            }
                                                        }

                                                        foreach (PickListDetails dtlpick in selectedObject.PickListDetails)
                                                        {
                                                            if (dtlpack.ItemCode.ItemCode == dtlpick.ItemCode.ItemCode && dtlpick.SOBaseDoc == so.DocNum &&
                                                            dtlpick.Oid == picklistoid)
                                                            {
                                                                if (picklistdone != null)
                                                                {
                                                                    string[] picklistdoneoid = picklistdone.Split('@');
                                                                    foreach (string dtldonepick in picklistdoneoid)
                                                                    {
                                                                        if (dtldonepick != null)
                                                                        {
                                                                            if (dtldonepick == dtlpick.Oid.ToString())
                                                                            {
                                                                                pickitem = true;
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                if (dtlpick.PickQty > 0 && pickitem == false)
                                                                {
                                                                    picklistdone = picklistdone + picklistoid + "@";
                                                                    foreach (SalesOrderDetails dtlsales in so.SalesOrderDetails)
                                                                    {
                                                                        if (dtlsales.ItemCode.ItemCode == dtlpack.ItemCode.ItemCode
                                                                            && dtlsales.Oid.ToString() == dtlpick.SOBaseId)
                                                                        {
                                                                            DeliveryOrderDetails newdeliveryitem = deiveryos.CreateObject<DeliveryOrderDetails>();

                                                                            newdeliveryitem.ItemCode = newdeliveryitem.Session.GetObjectByKey<vwItemMasters>(dtlpack.ItemCode.ItemCode);
                                                                            //temporary use picklist from bin
                                                                            if (dtlload.Bin != null)
                                                                            {
                                                                                newdeliveryitem.Warehouse = newdeliveryitem.Session.GetObjectByKey<vwWarehouse>(dtlload.Bin.Warehouse);
                                                                                newdeliveryitem.Bin = newdeliveryitem.Session.GetObjectByKey<vwBin>(dtlload.Bin.BinCode);
                                                                            }

                                                                            //foreach (PickListDetailsActual dtlpick in selectedObject.PickListDetailsActual)
                                                                            //{
                                                                            //    if (dtlpick.FromBin != null)
                                                                            //    {
                                                                            //        newdeliveryitem.Warehouse = newdeliveryitem.Session.GetObjectByKey<vwWarehouse>(dtlpick.FromBin.Warehouse);
                                                                            //        newdeliveryitem.Bin = newdeliveryitem.Session.GetObjectByKey<vwBin>(dtlpick.FromBin.BinCode);
                                                                            //    }
                                                                            //}

                                                                            newdeliveryitem.Quantity = dtlpick.PickQty;

                                                                            //foreach (SalesOrderDetails dtlsales in so.SalesOrderDetails)
                                                                            //{
                                                                            //    if (dtlsales.ItemCode.ItemCode == dtlpack.ItemCode.ItemCode)
                                                                            //    {
                                                                            newdeliveryitem.Price = dtlsales.AdjustedPrice;
                                                                            //    }
                                                                            //}
                                                                            newdeliveryitem.BaseDoc = newload.DocNum.ToString();
                                                                            newdeliveryitem.BaseId = dtlload.Oid.ToString();
                                                                            newdeliveryitem.SODocNum = reader1.GetString(0);
                                                                            newdeliveryitem.SOBaseID = dtlpick.SOBaseId;
                                                                            newdeliveryitem.PickListDocNum = dtlpack.PickListNo;
                                                                            newdeliveryitem.PackListLine = dtlpack.Oid.ToString();

                                                                            newdelivery.DeliveryOrderDetails.Add(newdeliveryitem);
                                                                        }
                                                                    }
                                                                    
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
                                #endregion
                            }
                        }
                        //else
                        //{
                        //    if (selectedObject.IsValid3 == true)
                        //    {
                        //        string docprefix = genCon.GetDocPrefix();

                        //        #region Add Pack List
                        //        string gettobin = "SELECT ToBin FROM PickListDetailsActual WHERE PickList = " + selectedObject.Oid + " GROUP BY ToBin";
                        //        if (conn.State == ConnectionState.Open)
                        //        {
                        //            conn.Close();
                        //        }
                        //        conn.Open();
                        //        SqlCommand cmd = new SqlCommand(gettobin, conn);
                        //        SqlDataReader reader = cmd.ExecuteReader();

                        //        IObjectSpace packos = Application.CreateObjectSpace();
                        //        PackList newpack = packos.CreateObject<PackList>();

                        //        IObjectSpace loados = Application.CreateObjectSpace();
                        //        Load newload = loados.CreateObject<Load>();

                        //        newload.DocNum = genCon.GenerateDocNum(DocTypeList.Load, loados, TransferType.NA, 0, docprefix);
                        //        newload.Status = DocStatus.Submitted;
                        //        if (selectedObject.Driver != null)
                        //        {
                        //            newload.Driver = newload.Session.GetObjectByKey<vwDriver>(selectedObject.Driver.DriverCode);
                        //        }
                        //        if (selectedObject.Vehicle != null)
                        //        {
                        //            newload.Vehicle = newload.Session.GetObjectByKey<vwVehicle>(selectedObject.Vehicle.VehicleCode);
                        //        }

                        //        while (reader.Read())
                        //        {
                        //            newpack.DocNum = genCon.GenerateDocNum(DocTypeList.PAL, packos, TransferType.NA, 0, docprefix);

                        //            newpack.PackingLocation = newpack.Session.GetObjectByKey<vwBin>(reader.GetString(0));
                        //            newpack.Status = DocStatus.Submitted;
                        //            newpack.CustomerGroup = selectedObject.CustomerGroup;

                        //            foreach (PickListDetailsActual dtl in selectedObject.PickListDetailsActual)
                        //            {
                        //                if (dtl.ToBin.BinCode == reader.GetString(0))
                        //                {
                        //                    PackListDetails newpackdetails = packos.CreateObject<PackListDetails>();

                        //                    newpackdetails.ItemCode = newpackdetails.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode.ItemCode);
                        //                    newpackdetails.ItemDesc = dtl.ItemDesc;
                        //                    newpackdetails.Bundle = newpackdetails.Session.GetObjectByKey<BundleType>(1);
                        //                    newpackdetails.Quantity = dtl.PickQty;
                        //                    newpackdetails.PickListNo = selectedObject.DocNum;
                        //                    if (dtl.SOTransporter != null)
                        //                    {
                        //                        newpackdetails.Transporter = packos.FindObject<vwTransporter>
                        //                            (CriteriaOperator.Parse("TransporterName = ?", dtl.SOTransporter));
                        //                    }
                        //                    newpackdetails.BaseDoc = selectedObject.DocNum;

                        //                    foreach (PickListDetails dtl2 in selectedObject.PickListDetails)
                        //                    {
                        //                        if (dtl2.ItemCode.ItemCode == dtl.ItemCode.ItemCode && dtl2.SOBaseDoc == dtl.SOBaseDoc)
                        //                        {
                        //                            newpackdetails.BaseId = dtl2.Oid.ToString();
                        //                        }
                        //                    }

                        //                    newpack.PackListDetails.Add(newpackdetails);
                        //                }
                        //            }

                        //            packos.CommitChanges();

                        //            #region Add Load
                        //            LoadDetails newloaddetails = loados.CreateObject<LoadDetails>();

                        //            newloaddetails.PackList = newpack.DocNum;
                        //            newloaddetails.Bundle = newloaddetails.Session.GetObjectByKey<BundleType>(1);
                        //            newloaddetails.Bin = newloaddetails.Session.GetObjectByKey<vwBin>(reader.GetString(0));
                        //            if (selectedObject.Transporter != null)
                        //            {
                        //                newloaddetails.Transporter = selectedObject.Transporter.TransporterName;
                        //            }
                        //            newloaddetails.BaseDoc = newpack.DocNum;
                        //            newloaddetails.BaseId = newpack.Oid.ToString();

                        //            newload.LoadDetails.Add(newloaddetails);
                        //            #endregion
                        //        }
                        //        conn.Close();
                        //        #endregion

                        //        loados.CommitChanges();

                        //        #region Add Delivery Order
                        //        string getso = "SELECT SOBaseDoc FROM PickListDetailsActual WHERE PickList = " + selectedObject.Oid + " GROUP BY SOBaseDoc";
                        //        if (conn.State == ConnectionState.Open)
                        //        {
                        //            conn.Close();
                        //        }
                        //        conn.Open();
                        //        SqlCommand cmd1 = new SqlCommand(getso, conn);
                        //        SqlDataReader reader1 = cmd1.ExecuteReader();
                        //        while (reader1.Read())
                        //        {
                        //            SalesOrder so = ObjectSpace.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", reader1.GetString(0)));

                        //            if (so != null)
                        //            {
                        //                IObjectSpace deiveryos = Application.CreateObjectSpace();
                        //                DeliveryOrder newdelivery = deiveryos.CreateObject<DeliveryOrder>();

                        //                newdelivery.DocNum = genCon.GenerateDocNum(DocTypeList.DO, deiveryos, TransferType.NA, 0, docprefix);
                        //                newdelivery.Customer = newdelivery.Session.GetObjectByKey<vwBusniessPartner>(so.Customer.BPCode);
                        //                newdelivery.CustomerName = so.CustomerName;
                        //                newdelivery.Status = DocStatus.Submitted;
                        //                newdelivery.CustomerGroup = selectedObject.CustomerGroup;

                        //                foreach (LoadDetails dtlload in newload.LoadDetails)
                        //                {
                        //                    //PackList pl = os.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtlload.PackList));

                        //                    foreach (PackListDetails dtlpack in newpack.PackListDetails)
                        //                    {
                        //                        if (dtlpack.Bundle.BundleID == dtlload.Bundle.BundleID)
                        //                        {
                        //                            DeliveryOrderDetails newdeliveryitem = deiveryos.CreateObject<DeliveryOrderDetails>();

                        //                            newdeliveryitem.ItemCode = newdeliveryitem.Session.GetObjectByKey<vwItemMasters>(dtlpack.ItemCode.ItemCode);
                        //                            //temporary use picklist from bin
                        //                            if (dtlload.Bin != null)
                        //                            {
                        //                                newdeliveryitem.Warehouse = newdeliveryitem.Session.GetObjectByKey<vwWarehouse>(dtlload.Bin.Warehouse);
                        //                                newdeliveryitem.Bin = newdeliveryitem.Session.GetObjectByKey<vwBin>(dtlload.Bin.BinCode);
                        //                            }

                        //                            //foreach (PickListDetailsActual dtlpick in selectedObject.PickListDetailsActual)
                        //                            //{
                        //                            //    if (dtlpick.FromBin != null)
                        //                            //    {
                        //                            //        newdeliveryitem.Warehouse = newdeliveryitem.Session.GetObjectByKey<vwWarehouse>(dtlpick.FromBin.Warehouse);
                        //                            //        newdeliveryitem.Bin = newdeliveryitem.Session.GetObjectByKey<vwBin>(dtlpick.FromBin.BinCode);
                        //                            //    }
                        //                            //}

                        //                            newdeliveryitem.Quantity = dtlpack.Quantity;

                        //                            foreach (SalesOrderDetails dtlsales in so.SalesOrderDetails)
                        //                            {
                        //                                if (dtlsales.ItemCode.ItemCode == dtlpack.ItemCode.ItemCode)
                        //                                {
                        //                                    newdeliveryitem.Price = dtlsales.AdjustedPrice;
                        //                                }
                        //                            }
                        //                            newdeliveryitem.BaseDoc = newload.DocNum.ToString();
                        //                            newdeliveryitem.BaseId = dtlload.Oid.ToString();
                        //                            newdeliveryitem.SODocNum = reader1.GetString(0);
                        //                            newdeliveryitem.PickListDocNum = dtlpack.PickListNo;
                        //                            newdeliveryitem.PackListLine = dtlpack.Oid.ToString();

                        //                            newdelivery.DeliveryOrderDetails.Add(newdeliveryitem);
                        //                        }
                        //                    }
                        //                }

                        //                deiveryos.CommitChanges();
                        //            }
                        //        }
                        //        conn.Close();
                        //        #endregion
                        //    }
                        //}

                        ObjectSpace.CommitChanges();
                        ObjectSpace.Refresh();

                        IObjectSpace os = Application.CreateObjectSpace();
                        PickList trx = os.FindObject<PickList>(new BinaryOperator("Oid", selectedObject.Oid));
                        openNewView(os, trx, ViewEditMode.View);
                        showMsg("Successful", "Submit Done.", InformationType.Success);
                    }
                }
                else
                {
                    showMsg("Error", "No bin selected.", InformationType.Error);
                }
            }
            else
            {
                showMsg("Error", "No Content.", InformationType.Error);
            }
        }

        private void SubmitPL_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CancelPL_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            PickList selectedObject = (PickList)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;

            selectedObject.Status = DocStatus.Cancelled;
            PickListDocTrail ds = ObjectSpace.CreateObject<PickListDocTrail>();
            ds.DocStatus = DocStatus.Cancelled;
            ds.DocRemarks = p.ParamString;
            selectedObject.PickListDocTrail.Add(ds);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("Oid", selectedObject.Oid));
            openNewView(os, trx, ViewEditMode.View);
            showMsg("Successful", "Cancel Done.", InformationType.Success);
        }

        private void CancelPL_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void PreviewPL_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename;

            PickList pl = (PickList)View.CurrentObject;
            SqlConnection conn = new SqlConnection(genCon.getConnectionString());
            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

            try
            {
                ReportDocument doc = new ReportDocument();
                strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\PickList.rpt"));
                strDatabase = conn.Database;
                strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                doc.Refresh();

                doc.SetParameterValue("dockey@", pl.Oid);
                doc.SetParameterValue("dbName@", conn.Database);

                filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                    + "_" + pl.Oid + "_" + user.UserName + "_PL_"
                    + DateTime.Parse(pl.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                doc.Close();
                doc.Dispose();

                string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                    ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                    + "_" + pl.Oid + "_" + user.UserName + "_PL_"
                    + DateTime.Parse(pl.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
                var script = "window.open('" + url + "');";

                WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", script);

                // Start ver 1.0.7
                //pl.PrintStatus = PrintStatus.Printed;
                //pl.PrintCount++;

                //ObjectSpace.CommitChanges();
                //ObjectSpace.Refresh();
                // End ver 1.0.7
            }
            catch (Exception ex)
            {
                showMsg("Fail", ex.Message, InformationType.Error);
            }
        }

        private void PLCopyFromSOCG_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    PickList pl = (PickList)View.CurrentObject;

                    string dupcustomer = null;
                    foreach (vwPaymentSOGroup customer in e.PopupWindowViewSelectedObjects)
                    {
                        if (dupcustomer != null)
                        {
                            if (customer.Customer != dupcustomer)
                            {
                                showMsg("Error", "Copy fail, duplicate customer found.", InformationType.Error);
                                return;
                            }
                        }
                        if (dupcustomer == null)
                        {
                            dupcustomer = customer.Customer;
                        }

                        foreach (PickListDetails pllist in pl.PickListDetails)
                        {
                            if (customer.Customer != pllist.Customer.BPCode)
                            {
                                showMsg("Error", "Copy fail, duplicate customer found.", InformationType.Error);
                                return;
                            }
                        }
                    }

                    foreach (vwPaymentSOGroup sog in e.PopupWindowViewSelectedObjects)
                    {
                        IObjectSpace os = Application.CreateObjectSpace();
                        vwPaymentSOGroup so = os.FindObject<vwPaymentSOGroup>(CriteriaOperator.Parse("DocNum = ?", sog.DocNum));

                        if (so == null)
                        {
                            showMsg("Error", "SO already created pick list, please refresh data.", InformationType.Error);
                            return;
                        }

                        IList<vwPaymentSO> solist = os.GetObjects<vwPaymentSO>
                            (CriteriaOperator.Parse("DocNum = ?", sog.DocNum));

                        foreach (vwPaymentSO dtl in solist)
                        {
                            PickListDetails newplitem = ObjectSpace.CreateObject<PickListDetails>();

                            vwBusniessPartner customer = ObjectSpace.FindObject<vwBusniessPartner>(CriteriaOperator.Parse("BPCode = ?", dtl.Customer));
                            if (customer != null)
                            {
                                pl.CustomerGroup = customer.GroupName;

                                // Start ver 1.0.8.1
                                if (pl.Customer == null)
                                {
                                    pl.Customer = customer.BPCode;
                                }
                                if (pl.CustomerName == null)
                                {
                                    pl.CustomerName = customer.BPName;
                                }
                                // End ver 1.0.8.1
                            }
                            if (dtl.Transporter != null)
                            {
                                //pl.Transporter = newplitem.Session.GetObjectByKey<vwTransporter>(dtl.Transporter.TransporterID);
                                pl.Transporter = newplitem.Session.FindObject<vwTransporter>(CriteriaOperator.Parse("TransporterName = ?", dtl.Transporter)); ;
                            }

                            newplitem.ItemCode = newplitem.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode);
                            newplitem.ItemDesc = dtl.ItemDesc;
                            newplitem.CatalogNo = dtl.CatalogNo;
                            if (dtl.Warehouse != null)
                            {
                                newplitem.Warehouse = newplitem.Session.GetObjectByKey<vwWarehouse>(dtl.Warehouse);
                            }
                            newplitem.PlanQty = dtl.Quantity;
                            if (dtl.Customer != null)
                            {
                                newplitem.Customer = newplitem.Session.GetObjectByKey<vwBusniessPartner>(dtl.Customer);
                            }
                            newplitem.SOBaseDoc = dtl.DocNum;
                            newplitem.SOBaseId = dtl.Oid;
                            newplitem.SOCreateDate = dtl.CreateDate;
                            newplitem.SOExpectedDate = dtl.PostingDate;
                            newplitem.SORemarks = dtl.Remarks;
                            newplitem.SOSalesperson = dtl.Salesperson;
                            newplitem.SODeliveryDate = dtl.DeliveryDate;
                            if (dtl.Priority != null)
                            {
                                newplitem.Priority = newplitem.Session.GetObjectByKey<PriorityType>(dtl.Priority.Oid);

                                // Start ver 1.0.8.1
                                if (pl.Priority == null)
                                {
                                    pl.Priority = pl.Session.GetObjectByKey<PriorityType>(dtl.Priority.Oid);
                                }
                                // End ver 1.0.8.1
                            }
                            if (dtl.Transporter != null)
                            {
                                //newplitem.SOTransporter = dtl.Transporter.TransporterName.ToString();
                                newplitem.SOTransporter = dtl.Transporter.ToString();
                            }

                            IObjectSpace sos = Application.CreateObjectSpace();
                            vwPaymentSO dupso = sos.FindObject<vwPaymentSO>(CriteriaOperator.Parse("Oid = ? and DocNum = ?",
                                dtl.Oid, dtl.DocNum));

                            if (dupso == null)
                            {
                                showMsg("Error", "SO already created pick list, please refresh data.", InformationType.Error);
                                return;
                            }

                            pl.PickListDetails.Add(newplitem);
                        }

                        showMsg("Success", "Copy Success.", InformationType.Success);
                    }

                    // Start ver 1.0.8.1
                    string dupsonum = null;
                    pl.SONumber = null;
                    foreach (PickListDetails dtlsonum in pl.PickListDetails)
                    {
                        if (dupsonum != dtlsonum.SOBaseDoc)
                        {
                            if (pl.SONumber == null)
                            {
                                pl.SONumber = dtlsonum.SOBaseDoc;
                            }
                            else
                            {
                                pl.SONumber = pl.SONumber + ", " + dtlsonum.SOBaseDoc;
                            }

                            dupsonum = dtlsonum.SOBaseDoc;
                        }
                    }

                    string deliverydate = pl.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.SODeliveryDate).Min().SODeliveryDate.Date.ToString();
                    pl.SODeliveryDate = deliverydate.Substring(0, 10);
                    // End ver 1.0.8.1

                    if (pl.DocNum == null)
                    {
                        string docprefix = genCon.GetDocPrefix();
                        pl.DocNum = genCon.GenerateDocNum(DocTypeList.PL, ObjectSpace, TransferType.NA, 0, docprefix);
                    }

                    ObjectSpace.CommitChanges();
                    ObjectSpace.Refresh();
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
        }

        private void PLCopyFromSOCG_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            PickList pl = (PickList)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(vwPaymentSOGroup));
            var cs = Application.CreateCollectionSource(os, typeof(vwPaymentSOGroup), viewId);
            if (pl.Warehouse != null)
            {
                cs.Criteria["Warehouse"] = new BinaryOperator("Warehouse", pl.Warehouse.WarehouseCode);
            }
            else
            {
                cs.Criteria["Warehouse"] = new BinaryOperator("Warehouse", "");
            }

            if (pl.Transporter != null)
            {
                cs.Criteria["Transporter"] = new BinaryOperator("Transporter", pl.Transporter.TransporterName);
            }

            if (pl.DeliveryDate != null)
            {
                cs.Criteria["DeliveryDate"] = new BinaryOperator("DeliveryDate", pl.DeliveryDate.Year + "-" + pl.DeliveryDate.Month.ToString("00") + "-" + pl.DeliveryDate.Day.ToString("00"), BinaryOperatorType.LessOrEqual);
            }

            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }

        private void PLCopyFromPLDetail_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    PickList pl = (PickList)View.CurrentObject;

                    foreach (PickListDetails pld in e.PopupWindowViewSelectedObjects)
                    {
                        PickListDetailsActual newplactualitem = ObjectSpace.CreateObject<PickListDetailsActual>();

                        newplactualitem.ItemCode = newplactualitem.Session.GetObjectByKey<vwItemMasters>(pld.ItemCode.ItemCode);
                        newplactualitem.Warehouse = newplactualitem.Session.GetObjectByKey<vwWarehouse>(pld.Warehouse.WarehouseCode);

                        newplactualitem.SOBaseDoc = pld.SOBaseDoc;
                        newplactualitem.SOBaseId = pld.SOBaseId;
                        newplactualitem.SOCreateDate = pld.SOCreateDate;
                        newplactualitem.SOExpectedDate = pld.SOExpectedDate;
                        newplactualitem.SORemarks = pld.SORemarks;
                        newplactualitem.SOSalesperson = pld.SOSalesperson;
                        newplactualitem.SOTransporter = pld.SOTransporter;
                        newplactualitem.Priority = newplactualitem.Session.GetObjectByKey<PriorityType>(pld.Priority.Oid);
                        newplactualitem.SODeliveryDate = pld.SODeliveryDate;
                        newplactualitem.PickListDetailOid = pld.Oid;

                        pl.PickListDetailsActual.Add(newplactualitem);

                        pld.CopyTo = true;

                        showMsg("Success", "Copy Success.", InformationType.Success);
                    }

                    // Start ver 1.0.8.1
                    pl.Customer = null;
                    pl.CustomerName = null;
                    pl.Priority = null;
                    pl.SONumber = null;
                    pl.SODeliveryDate = null;
                    string dupso = null;
                    foreach (PickListDetails dtl in pl.PickListDetails)
                    {
                        // Start ver 1.0.8.1
                        if (pl.Customer == null)
                        {
                            pl.Customer = dtl.Customer.BPCode;
                        }
                        if (pl.CustomerName == null)
                        {
                            pl.CustomerName = dtl.Customer.BPName;
                        }
                        if (pl.Priority == null)
                        {
                            pl.Priority = pl.Session.GetObjectByKey<PriorityType>(dtl.Priority.Oid);
                        }

                        if (dupso != dtl.SOBaseDoc)
                        {
                            if (pl.SONumber == null)
                            {
                                pl.SONumber = dtl.SOBaseDoc;
                            }
                            else
                            {
                                pl.SONumber = pl.SONumber + ", " + dtl.SOBaseDoc;
                            }

                            dupso = dtl.SOBaseDoc;
                        }

                        string deliverydate = pl.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.SODeliveryDate).Min().SODeliveryDate.Date.ToString();
                        pl.SODeliveryDate = deliverydate.Substring(0, 10);
                        // End ver 1.0.8.1
                    }
                    // End ver 1.0.8.1

                    ObjectSpace.CommitChanges();
                    ObjectSpace.Refresh();
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
        }

        private void PLCopyFromPLDetail_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            PickList pl = (PickList)View.CurrentObject;

            foreach (PickListDetails dtl in pl.PickListDetails)
            {
                dtl.CopyTo = false;

                foreach (PickListDetailsActual dtl2 in pl.PickListDetailsActual)
                {
                    if (dtl2.PickListDetailOid == dtl.Oid && dtl.PlanQty <= dtl.PickQty)
                    {
                        dtl.CopyTo = true;
                    }
                }
            }

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(PickListDetails));
            var cs = Application.CreateCollectionSource(os, typeof(PickListDetails), viewId);
            cs.Criteria["PickList.Oid"] = new BinaryOperator("PickList.Oid", pl.Oid);
            cs.Criteria["CopyTo"] = new BinaryOperator("CopyTo", "False");

            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }

        // Start ver 1.0.7
        private void PrintPL_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count >= 1)
            {
                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
                int cnt = 1;
                foreach (PickList dtl in e.SelectedObjects)
                {
                    string strServer;
                    string strDatabase;
                    string strUserID;
                    string strPwd;
                    string filename;

                    IObjectSpace os = Application.CreateObjectSpace();
                    PickList pl = os.FindObject<PickList>(new BinaryOperator("Oid", dtl.Oid));

                    try
                    {
                        ReportDocument doc = new ReportDocument();
                        strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                        doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\PickList.rpt"));
                        strDatabase = conn.Database;
                        strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                        strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                        doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                        doc.Refresh();

                        doc.SetParameterValue("dockey@", pl.Oid);
                        doc.SetParameterValue("dbName@", conn.Database);

                        filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                            + "_" + pl.Oid + "_" + user.UserName + "_PL_"
                            + DateTime.Parse(pl.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                        doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                        doc.Close();
                        doc.Dispose();

                        string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                            ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                            + "_" + pl.Oid + "_" + user.UserName + "_PL_"
                            + DateTime.Parse(pl.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
                        var script = "window.open('" + url + "');";

                        WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile" + cnt, script);

                        pl.PrintStatus = PrintStatus.Printed;
                        pl.PrintCount++;

                        os.CommitChanges();
                        os.Refresh();
                        cnt++;
                    }
                    catch (Exception ex)
                    {
                        showMsg("Fail", ex.Message, InformationType.Error);
                    }
                }
            }
            else
            {
                showMsg("Fail", "Please select pick list to print.", InformationType.Error);
            }
        }
        // End ver 1.0.7

        // Start ver 1.0.8
        private void PrintPLByZone_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count >= 1)
            {
                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
                int cnt = 1;
                foreach (PickList dtl in e.SelectedObjects)
                {
                    string strServer;
                    string strDatabase;
                    string strUserID;
                    string strPwd;
                    string filename;

                    IObjectSpace os = Application.CreateObjectSpace();
                    PickList pl = os.FindObject<PickList>(new BinaryOperator("Oid", dtl.Oid));

                    try
                    {
                        ReportDocument doc = new ReportDocument();
                        strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                        doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\PickListByZone.rpt"));
                        strDatabase = conn.Database;
                        strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                        strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                        doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                        doc.Refresh();

                        doc.SetParameterValue("dockey@", pl.Oid);
                        doc.SetParameterValue("dbName@", conn.Database);

                        filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                            + "_" + pl.Oid + "_" + user.UserName + "_PLByZone_"
                            + DateTime.Parse(pl.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                        doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                        doc.Close();
                        doc.Dispose();

                        string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                            ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                            + "_" + pl.Oid + "_" + user.UserName + "_PLByZone_"
                            + DateTime.Parse(pl.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
                        var script = "window.open('" + url + "');";

                        WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile" + cnt, script);

                        pl.PrintStatus = PrintStatus.Printed;
                        pl.PrintCount++;

                        os.CommitChanges();
                        os.Refresh();
                        cnt++;
                    }
                    catch (Exception ex)
                    {
                        showMsg("Fail", ex.Message, InformationType.Error);
                    }
                }
            }
            else
            {
                showMsg("Fail", "Please select pick list to print.", InformationType.Error);
            }
        }
        // End ver 1.0.8
    }
}
