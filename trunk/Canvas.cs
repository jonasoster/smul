using System;
using Gtk;
using Cairo;

public abstract class CanvasItem
{
    private double x0;
    public double X0 {
        get { return x0; }
        set { x0 = value; }
    }
    
    private double y0;
    public double Y0 {
        get { return y0; }
        set { y0 = value; }
    }
    
    private double x1;
    public double X1
    {
        get { return x1; }
        set { x1 = value; }
    }
    
    private double y1;
    public double Y1 {
        get { return y1; }
        set { y1 = value; }
    }
    
    abstract public void Layout(Cairo.Context cr);
    abstract public void Draw(Cairo.Context cr);

    public CanvasItem() {}
}

public class Box : CanvasItem
{
    public Box() {}
    
    override public void Layout(Cairo.Context cr) {}
    override public void Draw(Cairo.Context cr)
    {
        cr.Save();
        cr.Color = new Color(1.0, 1.0, 1.0);
        cr.Rectangle(X0, Y0, X1 - X0, Y1 - Y0);
        cr.Fill();
        cr.Color = new Color(0.0, 0.0, 0.0);
        cr.Rectangle(X0, Y0, X1 - X0, Y1 - Y0);
        cr.Stroke();
        cr.Restore();
    }
}

public class TextBox : CanvasItem
{
    private double y_top;
    public double YTop {
        get { return y_top; }
        set { y_top = value; }
    }

    private double x_center;
    public double XCenter {
        get { return x_center; }
        set { x_center = value; }
    }

    private string text;
    public string Text {
        get { return text; }
        set { text = value; }
    }

    private double width;
    public double Width {
        get { return width; }
        set { width = value; }
    }

    private double border_width;
    public double BorderWidth {
        get { return border_width; }
        set { border_width = value; }
    }
        
    public TextBox() {}

    Pango.Layout layout;
    override public void Layout(Cairo.Context cr)
    {
        layout = Pango.CairoHelper.CreateLayout(cr);
        Pango.FontDescription desc = Pango.FontDescription.FromString("sans 6");
        layout.FontDescription = desc;
        layout.SetText(Text);
        layout.Width = (int)(Pango.Scale.PangoScale*(Width - 2*BorderWidth));
        layout.Alignment = Pango.Alignment.Center;

        int layoutWidth, layoutHeight;
        layout.GetSize (out layoutWidth, out layoutHeight);
        double textHeight = (double)layoutHeight / Pango.Scale.PangoScale; 
        double textWidth = (double)layoutWidth / Pango.Scale.PangoScale;

        if(textWidth < Width)
        {
            textWidth = Width;
        }
        
        X0 = XCenter - textWidth/2.0 - BorderWidth;
        X1 = XCenter + textWidth/2.0 + BorderWidth;
        Y0 = YTop;
        Y1 = YTop + textHeight + 2.0*BorderWidth;
    }

    override public void Draw(Cairo.Context cr)
    {
        cr.MoveTo(X0 + BorderWidth, Y0 + BorderWidth);
        Pango.CairoHelper.ShowLayout(cr, layout);
        cr.Rectangle(X0, Y0, X1 - X0, Y1 - Y0);
        cr.Stroke();
    }
}

public class TextArrow : CanvasItem
{
    private string text;
    public string Text {
        get { return text; }
        set { text = value; }
    }

    private ArrowKind arrow_kind;
    public ArrowKind ArrowKind {
        get { return arrow_kind; }
        set { arrow_kind = value; }
    }

    private double x_text;
    public double XText {
        get { return x_text; }
        set { x_text = value; }
    }

    override public void Layout(Cairo.Context cr)
    {
        Y1 = Y0;
    }

    override public void Draw(Cairo.Context cr)
    {
        int dir = X1 > X0 ? 1 : -1;

        cr.Save();

        cr.SetFontSize(6);
        if(arrow_kind == ArrowKind.Return)
        {
            double[] returnDash = new double[]{ 3.0, 3.0 };
            cr.SetDash(returnDash, 0);
        }
        
        cr.MoveTo(X0, Y0);
        cr.LineTo(X1, Y1);
        cr.Stroke();

        cr.MoveTo(X1 - dir*7, Y1 - 3);
        cr.LineTo(X1, Y1);
        cr.LineTo(X1 - dir*7, Y1 + 3);
        if(arrow_kind == ArrowKind.Async)
        {
            cr.Stroke();
        }
        else
        {
            cr.MoveTo(X1 - dir*7, Y1 - 3);
            cr.Fill();
        }

        if((text != null) && !text.Equals(""))
        {
            if(dir > 0)
            {
                cr.MoveTo(XText + 5, Y0 - 5);
                cr.ShowText(text);
            }
            else
            {
                TextExtents te = cr.TextExtents(text);
                cr.MoveTo(XText - 5 - te.Width, Y0 - 5);
                cr.ShowText(text);
            }
        }
        cr.Restore();
    }
}

public class SelfTextArrow : TextArrow
{
    private double x0b;
    public double X0b {
        get { return x0b; }
        set { x0b = value; }
    }
        
    override public void Layout(Cairo.Context cr)
    {
    }

    override public void Draw(Cairo.Context cr)
    {
        cr.Save();

        cr.SetFontSize(6);
        if(ArrowKind == ArrowKind.Return)
        {
            double[] returnDash = new double[]{ 3.0, 3.0 };
            cr.SetDash(returnDash, 0);
        }
        
        cr.MoveTo(X0, Y0);
        cr.LineTo(X1, Y0);
        cr.LineTo(X1, Y1);
        cr.LineTo(X0b, Y1);
        cr.Stroke();

        cr.MoveTo(X0b + 7, Y1 - 3);
        cr.LineTo(X0b, Y1);
        cr.LineTo(X0b + 7, Y1 + 3);
        if(ArrowKind == ArrowKind.Async)
        {
            cr.Stroke();
        }
        else
        {
            cr.MoveTo(X0b + 7, Y1 - 3);
            cr.Fill();
        }

        if((Text != null) && !Text.Equals(""))
        {
            cr.MoveTo(XText + 5, Y0 - 5);
            cr.ShowText(Text);
        }
        cr.Restore();
    }
}
