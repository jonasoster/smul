using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Cairo;
using Gtk;

class DiagramArea : DrawingArea
{
    private Diagram diagram;
    public Diagram Diagram {
        get { return diagram; }
        set
        {
            diagram = value;
            QueueDraw();
        }
    }

    private int error_counter;
    public int ErrorCounter {
        get { return error_counter; }
    }
    
    void Draw (Context cr, int width, int height)
    {
        cr.LineWidth = 1.0;
        cr.Color = new Color(1.0, 1.0, 1.0);
        cr.Rectangle(0, 0, width, height);
        cr.Fill();

        try
        {
            if(diagram != null)
            {
                DiagramRenderer dr = new DiagramRenderer(cr, width, height, diagram);
                cr.Color = new Color(0.0, 0.0, 0.0);
                dr.Render();
                dr.Draw();
            }
        }
        catch
        {
            error_counter++;
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
}

class DiagramWin : Window
{
    string fileName;
    DiagramArea da;
    bool auto_reload = true;
    
    MenuBar CreateMenuBar()
    {
        MenuBar mb = new MenuBar();
        AccelGroup agrp = new AccelGroup();
        AddAccelGroup(agrp);
        
        Menu fileMenu = new Menu();
        MenuItem item = new MenuItem("_File");
        item.Submenu = fileMenu;
        mb.Append(item);

        item = new ImageMenuItem(Stock.Quit, agrp);
        item.Activated += OnMenuQuit;
        fileMenu.Append(item);
        
        Menu viewMenu = new Menu();
        item = new MenuItem("_View");
        item.Submenu = viewMenu;
        mb.Append(item);

        item = new MenuItem("_Reload");
        item.Activated += OnReload;
        viewMenu.Append(item);

        CheckMenuItem cmi = new CheckMenuItem("_Automatic reload");
        cmi.Active = true;
        cmi.Toggled += OnToggleAuto;
        viewMenu.Append(cmi);

        return mb;
    }
    
    public DiagramWin (string file_name) : base(file_name + " - smul")
    {
        this.fileName = file_name;
        Diagram diagram = Program.Load(file_name);
        
        VBox v = new VBox();
        MenuBar mb = CreateMenuBar();
        da = new DiagramArea();
        da.Diagram = diagram;
        SetDefaultSize (600, 600);
        DeleteEvent += new DeleteEventHandler (OnQuit);
        v.PackStart(mb, false, false, 0);
        v.PackStart(da, true, true, 0);
        Add(v);
        ShowAll();
        WatchFile();
    }

    void WatchFile()
    {
        string fullPath = System.IO.Path.GetFullPath(fileName);
        string dirName = System.IO.Path.GetDirectoryName(fullPath);
        string baseName = System.IO.Path.GetFileName(fullPath);
        FileSystemWatcher fsw = new FileSystemWatcher(dirName);
        fsw.Filter = baseName;

        fsw.Changed += OnFileChanged;
        fsw.EnableRaisingEvents = true;
    }

    void OnToggleAuto(object o, EventArgs e)
    {
        auto_reload = !auto_reload;
    }
    
    void OnFileChanged(object o, FileSystemEventArgs e)
    {
        // Do the work in the Gtk main loop thread
        GLib.Idle.Add(new GLib.IdleHandler(FileChangedIdleHandler));
    }
    
    void OnQuit(object sender, DeleteEventArgs e)
    {
        Application.Quit();
    }

    void OnMenuQuit(object o, EventArgs e)
    {
        Application.Quit();
    }

    void Reload()
    {
        Diagram diagram = Program.Load(fileName);
        da.Diagram = diagram;
    }

    bool FileChangedIdleHandler()
    {
        if(auto_reload)
        {
            Reload();
        }
        return false;
    }
    
    void OnReload(object o, EventArgs e)
    {
        Reload();
    }
}
