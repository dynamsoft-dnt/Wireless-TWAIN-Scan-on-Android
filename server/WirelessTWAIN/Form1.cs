using Dynamsoft.DotNet.TWAIN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WirelessTWAIN
{
    public partial class WirelessTWAIN : Form
    {
        private DynamicDotNetTwain dynamicDotNetTwain;
        private ServerManager serverManager;
        private const int DATA_WIDTH = 480, DATA_HEIGHT = 640;

        public WirelessTWAIN()
        {
            InitializeComponent();
            initTWAINComponent();
            initUIComponent();
            initServer();
        }

        private void initServer()
        {
            serverManager = new ServerManager(this);
            Thread newThread = new Thread(new ThreadStart(serverManager.run));
            newThread.Start();
        }

        private void initUIComponent()
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        public void scanImage()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                AcquireImage();
            });
        }

        private void initTWAINComponent()
        {
            dynamicDotNetTwain = new Dynamsoft.DotNet.TWAIN.DynamicDotNetTwain();
            dynamicDotNetTwain.IfShowUI = false;
            dynamicDotNetTwain.IfThrowException = true;
            dynamicDotNetTwain.MaxImagesInBuffer = 1;
            dynamicDotNetTwain.IfAppendImage = false;
            dynamicDotNetTwain.IfDisableSourceAfterAcquire = true;

            int iNum;
            dynamicDotNetTwain.OpenSourceManager();
            for (iNum = 0; iNum < dynamicDotNetTwain.SourceCount; iNum++)
            {
                comboBox1.Items.Add(dynamicDotNetTwain.SourceNameItems(Convert.ToInt16(iNum)));
            }
            if (iNum > 0)
                comboBox1.SelectedIndex = 0;

            dynamicDotNetTwain.OnPostAllTransfers += dynamicDotNetTwain_OnPostAllTransfers;
        }

        private void dynamicDotNetTwain_OnPostAllTransfers()
        {
            Image image = dynamicDotNetTwain.GetImage(dynamicDotNetTwain.CurrentImageIndexInBuffer);
            if (this.pictureBox1.Image != null)
                this.pictureBox1.Image.Dispose();
            this.pictureBox1.Image = image;
            sendImage(image);
            dynamicDotNetTwain.CloseSource();
        }

        private void sendImage(Image image)
        {
            Byte[] data = convertImage(image);
            serverManager.prepareData(data);
            serverManager.sendData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                //dlg.Filter = "bmp files (*.bmp)|*.bmp";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Image image = Image.FromFile(dlg.FileName);
                    this.pictureBox1.Image = image;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AcquireImage();
        }

        private void AcquireImage()
        {
            try
            {
                dynamicDotNetTwain.SelectSourceByIndex(Convert.ToInt16(comboBox1.SelectedIndex));
                dynamicDotNetTwain.OpenSource();
                dynamicDotNetTwain.AcquireImage();
            }
            catch (Dynamsoft.DotNet.TWAIN.TwainException exp)
            {
                String errorstr = "";
                errorstr += "Error " + exp.Code + "\r\n" + "Description: " + exp.Message + "\r\nPosition: " + exp.TargetSite + "\r\nHelp: " + exp.HelpLink + "\r\n";
                MessageBox.Show(errorstr);
            }
        }

        private static Byte[] convertImage(Image image)
        {
            if (image.Width > DATA_WIDTH || image.Height > DATA_HEIGHT)
            {
                image = resizeImage(image, new Size(DATA_WIDTH, DATA_HEIGHT));
            }

            ImageConverter imageConverter = new ImageConverter();
            Byte[] byteArray = (Byte[])imageConverter.ConvertTo(image, typeof(Byte[])); // convert image to byte
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

            image.Dispose();
            stream.Close();

            return byteArray;
        }

        private static Image resizeImage(Image img, Size size)
        {
            return (Image)(new Bitmap(img, size));
        }
    }
}
