using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlimSatimSistemi
{
    public partial class Form1 : Form
    {
        BorsavtDb db = new BorsavtDb();
        public Form1()
        {
            InitializeComponent();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex==3)
            {
                tabControl1.Size = new Size(260, 335);
                this.Size = new Size(272, 373);
            }
            else
            {
                tabControl1.Size = new Size(260, 150);
                this.Size = new Size(272, 186);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Kullanici alici = db.Kullanicilar.Where(x => x.KullaniciAdi == textBox1.Text && x.Sifre == textBox2.Text && x.UyeTuru=="Alici").FirstOrDefault();
            if (alici!= null)
            {
                new AliciEkrani(alici).ShowDialog();
            }
            else
            {
                MessageBox.Show("Kullanıcı adı/şifre hatalı.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Kullanici satici = db.Kullanicilar.Where(x => x.KullaniciAdi == textBox4.Text && x.Sifre == textBox3.Text && x.UyeTuru == "Satici").FirstOrDefault();
            if (satici != null)
            {
                new SaticiEkrani(satici).ShowDialog();
            }
            else
            {
                MessageBox.Show("Kullanıcı adı/şifre hatalı.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox6.Text=="admin"&&textBox5.Text=="admin")
            {
                new YoneticiEkrani().ShowDialog();
            }
            else
            {
                MessageBox.Show("Kullanıcı adı/şifre hatalı.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Kullanici yeniKayit = new Kullanici();
            if (db.Kullanicilar.Find(tbkadi.Text)==null)
            {
                yeniKayit.Ad = tbad.Text;
                yeniKayit.Soyad = tbsoyad.Text;
                yeniKayit.KullaniciAdi = tbkadi.Text;
                yeniKayit.Sifre = tbsifre.Text;
                yeniKayit.TC = tbtc.Text;
                yeniKayit.Telefon = tbtel.Text;
                yeniKayit.UyeTuru = radioalici.Checked ? "Alici" : "Satici";
                yeniKayit.Adres = tbadres.Text;
                yeniKayit.Eposta = tbeposta.Text;
                yeniKayit.Bakiye = 0;
                db.Kullanicilar.Add(yeniKayit);
                db.SaveChanges();
                MessageBox.Show("Kayıt başarılı, giriş yapabilirsiniz.");
            }
            else
            {
                MessageBox.Show("Bu kullanıcı adı ile kayıtlı bir kullanıcı var.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            Takas.TakaslariGerceklestir();
        }
    }
}
