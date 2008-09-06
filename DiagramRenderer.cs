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
        Stack<Box> activeBoxes;

        private double x_left_before;
        public double XLeftBefore {
            get { return x_left_before; }
        }

        private double x_left_after;
        public double XLeftAfter {
            get { return x_left_after; }
        }

        private double x_right_before;
        public double XRightBefore {
            get { return x_right_before; }
        }

        private double x_right_after;
        public double XRightAfter {
            get { return x_right_after; }
        }
        
        double GetXLeft()
        {
            if(activeBoxes.Count != 0)
            {
                return activeBoxes.Peek().X0;
            }
            else
            {
                return x;
            }
        }

        double GetXRight()
        {
            if(activeBoxes.Count != 0)
            {
                return activeBoxes.Peek().X1;
            }
            else
            {
                return x;
            }
        }

        public double XClearLeft {
            get { return x - 3; }
        }

        public double XClearRight {
            get { return GetXRight(); }
        }
        
        public LifeLine(string label, DiagramRenderer dr, double x, double y0)
        {
            this.label = label;
            this.dr = dr;
            this.x = x;
            this.y0 = y0;
            boxes = new List<CanvasItem>();
            activeBoxes = new Stack<Box>();
        }

        public void Activate()
        {
            Box box = new Box();
            box.Y0 = dr.YCurrent;
            box.X0 = GetXRight() - 3;
            box.X1 = GetXRight() + 3;
            activeBoxes.Push(box);
        }

        public void Deactivate()
        {
            if(activeBoxes.Count != 0)
            {
                Box box = activeBoxes.Pop();
                box.Y1 = dr.YCurrent;
                boxes.Add(box);
            }
        }

        public void BeforeActivations()
        {
            x_right_before = GetXRight();
            x_left_before = GetXLeft();
        }

        public void AfterActivations()
        {
            x_right_after = GetXRight();
            x_left_after = GetXLeft();
        }
        
        public void End()
        {
            y1 = dr.YCurrent;
            boxes.Reverse();
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
        double y_step = 25;
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

        foreach(Step step in diagram.sequence)
        {
            if(step.amount == 0)
            {
                step.amount = (int)y_step;
            }

            YCurrent += step.amount;

            foreach(LifeLine ll in lifeLines)
            {
                ll.BeforeActivations();
            }

            foreach(Activation a in step.activations)
            {
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

            foreach(LifeLine ll in lifeLines)
            {
                ll.AfterActivations();
            }

            foreach(Arrow arrow in step.arrows)
            {
                TextArrow ta;
                LifeLine to = llDict[arrow.to];
                LifeLine from = llDict[arrow.from];

                if(to != from)
                {
                    ta = new TextArrow();
                    bool right = to.X > from.X;
                    ta.Y0 = YCurrent;
                    if(right)
                    {
                        ta.X0 = from.XRightAfter;
                        ta.X1 = to.XLeftAfter;
                        ta.XText = from.XClearRight;
                    }
                    else
                    {
                        ta.X0 = from.XLeftAfter;
                        ta.X1 = to.XRightAfter;
                        ta.XText = from.XClearLeft;
                    }
                }
                else
                {
                    SelfTextArrow sta = new SelfTextArrow();
                    sta.Y0 = YCurrent - 10;
                    sta.Y1 = YCurrent;
                    sta.X0 = from.XRightBefore;
                    sta.X0b = from.XRightAfter;
                    sta.X1 = from.XRightBefore + 30;
                    sta.XText = from.XClearRight;
                    ta = sta;
                }

                ta.Text = arrow.text;
                ta.ArrowKind = arrow.type;
                ta.Layout(cr);
                items.Add(ta);
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
