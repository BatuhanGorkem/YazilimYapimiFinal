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
    public partial class AliciEkrani : Form
    {
        Kullanici aktifUye;
        BorsavtDb db = new BorsavtDb();
        public AliciEkrani(Kullanici alici)
        {
            this.aktifUye = alici;
            InitializeComponent();
        }

        private void AliciEkrani_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            Takas.TakaslariGerceklestir();
            TabloGuncelle();
            comboBox1.Items.AddRange(db.Urunler.Select(x => x.UrunAdi).ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tbtalep.Text != "")
            {
                /*
                BakiyeTalep talep = new BakiyeTalep();
                talep.Kullaniciadi = aktifUye.KullaniciAdi;
                do
                {
                    try
                    {
                        talep.TalepMiktari = (float)Takas.ParaDonustur(float.Parse(tbtalep.Text), comboBox2.SelectedItem.ToString());
                        break;
                    }
                    catch (Exception)
                    {
                        Takas.DovizTarihi = Takas.DovizTarihi.AddDays(-1);
                    }
                } while (true);
                db.BakiyeTalepleri.Add(talep);
                db.SaveChanges();
                aktifUye = db.Kullanicilar.Find(aktifUye.KullaniciAdi);
                MessageBox.Show("Talebiniz gönderildi.");
                */
                do
                {
                    try
                    {
                        aktifUye.Bakiye += (float)Takas.ParaDonustur(float.Parse(tbtalep.Text), comboBox2.SelectedItem.ToString());
                        break;
                    }
                    catch (Exception)
                    {
                        Takas.DovizTarihi = Takas.DovizTarihi.AddDays(-1);
                    }
                } while (true);
                db.SaveChanges();
                aktifUye = db.Kullanicilar.Find(aktifUye.KullaniciAdi);
                TabloGuncelle();
                MessageBox.Show("Bakiye eklendi.");
            }
            else
            {
                MessageBox.Show("Bir miktar giriniz.");
            }
        }
        private void TabloGuncelle()
        {
            aktifUye = db.Kullanicilar.Find(aktifUye.KullaniciAdi);
            BindingSource bindingSource1 = new BindingSource();
            bindingSource1.DataSource = (from r in aktifUye.Talepler
                                         select new { r.Urun.UrunAdi, r.Miktar, r.TalepTuru, r.BirimFiyat }).ToList();
            dgalistalepleri.DataSource = bindingSource1;

            BindingSource bindingSource2 = new BindingSource();
            bindingSource2.DataSource = (from r in aktifUye.KullaniciUrunleri
                                         where (r.Onay == 1)
                                         select new { r.Urun.UrunAdi, r.Miktar }).ToList();
            dgurunler.DataSource = bindingSource2;
            bakiyelbl.Text = "Bakiyeniz: " + aktifUye.Bakiye + " ₺";
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                Talep talep = new Talep();
                talep.Urunid = db.Urunler.Where(x => x.UrunAdi.Equals(comboBox1.Text)).FirstOrDefault().Id;
                talep.Miktar = int.Parse(textBox1.Text);
                talep.Kullaniciadi = aktifUye.KullaniciAdi;
                talep.TalepTuru = "Alış";
                talep.BirimFiyat = int.Parse(tbFiyat.Text);
                db.Talepler.Add(talep);
                db.SaveChanges();
                MessageBox.Show("Talebiniz oluşturuldu.");
            }
            else
            {
                MessageBox.Show("Bir ürün seçiniz.");
            }
            Takas.TakaslariGerceklestir();
            TabloGuncelle();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Takas.RaporYazdir(baslangicTarih.Value, bitisTarih.Value);
        }
    }
}
