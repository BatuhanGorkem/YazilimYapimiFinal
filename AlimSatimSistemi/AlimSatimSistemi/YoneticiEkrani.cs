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
    public partial class YoneticiEkrani : Form
    {
        BorsavtDb db = new BorsavtDb();
        public YoneticiEkrani()
        {
            InitializeComponent();
        }

        private void YoneticiEkrani_Load(object sender, EventArgs e)
        {
            Takas.TakaslariGerceklestir();
            cbalici.Items.AddRange(db.Kullanicilar.Where(x => x.UyeTuru == "Alici").Select(x=>x.KullaniciAdi).ToArray());
            cbsatici.Items.AddRange(db.Kullanicilar.Where(x => x.UyeTuru == "Satici").Select(x=>x.KullaniciAdi).ToArray());
            BindingSource bindingSource1 = new BindingSource();
            bindingSource1.DataSource = (from r in db.Talepler
                                         select new { r.Kullaniciadi,r.Urun.UrunAdi, r.Miktar ,r.TalepTuru,r.BirimFiyat}).ToList();
            dataGridView1.DataSource = bindingSource1;
        }

        private void cbalici_SelectedIndexChanged(object sender, EventArgs e)
        {
            BakiyeTalepGuncelle();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (bakiyetalepleri.SelectedIndex>=0)
            {
                string talepid = bakiyetalepleri.SelectedItem.ToString().Split('-')[0].Trim();
                Kullanici alici = db.Kullanicilar.Find(cbalici.Text);
                alici.Bakiye += alici.BakiyeTalepleri.Where(x=>x.Id.ToString()==talepid).FirstOrDefault().TalepMiktari;
                db.BakiyeTalepleri.Remove(alici.BakiyeTalepleri.Where(x=>x.Id.ToString()==talepid).FirstOrDefault());
                db.SaveChanges();
                BakiyeTalepGuncelle();
            }
        }

        private void cbsatici_SelectedIndexChanged(object sender, EventArgs e)
        {
            UrunTalepGuncelle();
        }

        void UrunTalepGuncelle()
        {
            Kullanici satici = db.Kullanicilar.Find(cbsatici.Text);
            uruntalepleri.Items.Clear();
            foreach (KullaniciUrun urun in satici.KullaniciUrunleri.Where(x => x.Onay == 0))
            {
                string icerik = urun.Id + "- Ürün Adı: " + urun.Urun.UrunAdi + "  Miktar: " + urun.Miktar;
                uruntalepleri.Items.Add(icerik);
            }
        }
        void BakiyeTalepGuncelle()
        {
            Kullanici alici = db.Kullanicilar.Find(cbalici.Text);
            bakiyetalepleri.Items.Clear();
            foreach (BakiyeTalep talep in alici.BakiyeTalepleri)
            {
                string icerik = talep.Id + "- Talep:";
                icerik += talep.TalepMiktari + " ₺";
                bakiyetalepleri.Items.Add(icerik);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (uruntalepleri.SelectedIndex >= 0)
            {
                string talepid = uruntalepleri.SelectedItem.ToString().Split('-')[0].Trim();
                Kullanici satici = db.Kullanicilar.Find(cbsatici.Text);
                satici.KullaniciUrunleri.Where(x=>x.Id.ToString()==talepid).FirstOrDefault().Onay = 1;
                db.SaveChanges();
                UrunTalepGuncelle();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BindingSource bindingSource1 = new BindingSource();
            bindingSource1.DataSource = (from r in db.Talepler
                                         select new { r.Kullaniciadi, r.Urun.UrunAdi, r.Miktar, r.TalepTuru, r.BirimFiyat }).ToList();
            dataGridView1.DataSource = bindingSource1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BindingSource bindingSource1 = new BindingSource();
            bindingSource1.DataSource = (from r in db.Islemler
                                         select new { r.IslemZamani,r.Detay, r.Tutar, r.KalanTutar, r.BirimFiyat}).ToList();
            dataGridView1.DataSource = bindingSource1;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Takas.DovizTarihi = dateTimePicker1.Value;
            MessageBox.Show("Güncelleme başarılı");
        }
    }
}
