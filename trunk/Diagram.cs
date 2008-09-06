using System;
using System.Collections.Generic;

public class ElemObj
{
    public string label;
    public string text;

    public ElemObj(string label, string text)
    {
//         Console.WriteLine("ElemObj {0} {1}", label, text);
        this.label = label;
        this.text = text;
    }
}

public class SeqObj
{
    public SeqObj()
    {
    }
}

public class Step : SeqObj
{
    public List<Arrow> arrows;
    public List<Activation> activations;
    public int amount;

    public Step()
    {
        amount = 0;
        activations = new List<Activation>();
        arrows = new List<Arrow>();
    }
}

public class Activation : SeqObj
{
    public string label;
    public bool on;

    public Activation(string label, bool on)
    {
//         Console.WriteLine("Activation {0} {1}", label, on);
        this.label = label;
        this.on = on;
    }
}

public enum ArrowKind { Call, Async, Return };

public class Arrow : SeqObj
{
    public string from;
    public string to;
    public ArrowKind type;
    public string text;
    
    public Arrow(string from, string to, ArrowKind type, string text)
    {
//         Console.WriteLine("Arrow {0} {1} {2} {3}", from, to, type, text);
        this.from = from;
        this.to = to;
        this.type = type;
        this.text = text;
    }
}

public class Diagram
{
    public List<ElemObj> elements;
    public List<Step> sequence;

    public Diagram()
    {
        elements = new List<ElemObj>();
        sequence = new List<Step>();
    }
}
