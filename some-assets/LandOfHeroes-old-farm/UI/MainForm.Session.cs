using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public partial class MainForm
{
    private SessionProfile sessionProfile = new SessionProfile();
    private TextBox sessionUsername, sessionPassword, sessionCharacter;
    private CheckBox rememberSession;
    private Label sessionStatus;
    private static string SessionProfilePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Onur", "session.dat"); } }
    private void InitializeSessionPanel()
    {
        mainTabs.ItemSize = new Size(132,48); tabDamage.Text="Saldırı"; inventoryPage.Text="Envanter";
        var page = new TabPage("Oturum") { BackColor=StudioSurface, ForeColor=StudioText };
        mainTabs.TabPages.Add(page);
        AddCaption(page,"Hesap ve karakter",24,24,610,34,18f,StudioText);
        AddCaption(page,"Kullanıcı adı",24,90,610,24,10f,StudioMuted);
        sessionUsername=SessionTextBox(page,120,false);
        AddCaption(page,"Şifre",24,167,610,24,10f,StudioMuted);
        sessionPassword=SessionTextBox(page,197,true);
        AddCaption(page,"Karakter adı",24,244,610,24,10f,StudioMuted);
        sessionCharacter=SessionTextBox(page,274,false);
        rememberSession=new CheckBox { Text="Bu Windows hesabında şifreli sakla",Location=new Point(24,324),Size=new Size(600,28),ForeColor=StudioText };
        page.Controls.Add(rememberSession);
        var save=new Button { Text="Hesap bilgilerini uygula",Location=new Point(24,370),Size=new Size(290,40),FlatStyle=FlatStyle.Flat,ForeColor=StudioAccent };
        save.Click+=(s,e)=> {
            sessionProfile=new SessionProfile { Username=sessionUsername.Text.Trim(),Password=sessionPassword.Text,CharacterName=sessionCharacter.Text.Trim() };
            LoadAutomationSelections();SaveAutomationSelections();sessionDirty=true;
            try {
                if(rememberSession.Checked) { SessionProfileStore.Save(SessionProfilePath,sessionProfile);sessionStatus.Text="Hesap bilgileri bu Windows kullanıcısına bağlı şifreleme ile saklandı."; }
                else { if(File.Exists(SessionProfilePath))File.Delete(SessionProfilePath);sessionStatus.Text="Hesap bilgileri yalnızca bu oturumda tutuluyor."; }
            } catch { sessionStatus.Text="Hesap ayarları kaydedilemedi. Windows kullanıcı izinlerini kontrol edin."; }
        };
        page.Controls.Add(save);
        sessionStatus=new Label { Text="Şifreni sohbet yerine bu alana girebilirsin.",Location=new Point(24,432),Size=new Size(610,60),ForeColor=StudioMuted };
        page.Controls.Add(sessionStatus);
        AddCaption(page,"Otomatik giriş, canlandırma ve pelerin bağlantısı henüz\ncanlı doğrulamayı geçmedi; bu sürümde etkin değildir.",24,524,610,64,10f,StudioMuted);
        try {
            if(File.Exists(SessionProfilePath)) {
                sessionProfile=SessionProfileStore.Load(SessionProfilePath);sessionUsername.Text=sessionProfile.Username;sessionPassword.Text=sessionProfile.Password;sessionCharacter.Text=sessionProfile.CharacterName;rememberSession.Checked=true;
            }
        } catch { sessionStatus.Text="Kaydedilmiş hesap bu Windows kullanıcısıyla çözülemedi. Bilgileri yeniden girin."; }
    }
    private TextBox SessionTextBox(TabPage page,int top,bool secret)
    {
        var box=new TextBox { Left=24,Top=top,Width=610,UseSystemPasswordChar=secret,BackColor=StudioBackground,ForeColor=StudioText,BorderStyle=BorderStyle.FixedSingle,MaxLength=128 };
        page.Controls.Add(box);return box;
    }
}
