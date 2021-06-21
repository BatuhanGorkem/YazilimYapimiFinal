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
    public partial class SaticiEkrani : Form
    {
        Kullanici aktifUye;
        BorsavtDb db = new BorsavtDb();
        public SaticiEkrani(Kullanici satici)
        {
            this.aktifUye = satici;
            InitializeComponent();
        }
        private void TabloGuncelle()
        {
            aktifUye = db.Kullanicilar.Find(aktifUye.KullaniciAdi);
            BindingSource bindingSource1 = new BindingSource();
            bindingSource1.DataSource = (from r in aktifUye.KullaniciUrunleri
                                         where (r.Onay==0)
                                         select new { r.Urun.UrunAdi, r.Miktar }).ToList();
            dguruntalepleri.DataSource = bindingSource1;

            BindingSource bindingSource2 = new BindingSource();
            bindingSource2.DataSource = (from r in aktifUye.KullaniciUrunleri
                                         where (r.Onay == 1)
                                         select new { r.Urun.UrunAdi, r.Miktar }).ToList();
            dgurunler.DataSource = bindingSource2;

            BindingSource bindingSource3 = new BindingSource();
            bindingSource3.DataSource = (from r in db.Talepler
                                         where (r.Kullaniciadi==aktifUye.KullaniciAdi)
                                         select new { r.Kullaniciadi, r.Miktar, r.BirimFiyat }).ToList();
            dataGridView1.DataSource = bindingSource3;


            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(aktifUye.KullaniciUrunleri.Where(x=>x.Onay==1).Select(x => x.Urun.UrunAdi).ToArray());
            bakiyelbl.Text = "Bakiyeniz: "+aktifUye.Bakiye.ToString() + " ₺";
        }
        private void SaticiEkrani_Load(object sender, EventArgs e)
        {
            Takas.TakaslariGerceklestir();
            TabloGuncelle();
            comboBox1.Items.AddRange(db.Urunler.Select(x => x.UrunAdi).ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex>=0)
            {
                KullaniciUrun urun = new KullaniciUrun();
                urun.Urunid = db.Urunler.Where(x => x.UrunAdi.Equals(comboBox1.Text)).FirstOrDefault().Id;
                urun.KullaniciAdi = aktifUye.KullaniciAdi;
                urun.Miktar = int.Parse(textBox1.Text);
                urun.Onay = 1;
                db.KullaniciUrunleri.Add(urun);
                db.SaveChanges();
                MessageBox.Show("Talebiniz oluşturuldu.");
            }
            else
            {
                MessageBox.Show("Bir seçim yapınız.");
            }
            TabloGuncelle();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblmiktar.Text = aktifUye.KullaniciUrunleri.Where(x => x.Urun.UrunAdi == comboBox2.Text&&x.Onay==1).FirstOrDefault().Miktar.ToString()+" kg";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex>=0)
            {
                Talep talep = new Talep();
                talep.Kullaniciadi = aktifUye.KullaniciAdi;
                talep.BirimFiyat = int.Parse(tbbirimfiyat.Text);
                talep.Miktar = int.Parse(tbmiktar.Text);
                talep.TalepTuru = "Satış";
                KullaniciUrun secim = aktifUye.KullaniciUrunleri.Where(x => x.Urun.UrunAdi == comboBox2.Text && x.Onay == 1 && x.Miktar >= int.Parse(tbmiktar.Text)).FirstOrDefault();
                if (secim!=null)
                {
                    talep.Urunid = secim.Id;
                    db.Talepler.Add(talep);
                    secim.Miktar -= talep.Miktar;
                    db.SaveChanges();
                    MessageBox.Show("Satış talebiniz oluşturuldu.");
                }
                else
                {
                    MessageBox.Show("Girdiğiniz miktar geçersiz.");
                }
            }
            else
            {
                MessageBox.Show("Tüm değerleri giriniz.");
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
