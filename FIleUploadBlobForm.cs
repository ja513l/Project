using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;
using _3ABlindsInventorySystem.Objects;

namespace _3ABlindsInventorySystem
{
    public partial class FIleUploadBlobForm : MetroFramework.Forms.MetroForm
    {
        public FIleUploadBlobForm(int _orderId)
        {
            InitializeComponent();
            OrderId = _orderId;
        }

        private int OrderId
        {
            get;set;
        }

      


        public string ImageLocation { get; set; }
        public string FileName { get; set; }


        public CommonDialog InvokeDialog;
        private Thread InvokeThread;
        private DialogResult InvokeResult;

        public FIleUploadBlobForm(CommonDialog dialog)
        {
            InvokeDialog = dialog;
            InvokeThread = new Thread(new ThreadStart(InvokeMethod));
            InvokeThread.SetApartmentState(ApartmentState.STA);
            InvokeResult = DialogResult.None;

        }
        public DialogResult Invoke()
        {
            InvokeThread.Start();
            InvokeThread.Join();
            return InvokeResult;
        }
        private void InvokeMethod()
        {
            InvokeResult = InvokeDialog.ShowDialog();
        }



        private async void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (!System.String.IsNullOrEmpty(lblFileName.Text))
                {
                    string CS = ConfigurationManager.ConnectionStrings["AzureConnection"].ConnectionString;
                    // container name should be lower case
                    string box = "order" + OrderId.ToString();



                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CS);
                    CloudBlobClient client = storageAccount.CreateCloudBlobClient();

                    //CloudBlobContainer container = client.GetContainerReference("blinds"); //table+primarykey
                    CloudBlobContainer container = client.GetContainerReference(box); //table+primarykey

                    //container.CreateIfNotExists();
                    bool x = await container.CreateIfNotExistsAsync();
                    //await container.CreateIfNotExistsAsync();

