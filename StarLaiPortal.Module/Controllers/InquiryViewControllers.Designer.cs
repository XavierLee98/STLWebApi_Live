namespace StarLaiPortal.Module.Controllers
{
    partial class InquiryViewControllers
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ViewOpenPickList = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ViewPickListDetailInquiry = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ViewPickListInquiry = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.InquiryStatus = new DevExpress.ExpressApp.Actions.SingleChoiceAction(this.components);
            this.InquiryDateFrom = new DevExpress.ExpressApp.Actions.ParametrizedAction(this.components);
            this.InquiryDateTo = new DevExpress.ExpressApp.Actions.ParametrizedAction(this.components);
            this.InquiryFilter = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.ViewSalesOrderInquiry = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            // 
            // ViewOpenPickList
            // 
            this.ViewOpenPickList.AcceptButtonCaption = null;
            this.ViewOpenPickList.CancelButtonCaption = null;
            this.ViewOpenPickList.Caption = "View";
            this.ViewOpenPickList.Category = "ListView";
            this.ViewOpenPickList.ConfirmationMessage = null;
            this.ViewOpenPickList.Id = "ViewOpenPickList";
            this.ViewOpenPickList.ToolTip = null;
            this.ViewOpenPickList.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewOpenPickList_CustomizePopupWindowParams);
            this.ViewOpenPickList.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewOpenPickList_Execute);
            // 
            // ViewPickListDetailInquiry
            // 
            this.ViewPickListDetailInquiry.AcceptButtonCaption = null;
            this.ViewPickListDetailInquiry.CancelButtonCaption = null;
            this.ViewPickListDetailInquiry.Caption = "View";
            this.ViewPickListDetailInquiry.Category = "ListView";
            this.ViewPickListDetailInquiry.ConfirmationMessage = null;
            this.ViewPickListDetailInquiry.Id = "ViewPickListDetailInquiry";
            this.ViewPickListDetailInquiry.ToolTip = null;
            this.ViewPickListDetailInquiry.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewPickListDetailInquiry_CustomizePopupWindowParams);
            this.ViewPickListDetailInquiry.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewPickListDetailInquiry_Execute);
            // 
            // ViewPickListInquiry
            // 
            this.ViewPickListInquiry.AcceptButtonCaption = null;
            this.ViewPickListInquiry.CancelButtonCaption = null;
            this.ViewPickListInquiry.Caption = "View";
            this.ViewPickListInquiry.Category = "ListView";
            this.ViewPickListInquiry.ConfirmationMessage = null;
            this.ViewPickListInquiry.Id = "ViewPickListInquiry";
            this.ViewPickListInquiry.ToolTip = null;
            this.ViewPickListInquiry.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewPickListInquiry_CustomizePopupWindowParams);
            this.ViewPickListInquiry.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewPickListInquiry_Execute);
            // 
            // InquiryStatus
            // 
            this.InquiryStatus.Caption = "Status";
            this.InquiryStatus.Category = "ObjectsCreation";
            this.InquiryStatus.ConfirmationMessage = null;
            this.InquiryStatus.Id = "InquiryStatus";
            this.InquiryStatus.ToolTip = null;
            this.InquiryStatus.Execute += new DevExpress.ExpressApp.Actions.SingleChoiceActionExecuteEventHandler(this.InquiryStatus_Execute);
            // 
            // InquiryDateFrom
            // 
            this.InquiryDateFrom.Caption = "From";
            this.InquiryDateFrom.Category = "ObjectsCreation";
            this.InquiryDateFrom.ConfirmationMessage = null;
            this.InquiryDateFrom.Id = "InquiryDateFrom";
            this.InquiryDateFrom.NullValuePrompt = null;
            this.InquiryDateFrom.ShortCaption = null;
            this.InquiryDateFrom.ToolTip = null;
            this.InquiryDateFrom.ValueType = typeof(System.DateTime);
            this.InquiryDateFrom.Execute += new DevExpress.ExpressApp.Actions.ParametrizedActionExecuteEventHandler(this.InquiryDateFrom_Execute);
            // 
            // InquiryDateTo
            // 
            this.InquiryDateTo.Caption = "To";
            this.InquiryDateTo.Category = "ObjectsCreation";
            this.InquiryDateTo.ConfirmationMessage = null;
            this.InquiryDateTo.Id = "InquiryDateTo";
            this.InquiryDateTo.NullValuePrompt = null;
            this.InquiryDateTo.ShortCaption = null;
            this.InquiryDateTo.ToolTip = null;
            this.InquiryDateTo.ValueType = typeof(System.DateTime);
            this.InquiryDateTo.Execute += new DevExpress.ExpressApp.Actions.ParametrizedActionExecuteEventHandler(this.InquiryDateTo_Execute);
            // 
            // InquiryFilter
            // 
            this.InquiryFilter.Caption = "Filter";
            this.InquiryFilter.Category = "ObjectsCreation";
            this.InquiryFilter.ConfirmationMessage = null;
            this.InquiryFilter.Id = "InquiryFilter";
            this.InquiryFilter.ToolTip = null;
            this.InquiryFilter.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.InquiryFilter_Execute);
            // 
            // ViewSalesOrderInquiry
            // 
            this.ViewSalesOrderInquiry.AcceptButtonCaption = null;
            this.ViewSalesOrderInquiry.CancelButtonCaption = null;
            this.ViewSalesOrderInquiry.Caption = "View";
            this.ViewSalesOrderInquiry.Category = "ListView";
            this.ViewSalesOrderInquiry.ConfirmationMessage = null;
            this.ViewSalesOrderInquiry.Id = "ViewSalesOrderInquiry";
            this.ViewSalesOrderInquiry.ToolTip = null;
            this.ViewSalesOrderInquiry.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewSalesOrderInquiry_CustomizePopupWindowParams);
            this.ViewSalesOrderInquiry.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewSalesOrderInquiry_Execute);
            // 
            // InquiryViewControllers
            // 
            this.Actions.Add(this.ViewOpenPickList);
            this.Actions.Add(this.ViewPickListDetailInquiry);
            this.Actions.Add(this.ViewPickListInquiry);
            this.Actions.Add(this.InquiryStatus);
            this.Actions.Add(this.InquiryDateFrom);
            this.Actions.Add(this.InquiryDateTo);
            this.Actions.Add(this.InquiryFilter);
            this.Actions.Add(this.ViewSalesOrderInquiry);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewOpenPickList;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewPickListDetailInquiry;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewPickListInquiry;
        private DevExpress.ExpressApp.Actions.SingleChoiceAction InquiryStatus;
        private DevExpress.ExpressApp.Actions.ParametrizedAction InquiryDateFrom;
        private DevExpress.ExpressApp.Actions.ParametrizedAction InquiryDateTo;
        private DevExpress.ExpressApp.Actions.SimpleAction InquiryFilter;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewSalesOrderInquiry;
    }
}
