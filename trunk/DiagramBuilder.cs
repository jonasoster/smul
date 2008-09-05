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
        foreach(XmlNode child in node.ChildNodes)
        {
            diagram.sequence.Add(SeqObjFromNode(child));
        }
    }

    ElemObj ElemObjFromNode(XmlNode node)
    {
        Assert(node.Name.Equals("obj"));
        Assert(node.Attributes["label"] != null);
        Assert(!node.Attributes["label"].Value.Equals(""));

        return new ElemObj(
            node.Attributes["label"].Value,
            node.ChildNodes[0].Value);
    }

    SeqObj SeqObjFromNode(XmlNode node)
    {
        if(node.Name.Equals("step"))
        {
            if((node.Attributes == null) || (node.Attributes["amount"] == null))
            {
                return new Step(0);
            }
            else
            {
                int amount = System.Int32.Parse(node.Attributes["amount"].Value);
                return new Step(amount);
            }
        }
        else if(node.Name.Equals("activate"))
        {
            return new Activation(
                node.Attributes["label"].Value, true);
        }
        else if(node.Name.Equals("deactivate"))
        {
            return new Activation(
                node.Attributes["label"].Value, false);
        }
        else if(node.Name.Equals("arrow"))
        {
            ArrowKind type =
                ArrowKindFromString(node.Attributes["type"].Value);
            string text;

            if(node.ChildNodes.Count != 0)
            {
                text = node.ChildNodes[0].Value;
            }
            else
            {
                text = "";
            }
            
            return new Arrow(
                node.Attributes["from"].Value,
                node.Attributes["to"].Value,
                type,
                text);                
        }
        else
        {
            Assert(false);
            return null;
        }
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
