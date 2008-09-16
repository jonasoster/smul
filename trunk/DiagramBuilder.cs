using System;
using System.Xml;
using System.Collections.Generic;

public class DiagramBuilder
{
    void Assert(bool ok)
    {
        if(!ok)
        {
            throw new System.Exception();
        }
    }
    
    public Diagram diagram;

    public DiagramBuilder()
    {
        diagram = null;
    }

    public Diagram Build(string filename)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(filename);
        DiagramFromNode(doc.DocumentElement);

        return diagram;
    }

    void DiagramFromNode(XmlNode node)
    {
        Assert(node.Name.Equals("diagram"));

        diagram = new Diagram();
        foreach(XmlNode child in node.ChildNodes)
        {
            if(child.Name.Equals("elements"))
            {
                ElementsFromNode(child);
            }
            else if(child.Name.Equals("sequence"))
            {
                SequenceFromNode(child);
            }
            else
            {
                Assert(false);
            }
        }
    }

    void ElementsFromNode(XmlNode node)
    {
        foreach(XmlNode child in node.ChildNodes)
        {
            diagram.elements.Add(ElemObjFromNode(child));
        }
    }

    void SequenceFromNode(XmlNode node)
    {
        Step currentStep = new Step();
        foreach(XmlNode child in node.ChildNodes)
        {
            if(child.Name.Equals("activate"))
            {
                currentStep.activations.Add(new Activation(
                        child.Attributes["label"].Value, true));
            }
            else if(child.Name.Equals("deactivate"))
            {
                currentStep.activations.Add(new Activation(
                        child.Attributes["label"].Value, false));
            }
            else if(child.Name.Equals("arrow"))
            {
                ArrowKind type =
                    ArrowKindFromString(child.Attributes["type"].Value);
                string text;

                if(child.ChildNodes.Count != 0)
                {
                    text = child.ChildNodes[0].Value;
                }
                else
                {
                    text = "";
                }
            
                currentStep.arrows.Add(new Arrow(
                        child.Attributes["from"].Value,
                        child.Attributes["to"].Value,
                        type,
                        text));                
            }
            else if(child.Name.Equals("step"))
            {
                if((child.Attributes == null) || (child.Attributes["amount"] == null))
                {
                }
                else
                {
                    int amount = System.Int32.Parse(child.Attributes["amount"].Value);
                    currentStep.amount = amount;
                }

                diagram.sequence.Add(currentStep);
                currentStep = new Step();
            }
        }

        diagram.sequence.Add(currentStep);
    }

    ElemObj ElemObjFromNode(XmlNode node)
    {
        Assert(node.Name.Equals("obj"));
        Assert(node.Attributes["label"] != null);
        Assert(!node.Attributes["label"].Value.Equals(""));

        Console.WriteLine("InnerText: {0}", node.InnerXml);
        
        return new ElemObj(
            node.Attributes["label"].Value,
            node.InnerXml);
    }

    ArrowKind ArrowKindFromString(string name)
    {
        switch(name)
        {
        case "call":
            return ArrowKind.Call;
        case "async":
            return ArrowKind.Async;
        case "return":
            return ArrowKind.Return;
        }
        Assert(false);
        return ArrowKind.Call;
    }
}
