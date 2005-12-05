// $Id$

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

using PlayOnline.Core;

namespace PlayOnline.FFXI {

  public class Macro {

    public Macro() : this(String.Empty) { }

    public Macro(string Name) {
      this.Name_ = Name;
      this.Commands_ = new string[6];
    }

    public void Clear() {
      this.Name_ = String.Empty;
      this.Commands_ = new string[6];
    }

    public Macro Clone() {
    Macro M = new Macro(this.Name_);
      for (int i = 0; i < 6; ++i)
	M.Commands_[i] = this.Commands_[i];
      return M;
    }

    #region Properties

    public string Name {
      get { return this.Name_;  }
      set { this.Name_ = value; }
    }

    public string[] Commands {
      get { return this.Commands_; }
    }

    public bool Empty {
      get {
	if (this.Name_ != null && this.Name_ != String.Empty)
	  return false;
	foreach (string Command in this.Commands_) {
	  if (Command != null && Command != String.Empty)
	    return false;
	}
	return true;
      }
    }

    #region Private Data

    private string   Name_;
    private string[] Commands_;

    #endregion

    #endregion

    #region MacroBar Access

    internal static Macro ReadFromMacroBar(BinaryReader BR, Encoding E) {
    Macro M = new Macro();
      if (BR != null) {
	BR.ReadInt32(); // Unknown
	for (int i = 0; i < 6; ++i) { // 6 Lines of text, 61 bytes each, null-terminated shift-jis
	string Command = "";
	  Command = E.GetString(BR.ReadBytes(61));
	  M.Commands_[i] = Command.TrimEnd('\0');
	}
	M.Name_ = E.GetString(BR.ReadBytes(10)).TrimEnd('\0');
      }
      return M;
    }

    internal void WriteToMacroBar(BinaryWriter BW, Encoding E) {
      BW.Write((uint) 0);
      for (int i = 0; i < 6; ++i) // 6 Lines of text, 61 bytes each, nul-terminated shift-jis
	this.WriteEncodedString(BW, this.Commands_[i], E, 61);
      this.WriteEncodedString(BW, this.Name_, E, 10);
    }

    private void WriteEncodedString(BinaryWriter BW, string Text, Encoding E, int Bytes) {
    ArrayList OutBytes = new ArrayList(Bytes);
      if (Text == null)
	Text = String.Empty;
      OutBytes.AddRange(E.GetBytes(Text));
      while (OutBytes.Count > Bytes)
	OutBytes.RemoveAt(Bytes);
      while (OutBytes.Count < Bytes)
	OutBytes.Add((byte) 0);
      BW.Write((byte[]) OutBytes.ToArray(typeof(byte)));
    }

    #endregion

    #region XML Access

    internal static Macro LoadFromXml(XmlElement MacroNode) {
    Macro M = new Macro();
      if (MacroNode.Attributes["name"] != null)
	M.Name_ = MacroNode.Attributes["name"].InnerText;
    Encoding E = new FFXIEncoding();
      for (int i = 0; i < 6; ++i) {
      XmlNode CommandNode = MacroNode.SelectSingleNode(String.Format("command[@line = {0}]", i + 1));
	if (CommandNode != null && CommandNode is XmlElement) {
	string CommandText = String.Empty;
	  foreach (XmlNode XN in CommandNode.ChildNodes) {
	    if (XN is XmlText)
	      CommandText += XN.InnerText;
	    // Backwards compatibility - to be removed
	    else if (XN is XmlElement && XN.Name == "autotrans") {
	    ushort Category = 0; try { XmlAttribute XCat   = XN.Attributes["category"]; Category = ushort.Parse(XCat.InnerText);   } catch {}
	    byte   Group    = 0; try { XmlAttribute XGroup = XN.Attributes["group"];    Group    =   byte.Parse(XGroup.InnerText); } catch {}
	    byte   ID       = 0; try { XmlAttribute XID    = XN.Attributes["id"];       ID       =   byte.Parse(XID.InnerText);    } catch {}
	    byte[] ResourceMarker = new byte[] { 0xFD, (byte) (Category & 0xff), (byte) (Category >> 8), Group, ID, 0xFD };
	      CommandText += E.GetString(ResourceMarker);
	    }
	  }
	  M.Commands_[i] = CommandText;
	}
      }
      return M;
    }

    internal void WriteToXml(XmlDocument XDoc, XmlNode Parent) {
    XmlElement XMacro = XDoc.CreateElement("macro");
      if (this.Name_ != null && this.Name_ != String.Empty) {
      XmlAttribute XName = XDoc.CreateAttribute("name");
	XName.InnerText = this.Name_;
	XMacro.Attributes.Append(XName);
      }
      for (int i = 0; i < 6; ++i) {
	if (this.Commands_[i] != null && this.Commands_[i] != String.Empty) {
	XmlElement XCommand = XDoc.CreateElement("command");
	  {
	  XmlAttribute XLine = XDoc.CreateAttribute("line");
	    XLine.InnerText = String.Format("{0}", i + 1);
	    XCommand.Attributes.Append(XLine);
	  }
	  XCommand.AppendChild(XDoc.CreateTextNode(this.Commands_[i]));
	  XMacro.AppendChild(XCommand);
	}
      }
      Parent.AppendChild(XMacro);
    }

    #endregion

  }

}
