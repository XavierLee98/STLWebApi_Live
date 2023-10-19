using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web.Templates.ActionContainers.Menu;
using DevExpress.ExpressApp.Web.Templates.ActionContainers;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Dashboard;
using StarLaiPortal.Module.BusinessObjects.Inquiry_View;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Web;
using System.Web.UI.WebControls;
using DevExpress.Xpo.DB;
using DevExpress.ExpressApp.Web.Templates;
using DevExpress.ExpressApp.Web;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;

// 2023-09-14 - add filter into inquiry - ver 1.0.9
// 2023-10-16 - sales order inquiry add "All" option for filter and view button - ver 1.0.11

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class InquiryViewControllers : ViewController
    {
        // Start ver 1.0.9
        private DateTime Fromdate;
        private DateTime Todate;
        // End ver 1.0.9
        public InquiryViewControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.ViewOpenPickList.Active.SetItemValue("Enabled", false);
            this.ViewPickListDetailInquiry.Active.SetItemValue("Enabled", false);
            this.ViewPickListInquiry.Active.SetItemValue("Enabled", false);
            // Start ver 1.0.9
            this.InquiryStatus.Active.SetItemValue("Enabled", false);
            this.InquiryDateFrom.Active.SetItemValue("Enabled", false);
            this.InquiryDateTo.Active.SetItemValue("Enabled", false);
            this.InquiryFilter.Active.SetItemValue("Enabled", false);
            // End ver 1.0.9
            // Start ver 1.0.11
            this.ViewSalesOrderInquiry.Active.SetItemValue("Enabled", false);
            // End ver 1.0.11

            if (typeof(vwInquiryOpenPickList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryOpenPickList))
                {
                    this.ViewOpenPickList.Active.SetItemValue("Enabled", true);
                    this.ViewOpenPickList.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                }
            }

            if (typeof(vwInquiryPickListDetails).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickListDetails))
                {
                    this.ViewPickListDetailInquiry.Active.SetItemValue("Enabled", true);
                    this.ViewPickListDetailInquiry.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                }
            }

            if (typeof(vwInquiryPickList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickList))
                {
                    this.ViewPickListInquiry.Active.SetItemValue("Enabled", true);
                    this.ViewPickListInquiry.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                }
            }

            // Start ver 1.0.9
            if (typeof(vwInquirySalesOrder).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquirySalesOrder))
                {
                    this.ViewSalesOrderInquiry.Active.SetItemValue("Enabled", true);
                    this.ViewSalesOrderInquiry.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;

                    if (View.Id == "vwInquirySalesOrder_ListView")
                    {
                        InquiryStatus.Items.Clear();

                        // Start ver 1.0.11
                        InquiryStatus.Items.Add(new ChoiceActionItem("All", "All"));
                        // End ver 1.0.11
                        InquiryStatus.Items.Add(new ChoiceActionItem("Open", "Open"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Draft", "Draft"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Closed", "Closed"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Posted", "Posted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Pending Post", "Pending Post"));

                        InquiryStatus.SelectedIndex = 1;

                        this.InquiryStatus.Active.SetItemValue("Enabled", true);
                        InquiryStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        InquiryStatus.CustomizeControl += action_CustomizeControl;

                        this.InquiryDateFrom.Active.SetItemValue("Enabled", true);
                        this.InquiryDateFrom.Value = DateTime.Today.AddDays(-7);
                        InquiryDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.InquiryDateTo.Active.SetItemValue("Enabled", true);
                        this.InquiryDateTo.Value = DateTime.Today.AddDays(1);
                        InquiryDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.InquiryFilter.Active.SetItemValue("Enabled", true);

                        // Start ver 1.0.11
                        if (InquiryStatus.SelectedItem.Id != "All")
                        {
                            // End ver 1.0.11
                            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?",
                            InquiryStatus.SelectedItem.Id, InquiryDateFrom.Value, InquiryDateTo.Value);
                        // Start ver 1.0.11
                        }
                        else
                        {
                            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                                InquiryDateFrom.Value, InquiryDateTo.Value);
                        }
                        // End ver 1.0.11
                    }
                }
            }

            if (typeof(vwInquiryPickList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickList))
                {
                    if (View.Id == "vwInquiryPickList_ListView")
                    {
                        InquiryStatus.Items.Clear();

                        InquiryStatus.Items.Add(new ChoiceActionItem("Open", "Open"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Draft", "Draft"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Closed", "Closed"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Posted", "Posted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Pending Post", "Pending Post"));

                        InquiryStatus.SelectedIndex = 1;

                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ?",
                            InquiryStatus.SelectedItem.Id);

                        this.InquiryStatus.Active.SetItemValue("Enabled", true);
                        InquiryStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        InquiryStatus.CustomizeControl += action_CustomizeControl;

                        this.InquiryDateFrom.Active.SetItemValue("Enabled", true);
                        this.InquiryDateFrom.Value = DateTime.Today.AddDays(-7);
                        InquiryDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.InquiryDateTo.Active.SetItemValue("Enabled", true);
                        this.InquiryDateTo.Value = DateTime.Today.AddDays(1);
                        InquiryDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.InquiryFilter.Active.SetItemValue("Enabled", true);

                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                        "and DocDate >= ? and DocDate <= ?",
                        InquiryStatus.SelectedItem.Id, InquiryDateFrom.Value, InquiryDateTo.Value);
                    }
                }
            }
            // End ver 1.0.9
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control. 
        }

        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        void action_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            SingleChoiceActionAsModeMenuActionItem actionItem = e.Control as SingleChoiceActionAsModeMenuActionItem;
            if (actionItem != null && actionItem.Action.PaintStyle == DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption)
            {
                DropDownSingleChoiceActionControlBase control = (DropDownSingleChoiceActionControlBase)actionItem.Control;
                control.Label.Text = actionItem.Action.Caption;
                control.Label.Style["padding-right"] = "5px";
                control.ComboBox.Width = 100;
            }
        }

        private void DateActionFrom_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            ParametrizedActionMenuActionItem actionItem = e.Control as ParametrizedActionMenuActionItem;

            if (actionItem != null)
            {
                ASPxDateEdit dateEdit = actionItem.Control.Editor as ASPxDateEdit;
                if (dateEdit != null)
                {
                    dateEdit.Width = 110;
                    dateEdit.Buttons.Clear();
                    if (dateEdit.Text != "")
                    {
                        Fromdate = Convert.ToDateTime(dateEdit.Text);
                    }
                }
            }
        }

        private void DateActionTo_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            ParametrizedActionMenuActionItem actionItem = e.Control as ParametrizedActionMenuActionItem;

            if (actionItem != null)
            {
                ASPxDateEdit dateEdit = actionItem.Control.Editor as ASPxDateEdit;
                if (dateEdit != null)
                {
                    dateEdit.Width = 110;
                    dateEdit.Buttons.Clear();
                    if (dateEdit.Text != "")
                    {
                        Todate = Convert.ToDateTime(dateEdit.Text);
                    }
                }
            }
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

        private void ViewOpenPickList_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquiryOpenPickList selectedObject = (vwInquiryOpenPickList)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewOpenPickList_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquiryOpenPickList selectedObject = (vwInquiryOpenPickList)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "PickList_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }

        private void ViewPickListDetailInquiry_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquiryPickListDetails selectedObject = (vwInquiryPickListDetails)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewPickListDetailInquiry_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquiryPickListDetails selectedObject = (vwInquiryPickListDetails)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "PickList_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }

        private void ViewPickListInquiry_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquiryPickList selectedObject = (vwInquiryPickList)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewPickListInquiry_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquiryPickList selectedObject = (vwInquiryPickList)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "PickList_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }

        // Start ver 1.0.9
        private void InquiryStatus_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            // Start ver 1.0.11
            if (InquiryStatus.SelectedItem.Id != "All")
            {
            // End ver 1.0.11
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                    "and DocDate >= ? and DocDate <= ?",
                    InquiryStatus.SelectedItem.Id, Fromdate, Todate);
            // Start ver 1.0.11
            }
            else
            {
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                    Fromdate, Todate);
            }
            // End ver 1.0.11
        }

        private void InquiryDateFrom_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                "and DocDate >= ? and DocDate <= ?",
                InquiryStatus.SelectedItem.Id, Fromdate, Todate);
        }

        private void InquiryDateTo_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                "and DocDate >= ? and DocDate <= ?",
                InquiryStatus.SelectedItem.Id, Fromdate, Todate);
        }

        private void InquiryFilter_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Start ver 1.0.11
            if (InquiryStatus.SelectedItem.Id != "All")
            {
            // End ver 1.0.11
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                "and DocDate >= ? and DocDate <= ?",
                InquiryStatus.SelectedItem.Id, Fromdate, Todate);
            // Start ver 1.0.11
            }
            else
            {
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                    Fromdate, Todate);
            }
            // End ver 1.0.11
        }
        // End ver 1.0.9

        // Start ver 1.0.11
        private void ViewSalesOrderInquiry_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquirySalesOrder selectedObject = (vwInquirySalesOrder)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            SalesOrder trx = os.FindObject<SalesOrder>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewSalesOrderInquiry_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquirySalesOrder selectedObject = (vwInquirySalesOrder)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            SalesOrder trx = os.FindObject<SalesOrder>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "SalesOrder_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }
        // End ver 1.0.11
    }
}
