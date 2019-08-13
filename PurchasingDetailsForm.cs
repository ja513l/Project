using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using _3ABlindsInventorySystem.Manager;
using _3ABlindsInventorySystem.HelperClasses;
using System.Configuration;

namespace _3ABlindsInventorySystem
{
    public partial class PurchasingDetailsForm : MetroFramework.Forms.MetroForm
    {
        public PurchasingDetailsForm(int _purchaseOrderId, bool isAddMode)
        {
            InitializeComponent();
            PurchaseOrderId = _purchaseOrderId;
            IsAddMode = IsAddMode;
          
        }

        private bool IsAddMode
        {
            get;set;
        }

        private Boolean AreAllItemsReceived
        {
            get;set;

        }
        private Boolean IsUpdatedToDB
        {
            get;set;
        }
        public int PurchaseOrderId
        {
            get;set;
        }

        private int PurchaseOrderStatusId
        {
            get;set;
        }

        private int CustomerId
        {
            get;set;
        }

     

        public int SupplierId
        {
            get;set;
        }

        private void LoadProductList(int purchaseOrderId)
        {
            using (InventoryDataContext context = new InventoryDataContext())
            {
                PurchaseManager manager = new PurchaseManager();
               
                grdMetroProduct.DataSource =  manager.GetPurchaseProductList(context, purchaseOrderId);
                grdMetroProduct.Columns[3].HeaderText = "Status";

                VisibleControls();

            }
        }

        private void AddDynamicLinkOnGrid(MetroFramework.Controls.MetroGrid Grid, int viewEditIndex, int deleteIndex)
        {
            if (!IsUpdatedToDB)
            {
                DataGridViewLinkColumn linkColumnViewEdit = new DataGridViewLinkColumn();
                linkColumnViewEdit.Name = "View/Edit";
                linkColumnViewEdit.HeaderText = "View/Edit";
                linkColumnViewEdit.Text = "View/Edit";
                linkColumnViewEdit.UseColumnTextForLinkValue = true;
                Grid.Columns.Insert(viewEditIndex, linkColumnViewEdit);

                if (PurchaseOrderStatusId == (int)Constants.PurchaseOrderStatus.Pending)
                {
                    DataGridViewLinkColumn linkColumnDelete = new DataGridViewLinkColumn();
                    linkColumnDelete.Name = "Delete";
                    linkColumnDelete.HeaderText = "Delete";
                    linkColumnDelete.Text = "Delete";
                    linkColumnDelete.UseColumnTextForLinkValue = true;
                    Grid.Columns.Insert(deleteIndex, linkColumnDelete);

                }
                else
                {
                    AddChangeStatusLinkOnGrid();
                }
            }
        }

        private void AddChangeStatusLinkOnGrid()
        {
            DataGridViewLinkColumn linkColumnChangeStatus = new DataGridViewLinkColumn();
            linkColumnChangeStatus.Name = "Change Status";
            linkColumnChangeStatus.HeaderText = "Change Status";
            linkColumnChangeStatus.Text = "Change Status";
            linkColumnChangeStatus.UseColumnTextForLinkValue = true;
            grdMetroProduct.Columns.Insert(13, linkColumnChangeStatus);
        }



        private void EnableHideColumnGrid()
        {

            grdMetroProduct.Columns[Constants.PurchaseProduct.PurchaseOrderId.ToString()].Visible = false;
            grdMetroProduct.Columns[Constants.PurchaseProduct.PurchaseProductId.ToString()].Visible = false;
            grdMetroProduct.Columns[Constants.PurchaseProduct.ProductId.ToString()].Visible = false;
            grdMetroProduct.Columns[Constants.PurchaseProduct.PurchaseProductStatusId.ToString()].Visible = false;

        }

        private void VisibleControls()
        {
            btnPrintExport.Visible = (grdMetroProduct.RowCount > 0 ? true : false);
            btnSubmit.Visible = (PurchaseOrderStatusId == (int)Constants.PurchaseOrderStatus.Pending && grdMetroProduct.RowCount > 0) ? true : false;
            btnUpdate.Visible = (PurchaseOrderStatusId == (int)Constants.PurchaseOrderStatus.Received && grdMetroProduct.RowCount > 0 && !IsUpdatedToDB && AreAllItemsReceived) ? true : false;
            btnAddProduct.Visible = (!IsUpdatedToDB) ? true : false; 
            lnlkLabelCustomerName.Visible = (IsUpdatedToDB) ? false : true;
            lnkLblClear.Visible = (IsUpdatedToDB) ? false : true;
        }

