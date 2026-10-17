using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

// One button that re-finds everything the app depends on after a client or file update: the game process and
// executable (PID, path, version, SHA-256), every executable-specific address (ClientProfile), the memory
// addresses used for movement/attack/radar, a live Python bridge test, the inventory reader, the pack folder and
// optionally the maps. Results are stored per executable so the next start needs no scan.
public partial class MainForm
{
    private TabPage compatibilityPage;private Label compatibilityStatus,compatibilityArchitecture;private Button compatibilityScanButton,compatibilityLiveScanButton,compatibilityFolderButton,compatibilityPackExtractButton;private Guna2CheckBox compatibilityMaps;
    private const string DefaultClientPath=@"C:\Program Files\LandOfHeroesEU\LandOfHeroesEU.exe";
    private TextBox clientExeBox;
    private static string ClientExeFile{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","istemci-exe.txt");}}
    // The game client this app integrates with (chosen once in Uyumluluk); its file name is the process to look for.
    private string ClientExe
    {
        get
        {
            var chosen=clientExeBox?.Text.Trim();if(!string.IsNullOrEmpty(chosen))return chosen;
            try{if(File.Exists(ClientExeFile)){var saved=File.ReadAllText(ClientExeFile).Trim();if(saved.Length>0)return saved;}}catch{}
            return DefaultClientPath;
        }
    }
    private string ClientProcessName{get{var n=Path.GetFileNameWithoutExtension(ClientExe);return string.IsNullOrEmpty(n)?"LandOfHeroesEU":n;}}
    private void InitializeCompatibilityPanel()
    {
        compatibilityPage=new TabPage("Uyumluluk"){Name="compatibilityPage",BackColor=StudioSurface,ForeColor=StudioText,AutoScroll=true};mainTabs.TabPages.Add(compatibilityPage);
        AddCaption(compatibilityPage,"Otomatik uyumluluk",28,26,650,36,20f,StudioText);
        AddCaption(compatibilityPage,"Client ya da dosyalar değişince uygulamanın ihtiyaç duyduğu her şeyi tek tuşla yeniden bulur.",28,67,650,28,9.5f,StudioMuted);
        AddSectionCard(compatibilityPage,"NE BULUNUR","PID, sürüm ve SHA-256 · köprü ve envanter adresleri · hareket ve saldırı adresleri · haritalar.",28,112,650,80);
        compatibilityScanButton=NewPresetButton("EXE'yi analiz et",50,210,190);compatibilityScanButton.Name="compatibilityScanButton";compatibilityScanButton.Click+=async(s,e)=>await ScanCompatibilityAsync(false,true);compatibilityPage.Controls.Add(compatibilityScanButton);
        compatibilityLiveScanButton=NewPresetButton("Açık oyunu analiz et",252,210,205);compatibilityLiveScanButton.Name="compatibilityLiveScanButton";compatibilityLiveScanButton.Click+=async(s,e)=>await ScanCompatibilityAsync(true,false);compatibilityPage.Controls.Add(compatibilityLiveScanButton);
        compatibilityMaps=new Guna2CheckBox{Name="compatibilityMaps",Text="Haritaları çıkar",Location=new Point(475,214),Size=new Size(180,30),ForeColor=StudioText,Checked=false};compatibilityPage.Controls.Add(compatibilityMaps);
        var exeCaption=new Label{Text="İstemci exe",Location=new Point(50,262),Size=new Size(110,24),ForeColor=StudioMuted};
        clientExeBox=new TextBox{Name="clientExeBox",Location=new Point(160,262),Size=new Size(380,24),Text=ClientExe};
        var exeBrowse=NewPresetButton("Yeni exe seç",552,254,108);exeBrowse.Name="clientExeBrowse";
        exeBrowse.Click+=async(s,e)=>
        {
            using(var d=new OpenFileDialog{Filter="Oyun istemcisi|*.exe",InitialDirectory=File.Exists(ClientExe)?Path.GetDirectoryName(ClientExe):""})
                if(d.ShowDialog(this)==DialogResult.OK){clientExeBox.Text=d.FileName;SaveClientExe();await ScanCompatibilityAsync(false,true);}
        };
        clientExeBox.Leave+=(s,e)=>{SaveClientExe();RefreshCompatibilityArchitecture();};
        clientExeBox.TextChanged+=(s,e)=>RefreshCompatibilityArchitecture();
        compatibilityPage.Controls.AddRange(new Control[]{exeCaption,clientExeBox,exeBrowse});
        compatibilityFolderButton=NewPresetButton("Files klasörü seç",50,300,160);compatibilityFolderButton.Name="compatibilityFolderButton";
        compatibilityFolderButton.Click+=async(s,e)=>
        {
            using(var d=new FolderBrowserDialog{Description="Yeni files klasörünü seçin",SelectedPath=File.Exists(ClientExe)?Path.GetDirectoryName(ClientExe):""})
            {
                if(d.ShowDialog(this)!=DialogResult.OK)return;
                string found=ClientBinaryInfo.FindBestExecutable(d.SelectedPath);
                if(string.IsNullOrEmpty(found)){MessageBox.Show("Bu klasörde uygun x86 veya x64 oyun istemcisi bulunamadı.");return;}
                clientExeBox.Text=found;SaveClientExe();RefreshCompatibilityArchitecture();await ScanCompatibilityAsync(false,true);
            }
        };
        compatibilityArchitecture=new Label{Name="compatibilityArchitecture",Location=new Point(225,302),Size=new Size(260,28),ForeColor=StudioAccent,Font=new Font("Segoe UI Semibold",9.5f)};
        compatibilityPackExtractButton=NewPresetButton("Packları çıkar",500,300,160);compatibilityPackExtractButton.Name="compatibilityPackExtractButton";compatibilityPackExtractButton.Click+=async(s,e)=>await ExtractPacksFromFolderAsync();
        compatibilityPage.Controls.AddRange(new Control[]{compatibilityFolderButton,compatibilityArchitecture,compatibilityPackExtractButton});
        compatibilityStatus=new Label{Name="compatibilityStatus",Text="Hazır · yeni bir client sürümü bağlanırken de adresler otomatik bulunur.",Location=new Point(50,344),Size=new Size(610,336),ForeColor=StudioMuted,BackColor=Color.Transparent,Font=new Font("Consolas",9.5f)};compatibilityPage.Controls.Add(compatibilityStatus);RefreshCompatibilityArchitecture();StyleControls(compatibilityPage);
    }

    private void RefreshCompatibilityArchitecture()
    {
        if(compatibilityArchitecture==null)return;
        ClientBinaryInfo info;
        compatibilityArchitecture.Text=ClientBinaryInfo.TryRead(clientExeBox==null?null:clientExeBox.Text.Trim(),out info)
            ?"Algılandı: "+info.DisplayName+" · otomatik profil"
            :"EXE seçildiğinde x86/x64 otomatik algılanır";
    }

    private async Task ScanCompatibilityAsync(bool liveOnly=false,bool diskOnly=false)
    {
        compatibilityScanButton.Enabled=false;compatibilityLiveScanButton.Enabled=false;var report=new StringBuilder();
        Action<string> line=t=>{report.AppendLine(t);compatibilityStatus.Text=report.ToString();};
        try
        {
            // 1. Game process and executable
            string pickReason=null;Process process=null;
            if(!diskOnly)
            {
                process=memory.Field_0197?.Property_0268;
                if(process!=null&&(process.HasExited||!process.ProcessName.Equals(ClientProcessName,StringComparison.OrdinalIgnoreCase)))process=null;
                if(process==null)process=PickClientProcess(out pickReason);
            }
            if(liveOnly&&process==null){line("✗ Açık oyun bulunamadı: "+(pickReason??"seçili EXE ile oyunu açıp tekrar deneyin."));return;}
            string exe=process!=null?process.MainModule.FileName:(File.Exists(ClientExe)?ClientExe:null);
            if(exe==null){line("✗ İstemci EXE bulunamadı; EXE veya files klasörünü seçin.");return;}
            line(diskOnly?"MOD: EXE dosyasından çevrimdışı analiz":"MOD: Açık oyunda canlı analiz");
            line(process!=null?"✓ Oyun süreci: PID "+process.Id:"• Oyun açık değil; exe dosyası diskten taranıyor.");
            var binary=ClientBinaryInfo.Read(exe);var version=FileVersionInfo.GetVersionInfo(exe);
            line("✓ EXE: "+exe);line("✓ Mimari: "+binary.DisplayName+" · görüntü tabanı 0x"+binary.ImageBase.ToString("X"));
            line("  Sürüm: "+(version.FileVersion??"-")+" · Boyut: "+new FileInfo(exe).Length.ToString("N0")+" bayt");
            string sha=await Task.Run(()=>ClientProfile.Hash(exe));line("  SHA-256: "+sha);

            // 2. Executable-specific addresses
            line("");line("Exe adresleri taranıyor…");
            ClientProfile profile=await Task.Run(()=>ClientProfile.ForFile(exe,true));
            report.Length-=("Exe adresleri taranıyor…"+Environment.NewLine).Length;
            foreach(var l in profile.Report)line(l);
            line(profile.Complete?"✓ Tüm exe adresleri bulundu ve istemci profili kaydedildi.":"• Profil kısmen oluşturuldu; bulunan değerler SHA-256 profiline kaydedildi.");
            line("  Profil sürümü: "+profile.ProfileVersion+" · Mimari: "+profile.Architecture+" · İşaretçi: "+profile.PointerSize+" bayt");
            line("");line("YETENEK DURUMU");
            line((profile.PythonComplete?"✓ ":profile.Is64Bit?"✗ ":"• ")+"Python köprü profili"+(profile.PythonComplete?" hazır":profile.Is64Bit?" eksik":" canlı modül dışa aktarımlarından aranacak"));
            line((profile.InventoryComplete?"✓ ":"• ")+"Envanter / beceri profili"+(profile.InventoryComplete?" hazır":" canlı doğrulama bekliyor"));
            line((profile.Is64Bit?"✓ ":"• ")+"Hareket, saldırı ve radar çekirdeği"+(profile.Is64Bit?" canlı taramaya hazır":" x86 canlı istemci örneğiyle doğrulanacak"));
            if(process==null){line("");line("Oyunu açıp ‘Açık oyunu analiz et’ dediğinde canlı testler tamamlanır.");return;}

            // 3. Memory addresses used for movement, attack, radar and camera
            line("");
            string reason;
            if(memory.Field_0197?.Property_0268?.Id!=process.Id&&!memory.Method_015D(process.Id,out reason)){line("✗ Belleğe bağlanılamadı: "+reason);}
            else
            {
                string coreReport=await AddressCatalog.ResolveAddressesAsync(memory);
                bool core=AddressCatalog.Field_0370!="0x0"&&AddressCatalog.Field_0369!=0&&AddressCatalog.Field_036D!=0;
                line((core?"✓ ":"✗ ")+"Konum, hareket ve saldırı adresleri"+(core?"":" eksik"));
                if(!binary.Is64Bit)line("  x86 güvenlik kilidi: doğrulanmamış 64 bit adresleri kullanılmadı.");
                line("  Çekirdek tarama: "+coreReport.Replace("\n"," · "));
                bool camera=false;try{float angle=memory.ReadFloat(AddressCatalog.Property_0382);camera=!float.IsNaN(angle)&&!float.IsInfinity(angle);}catch{}
                line((camera?"✓ ":"✗ ")+"Kamera / yön adresi");
                int entities=0;try{entities=ReadRadarEntities().Count;}catch{}
                line((entities>0?"✓ ":"• ")+"Radar: "+entities+" varlık okundu"+(entities>0?"":" (karakterle oyunda değilsen normal)"));
            }

            // 4. Live Python bridge test
            string bridge=await Task.Run(()=>TestBridge(process));line(bridge);

            // 5. Inventory reader
            try{using(var reader=new PlayerInventoryReader(process)){var snap=reader.Capture();line("✓ Envanter okuyucu: "+snap.Items.Count+" eşya, "+snap.Skills.Count+" beceri");}}
            catch(Exception ex){line("• Envanter okuyucu: "+ex.Message);}

            // 6. Packs and maps
            string packDir=PackExtractor.PackFolder(process);
            int packs=Directory.Exists(packDir)?Directory.GetFiles(packDir,"*.idx").Length:0;
            line((packs>0?"✓ ":"✗ ")+"Pack klasörü: "+packDir+" ("+packs+" paket)");
            int maps=Directory.Exists(MapsFolder)?Directory.GetDirectories(MapsFolder,"*",SearchOption.AllDirectories).Count(d=>Directory.GetDirectories(d).Any(a=>Path.GetFileName(a).Length==6&&Path.GetFileName(a).All(char.IsDigit))):0;
            line((maps>0?"✓ ":"• ")+"Haritalar: "+maps+" harita ("+MapsFolder+")");
            if(compatibilityMaps.Checked){line("Haritalar yeniden çıkarılıyor…");await UpdateMapsAsync(m=>{compatibilityStatus.Text=report+m;});line("✓ Haritalar güncellendi.");}

            line("");line("Profil: "+Path.Combine(ClientProfile.Folder,sha+".txt"));
        }
        catch(Exception ex){line("✗ Tarama tamamlanamadı: "+ex.Message);}
        finally
        {
            compatibilityScanButton.Enabled=true;compatibilityLiveScanButton.Enabled=true;
            try{string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","uyumluluk");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,DateTime.Now.ToString("yyyyMMdd-HHmmss")+".txt"),report.ToString());}catch{}
        }
    }

    private void SaveClientExe(){try{Directory.CreateDirectory(Path.GetDirectoryName(ClientExeFile));File.WriteAllText(ClientExeFile,clientExeBox.Text.Trim());}catch{}RefreshClientList();}

    // Runs a one-line script in the game and checks its output file.
    private string TestBridge(Process process)
    {
        string output=Path.Combine(runtimeFolder,"bridge-test.txt");
        try
        {
            Directory.CreateDirectory(runtimeFolder);if(File.Exists(output))File.Delete(output);
            using(var bridge=new QueuedPythonBridge(process))
            {
                bridge.Queue(LocalPythonScript("f.write('ok')",output));
                var sw=Stopwatch.StartNew();while(!bridge.Poll()){if(sw.ElapsedMilliseconds>8000)return "✗ Python köprüsü: oyun yanıt vermedi";Thread.Sleep(40);}
            }
            for(int i=0;i<40&&!File.Exists(output);i++)Thread.Sleep(50);
            return File.Exists(output)&&File.ReadAllText(output)=="ok"?"✓ Python köprüsü: oyunda komut çalıştırıldı":"✗ Python köprüsü: komut çalışmadı";
        }
        catch(Exception ex){return "✗ Python köprüsü: "+ex.Message;}
        finally{try{File.Delete(output);}catch{}}
    }
}
