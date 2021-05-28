using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace puppet_windows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            update_qr("Prajwal");
        }

        private void btn_main_Click(object sender, EventArgs e)
        {
            update_qr(text_name.Text);
        }



        private void update_qr(String text)
        {
            QRCodeGenerator qr = new QRCodeGenerator();
            QRCodeData data = qr.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode code = new QRCode(data);
            picture_qr.Image = code.GetGraphic(5);
        }
    }
}
