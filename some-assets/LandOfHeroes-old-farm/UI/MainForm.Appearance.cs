using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Guna.UI2.WinForms;

public partial class MainForm
{
 private static readonly Color NightPage=Color.FromArgb(8,13,25),NightSurface=Color.FromArgb(15,23,40),NightRaised=Color.FromArgb(22,32,53),NightInput=Color.FromArgb(17,27,47),NightBorder=Color.FromArgb(43,57,82),NightText=Color.FromArgb(239,243,250),NightMuted=Color.FromArgb(148,163,184),NightAccent=Color.FromArgb(117,132,255),NightAccentHover=Color.FromArgb(145,157,255),NightAccentSoft=Color.FromArgb(33,43,85);
 private static Color CurrentThemeInput=Color.White,CurrentThemeBorder=LightBorder,CurrentThemeAccent=LightAccent,CurrentThemeText=LightText,CurrentThemeMuted=LightMuted,CurrentThemeAccentSoft=LightAccentSoft; private static bool CurrentThemeNight;
 private Button themeToggleButton; private bool nightTheme;
 private readonly string appearanceThemePath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","appearance.theme");

 private void InitializeAppearanceTheme()
 {
  Opacity=1d;AllowTransparency=false;TransparencyKey=Color.Empty;
  Field_02FB.UseTransparentDrag=false;Field_02FC.UseTransparentDrag=false;Field_02FD.UseTransparentDrag=false;Field_0302.UseTransparentDrag=false;
  themeToggleButton=new Button{Name="themeToggleButton",FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand,TabStop=true,UseVisualStyleBackColor=false,Font=new Font("Segoe UI Semibold",8.5f),Location=new Point(642,17),Size=new Size(108,31)};
  themeToggleButton.Click+=(s,e)=>ApplyAppearanceTheme(!nightTheme,true);
  headerPanel.Controls.Add(themeToggleButton);themeToggleButton.BringToFront();
  bool savedNight=false;try{if(File.Exists(appearanceThemePath))savedNight=string.Equals(File.ReadAllText(appearanceThemePath).Trim(),"night",StringComparison.OrdinalIgnoreCase);}catch{}
  ApplyAppearanceTheme(savedNight,false);
 }

 private void ApplyAppearanceTheme(bool useNight,bool persist)
 {
  nightTheme=useNight;CurrentThemeNight=useNight;
  Color page=useNight?NightPage:LightPage,surface=useNight?NightSurface:LightSurface,raised=useNight?NightRaised:LightRaised,input=useNight?NightInput:Color.White,border=useNight?NightBorder:LightBorder,text=useNight?NightText:LightText,muted=useNight?NightMuted:LightMuted,accent=useNight?NightAccent:LightAccent,accentHover=useNight?NightAccentHover:LightAccentHover,accentSoft=useNight?NightAccentSoft:LightAccentSoft;
  CurrentThemeInput=input;CurrentThemeBorder=border;CurrentThemeAccent=accent;CurrentThemeText=text;CurrentThemeMuted=muted;CurrentThemeAccentSoft=accentSoft;
  SuspendLayout();BackColor=page;ForeColor=text;ApplyAppearanceToTree(this,page,surface,raised,input,border,text,muted,accent,accentHover,accentSoft);
  headerPanel.FillColor=surface;headerPanel.BackColor=surface;headerPanel.BorderColor=border;headerPanel.CustomBorderColor=border;
  leftPanel.FillColor=surface;leftPanel.BackColor=surface;leftPanel.BorderColor=border;
  mainTabs.TabMenuBackColor=page;mainTabs.TabButtonIdleState.FillColor=page;mainTabs.TabButtonIdleState.ForeColor=muted;mainTabs.TabButtonIdleState.InnerColor=page;
  mainTabs.TabButtonHoverState.FillColor=raised;mainTabs.TabButtonHoverState.ForeColor=text;mainTabs.TabButtonHoverState.InnerColor=raised;
  mainTabs.TabButtonSelectedState.FillColor=surface;mainTabs.TabButtonSelectedState.ForeColor=text;mainTabs.TabButtonSelectedState.InnerColor=accent;
  rndTitleLbl.ForeColor=text;pidLbl.ForeColor=accent;foreach(var box in new[]{guna2ControlBox1,guna2ControlBox2}){box.FillColor=surface;box.IconColor=muted;}
  themeToggleButton.Text=useNight?"GÜNDÜZ":"GECE";themeToggleButton.AccessibleName=useNight?"Gündüz temasına geç":"Gece temasına geç";
  themeToggleButton.BackColor=raised;themeToggleButton.ForeColor=text;themeToggleButton.FlatAppearance.BorderColor=border;themeToggleButton.FlatAppearance.MouseOverBackColor=accentSoft;themeToggleButton.FlatAppearance.MouseDownBackColor=accentSoft;
  if(!clientConnected){var old=radarPanel.BackgroundImage;radarPanel.BackgroundImage=RadarPlaceholder(radarPanel.Width,radarPanel.Height);if(old!=null)old.Dispose();}
  radarPanel.BackColor=useNight?Color.FromArgb(6,10,19):LightRaised;ApplyBigMapAppearance();ApplyControlCenterAppearance();Invalidate(true);ResumeLayout(true);
  if(persist)try{Directory.CreateDirectory(Path.GetDirectoryName(appearanceThemePath));File.WriteAllText(appearanceThemePath,useNight?"night":"day");}catch{}
 }

