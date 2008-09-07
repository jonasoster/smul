using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Cairo;
using Gtk;

class DiagramWin : DrawingArea
{
    Diagram diagram;
    
    public DiagramWin (string file_name)
    {
        diagram = Program.Load(file_name);

        Window win = new Window ("Smul");
        win.SetDefaultSize (595, 400);
        win.DeleteEvent += new DeleteEventHandler (OnQuit);
        win.Add (this);
        win.ShowAll ();
    }
    
    void Draw (Context cr, int width, int height)
    {
        cr.LineWidth = 1.0;
        cr.Color = new Color(1.0, 1.0, 1.0);
        cr.Rectangle(0, 0, width, height);
        cr.Fill();

        if(diagram != null)
        {
            DiagramRenderer dr = new DiagramRenderer(cr, width, height, diagram);
            cr.Color = new Color(0.0, 0.0, 0.0);
            dr.Render();
            dr.Draw();
        }
    }

    protected override bool OnExposeEvent (Gdk.EventExpose e)
    {
        using(Context cr = Gdk.CairoHelper.Create (e.Window))
        {
            int w, h;
            e.Window.GetSize (out w, out h);
            Draw (cr, w, h);
        }
        return true;
    }

    void OnQuit (object sender, DeleteEventArgs e)
    {
        Application.Quit ();
    }
}
