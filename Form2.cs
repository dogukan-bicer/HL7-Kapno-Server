using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using ZedGraph;
using FontAwesome.Sharp;

namespace HL7_Kapno_Server
{

    public partial class Form2 : Form
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"HL7_Kapno_Server");//dosya konumuna kaydeder C:\Users\doguk\AppData\Roaming\HL7_Kapno_Server
        string dosya_konumu = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"HL7_Kapno_Server"), @"kayit.txt");

        //string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        public void yaz()
        {
            StreamWriter sw = new StreamWriter(dosya_konumu);
            sw.WriteLine(rjTextBox1.Texts + "\n" + rjTextBox2.Texts + "\n" + rjTextBox3.Texts + "\n" + rjTextBox4.Texts + "\n" + rjTextBox5.Texts + "\n");
            sw.Close();
        }

        public void oku()
        {
            StreamReader sr = new StreamReader(dosya_konumu);
            rjTextBox1.Texts = sr.ReadLine();
            rjTextBox2.Texts = sr.ReadLine();
            rjTextBox3.Texts = sr.ReadLine();
            rjTextBox4.Texts = sr.ReadLine();
            rjTextBox5.Texts = sr.ReadLine();
            sr.Close();
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
         int nLeftRect,     // x-coordinate of upper-left corner
         int nTopRect,      // y-coordinate of upper-left corner
         int nRightRect,    // x-coordinate of lower-right corner
         int nBottomRect,   // y-coordinate of lower-right corner
         int nWidthEllipse, // height of ellipse
         int nHeightEllipse // width of ellipse
        );

        public Form2()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 35, 35));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!File.Exists(dosya_konumu))//konumda dosya yoksa yeni oluşur
            {
                StreamWriter sw = new StreamWriter(dosya_konumu);
                sw.WriteLine("localhost" + "\n" + "hl7_server" + "\n" + "root" + "\n" + "123456789" + "\n" + "SELECT * FROM `hl7data`" + "\n");
                sw.Close();
            }
            oku();

        }

        static public string server_data;//ip adresi
        static public string database_data;//database adı
        static public string username_data;//kullanıcı adı
        static public string password_data;//şifre
        static public string bolum_data;//database deki bölüm string bolum = "SELECT * FROM `hl7data`";//database deki bölüm
        static public string test;

        //public string server = "localhost";
        //public string database = "hl7_server";
        //public string username = "root";
        //public string password = "123456789";
        //public string bolum = "SELECT * FROM `hl7data`";

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void rjButton1_Click(object sender, EventArgs e)//bağlan
        {
            server_data = rjTextBox1.Texts;
            database_data = rjTextBox2.Texts;
            username_data = rjTextBox3.Texts;
            password_data = rjTextBox4.Texts;
            bolum_data = rjTextBox5.Texts;
            yaz();
            this.Hide();
        }

        private void rjButton2_Click(object sender, EventArgs e)//kaydet
        {
            yaz();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void ıconButtonWindowClose_Click_1(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
