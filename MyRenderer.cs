// https://stackoverflow.com/questions/32786250/windows-10-styled-contextmenustrip

using System.Drawing.Drawing2D;

public class MyRenderer : ToolStripProfessionalRenderer
{
    public MyRenderer()
        : base(new MyColorTable())
    {
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = Color.White;
        e.TextFont = new Font("Segoe UI", 9, FontStyle.Regular);
        base.OnRenderItemText(e);
    }
}

public class MyColorTable : ProfessionalColorTable
{
    public override Color MenuItemBorder
    {
        get { return Color.FromArgb(255, 65, 65, 65); }
    }
    public override Color MenuItemSelectedGradientBegin
    {
        get { return Color.FromArgb(255, 65, 65, 65); }
    }
    public override Color MenuItemSelectedGradientEnd
    {
        get { return Color.FromArgb(255, 65, 65, 65); }
    }
    public override Color ToolStripDropDownBackground
    {
        get { return Color.FromArgb(255, 45, 46, 46); }
    }
    public override Color ImageMarginGradientBegin
    {
        get { return Color.FromArgb(255, 45, 46, 46); }
    }
    public override Color ImageMarginGradientMiddle
    {
        get { return Color.FromArgb(255, 45, 46, 46); }
    }
    public override Color ImageMarginGradientEnd
    {
        get { return Color.FromArgb(255, 45, 46, 46); }
    }
}