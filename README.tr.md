# ByeDPI Manager

[Русский](README.md) | [English](README.en.md) | Türkçe

ByeDPI ve ProxiFyre'ı çalıştırmak için küçük bir araç.

![Interface Screenshot](screens/screen_en.png)

## Gereksinimler

1. Windows 7+, [.NET Framework 4.7.2+](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net472-offline-installer)
2. [ProxiFyre](https://github.com/wiresock/proxifyre), [Windows Packet Filter](https://github.com/wiresock/ndisapi), [Visual C++ Redist 2022](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)
3. [ByeDPI](https://github.com/hufrea/byedpi)

## Kurulum

* Topluluk tarafından hazırlanan kurulum rehberi: [ByeDPI Manager Manual](https://github.com/BDManual/ByeDPIManager-Manual)

### Seçenek 1: All-in-One (Başlangıç için önerilir)

Bu seçenek gerekli tüm bileşenleri tek bir arşivin içinde toplar.

1. **İndirme:**

   * Sürümler sayfasına gidin: [https://github.com/romanvht/ByeDPIManager/releases/latest](https://github.com/romanvht/ByeDPIManager/releases/latest)
   * `All_In_One_w64.zip` dosyasını indirin.

2. **Arşivden çıkarma:**

   * Bilgisayarınızdaki indirilmiş dosyaya gidin.
   * Sağ tıklayın ve “Tümünü Ayıkla…” seçeneğini seçin.
   * Bir kurulum klasörü seçin (ör. `C:\APPS\ByeDPIManager`)

3. **Bağımlılıkları yükleme:**

   * Ayıklanmış arşivin içindeki `redist` klasörünü açın. 
   * Bu klasörün içindeki iki uygulamayı da kurun:

     * Windows Packet Filter (ProxiFyre için gerekli)
     * Visual C++ Redistributable 2022

### Seçenek 2: Manuel Kurulum (Gelişmiş kullanıcılar için)

Bileşenleri ayrı bir şekilde yönetmeyi tercih ediyorsanız:

1. **Bileşenleri ayrıca indirin:**

   * [Manager](https://github.com/romanvht/ByeDPIManager/releases/latest)
   * [ByeDPI](https://github.com/hufrea/byedpi)
   * [ProxiFyre](https://github.com/wiresock/proxifyre)

2. **Bağımlılıkları yükleyin:**

   * [Windows Packet Filter](https://github.com/wiresock/ndisapi) (ProxiFyre için gerekli)
   * [Visual C++ Redistributable 2022](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)

3. **Tüm bileşenleri uygun klasörlere ayıklayın**

4. **Çalıştırın ve yolları ayarlayın:**

   * ByeDPI sekmesinde `ciadpi.exe` için doğru yolu belirtin. 
   * ProxiFyre sekmesinde `proxifyre.exe` için doğru yolu belirtin. 

## Yapılandırma

### İlk kurulum

1. **Uygulamayı başlatın:**

   * `ByeDPI Manager.exe`'yi çalıştırın.
   * "Ayarlar" düğmesine tıklayın.

2. **ProxiFyre kurulumu:**

   * “ProxiFyre” sekmesine gidin.
   * Engeli aşılacak uygulamaları seçin (ör. Chrome, Firefox vb.)

### Strateji Yapılandırması

#### Hazır tanımlanmış stratejiyi kullanma

* “ByeDPI” sekmesindeki “Parametreler” alanına istediğiniz stratejiyi girin.

#### Strateji Testi (isteğe bağlı)

Eğer hazır tanımlanmış bir stratejiniz yoksa, yerleşik test aracını kullanabilirsiniz.

1. **Test aracı sekmesine gidin:**

   * “Denemeler (Beta)” sekmesini açın 

2. **Testi başlatın:**

   * “Başla” düğmesine tıklayın.
   * İlk çalıştırmada, `ciadpi.exe` için internet izni vermeniz istenecek – “İzin ver” düğmesine tıklayın.

3. **Bir strateji seçin:**

   * Test bittikten sonra, başarı oranı %50'nin üstünde olan stratejiler günlük bölümünde listelenecektir.
   * En iyisini seçin ve kopyalayın (Ctrl+C)

4. **Stratejiyi uygulayın:**

   * “ByeDPI” sekmesine geri gidin. 
   * Kopyaladığınız stratejiyi “Parametreler” alanına yapıştırın (Ctrl+V)

5. **Testi özelleştirin (isteğe bağlı):**

   * `proxytest` klasöründeki dosyaları düzenleyin:

     * `sites.txt` – kendi istediğiniz siteleri teste ekleyin.
     * `cmds.txt` – kontrol etmek için kendi stratejilerinizi ekleyin.

### Başlatın ve Test Edin

1. **Etkinleştirin:**

   * Ana pencerede “Bağlan” düğmesine basın.
   * İlk çalıştırmada, ProxiFyre internet izni isteyecek – “İzin ver” düğmesine tıklayın.

2. **Çalıştığından emin olun:**

   * Yapılandırdığınız tarayıcı veya uygulamayı açın.
   * Sitelerin veya hizmetlerin erişilebilir olduğunu doğrulayın.

## Sorun Giderme

* Eğer uygulama başlamazsa, .NET Framework 4.7.2+ yüklü olduğundan emin olun.
* Eğer engel aşma çalışmazsa, başka bir strateji deneyin.
* Eğer bağlantı sorunları olursa, Windows Packet Filter'ın doğru şekilde yüklendiğinden emin olun.
* Antivirüs veya güvenlik duvarınızın uygulamayı engellemediğinden emin olun.

## Özel Teşekkür

* [ByeDPI](https://github.com/hufrea/byedpi)
* [ProxiFyre](https://github.com/wiresock/proxifyre)
* [Windows Packet Filter](https://github.com/wiresock/ndisapi)
* [SocksSharp](https://github.com/extremecodetv/SocksSharp)
