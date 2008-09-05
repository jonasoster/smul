using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Cairo;
using Gtk;


class DiagramWin : DrawingArea
{
    Diagram diagram;
    
    public DiagramWin (Diagram diagram)
    {
        this.diagram = diagram;
        Window win = new Window ("Smul");
        win.SetDefaultSize (595, 400);
        win.DeleteEvent += new DeleteEventHandler (OnQuit);
        win.Add (this);
        win.ShowAll ();
    }
    
    void Draw (Context cr, int width, int height)
    {
        DiagramRenderer dr = new DiagramRenderer(cr, width, height, diagram);
        
        cr.LineWidth = 1.0;
        cr.Color = new Color(1.0, 1.0, 1.0);
        cr.Rectangle(0, 0, width, height);
        cr.Fill();
            
        cr.Color = new Color(0.0, 0.0, 0.0);
        dr.Render();
        dr.Draw();
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

public class Program
{
    bool graphical = false;
    bool pdf = false;
    bool ps = false;
    string file_name;
    Diagram diagram;
    
    Program(string[] args)
    {
        foreach(string arg in args)
        {
            if(arg.Equals("-graphical"))
            {
                graphical = true;
            }
            else if(arg.Equals("-pdf"))
            {
                pdf = true;
            }
            else if(arg.Equals("-ps"))
            {
                ps = true;
            }
            else
            {
                file_name = arg;
            }
        }

        if(file_name != null)
        {
            DiagramBuilder db = new DiagramBuilder();

            try
            {
                diagram = db.Build(file_name);
            }
            catch
            {
                Console.WriteLine("Could not parse {0} as diagram definition", file_name);
            }

            if(diagram != null)
            {
                if(pdf)
                {
                    string pdfName = System.IO.Path.ChangeExtension(file_name, "pdf");
                    
                    using(PdfSurface pdfs = new PdfSurface(pdfName, 595, 842))
                    {
                        using(Context cr = new Context(pdfs))
                        {
                            DiagramRenderer dr = new DiagramRenderer(cr, 0, 0, diagram);
        
                            cr.LineWidth = 1.0;
                            cr.Color = new Color(0.0, 0.0, 0.0);
                            dr.Render();
                            pdfs.SetSize(dr.width, dr.height);                            
                            dr.Draw();

                            cr.ShowPage();
                        }
                    }
                }

                if(ps)
                {
                    string psName = System.IO.Path.ChangeExtension(file_name, "ps");

                    // It seems the page size cannot be made smaller
                    // using SetSize(), only larger. Start with a
                    // small surface and enlarge it after layout.
                    using(PSSurface pss = new PSSurface(psName, 10, 10))
                    {
                        using(Context cr = new Context(pss))
                        {
                            DiagramRenderer dr = new DiagramRenderer(cr, 0, 0, diagram);
        
                            cr.LineWidth = 1.0;
                            cr.Color = new Color(0.0, 0.0, 0.0);
                            dr.Render();
                            pss.SetSize(dr.width, dr.height);                            
                            dr.Draw();

                            cr.ShowPage();
                        }
                    }
                }

                if(graphical)
                {
                    Application.Init ();
                    new DiagramWin(diagram);
                    Application.Run ();
                }
            }
        }
    }
    
    public static void Main(string[] args)
    {
        new Program(args);
    }

//     public static void PrintNode(XmlNode node, int depth)
//     {
//         string indent = new string(' ', depth*2);
//         Console.WriteLine("{0}{1} {2} {3}",
//             indent, node.NodeType, node.Name, node.Value);

//         if(node.Attributes != null)
//         {
//             foreach(XmlNode attr in node.Attributes)
//             {
//                 PrintNode(attr, depth+3);
//             }
//         }
        
//         foreach(XmlNode child in node.ChildNodes)
//         {
//             PrintNode(child, depth+1);
//         }
//     }
}
