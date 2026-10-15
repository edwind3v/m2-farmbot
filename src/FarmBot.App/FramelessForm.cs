namespace FarmBot;

public class FramelessForm : Form
{
    protected override CreateParams CreateParams
    {
        get
        {
            const int wsThickframe = 0x00040000;
            var cp = base.CreateParams;
            cp.Style |= wsThickframe;
            return cp;
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        FitMaximizeBounds();
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        if (IsHandleCreated)
            FitMaximizeBounds();
    }

    private void FitMaximizeBounds()
    {
        MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
    }
}
