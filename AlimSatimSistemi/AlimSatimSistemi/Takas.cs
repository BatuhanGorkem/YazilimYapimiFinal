using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace AlimSatimSistemi
{
    class Takas
    {
        public static DateTime DovizTarihi=DateTime.Now;
        private static Talep SatisTalebiBul(Urun urun)
        {
            BorsavtDb db = new BorsavtDb();
            Talep aranan = null;
            foreach (Talep talep in db.Talepler.Where(x => x.TalepTuru == "Satış" && x.Urun.UrunAdi == urun.UrunAdi))
            {
                if (aranan == null)
                {
                    aranan = talep;
                }
                else
                {
                    if (talep.BirimFiyat > aranan.BirimFiyat)
                    {
                        aranan = talep;
                    }
                }
            }
            return aranan;
        }
        public static void TakaslariGerceklestir()
        {
            BorsavtDb db = new BorsavtDb();
            foreach (Talep talep in db.Talepler.Where(x => x.TalepTuru == "Alış" && x.Miktar > 0))
            {
                Talep talepara = SatisTalebiBul(talep.Urun);
                Kullanici alici = talep.Kullanici;
                //User Story 5
                if (talepara != null && alici.Bakiye > talepara.BirimFiyat && talep.BirimFiyat >= talepara.BirimFiyat)
                {
                    Kullanici satici = db.Kullanicilar.Find(talepara.Kullaniciadi);
                    KullaniciUrun alinanUrun = null;
                    int? alinacakMiktar;
                    if (talepara.Miktar >= talep.Miktar)
                    {
                        alinacakMiktar = talep.Miktar;
                    }
                    else
                    {
                        alinacakMiktar = talepara.Miktar;
                    }
                    int? toplamTutar = alinacakMiktar * talepara.BirimFiyat;
                    if (alici.Bakiye < toplamTutar)
                    {
                        alinacakMiktar = (int?)(alici.Bakiye / talepara.BirimFiyat);
                        toplamTutar = alinacakMiktar * talepara.BirimFiyat;
                    }
                    alinanUrun = new KullaniciUrun();
                    alinanUrun.Miktar = alinacakMiktar;
                    alinanUrun.Urunid = talepara.Urunid;
                    alinanUrun.KullaniciAdi = alici.KullaniciAdi;
                    alinanUrun.Onay = 1;
                    KullaniciUrun kullanicininUrunu = alici.KullaniciUrunleri.Where(x => x.Urunid == talepara.Urunid).FirstOrDefault();
                    if (kullanicininUrunu != null)
                    {
                        kullanicininUrunu.Miktar += alinacakMiktar;
                    }
                    else
                    {
                        alici.KullaniciUrunleri.Add(alinanUrun);
                    }
                    alici.Bakiye -= toplamTutar;
                    alici.Bakiye -= (float)(toplamTutar * 0.01); //User Story 8
                    satici.Bakiye += toplamTutar;
                    talepara.Miktar -= alinacakMiktar;
                    talep.Miktar -= alinacakMiktar;
                    Islem islem = new Islem();
                    islem.IslemZamani = DateTime.Now;
                    islem.Detay = talep.Urun.UrunAdi;
                    islem.Tutar = toplamTutar.ToString();
                    islem.KalanTutar = alici.Ad + " " + alici.Bakiye + " tl parası kaldı.";
                    islem.BirimFiyat = talepara.BirimFiyat.ToString();
                    db.Islemler.Add(islem);
                }
            }
            db.SaveChanges();
            List<Talep> talepler = db.Talepler.Where(x => x.Miktar == 0).ToList();
            foreach (Talep silinecekTalep in talepler)
            {
                db.Talepler.Remove(db.Talepler.Find(silinecekTalep.Id));
            }
            List<KullaniciUrun> silinecekUrunler = db.KullaniciUrunleri.Where(x => x.Miktar == 0 && x.Onay == 1).ToList();
            foreach (KullaniciUrun silinecek in silinecekUrunler)
            {
                db.KullaniciUrunleri.Remove(db.KullaniciUrunleri.Find(silinecek.Id));
            }
            db.SaveChanges();
        }
        //User Story 6
        public static void RaporYazdir(DateTime baslangic, DateTime bitis)
        {
            StringBuilder csv = new StringBuilder();
            BorsavtDb db = new BorsavtDb();
            csv.AppendLine(string.Format("{0},{1},{2},{3}", "Tarih", "Urun Tipi", "Alim Tutari", "Miktar"));
            foreach (Islem islem in db.Islemler)
            {
                if (islem.IslemZamani >= baslangic && islem.IslemZamani <= bitis)
                {
                    csv.AppendLine(string.Format("{0},{1},{2},{3}", islem.IslemZamani, islem.Detay, islem.BirimFiyat, (int.Parse(islem.Tutar) / int.Parse(islem.BirimFiyat))));
                }
            }
            File.WriteAllText("Rapor.csv", csv.ToString());
            Process.Start("Rapor.csv");
        }
        //User Story 7
        public static double ParaDonustur(double fiyat, string doviz)
        {
            try
            {
                string tarihFormat = DovizTarihi.ToString("yyyy MM") + DovizTarihi.ToString("/dd MM yyyy");
                tarihFormat = tarihFormat.Replace(" ", "");
                XmlDocument dovizVerileri = new XmlDocument();
                dovizVerileri.Load("http://www.tcmb.gov.tr/kurlar/" + tarihFormat + ".xml");
                string KOD = "";
                if (doviz == "$")
                {
                    KOD = "USD";
                }
                else if (doviz == "€")
                {
                    KOD = "EUR";
                }
                else if (doviz == "£")
                {
                    KOD = "GBP";
                }
                else
                {
                    return fiyat;
                }
                return fiyat * Convert.ToDouble(dovizVerileri.SelectSingleNode(string.Format("Tarih_Date/Currency[@Kod='{0}']/ForexSelling", KOD)).InnerText);
            }
            catch (XmlException exception)
            {
                return fiyat;
            }
        }
    }
}
