using System;
using System.IO;
using System.Xml;
using Cairo;
using Gtk;

public class PdfWriter
{
    public static void Write(string fileName)
    {
        Diagram diagram = Program.Load(fileName);
        string pdfName = System.IO.Path.ChangeExtension(fileName, "pdf");

        if(diagram != null)
        {
            try
            {
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

                Console.WriteLine("Wrote {0}", pdfName);
            }
            catch
            {
                Console.WriteLine("Error writing {0}", pdfName);
            }
        }
    }
}

public class PsWriter
{
    public static void Write(string fileName)
    {
        Diagram diagram = Program.Load(fileName);
        string psName = System.IO.Path.ChangeExtension(fileName, "ps");

        if(diagram != null)
        {
            try
            {
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

                Console.WriteLine("Wrote {0}", psName);
            }
            catch
            {
                Console.WriteLine("Error writing {0}", psName);
            }
        }
    }
}

public class Program
{
    bool graphical = true;
    bool pdf = false;
    bool ps = false;
    string file_name;

    public static Diagram Load(string file_name)
    {
        DiagramBuilder db = new DiagramBuilder();

        try
        {
            return db.Build(file_name);
        }
        catch
        {
            Console.WriteLine("Could not parse {0} as diagram definition", file_name);
        }

        return null;
    }
    
    Program(string[] args)
    {
        foreach(string arg in args)
        {
            if(arg.Equals("-pdf"))
            {
                pdf = true;
                graphical = false;
            }
            else if(arg.Equals("-ps"))
            {
                ps = true;
                graphical = false;
            }
            else
            {
                file_name = arg;
            }
        }

        if(file_name != null)
        {
            if(pdf)
            {
                PdfWriter.Write(file_name);
            }

            if(ps)
            {
                PsWriter.Write(file_name);
            }

            if(graphical)
            {
                Application.Init ();
                new DiagramWin(file_name);
                Application.Run ();
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
