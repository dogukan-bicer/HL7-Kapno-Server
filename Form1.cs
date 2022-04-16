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
    public partial class Form1 : Form
    {
        

        MySqlConnection Baglanti;
        MySqlCommand komut;
        MySqlDataReader okuyucu;

        GraphPane myPaneEtco2 = new GraphPane();
        PointPairList listPointsEtco2 = new PointPairList();
        LineItem myCurveEtco2;

        public Boolean baslat = false;
        public Boolean yenile = false;

        public string etco2;
        public string fico2;
        public string etco2rr;
        public string tarih;

        double zaman = 0;

     
        private int borderSize = 2;
        private Color borderColor = Color.FromArgb(48, 48, 48);

        public struct hl7_baslik
        {
            public string MSH;
            public string PID;
            public string PV1;
            public string OBR;
            public string baslik;
            public string etco2_data;
            public string fico2_data;
            public string etco2rr_data;
        }
        hl7_baslik veri = new hl7_baslik();

        public void baslıkYaz()
        {
            veri.MSH = "MSH|^~/&|PSN|PSN|||20111021175130.7370-0700||ORU^R01|92176046-e469-4d34-9e56-711ab3adeb04|P|2.4|||NE|NE$";
            veri.PID = "PID|||27^3^M10||BİCER^DOGUKAN^MIDDLE^^^^||19990307|M|||^^^^^^P~^^^^^^VACAE~^^^^^^VACAA~^^^^^^VACAC~^^^^^^VACAM~^^^^^^VACAO$";
            veri.PV1 = "PV1||I|MED/SURG^201^2$";
            veri.OBR = "OBR|1|8224e161-89a1-4fe8-aad2-a4c5578ab635^^000180ffff7f0b15^EUI-64|0609c6eb-d8ce-4180-b73d-343c922f2776 ^ ^000180ffff7f0b15 ^ EUI - 64 | 44616 - 1 ^ Pulse oximetry panel^LN |||20130328160302.2739-0500||||||||||||||||||||||||||||||||||||| 252465000 ^ Pulse oximetry ^ SCT | 255238004 ^ Continuous ^ SCT$";
            veri.baslik = veri.MSH + veri.PID + veri.PV1 + veri.OBR;
            veri.baslik = veri.baslik.Replace("$", " " + System.Environment.NewLine);
            textBox1.Text = veri.baslik;
            //rjTextBox1.Texts = veri.baslik;
        }

        public void GrafikHazirla()
        {
            GraphPane mypane = zedGraphControl1.GraphPane;
            mypane.Title.Text = "Etco2 grafik";
            mypane.XAxis.Title.Text = "Zaman (sn)";
            mypane.YAxis.Title.Text = "% Etco2";
            myPaneEtco2 = zedGraphControl1.GraphPane;
            myPaneEtco2.YAxis.Scale.Min = 0;
            myPaneEtco2.YAxis.Scale.Max = 1100;//max y ekseni
            myCurveEtco2 = myPaneEtco2.AddCurve(null, listPointsEtco2, Color.Red, SymbolType.None);
            //myCurveEtco2.Line.IsSmooth = true;
            //myCurveEtco2.Line.SmoothTension = 0.1f;
            myCurveEtco2.Line.Width = 3;
            // This will do the area outside of the graphing area
            mypane.Fill.Color = (Color.FromArgb(100, 100, 100));
            // This will do the area inside the graphing area
            mypane.Chart.Fill.Brush = new System.Drawing.SolidBrush(Color.FromArgb(100, 100, 100));
        }

        //[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        //private static extern IntPtr CreateRoundRectRgn
        //(
        //int nLeftRect,     // x-coordinate of upper-left corner
        //int nTopRect,      // y-coordinate of upper-left corner
        //int nRightRect,    // x-coordinate of lower-right corner
        //int nBottomRect,   // y-coordinate of lower-right corner
        //int nWidthEllipse, // height of ellipse
        //int nHeightEllipse // width of ellipse
        // );
        public Form1()
        {
            InitializeComponent();

            GraphPane mypane = zedGraphControl1.GraphPane;
            mypane.Title.Text = "Etco2 grafik";
            mypane.XAxis.Title.Text = "Zaman (sn)";
            mypane.YAxis.Title.Text = "% Etco2";
            // This will do the area outside of the graphing area
            mypane.Fill.Color = (Color.FromArgb(100, 100, 100));
            // This will do the area inside the graphing area
            mypane.Chart.Fill.Brush = new System.Drawing.SolidBrush(Color.FromArgb(100, 100, 100)); 


            //this.formborderstyle = formborderstyle.none;
            //this.region = system.drawing.region.fromhrgn(createroundrectrgn(0,0,width,height,30,30));
            rjButton3.Enabled = false;
            baslıkYaz();

            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(borderSize);
           // this.panelTitleBar.BackColor = borderColor;
            this.BackColor = borderColor;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Form2 f2 = new Form2();
                f2.ShowDialog(); // bağlantı ayarlarını göster
                Baglanti = new MySqlConnection("Server=" + Form2.server_data + ";" + "DATABASE=" + Form2.database_data + ";" + "UID=" + Form2.username_data + ";" + "PASSWORD=" + Form2.password_data);////servera baglan
                komut = new MySqlCommand(Form2.bolum_data, Baglanti);
                Baglanti.Open();
            if (Baglanti.State == ConnectionState.Open)
            {
                label1.Text = "Bağlandı";
                label1.ForeColor = Color.Green;
                label4.ForeColor = Color.Blue;
                label6.ForeColor = Color.Blue;
                label8.ForeColor = Color.Blue;
                baslat = true;
                // timer1.Enabled = true;//serverdan veri almaya başla
            }
            else if (Baglanti.State == ConnectionState.Closed)
            {
                Baglanti.Close();
                label1.Text = "Bağlantı Yok!!";
                label1.ForeColor = Color.Red;
                rjButton3.Enabled = false;

            }
            }
            catch (Exception)
            {
                label1.Text = "Bağlantı Yok!!"; ;//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void button2_Click(object sender, EventArgs e)//hl7 verisini serverdan al
        {
            try
            {
                tarih = tarih.Replace(":", "");
                tarih = tarih.Replace(" ", ".");
                string[] parcalar = tarih.Split('.');
                tarih = parcalar[2] + parcalar[1] + parcalar[0] + parcalar[3];//serverdan gelen zaman etiketini hl7 formatına göre düzenleme
                veri.etco2_data = "OBX|1|NM|EtCO2|1.0.1.1|" + etco2 + "|%^Percent^UCUM|||||F|||" + tarih;
                veri.fico2_data = "OBX|2|NM|FiCO2|1.0.1.2|" + fico2 + "|%^Percent^UCUM|||||F|||" + tarih;
                veri.etco2rr_data = "OBX|3|NM|EtCO2RR|1.0.1.3|" + etco2rr + "|{breaths}/min^breaths per minute^UCUM |||||F|||" + tarih;
                string cikti = veri.etco2_data + "$" + veri.fico2_data + "$" + veri.etco2rr_data + "$";
                cikti = cikti.Replace("$", " " + System.Environment.NewLine);
                string textbox_ilk = textBox1.Text;
                textBox1.Text = textbox_ilk + cikti;
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        public void timer1_Tick(object sender, EventArgs e)//100ms de 1 okuma yap
        {
            try
            {
                using (okuyucu = komut.ExecuteReader())//okuyucu methodu oluştur
            {
                while (okuyucu.Read())//mysql den verileri oku
                {
                    etco2 = (Convert.ToString(okuyucu["etco2"]));
                    fico2 = (Convert.ToString(okuyucu["fico2"]));
                    etco2rr = (Convert.ToString(okuyucu["etco2rr"]));
                    tarih = (Convert.ToString(okuyucu["zaman"]));
                }
            }//işlemler bitince methdou sil
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
            label4.Text = etco2+"%";
            label6.Text = fico2+"%";
            label8.Text = etco2rr+" bpm";

            zaman += 0.05;
            listPointsEtco2.Add(new PointPair(zaman, Convert.ToDouble(etco2)));
            myPaneEtco2.XAxis.Scale.Max = zaman;
            myPaneEtco2.AxisChange();
            zedGraphControl1.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)//grafigi başlat
        {
            timer1.Enabled = true;
            rjButton3.Enabled = true;
            GrafikHazirla();
        }

        private void button5_Click(object sender, EventArgs e)//grafigi durdur
        {
            timer1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)//hl7 kaydet
        {
            try
            {
                saveFileDialog1.FileName = "*";//Varsayılan olarak görüntülenecek Dosya Adı kısmını belirliyoruz
                saveFileDialog1.Filter = "HL7 Dosyaları (*hl7)|*.hl7";//Bu kısım önemli, Kaydedilmesi gereken dosyamızın filtre değerlerini belirliyoruz
                saveFileDialog1.DefaultExt = "hl7";//Varsayılan olarak görüntülenecek dosya uzantısını belirliyoruz
                saveFileDialog1.ShowDialog();//Diyalog Penceresini kullanıcıya gösterip dosya yolu seçmesini sağlıyoruz

                StreamWriter yazmaislemi = new StreamWriter(saveFileDialog1.FileName);//System.IO kütüphanesinden faydalanarak SystemWriter olarak bir yazıcı belirliyoruz ve bu yazıcının dosya yolunu belirliyoruz
                yazmaislemi.WriteLine(textBox1.Text);//TextBoxumuzda bulunan satırları WriteLine metodu ile oluşturduğumuz txt dosyamıza kaydediyoruz
                yazmaislemi.Close();//Hata mesajı almamak için yazma işlemimizi sonlandırıp bağlantımızı kesiyoruz
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void button7_Click(object sender, EventArgs e)//grafiği kaydet
        {
            try
            {
                saveFileDialog1.FileName = "*";
                saveFileDialog1.Filter = "bpm Dosyaları (*bmp)|*.bmp";
                saveFileDialog1.DefaultExt = "bpm";
                saveFileDialog1.ShowDialog();
                myPaneEtco2.GetImage().Save(saveFileDialog1.FileName);
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void button6_Click(object sender, EventArgs e)//grafigi resetle
        {
            myPaneEtco2.CurveList.Clear();
            myPaneEtco2.GraphObjList.Clear();
            myCurveEtco2.Clear();
            zaman = 0;
            listPointsEtco2.Add(new PointPair(0, Convert.ToDouble(etco2)));
            myPaneEtco2.XAxis.Scale.Max = 0;
            myPaneEtco2.AxisChange();
            zedGraphControl1.Refresh();
        }

        private void ıconButtonWindowClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ıconButtonWindowMaximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;
        }

        private void ıconButtonWindowMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void ıconButton1_Click(object sender, EventArgs e)//grafiği başlat
        {
            if (baslat==true)
            {
                timer1.Enabled = true;
                rjButton3.Enabled = true;
                GrafikHazirla();
                yenile = true;
            }
        }

        private void ıconButton3_Click(object sender, EventArgs e)//grafiği durdur
        {
            timer1.Enabled = false;
        }

        private void ıconButton2_Click(object sender, EventArgs e)//grafiği resetle
        {
            if (yenile == true)
            {
            myPaneEtco2.CurveList.Clear();
            myPaneEtco2.GraphObjList.Clear();
            myCurveEtco2.Clear();
            zaman = 0;
            listPointsEtco2.Add(new PointPair(0, Convert.ToDouble(etco2)));
            myPaneEtco2.XAxis.Scale.Max = 0;
            myPaneEtco2.AxisChange();
            zedGraphControl1.Refresh();
            }
        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void rjButton1_Click(object sender, EventArgs e)//grafigi kaydet
        {
            try
            {
                saveFileDialog1.FileName = "*";
                saveFileDialog1.Filter = "bpm Dosyaları (*bmp)|*.bmp";
                saveFileDialog1.DefaultExt = "bpm";
                saveFileDialog1.ShowDialog();
                myPaneEtco2.GetImage().Save(saveFileDialog1.FileName);
            }
            catch (Exception hata)
            {
                //MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void rjButton2_Click(object sender, EventArgs e)//hl7 kaydet
        {
            try
            {
                saveFileDialog1.FileName = "*";//Varsayılan olarak görüntülenecek Dosya Adı kısmını belirliyoruz
                saveFileDialog1.Filter = "HL7 Dosyaları (*hl7)|*.hl7";//Bu kısım önemli, Kaydedilmesi gereken dosyamızın filtre değerlerini belirliyoruz
                saveFileDialog1.DefaultExt = "hl7";//Varsayılan olarak görüntülenecek dosya uzantısını belirliyoruz
                saveFileDialog1.ShowDialog();//Diyalog Penceresini kullanıcıya gösterip dosya yolu seçmesini sağlıyoruz

                StreamWriter yazmaislemi = new StreamWriter(saveFileDialog1.FileName);//System.IO kütüphanesinden faydalanarak SystemWriter olarak bir yazıcı belirliyoruz ve bu yazıcının dosya yolunu belirliyoruz
                yazmaislemi.WriteLine(textBox1.Text);//TextBoxumuzda bulunan satırları WriteLine metodu ile oluşturduğumuz txt dosyamıza kaydediyoruz
                yazmaislemi.Close();//Hata mesajı almamak için yazma işlemimizi sonlandırıp bağlantımızı kesiyoruz
            }
            catch (Exception )
            {
               // MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void rjButton3_Click(object sender, EventArgs e)//hl7 verisini serverdan al
        {
            try
            {
                tarih = tarih.Replace(":", "");
                tarih = tarih.Replace(" ", ".");
                string[] parcalar = tarih.Split('.');
                tarih = parcalar[2] + parcalar[1] + parcalar[0] + parcalar[3];//serverdan gelen zaman etiketini hl7 formatına göre düzenleme
                veri.etco2_data = "OBX|1|NM|EtCO2|1.0.1.1|" + etco2 + "|%^Percent^UCUM|||||F|||" + tarih;
                veri.fico2_data = "OBX|2|NM|FiCO2|1.0.1.2|" + fico2 + "|%^Percent^UCUM|||||F|||" + tarih;
                veri.etco2rr_data = "OBX|3|NM|EtCO2RR|1.0.1.3|" + etco2rr + "|{breaths}/min^breaths per minute^UCUM |||||F|||" + tarih;
                string cikti = veri.etco2_data + "$" + veri.fico2_data + "$" + veri.etco2rr_data + "$";
                cikti = cikti.Replace("$", " " + System.Environment.NewLine);
                string textbox_ilk = textBox1.Text;
                textBox1.Text = textbox_ilk + cikti;
            }
            catch (Exception hata)
            {
                //MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void rjButton4_Click(object sender, EventArgs e)//servera baglan
        {
            try
            {
                Form2 f2 = new Form2();
                f2.ShowDialog(); // bağlantı ayarlarını göster
                Baglanti = new MySqlConnection("Server=" + Form2.server_data + ";" + "DATABASE=" + Form2.database_data + ";" + "UID=" + Form2.username_data + ";" + "PASSWORD=" + Form2.password_data);////servera baglan
                komut = new MySqlCommand(Form2.bolum_data, Baglanti);
                Baglanti.Open();
            if (Baglanti.State == ConnectionState.Open)
            {
                label1.Text = "Bağlandı";
                label1.ForeColor = Color.Green;
                label4.ForeColor = Color.Blue;
                label6.ForeColor = Color.Blue;
                label8.ForeColor = Color.Blue;
                baslat = true;
                // timer1.Enabled = true;//serverdan veri almaya başla
            }
            else if (Baglanti.State == ConnectionState.Closed)
            {
                Baglanti.Close();
                label1.Text = "Bağlantı Yok!!";
                label1.ForeColor = Color.Red;
                rjButton3.Enabled = false;

            }
            }
            catch (Exception)
            {
                label1.Text = "Bağlantı Yok!!"; ;//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }
    }
}