        private void PurchasingDetailsForm_Load(object sender, EventArgs e)
        {
            try
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                TopMost = false;

                using (InventoryDataContext context = new InventoryDataContext())
                {
                    GetPurchaseOrderByIdResult obj = new GetPurchaseOrderByIdResult();
                    PurchaseManager manager = new PurchaseManager();
                    obj = manager.GetPurchaseOrderById(context, PurchaseOrderId).Single();

                    txtBoxMetroSupplierName.Text = obj.SupplierName;
                    txtBoxMetroStatus.Text = obj.PurchaseOrderStatus;
                    txtBoxMetroOrderNumber.Text = obj.PurchaseOrderId.ToString();

                    PurchaseOrderId = obj.PurchaseOrderId;
                    PurchaseOrderStatusId = obj.PurchaseOrderStatusId;
                    SupplierId = obj.SupplierId;
                    IsUpdatedToDB = obj.IsUpdatedToDB;
                    
                    if (obj.CustomerId != null)
                    {
                        CustomerId = (int)obj.CustomerId;
                        txtBoxMetroCustomerName.Text = obj.LastName + ", " + obj.FirstName;
                    }
                    if (obj.OrderDate != null)
                    {
                        dateTimeMetroOrderDate.Value = System.Convert.ToDateTime(obj.OrderDate);
                    }
                    txtBoxMetroTotalAmount.Text = obj.TotalAmount.ToString();

                    if (!IsAddMode)
                    {
                        AreAllItemsReceived = System.Convert.ToBoolean(manager.CheckPurchaseProductIfReceived(context, PurchaseOrderId));
                    }
                }

                if (!IsAddMode)
                {
                    LoadProductList(PurchaseOrderId);
                    AddDynamicLinkOnGrid(grdMetroProduct, 13, 14);
                    EnableHideColumnGrid();
                }
                else
                {
                    VisibleControls();
                }
             
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void lnlkLabelCustomerName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                CustomerForm form = new CustomerForm();
                form.IsFromPurchaseForm = true;
                form.ShowDialog();
                if (form.PurchaseForCustomerId > 0)
                {
                    using (InventoryDataContext context = new InventoryDataContext())
                    {
                        CustomerId = form.PurchaseForCustomerId;
                        CustomerManager manager = new CustomerManager();
                        GetCustomerByIdResult obj = new GetCustomerByIdResult();
                        obj = manager.GetCustomerById(context, CustomerId).Single();
                        txtBoxMetroCustomerName.Text = obj.LastName + ", " + obj.FirstName;
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnSaveOrderDetails_Click(object sender, EventArgs e)
        {
            try
            {
                using (InventoryDataContext context = new InventoryDataContext())
                {
                    int? _customerId = null;
                    if (CustomerId > 0)
                    {
                        _customerId = CustomerId;
                    }
                    PurchaseManager manager = new PurchaseManager();
                    manager.UpdatePurchaseOrder(context, PurchaseOrderId, _customerId, dateTimeMetroOrderDate.Value, System.Convert.ToDecimal(txtBoxMetroTotalAmount.Text));
                    MessageBox.Show("Save Successfully");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

   

        private void lnkLblClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                CustomerId = 0;
                txtBoxMetroCustomerName.Text = String.Empty;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void txtBoxMetroTotalAmount_Leave(object sender, EventArgs e)
        {
            try
            {
                Function obj = new Function();
                if (obj.IsDecimal(txtBoxMetroTotalAmount.Text))
                {
                    txtBoxMetroTotalAmount.Text = String.Format("{0:0.00}", System.Convert.ToDecimal(txtBoxMetroTotalAmount.Text));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
        }

        private void txtBoxMetroTotalAmount_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Function obj = new Function();
                if (System.String.IsNullOrEmpty(txtBoxMetroTotalAmount.Text))
                {
                    e.Cancel = true;
                    errProPurchaseOrder.SetError(txtBoxMetroTotalAmount, ConfigurationManager.AppSettings.Get(Constants.Validation.TotalAmountRequired.ToString()));
                }
                else if (!obj.IsDecimal(txtBoxMetroTotalAmount.Text))
                {
                    e.Cancel = true;
                    errProPurchaseOrder.SetError(txtBoxMetroTotalAmount, ConfigurationManager.AppSettings.Get(Constants.Validation.IsDecimal.ToString()));
                }
                else
                {
                    e.Cancel = false;
                    errProPurchaseOrder.SetError(txtBoxMetroTotalAmount, null);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void tileMetroHome_Click(object sender, EventArgs e)
        {
            try
            {
                Main home = new Main();
                home.Show();
                this.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            try
            {
                AddPurchaseProductForm form = new AddPurchaseProductForm(SupplierId, PurchaseOrderId, null, true);
                form.ShowDialog();
                if (form.PurchaseOrderId > 0)
                {
                    PurchaseOrderId = form.PurchaseOrderId;
                    LoadProductList(PurchaseOrderId);
                    VisibleControls();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void grdMetroProduct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                
                if (grdMetroProduct.RowCount > 0)
                {
                    
                    if (e.RowIndex >= 0)
                    {
                        int Id;
                        DataGridViewRow row = grdMetroProduct.Rows[e.RowIndex];

                        if (grdMetroProduct.Columns[e.ColumnIndex].Name == "View/Edit")
                        {
                            Id = System.Convert.ToInt32(row.Cells["PurchaseProductId"].Value.ToString());
                            AddPurchaseProductForm form = new AddPurchaseProductForm(SupplierId, PurchaseOrderId, Id, false);
                            form.ShowDialog();
                            if (!form.IsCancelUpdate)
                            {
                                LoadProductList(PurchaseOrderId);

                            }
                        }
                        else if (grdMetroProduct.Columns[e.ColumnIndex].Name == "Delete")
                        {
                            if (MessageBox.Show("Are you sure do you want to delete this product?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                Id = System.Convert.ToInt32(row.Cells["PurchaseProductId"].Value.ToString());
                                using (InventoryDataContext context = new InventoryDataContext())
                                {
                                    PurchaseManager manager = new PurchaseManager();
                                    manager.DeletePurchaseProductById(context, Id);
                                }
                                LoadProductList(PurchaseOrderId);
                            }
                        }
                        else if (grdMetroProduct.Columns[e.ColumnIndex].Name == "Change Status")
                        {


                            int pOrderId = System.Convert.ToInt32(row.Cells["PurchaseOrderId"].Value.ToString());
                            int pId = System.Convert.ToInt32(row.Cells["PurchaseProductId"].Value.ToString());
                            string status = row.Cells["PurchaseProductStatus"].Value.ToString();
                            string remarks = row.Cells["Remarks"].Value.ToString();
                            PurchaseProductStatusForm form = new PurchaseProductStatusForm(pOrderId, pId, status, remarks);
                            form.ShowDialog();
                            if (!form.IsCancelUpdate)
                            {
                                using (InventoryDataContext context = new InventoryDataContext())
                                {
                                    PurchaseManager manager = new PurchaseManager();
                                    GetPurchaseOrderByIdResult obj = new GetPurchaseOrderByIdResult();
                                    obj = manager.GetPurchaseOrderById(context, PurchaseOrderId).Single();
                                    PurchaseOrderStatusId = obj.PurchaseOrderStatusId;
                                    txtBoxMetroStatus.Text = obj.PurchaseOrderStatus.ToString();
                                    AreAllItemsReceived = form.AreAllItemsReceived;
                                    LoadProductList(PurchaseOrderId);
                                }
                            }
                        }

                    }
                   
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        //private void btnSubmitOrder_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        using (InventoryDataContext context = new InventoryDataContext())
        //        {
        //            if (MessageBox.Show("Submit Order?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        //            {
        //                PurchaseManager manager = new PurchaseManager();
        //                manager.SubmitPurchaseOrder(context, PurchaseOrderId);
        //                PurchaseOrderStatusId = (int)Constants.PurchaseOrderStatus.Submitted;
        //                txtBoxMetroStatus.Text = Constants.PurchaseOrderStatus.Submitted.ToString();
        //                grdMetroProduct.Columns.Remove("Delete");
        //                AddChangeStatusLinkOnGrid();
        //                LoadProductList(PurchaseOrderId);
        //                VisibleControls();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        MessageBox.Show(ex.Message);
        //    }
        //}

        private void btnPrintExport_Click(object sender, EventArgs e)
        {
            try
            {
                PurchaseOrderViewerForm form = new PurchaseOrderViewerForm(PurchaseOrderId);
                form.ShowDialog();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdateToDataBase_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Update to DataBase?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (InventoryDataContext context = new InventoryDataContext())
                    {
                        
                        PurchaseManager manager = new PurchaseManager();
                        IsUpdatedToDB = System.Convert.ToBoolean(manager.UpdatePurchaseProductToDB(context, PurchaseOrderId, Global.UserIdToken));
                        if (IsUpdatedToDB)
                        {
                            grdMetroProduct.Columns.Remove("View/Edit");
                            grdMetroProduct.Columns.Remove("Change Status");
                            VisibleControls();
                            MessageBox.Show("Update to Inventory was successful..");

                        }
                    }
                }     
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void tileMetroPurchaseList_Click(object sender, EventArgs e)
        {
            try
            {
                PurchasingForm form = new PurchasingForm();
                form.Show();
                this.Dispose();
                this.Close();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                using (InventoryDataContext context = new InventoryDataContext())
                {
                    if (MessageBox.Show("Submit Order?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        PurchaseManager manager = new PurchaseManager();
                        manager.SubmitPurchaseOrder(context, PurchaseOrderId);
                        PurchaseOrderStatusId = (int)Constants.PurchaseOrderStatus.Submitted;
                        txtBoxMetroStatus.Text = Constants.PurchaseOrderStatus.Submitted.ToString();
                        grdMetroProduct.Columns.Remove("Delete");
                        grdMetroProduct.Columns.Remove("View/Edit");
                        AddChangeStatusLinkOnGrid();
                        LoadProductList(PurchaseOrderId);
                        VisibleControls();
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
    }
}
