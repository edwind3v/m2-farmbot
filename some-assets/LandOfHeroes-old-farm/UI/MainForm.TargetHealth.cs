using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm
{
    private TargetHealthView movementTargetHealth,dungeonTargetHealth;
    private System.Windows.Forms.Timer targetHealthTimer;
    private string TargetHealthPath { get { return Path.Combine(runtimeFolder,"target-health.json"); } }
    private void InitializeTargetHealth()
    {
        movementTargetHealth=AddTargetHealthView(tabMain,"movementTargetHealth");
        dungeonTargetHealth=AddTargetHealthView(taskPage,"dungeonTargetHealth");
        targetHealthTimer=new System.Windows.Forms.Timer(components){Interval=200};
        targetHealthTimer.Tick+=(s,e)=>RefreshTargetHealth();targetHealthTimer.Start();sessionDirty=true;
    }
    private TargetHealthView AddTargetHealthView(TabPage page,string name)
    {
        foreach(Control child in page.Controls)child.Top+=88;
        var view=new TargetHealthView{Name=name,Left=12,Top=6,Width=Math.Max(300,page.ClientSize.Width-32),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right,Font=new Font("Segoe UI",9f),BackColor=page.BackColor,ForeColor=page.ForeColor};
        page.Controls.Add(view);view.BringToFront();page.AutoScroll=true;
        page.AutoScrollMinSize=new Size(0,Math.Max(page.AutoScrollMinSize.Height+88,page.Controls.Cast<Control>().Max(c=>c.Bottom)+16));
        return view;
    }
    private void RefreshTargetHealth()
    {
        TargetHealthSnapshot snapshot=null;
        try
        {
            var process=memory.Field_0197?.Property_0268;
            if(connectionRequested&&clientConnected&&process!=null&&!process.HasExited&&File.Exists(TargetHealthPath))
                snapshot=TargetHealthSnapshot.Parse(File.ReadAllText(TargetHealthPath),process.Id,atlasName,DateTime.UtcNow);
        }
        catch(IOException){}catch(InvalidOperationException){}
        movementTargetHealth.ShowHealth(snapshot);dungeonTargetHealth.ShowHealth(snapshot);
        routeTargetHealth?.ShowHealth(snapshot);
    }
}
