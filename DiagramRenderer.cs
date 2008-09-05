using System;
using System.Xml;
using System.Collections.Generic;
using Cairo;

public class DiagramRenderer
{
    Context cr;
    public double width;
    public double height;
    Diagram diagram;
    
    public DiagramRenderer(
        Context cr,
        double width,
        double height,
        Diagram diagram)
    {
        this.cr =  cr;
        this.width = width;
        this.height = height;
        this.diagram = diagram;        
    }

    private double y_current;
    public double YCurrent {
        get { return y_current; }
        set { y_current = value; }
    }

    class LifeLine
    {
        public string label;
        private double x;
        public double X {
            get { return x; }
            set { x = value; }
        }
        double y0, y1;
        DiagramRenderer dr;
        List<CanvasItem> boxes;
        Box box;

        public double XLeft {
            get
            {
                if(box != null)
                {
                    return box.X0;
                }
                else
                {
                    return x;
                }
            }
        }

        public double XRight {
            get
            {
                if(box != null)
                {
                    return box.X1;
                }
                else
                {
                    return x;
                }
            }
        }
        
        public LifeLine(string label, DiagramRenderer dr, double x, double y0)
        {
            this.label = label;
            this.dr = dr;
            this.x = x;
            this.y0 = y0;
            boxes = new List<CanvasItem>();
//             Console.WriteLine("ll {0} {1} {2}",
//                 label, x, y0);
        }

        public void Activate()
        {
            box = new Box();
            box.Y0 = dr.YCurrent;
            box.X0 = X - 3;
            box.X1 = X + 3;
        }

        public void Deactivate()
        {
            if(box != null)
            {
                box.Y1 = dr.YCurrent;
                boxes.Add(box);
                box = null;
            }
        }
        
        public void End()
        {
            y1 = dr.YCurrent;

//             Console.WriteLine("ll {0} {1}",
//                 label, y1);
        }

        public void Draw()
        {
            dr.cr.Save();
            double[] llDash = new double[]{ 10.0, 5.0 };
            dr.cr.SetDash(llDash, 0);
            dr.cr.MoveTo(x, y1);
            dr.cr.LineTo(x, y0);
            dr.cr.Stroke();
            dr.cr.Restore();

            foreach(CanvasItem item in boxes)
            {
                item.Draw(dr.cr);
            }
        }
    }
    
    List<CanvasItem> items;
    List<LifeLine> lifeLines;
    
    public void Render()
    {
        items = new List<CanvasItem>();
        lifeLines = new List<LifeLine>();
        Dictionary<string, LifeLine> llDict = new Dictionary<string, LifeLine>();
        double x_where = 50.5;
        double y_step = 20;
        y_current = 0;

        width = 0;
        height = 0;
        
        foreach(ElemObj elem in diagram.elements)
        {
            TextBox tb = new TextBox();
            tb.YTop = 10.5;
            tb.XCenter = x_where;
            x_where += 100;
            width += 100;
            tb.Width = 70;
            tb.BorderWidth = 3;
            tb.Text = elem.text;

            tb.Layout(cr);
            items.Add(tb);
            LifeLine ll = new LifeLine(elem.label, this, tb.XCenter, tb.Y1);
            lifeLines.Add(ll);
            llDict.Add(elem.label, ll);
            if(tb.Y1 > YCurrent)
            {
                YCurrent = tb.Y1;
            }
        }

        YCurrent += y_step;

        foreach(SeqObj seqobj in diagram.sequence)
        {
            // Todo: learn the visitor pattern
            if(seqobj is Arrow)
            {
                Arrow arrow = (Arrow)seqobj;
                TextArrow ta = new TextArrow();
                LifeLine to = llDict[arrow.to];
                LifeLine from = llDict[arrow.from];
                bool right = to.X > from.X;
                ta.Y0 = YCurrent;
                if(right)
                {
                    ta.X0 = from.XRight;
                    ta.X1 = to.XLeft;
                }
                else
                {
                    ta.X0 = from.XLeft;
                    ta.X1 = to.XRight;
                }
                ta.Text = arrow.text;
                ta.ArrowKind = arrow.type;
                ta.Layout(cr);
                items.Add(ta);
            }
            else if(seqobj is Activation)
            {
                Activation a = (Activation)seqobj;
                LifeLine ll = llDict[a.label];

                if(a.on)
                {
                    ll.Activate();
                }
                else
                {
                    ll.Deactivate();
                }
            }
            else
                if(seqobj is Step)
            {
                Step step = (Step)seqobj;

                if(step.amount == 0)
                {
                    YCurrent += y_step;                
                }
                else
                {
                    YCurrent += step.amount;
                }
            }
        }

        foreach(LifeLine ll in lifeLines)
        {
            ll.End();
        }

        height = YCurrent;
    }
    
    public void Draw()
    {
        foreach(LifeLine ll in lifeLines)
        {
            ll.Draw();
        }

        foreach(CanvasItem item in items)
        {
            item.Draw(cr);
        }
    }
}
