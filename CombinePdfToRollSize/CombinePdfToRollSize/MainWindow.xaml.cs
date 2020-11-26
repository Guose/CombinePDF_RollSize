using System;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Threading;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CombinePdfToRollSize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private string _path = string.Empty;
        private FileInfo[] _fi = null;
        private int _fileNum = 0;
        private string _countyId = string.Empty;
        private string _ilk = string.Empty;
        const int _rollSize = 37000;
        public delegate void delUpdateUIListbox(int i);
        private Thread workerThread;

        private void UpdatePdfImageListbox(int index)
        {
            Dispatcher.BeginInvoke((Action)(() => lstFiles.SelectedIndex = index)
                , System.Windows.Threading.DispatcherPriority.Input);
        }

        private void DoWorkOnImages()
        {
            int pages = 0;
            int index = 0;
            string targetPath = $"{_path}\\MULTIBOX FILES";

            Directory.CreateDirectory(targetPath);

            delUpdateUIListbox delImageSelection = new delUpdateUIListbox(UpdatePdfImageListbox);

            //_rollSize = Convert.ToInt32(txtRollSize.Text);

            var targetPdf = new PdfDocument();

            foreach (var item in _fi)
            {
                PdfDocument pdfDoc = new PdfDocument();

                using (pdfDoc = PdfReader.Open(item.FullName, PdfDocumentOpenMode.Import))
                {
                    pages += pdfDoc.PageCount / 2;

                    if (pages <= _rollSize)
                    {
                        for (int i = 0; i < pdfDoc.PageCount; i++)
                        {
                            targetPdf.AddPage(pdfDoc.Pages[i]);
                        }

                    }
                    else
                    {
                        targetPdf.Save($"{targetPath}\\{_countyId}_{_ilk} MULTIBOX FILE-{_fileNum}.pdf");
                        _fileNum++;
                        targetPdf = new PdfDocument();

                        for (int j = 0; j < pdfDoc.PageCount; j++)
                        {
                            targetPdf.AddPage(pdfDoc.Pages[j]);
                        }

                        pages = pdfDoc.PageCount / 2;

                    }

                    delImageSelection(index);
                    index++;

                    Dispose(pdfDoc);
                }
            }
            targetPdf.Save($"{targetPath}\\{_countyId}_{_ilk} MULTIBOX FILE-{_fileNum}.pdf");
            Dispose(targetPdf);

            System.Windows.MessageBox.Show("Multi-Box Creation Done!", "COMPLETED!", MessageBoxButton.OK);
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.Items != null)
            {
                lstFiles.Items.Clear();
            }

            FolderBrowserDialog fld = new FolderBrowserDialog();
            DialogResult result = fld.ShowDialog();
            string originFile = string.Empty;
            string destFile = string.Empty;

            if (result.ToString() == "OK")
            {
                _path = fld.SelectedPath;
                txtPath.Text = _path;

                DirectoryInfo d = new DirectoryInfo(_path);
                btnMerge.IsEnabled = true;

                _fi = d.GetFiles("*.pdf", SearchOption.AllDirectories);

                foreach (var item in _fi)
                {
                    item.IsReadOnly = false;
                    lstFiles.Items.Add(item);
                }
            }
        }

        private void BtnMerge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ilk = txtIlk.Text;
                _fileNum = Convert.ToInt32(txtFileNumber.Text);
                _countyId = txtCountyId.Text;

                Cursor = System.Windows.Input.Cursors.AppStarting;

                workerThread = new Thread(DoWorkOnImages);
                workerThread.IsBackground = true;
                workerThread.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "ERROR COMBINING FILES", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lstFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstFiles.ScrollIntoView(lstFiles.SelectedItem);

            if (lstFiles.SelectedIndex == lstFiles.Items.Count - 1)
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing, PdfDocument pdf)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    pdf.Close();
                    pdf = new PdfDocument();
                }

                disposedValue = true;
            }
        }

        public void Dispose(PdfDocument pdf)
        {
            Dispose(true, pdf);

            GC.SuppressFinalize(this);
        }

    }
}
