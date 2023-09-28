using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.XtraGrid.EditForm.Helpers;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Advanced_Shipment_Notice;
using StarLaiPortal.Module.BusinessObjects.Credit_Notes_Cancellation;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.GRN;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.Purchase_Order;
using StarLaiPortal.Module.BusinessObjects.Purchase_Return;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using StarLaiPortal.Module.BusinessObjects.Sales_Order_Collection;
using StarLaiPortal.Module.BusinessObjects.Sales_Quotation;
using StarLaiPortal.Module.BusinessObjects.Sales_Refund;
using StarLaiPortal.Module.BusinessObjects.Sales_Return;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.Stock_Adjustment;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using StarLaiPortal.Module.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;

// 2023-07-28 add AR Downpayment cancalletion ver 1.0.7
// 2023-08-25 add picklistactual validation ver 1.0.9
// 2023-04-09 fix speed issue ver 1.0.8.1
// 2023-09-25 copy warehouse ver 1.0.10

namespace StarLaiPortal.Module.Web.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class WebModificationControllers : WebModificationsController
    {
        GeneralControllers genCon;
        public WebModificationControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            Frame.GetController<ModificationsController>().SaveAndNewAction.Active.SetItemValue("Enabled", false);
            Frame.GetController<ModificationsController>().SaveAndCloseAction.Active.SetItemValue("Enabled", false);
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GeneralControllers>();
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
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

        protected override void Save(SimpleActionExecuteEventArgs args)
        {
            if (View.ObjectTypeInfo.Type == typeof(SalesQuotation))
            {
                SalesQuotation CurrObject = (SalesQuotation)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SQ, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                if (CurrObject.Series != null)
                {
                    if (CurrObject.Series.SeriesName == "Dropship")
                    {
                        genCon.showMsg("Warning", "Please change Shipping Address.", InformationType.Warning);
                    }
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(SalesOrder))
            {
                SalesOrder CurrObject = (SalesOrder)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SO, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(PickList))
            {
                PickList CurrObject = (PickList)args.CurrentObject;
                bool over = false;
                string overitem = null;

                CurrObject.Customer = null;
                CurrObject.CustomerName = null;
                CurrObject.Priority = null;
                CurrObject.SONumber = null;
                CurrObject.SODeliveryDate = null;
                string dupso = null;
                foreach (PickListDetails dtl in CurrObject.PickListDetails)
                {
                    int pickqty = 0;
                    foreach (PickListDetailsActual dtl2 in CurrObject.PickListDetailsActual)
                    {
                        if (dtl2.PickListDetailOid == dtl.Oid)
                        {
                            pickqty = pickqty + (int)dtl2.PickQty;
                        }
                    }

                    dtl.PickQty = pickqty;

                    if (pickqty > dtl.PlanQty)
                    {
                        over = true;
                        overitem = dtl.ItemCode.ItemCode;
                    }

                    // Start ver 1.0.8.1
                    if (CurrObject.Customer == null)
                    {
                        CurrObject.Customer = dtl.Customer.BPCode;
                    }
                    if (CurrObject.CustomerName == null)
                    {
                        CurrObject.CustomerName = dtl.Customer.BPName;
                    }
                    if (CurrObject.Priority == null)
                    {
                        CurrObject.Priority = CurrObject.Session.GetObjectByKey<PriorityType>(dtl.Priority.Oid);
                    }

                    if (dupso != dtl.SOBaseDoc)
                    {
                        if (CurrObject.SONumber == null)
                        {
                            CurrObject.SONumber = dtl.SOBaseDoc;
                        }
                        else
                        {
                            CurrObject.SONumber = CurrObject.SONumber + ", " + dtl.SOBaseDoc;
                        }

                        dupso = dtl.SOBaseDoc;
                    }

                    string deliverydate = CurrObject.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.SODeliveryDate).Min().SODeliveryDate.Date.ToString();
                    CurrObject.SODeliveryDate = deliverydate.Substring(0, 10);
                    // End ver 1.0.8.1
                }

                if (over == true)
                {
                    showMsg("Error", "Pick qty more than plan qty. Item : " + overitem, InformationType.Error);
                    return;
                }

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.PL, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(PackList))
            {
                PackList CurrObject = (PackList)args.CurrentObject;

                // Start ver 1.0.8.1
                string duppl = null;
                string dupso = null;
                string dupcustomer = null;
                CurrObject.SONumber = null;
                CurrObject.SAPSONo = null;
                CurrObject.Customer = null;
                CurrObject.PickListNo = null;
                CurrObject.Priority = null;
                foreach (PackListDetails dtl in CurrObject.PackListDetails)
                {
                    if (duppl != dtl.PickListNo)
                    {
                        PickList picklist = ObjectSpace.FindObject<PickList>(CriteriaOperator.Parse("DocNum = ?", dtl.PickListNo));

                        if (picklist != null)
                        {
                            foreach (PickListDetails dtl2 in picklist.PickListDetails)
                            {
                                if (dupso != dtl2.SOBaseDoc)
                                {
                                    if (CurrObject.SONumber == null)
                                    {
                                        CurrObject.SONumber = dtl2.SOBaseDoc;
                                    }
                                    else
                                    {
                                        CurrObject.SONumber = CurrObject.SONumber + ", " + dtl2.SOBaseDoc;
                                    }

                                    SalesOrder salesorder = ObjectSpace.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", dtl2.SOBaseDoc));

                                    if (salesorder != null)
                                    {
                                        if (CurrObject.SAPSONo == null)
                                        {
                                            CurrObject.SAPSONo = salesorder.SAPDocNum;
                                        }
                                        else
                                        {
                                            CurrObject.SAPSONo = CurrObject.SAPSONo + ", " + salesorder.SAPDocNum;
                                        }
                                    }

                                    dupso = dtl2.SOBaseDoc;
                                }

                                if (dupcustomer != dtl2.Customer.BPName)
                                {
                                    if (CurrObject.Customer == null)
                                    {
                                        CurrObject.Customer = dtl2.Customer.BPName;
                                    }
                                    else
                                    {
                                        CurrObject.Customer = CurrObject.Customer + ", " + dtl2.Customer.BPName;
                                    }

                                    dupcustomer = dtl2.Customer.BPName;
                                }
                            }

                            if (CurrObject.Priority == null)
                            {
                                CurrObject.Priority = picklist.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.Priority).Max().Priority;
                            }

                            // Start ver 1.0.10
                            if (CurrObject.Warehouse == null)
                            {
                                CurrObject.Warehouse = CurrObject.Session.GetObjectByKey<vwWarehouse>(picklist.Warehouse.WarehouseCode);
                            }
                            // End ver 1.0.10
                        }

                        if (CurrObject.PickListNo == null)
                        {
                            CurrObject.PickListNo = dtl.PickListNo;
                        }
                        else
                        {
                            CurrObject.PickListNo = CurrObject.PickListNo + ", " + dtl.PickListNo;
                        }

                        duppl = dtl.PickListNo;
                    }
                }    
                // End ver 1.0.8.1

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.PAL, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(Load))
            {
                Load CurrObject = (Load)args.CurrentObject;

                // Start ver 1.0.8.1
                CurrObject.PackListNo = null;
                CurrObject.SONumber = null;
                CurrObject.Priority = null;
                string duppack = null;
                foreach (LoadDetails dtl in CurrObject.LoadDetails)
                {
                    if (duppack != dtl.BaseDoc)
                    {
                        if (CurrObject.PackListNo == null)
                        {
                            CurrObject.PackListNo = dtl.BaseDoc;
                        }
                        else
                        {
                            CurrObject.PackListNo = CurrObject.PackListNo + ", " + dtl.BaseDoc;
                        }

                        duppack = dtl.BaseDoc;
                    }

                    PackList pack = ObjectSpace.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtl.PackList));

                    if (pack != null)
                    {
                        if (CurrObject.SONumber == null)
                        {
                            CurrObject.SONumber = pack.SONumber;
                        }

                        if (CurrObject.Priority == null)
                        {
                            CurrObject.Priority = pack.Priority;
                        }

                        // Start ver 1.0.10
                        if (CurrObject.Warehouse == null)
                        {
                            CurrObject.Warehouse = CurrObject.Session.GetObjectByKey<vwWarehouse>(pack.Warehouse.WarehouseCode);
                        }
                        // End ver 1.0.10
                    }
                }
                // End ver 1.0.8.1

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.Load, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(PurchaseOrders))
            {
                PurchaseOrders CurrObject = (PurchaseOrders)args.CurrentObject;
                bool sellingprice = false;
                bool zerototal = false;
                string sellingitem = null;

                if (CurrObject.PurchaseOrderDetails.Sum(s => s.Total) <= 0)
                {
                    zerototal = true;
                }

                foreach (PurchaseOrderDetails dtl in CurrObject.PurchaseOrderDetails)
                {
                    if (dtl.AdjustedPrice > dtl.SellingPrice && dtl.BaseDoc != null)
                    {
                        if (dtl.Series == "BackOrdP" || dtl.Series == "BackOrdS")
                        {
                            sellingprice = true;
                            if (sellingitem == null)
                            {
                                sellingitem = dtl.ItemCode.ItemCode;
                            }
                            else
                            {
                                sellingitem = sellingitem + ", " + dtl.ItemCode.ItemCode;
                            }
                        }
                    }
                }

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.PO, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();

                if (sellingprice == true && zerototal == false)
                {
                    showMsg("Warning", "Item: " + sellingitem + " adjusted price higher than selling price.", InformationType.Warning);
                }

                if (sellingprice == false && zerototal == true)
                {
                    showMsg("Warning", "Document with 0 amount.", InformationType.Warning);
                }

                if (sellingprice == true && zerototal == true)
                {
                    showMsg("Warning", "Item: " + sellingitem + " adjusted price higher than selling price."
                        + System.Environment.NewLine + System.Environment.NewLine +
                        "Document with 0 amount.", InformationType.Warning);
                }
            }
            else if (View.ObjectTypeInfo.Type == typeof(ASN))
            {
                ASN CurrObject = (ASN)args.CurrentObject;

                // Start ver 1.0.8.1
                CurrObject.PONo = null;
                string duppo = null;
                foreach (ASNDetails dtl in CurrObject.ASNDetails)
                {
                    dtl.OIDKey = dtl.Oid;

                    if (duppo != dtl.PORefNo)
                    {
                        if (CurrObject.PONo == null)
                        {
                            CurrObject.PONo = dtl.PORefNo;
                        }
                        else
                        {
                            CurrObject.PONo = CurrObject.PONo + ", " + dtl.PORefNo;
                        }

                        duppo = dtl.PORefNo;
                    }
                }
                // End ver 1.0.8.1

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.ASN, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(GRN))
            {
                GRN CurrObject = (GRN)args.CurrentObject;

                // Start ver 1.0.8.1
                string duppo = null;
                string dupporef = null;
                string dupasn = null;
                CurrObject.SAPPONo = null;
                CurrObject.PortalPONo = null;
                CurrObject.ASNNo = null;
                foreach (GRNDetails dtl in CurrObject.GRNDetails)
                {
                    dtl.OIDKey = dtl.Oid;

                    if (dtl.PONo != null)
                    {
                        if (duppo != dtl.PONo)
                        {
                            if (CurrObject.SAPPONo == null)
                            {
                                CurrObject.SAPPONo = dtl.PONo;
                            }
                            else
                            {
                                CurrObject.SAPPONo = CurrObject.SAPPONo + ", " + dtl.PONo;
                            }

                            duppo = dtl.PONo;
                        }
                    }

                    if (dtl.PORefNo != null)
                    {
                        if (dupporef != dtl.PORefNo)
                        {
                            if (CurrObject.PortalPONo == null)
                            {
                                CurrObject.PortalPONo = dtl.PORefNo;
                            }
                            else
                            {
                                CurrObject.PortalPONo = CurrObject.PortalPONo + ", " + dtl.PORefNo;
                            }

                            dupporef = dtl.PORefNo;
                        }
                    }

                    if (dtl.ASNBaseDoc != null)
                    {
                        if (dupasn != dtl.ASNBaseDoc)
                        {
                            if (CurrObject.ASNNo == null)
                            {
                                CurrObject.ASNNo = dtl.ASNBaseDoc;
                            }
                            else
                            {
                                CurrObject.ASNNo = CurrObject.ASNNo + ", " + dtl.ASNBaseDoc;
                            }

                            dupasn = dtl.ASNBaseDoc;
                        }
                    }
                }
                // End ver 1.0.8.1

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.GRN, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                foreach (GRNDetails dtl in CurrObject.GRNDetails)
                {
                    if (dtl.ASNBaseDoc != null)
                    {
                        genCon.CloseASN(dtl.ASNBaseDoc, "Copy", ObjectSpace);
                        break;
                    }
                }

                IObjectSpace os = Application.CreateObjectSpace();
                GRN trx = os.FindObject<GRN>(new BinaryOperator("Oid", CurrObject.Oid));

                foreach (GRNDetails dtl2 in trx.GRNDetails)
                {
                    dtl2.OIDKey = dtl2.Oid;
                }

                os.CommitChanges();
                os.Refresh();

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequests))
            {
                PurchaseReturnRequests CurrObject = (PurchaseReturnRequests)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.PRR, ObjectSpace, TransferType.NA, CurrObject.Series.Oid, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(PurchaseReturns))
            {
                PurchaseReturns CurrObject = (PurchaseReturns)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.PR, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                foreach(PurchaseReturnDetails dtl in CurrObject.PurchaseReturnDetails)
                {
                    if (dtl.BaseDoc != null)
                    {
                        genCon.ClosePurchaseReturnReq(dtl.BaseDoc, "Copy", ObjectSpace, CurrObject.Requestor.SlpCode);
                        break;
                    }
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(SalesReturnRequests))
            {
                SalesReturnRequests CurrObject = (SalesReturnRequests)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SRR, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(SalesReturns))
            {
                SalesReturns CurrObject = (SalesReturns)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SR, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                foreach (SalesReturnDetails dtl in CurrObject.SalesReturnDetails)
                {
                    if (dtl.BaseDoc != null)
                    {
                        genCon.CloseSalesReturnReq(dtl.BaseDoc, "Copy", ObjectSpace, CurrObject.Salesperson.SlpCode);
                        break;
                    }
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(WarehouseTransferReq))
            {
                WarehouseTransferReq CurrObject = (WarehouseTransferReq)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.WTR, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(WarehouseTransfers))
            {
                WarehouseTransfers CurrObject = (WarehouseTransfers)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.WT, ObjectSpace, CurrObject.TransferType, 0, docprefix);
                }

                foreach (WarehouseTransferDetails dtl in CurrObject.WarehouseTransferDetails)
                {
                    if (dtl.BaseDoc != null)
                    {
                        genCon.CloseWarehouseTransferReq(dtl.BaseDoc, "Copy", ObjectSpace);
                        break;
                    }
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(StockAdjustmentRequests))
            {
                StockAdjustmentRequests CurrObject = (StockAdjustmentRequests)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SAR, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(StockAdjustments))
            {
                StockAdjustments CurrObject = (StockAdjustments)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SA, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                foreach (StockAdjustmentDetails dtl in CurrObject.StockAdjustmentDetails)
                {
                    if (dtl.BaseDoc != null)
                    {
                        genCon.CloseWarehouseTransferReq(dtl.BaseDoc, "Copy", ObjectSpace);
                        break;
                    }
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if(View.ObjectTypeInfo.Type == typeof(SalesQuotationDetails))
            {
                SalesQuotationDetails CurrObject = (SalesQuotationDetails)args.CurrentObject;

                if (CurrObject.AdjustedPrice < CurrObject.Price)
                {
                    genCon.showMsg("Warning", "Adjust price lower than original price.", InformationType.Warning);
                }

                base.Save(args);
            }
            else if (View.ObjectTypeInfo.Type == typeof(SalesOrderCollection))
            {
                SalesOrderCollection CurrObject = (SalesOrderCollection)args.CurrentObject;

                // Start ver 1.0.8.1
                CurrObject.SONumber = null;
                foreach (SalesOrderCollectionDetails dtl in CurrObject.SalesOrderCollectionDetails)
                {
                    if (CurrObject.SONumber != null)
                    {
                        CurrObject.SONumber = CurrObject.SONumber + ", " + dtl.SalesOrder;
                    }
                    else
                    {
                        CurrObject.SONumber = dtl.SalesOrder;
                    }
                }
                // End ver 1.0.8.1

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.ARD, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(SalesRefundRequests))
            {
                SalesRefundRequests CurrObject = (SalesRefundRequests)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SRF, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(SalesRefunds))
            {
                SalesRefunds CurrObject = (SalesRefunds)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.SRefund, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                foreach (SalesRefundDetails dtl in CurrObject.SalesRefundDetails)
                {
                    if (dtl.BaseDoc != null)
                    {
                        genCon.CloseSalesRefund(dtl.BaseDoc, "Copy", ObjectSpace);
                        break;
                    }
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            else if (View.ObjectTypeInfo.Type == typeof(DeliveryOrder))
            {
                DeliveryOrder CurrObject = (DeliveryOrder)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.DO, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            // Start ver 1.0.7
            else if (View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancel))
            {
                ARDownpaymentCancel CurrObject = (ARDownpaymentCancel)args.CurrentObject;

                base.Save(args);
                if (CurrObject.DocNum == null)
                {
                    string docprefix = genCon.GetDocPrefix();
                    CurrObject.DocNum = genCon.GenerateDocNum(DocTypeList.ARDC, ObjectSpace, TransferType.NA, 0, docprefix);
                }

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
            // End ver 1.0.7
            // Start ver 1.0.9
            else if (View.ObjectTypeInfo.Type == typeof(PickListDetailsActual))
            {
                PickListDetailsActual CurrObject = (PickListDetailsActual)args.CurrentObject;

                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();

                bool over = false;
                string overitem = null;

                foreach (PickListDetails dtl in CurrObject.PickList.PickListDetails)
                {
                    int pickqty = 0;
                    if (CurrObject.PickListDetailOid == dtl.Oid)
                    {
                        pickqty = pickqty + (int)CurrObject.PickQty;
                    }

                    dtl.PickQty = pickqty;

                    if (pickqty > dtl.PlanQty)
                    {
                        over = true;
                        overitem = dtl.ItemCode.ItemCode;
                    }
                }

                if (over == true)
                {
                    showMsg("Error", "Pick qty more than plan qty. Item : " + overitem, InformationType.Error);
                    return;
                }
            }
            // End ver 1.0.9
            else
            {
                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
        }
    }
}
