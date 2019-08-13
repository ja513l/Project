using _3ABlindsInventorySystem.HelperClasses;
using MaterialSkin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3ABlindsInventorySystem
{
    public partial class SalesReportViewer : MaterialSkin.Controls.MaterialForm
    {
        public SalesReportViewer()
        {
            InitializeComponent();
        }

        private void SalesReportViewer_Load(object sender, EventArgs e)
        {
            try
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;

                MaterialSkinManager skinManager;
                skinManager = MaterialSkinManager.Instance;
                skinManager.AddFormToManage(this);
                skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
                SkinManager.ColorScheme = new ColorScheme(Primary.Amber600, Primary.Amber900, Primary.Amber900, Accent.Amber200, TextShade.WHITE);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

 
        }

        private void btnLoadReport_Click(object sender, EventArgs e)
        {
            try
            {
                using (InventoryDataContext context = new InventoryDataContext())
                {
                    GetSalesReportByDateResultBindingSource.DataSource = context.GetSalesReportByDate(dateFrom.Value, dateTo.Value);

                    GetSalesSumReportByDateResult obj = new GetSalesSumReportByDateResult();
                    obj = context.GetSalesSumReportByDate(dateFrom.Value, dateTo.Value).Single();

                    Microsoft.Reporting.WinForms.ReportParameter[] rParams = new Microsoft.Reporting.WinForms.ReportParameter[]
                       {
                        new Microsoft.Reporting.WinForms.ReportParameter("retailSale", obj.RetaiSale.ToString()),
                        new Microsoft.Reporting.WinForms.ReportParameter("wholeSale", obj.WholeSale.ToString()),
                        new Microsoft.Reporting.WinForms.ReportParameter("profit", obj.Profit.ToString())

                       };
                    rptSalesReport.LocalReport.SetParameters(rParams);
                    rptSalesReport.RefreshReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