                    CloudBlockBlob blob = container.GetBlockBlobReference(FileName);
                    using (System.IO.FileStream stream = new System.IO.FileStream(ImageLocation, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        // blob.UploadFromStream(stream);
                        await blob.UploadFromStreamAsync(stream);
                    };

                    List<BlobObject> BlobList = new List<BlobObject>();

                    foreach (CloudBlob blobItem in container.ListBlobs())
                    // foreach (CloudBlob blobItem in container.ListBlobs())
                    {
                        BlobObject _object = new BlobObject();
                        //_object.BlobContainerName = blobItem.Container.Name;
                        // _object.StorageUri = blobItem.StorageUri.PrimaryUri.ToString();
                        //_object.PrimaryUri = blobItem.StorageUri.PrimaryUri.ToString();
                        string _name = blobItem.Uri.AbsoluteUri.Substring(blobItem.Uri.AbsoluteUri.LastIndexOf("/") + 1);
                        _object.ActualFileName = _name.Replace("%20", " ");
                        //_object.FileExtension = System.IO.Path.GetExtension(blobItem.Uri.AbsoluteUri.Substring(blobItem.Uri.AbsoluteUri.LastIndexOf("/") + 1));
                        BlobList.Add(_object);
                    }

                    grdViewBlobList.DataSource = BlobList;
                    lblFileName.Text = string.Empty;
                  
                        //DataGridViewLinkColumn linkColumnDelete = new DataGridViewLinkColumn();
                        //linkColumnDelete.Name = "Delete";
                        //linkColumnDelete.HeaderText = "Delete";
                        //linkColumnDelete.Text = "Delete";
                        //linkColumnDelete.UseColumnTextForLinkValue = true;
                        //grdViewBlobList.Columns.Insert(1, linkColumnDelete);

                        //DataGridViewLinkColumn linkColumnDownload = new DataGridViewLinkColumn();
                        //linkColumnDownload.Name = "Download";
                        //linkColumnDownload.HeaderText = "Download";
                        //linkColumnDownload.Text = "Download";
                        //linkColumnDownload.UseColumnTextForLinkValue = true;
                        //grdViewBlobList.Columns.Insert(2, linkColumnDownload);









                    //List<string> blobs = new List<string>();
                    //foreach (CloudBlockBlob blobItem in container.ListBlobs())
                    //{
                    //    blobs.Add(blobItem.Container.Name);
                    //}

                    //grdViewBlobList.DataSource = blobs.ToList();

                }

            }
            catch(Exception ex)
                {
                MessageBox.Show(ex.Message);
            }
        
        }

        private void ViewBlobList()
        {
            try
            {

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void lnkAttachFIle_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "png files(*.png)|*.png|jpg files(*.jpg)|*.jpg|All files(*.*)|*.*";
            dialog.Filter = "pdf files(*.pdf)|*.pdf|docx files(*.docx)|*.docx|xlsx files(*.xlsx)|*.xlsx|All files(*.*)|*.*";



            //if (DialogResult.OK == (new FileUploadForm(dialog).Invoke()))
            if (dialog.ShowDialog() == DialogResult.OK)
            {


                ImageLocation = dialog.FileName.ToString();
                FileName = System.IO.Path.GetFileName(dialog.FileName);

                string extension = System.IO.Path.GetExtension(FileName);

                System.IO.FileInfo file = new System.IO.FileInfo(dialog.FileName);
                int fileSize = (int)file.Length;

                lblFileName.Text = System.IO.Path.GetFileName(dialog.FileName);

            }
        }

        private  void grdViewBlobList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {


                    string CS = ConfigurationManager.ConnectionStrings["AzureConnection"].ConnectionString;

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CS);
                    CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                    string box = "order" + OrderId.ToString();
                    CloudBlobContainer container = client.GetContainerReference(box);
                 

                    DataGridViewRow row = grdViewBlobList.Rows[e.RowIndex];

                    if (grdViewBlobList.Columns[e.ColumnIndex].Name == "Download")
                    {
                        //FileId = System.Convert.ToInt32(row.Cells["FileId"].Value.ToString());
                        // Download(FileId);
                        using (SaveFileDialog saveFile = new SaveFileDialog())
                        {
                            saveFile.FileName = row.Cells["ActualFileName"].Value.ToString();
                            CloudBlockBlob blob = container.GetBlockBlobReference(row.Cells["ActualFileName"].Value.ToString());


                            string extension = System.IO.Path.GetExtension(row.Cells["ActualFileName"].Value.ToString());
                            string subString = extension.Substring(1);

                            string filter = "var files(*.var)|*.var";
                            string replace = filter.Replace("var", subString);

                          
                            saveFile.Filter = replace;

                            if (DialogResult.OK != saveFile.ShowDialog())
                                return;
                            using (FileStream fs = (FileStream)saveFile.OpenFile())
                            {
                                blob.DownloadToStream(fs);
                            }
                            //System.IO.FileStream stream = new System.IO.FileStream(row.Cells["StorageUri"].Value.ToString(), System.IO.FileMode.Open, System.IO.FileAccess.Read);
                           

                             //blob.DownloadToFileAsync(System.IO.Path.GetDirectoryName(saveFile.FileName), FileMode.Create);
                        }

                    }
                    else if (grdViewBlobList.Columns[e.ColumnIndex].Name == "Delete")
                    {
                        if (MessageBox.Show("Are you sure do you want to delete this file?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            string name = row.Cells["ActualFileName"].Value.ToString();
                            string fileName = name.Replace("%20", " ");
                            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
                            blockBlob.DeleteIfExists();

                            List<BlobObject> BlobList = new List<BlobObject>();


                            foreach (CloudBlob blobItem in container.ListBlobs())
                            {
                                BlobObject _object = new BlobObject();

                                string _name = blobItem.Uri.AbsoluteUri.Substring(blobItem.Uri.AbsoluteUri.LastIndexOf("/") + 1);

                                _object.ActualFileName = _name.Replace("%20", " ");

                                //_object.ActualFileName = blobItem.Uri.AbsoluteUri.Substring(blobItem.Uri.AbsoluteUri.LastIndexOf("/") + 1); //%20

                                BlobList.Add(_object);


                              
                            }

                            grdViewBlobList.DataSource = BlobList;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
          
        }

        private void FIleUploadBlobForm_Load(object sender, EventArgs e)
        {
            try
            {
             
                string CS = ConfigurationManager.ConnectionStrings["AzureConnection"].ConnectionString;
                string box = "order" + OrderId.ToString();


                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CS);
                CloudBlobClient client = storageAccount.CreateCloudBlobClient();

                //CloudBlobContainer container = client.GetContainerReference("blinds"); //table+primarykey
                CloudBlobContainer container = client.GetContainerReference(box); //table+primarykey

                //if (container.Exists())
                //{
                    List<BlobObject> BlobList = new List<BlobObject>();
                if (container.Exists())
                {
                    foreach (CloudBlob blobItem in container.ListBlobs())
                    // foreach (CloudBlob blobItem in container.ListBlobs())
                    {
                        BlobObject _object = new BlobObject();
                        //_object.BlobContainerName = blobItem.Container.Name;
                        // _object.StorageUri = blobItem.StorageUri.PrimaryUri.ToString();
                        //_object.PrimaryUri = blobItem.StorageUri.PrimaryUri.ToString();
                        string _name = blobItem.Uri.AbsoluteUri.Substring(blobItem.Uri.AbsoluteUri.LastIndexOf("/") + 1);
                        _object.ActualFileName = _name.Replace("%20", " ");
                        //_object.FileExtension = System.IO.Path.GetExtension(blobItem.Uri.AbsoluteUri.Substring(blobItem.Uri.AbsoluteUri.LastIndexOf("/") + 1));
                        BlobList.Add(_object);
                    }
                }
                    //if (BlobList.Count > 0)
                    //{
                    //    Marker += 1;
                    //}

                    grdViewBlobList.DataSource = BlobList;

                
                      
                        DataGridViewLinkColumn linkColumnDelete = new DataGridViewLinkColumn();
                        linkColumnDelete.Name = "Delete";
                        linkColumnDelete.HeaderText = "Delete";
                        linkColumnDelete.Text = "Delete";
                        linkColumnDelete.UseColumnTextForLinkValue = true;
                        grdViewBlobList.Columns.Insert(1, linkColumnDelete);

                        DataGridViewLinkColumn linkColumnDownload = new DataGridViewLinkColumn();
                        linkColumnDownload.Name = "Download";
                        linkColumnDownload.HeaderText = "Download";
                        linkColumnDownload.Text = "Download";
                        linkColumnDownload.UseColumnTextForLinkValue = true;
                        grdViewBlobList.Columns.Insert(2, linkColumnDownload);

                //}



            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Dispose();
                this.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
    }
}