 private void ApplyAppearanceToTree(Control parent,Color page,Color surface,Color raised,Color input,Color border,Color text,Color muted,Color accent,Color accentHover,Color accentSoft)
 {
  foreach(Control c in parent.Controls)
  {
   if(c==themeToggleButton||c==emergencyButton)continue;
   var gp=c as Guna2Panel;var gb=c as Guna2Button;var gc=c as Guna2CheckBox;var gt=c as Guna2TrackBar;var grid=c as DataGridView;var label=c as Label;var toggle=c as ToggleSwitch;
   if(gp!=null){bool card=Convert.ToString(gp.Tag)=="section-card";gp.FillColor=card?raised:surface;gp.BackColor=card?raised:surface;gp.BorderColor=border;gp.ShadowDecoration.Enabled=false;}
   else if(gb!=null){bool primary=PrimaryButtons.Contains(gb.Name)||gb==taskRun;Color primaryText=nightTheme?NightPage:Color.White;gb.FillColor=primary?accent:input;gb.ForeColor=primary?primaryText:text;gb.BorderColor=border;gb.BorderThickness=primary?0:1;gb.HoverState.FillColor=primary?accentHover:raised;gb.HoverState.ForeColor=primary?primaryText:text;gb.HoverState.BorderColor=border;gb.DisabledState.FillColor=raised;gb.DisabledState.ForeColor=muted;gb.DisabledState.BorderColor=border;}
   else if(gc!=null){gc.ForeColor=text;gc.CheckedState.FillColor=accent;gc.CheckedState.BorderColor=accent;gc.UncheckedState.FillColor=input;gc.UncheckedState.BorderColor=muted;}
   else if(gt!=null){gt.ThumbColor=accent;gt.FillColor=border;gt.HoverState.ThumbColor=accentHover;}
   else if(grid!=null){grid.BackgroundColor=surface;grid.GridColor=border;grid.DefaultCellStyle.BackColor=surface;grid.DefaultCellStyle.ForeColor=text;grid.DefaultCellStyle.SelectionBackColor=accentSoft;grid.DefaultCellStyle.SelectionForeColor=text;grid.AlternatingRowsDefaultCellStyle.BackColor=raised;grid.AlternatingRowsDefaultCellStyle.ForeColor=text;grid.ColumnHeadersDefaultCellStyle.BackColor=raised;grid.ColumnHeadersDefaultCellStyle.ForeColor=muted;grid.RowHeadersDefaultCellStyle.BackColor=raised;grid.RowHeadersDefaultCellStyle.ForeColor=muted;}
   else if(c is TextBox||c is NumericUpDown||c is ComboBox||c is ListBox){c.BackColor=input;c.ForeColor=text;}
   else if(c is TabPage){c.BackColor=surface;c.ForeColor=text;}
   else if(toggle!=null){toggle.OnColor=accent;toggle.OffColor=border;toggle.KnobColor=nightTheme?NightText:Color.White;toggle.DisabledBlendColor=surface;toggle.Invalidate();}
   else if(label!=null){if(label==rndTitleLbl)label.ForeColor=text;else if(label==pidLbl)label.ForeColor=accent;else if(IsThemeAccent(label.ForeColor))label.ForeColor=accent;else if(IsThemeText(label.ForeColor))label.ForeColor=text;else if(IsThemeMuted(label.ForeColor))label.ForeColor=muted;if(label.BackColor!=Color.Transparent)label.BackColor=ParentSurface(label.Parent,page,surface,raised);}
   else if(c is Button){var button=(Button)c;button.BackColor=input;button.ForeColor=text;button.FlatStyle=FlatStyle.Flat;button.FlatAppearance.BorderColor=border;button.FlatAppearance.MouseOverBackColor=raised;button.FlatAppearance.MouseDownBackColor=accentSoft;}
   else if(c is Panel){var panel=(Panel)c;if(panel.Dock==DockStyle.Bottom&&panel.Height==1)panel.BackColor=border;else if(panel.BackColor!=Color.Transparent)panel.BackColor=Convert.ToString(panel.Tag)=="section-card"?raised:ParentSurface(panel.Parent,page,surface,raised);}
   ApplyAppearanceToTree(c,page,surface,raised,input,border,text,muted,accent,accentHover,accentSoft);
  }
 }
 private static Color ParentSurface(Control parent,Color page,Color surface,Color raised){if(parent==null)return page;if(Convert.ToString(parent.Tag)=="section-card")return raised;if(parent is TabPage||parent is Guna2Panel)return surface;return page;}
 private static bool IsThemeAccent(Color c){return c==LightAccent||c==LightAccentHover||c==StudioAccent||c==NightAccent||c==NightAccentHover;}
 private static bool IsThemeText(Color c){return c==LightText||c==StudioText||c==NightText||c==Color.White||c==Color.Black;}
 private static bool IsThemeMuted(Color c){return c==LightMuted||c==StudioMuted||c==NightMuted||c==Color.Gray||c==Color.DimGray;}
 private void ApplyBigMapAppearance(){if(bigMapForm==null||bigMapForm.IsDisposed)return;Color background=nightTheme?Color.FromArgb(8,12,20):LightPage;bigMapForm.BackColor=background;bigMapForm.ForeColor=nightTheme?NightText:LightText;bigMapCanvas.BackColor=background;bigMapCanvas.Invalidate();}
}
