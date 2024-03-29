// $Id$

// Copyright � 2004-2010 Tim Van Holder
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS"
// BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;

using PlayOnline.Core;

namespace PlayOnline.FFXI.Things {

  public class SimpleStringTableEntry : Thing {

    public SimpleStringTableEntry() {
      // Clear fields
      this.Clear();
    }

    public override string ToString() {
      return this.Text_;
    }

    public override List<PropertyPages.IThing> GetPropertyPages() {
      return base.GetPropertyPages();
    }

    #region Fields

    public static List<string> AllFields {
      get {
	return new List<string>(new string[] {
	  "id",
	  "text",
	});
      }
    }

    public override List<string> GetAllFields() {
      return SimpleStringTableEntry.AllFields;
    }

    #region Data Fields

    private uint?  ID_;
    private string Text_;
    
    #endregion

    public override void Clear() {
      this.ID_   = null;
      this.Text_ = null;
    }

    #endregion

    #region Field Access

    public override bool HasField(string Field) {
      switch (Field) {
	case "id":   return this.ID_.HasValue;
	case "text": return (this.Text_ != null);
	default:     return false;
      }
    }

    public override string GetFieldText(string Field) {
      switch (Field) {
	case "id":   return (!this.ID_.HasValue ? String.Empty : String.Format("{0:00000}", this.ID_.Value));
	case "text": return this.Text_;
	default:     return null;
      }
    }

    public override object GetFieldValue(string Field) {
      switch (Field) {
	case "id":   return (!this.ID_.HasValue ? null : (object) this.ID_.Value);
	case "text": return this.Text_;
	default:     return null;
      }
    }

    protected override void LoadField(string Field, System.Xml.XmlElement Node) {
      switch (Field) {
	case "id":   this.ID_   = (uint) this.LoadUnsignedIntegerField(Node); break;
	case "text": this.Text_ =        this.LoadTextField           (Node); break;
      }
    }

    #endregion

    #region ROM File Reading

    public bool Read(BinaryReader BR) {
      this.Clear();
      try {
	this.ID_ = BR.ReadUInt32();
        // Read entry
      byte[] TextBytes = BR.ReadBytes(0x3b);
	this.Text_ = String.Empty;
	// Trim trailing NULs
      int ByteCount = 0x3b;
        while (TextBytes[ByteCount - 1] == 0x00)
          --ByteCount;
        // Process the message
      FFXIEncoding E = new FFXIEncoding();
      int LastPos = 0;
	for (int i = 0; i < ByteCount; ++i) {
	  if (TextBytes[i] == 0x07) { // Line Break
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += "\r\n";
	    LastPos = i + 1;
	  }
	  else if (TextBytes[i] == 0x08) { // Character Name (You)
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Player Name{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    LastPos = i + 1;
	  }
	  else if (TextBytes[i] == 0x09) { // Character Name (They)
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Speaker Name{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    LastPos = i + 1;
	  }
	  else if (TextBytes[i] == 0x0a && i + 1 < TextBytes.Length) {
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Numeric Parameter {2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1]);
	    LastPos = i + 2;
	    ++i;
	  }
	  else if (TextBytes[i] == 0x0b) { // Indicates that the lines after this are in a prompt window
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Selection Dialog{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    LastPos = i + 1;
	  }
	  else if (TextBytes[i] == 0x0c && i + 1 < TextBytes.Length) {
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Multiple Choice (Parameter {2}){1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1]);
	    LastPos = i + 2;
	    ++i;
	  }
	  else if (TextBytes[i] == 0x19 && i + 1 < TextBytes.Length) {
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Item Parameter {2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1]);
	    LastPos = i + 2;
	    ++i;
	  }
	  else if (TextBytes[i] == 0x1a && i + 1 < TextBytes.Length) {
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Key Item Parameter {2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1]);
	    LastPos = i + 2;
	    ++i;
	  }
	  else if (TextBytes[i] == 0x1c && i + 1 < TextBytes.Length) { // Chocobo Name
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Player/Chocobo Parameter {2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1]);
	    LastPos = i + 2;
	    ++i;
	  }
	  else if (TextBytes[i] == 0x1e && i + 1 < TextBytes.Length) {
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Set Color #{2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1]);
	    LastPos = i + 2;
	    ++i;
	  }
	  else if (TextBytes[i] == 0x7f && i + 1 < TextBytes.Length) { // Various stuff
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    if (TextBytes[i + 1] == 0x31 && i + 2 < TextBytes.Length) { // Unknown, but seems to indicate user needs to hit RET
	      if (TextBytes[i + 2] != 0)
		this.Text_ += String.Format("{0}{2}-Second Delay + Prompt{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 2]);
	      else
		this.Text_ += String.Format("{0}Prompt{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	      ++LastPos;
	      ++i;
	    }
	    else if (TextBytes[i + 1] == 0x85) // Multiple Choice: Player Gender
	      this.Text_ += String.Format("{0}Multiple Choice (Player Gender){1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    else if (TextBytes[i + 1] == 0x8D && i + 2 < TextBytes.Length) {
	      this.Text_ += String.Format("{0}Weather Event Parameter {2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 2]);
	      ++LastPos;
	      ++i;
	    }
	    else if (TextBytes[i + 1] == 0x8E && i + 2 < TextBytes.Length) {
	      this.Text_ += String.Format("{0}Weather Type Parameter {2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 2]);
	      ++LastPos;
	      ++i;
	    }
	    else if (TextBytes[i + 1] == 0x92 && i + 2 < TextBytes.Length) {
	      this.Text_ += String.Format("{0}Singular/Plural Choice (Parameter {2}){1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 2]);
	      ++LastPos;
	      ++i;
	    }
	    else if (TextBytes[i + 1] == 0xB1 && i + 2 < TextBytes.Length) { // Usually found before an item name or key item name
	      this.Text_ += String.Format("{0}Title Parameter {2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 2]);
	      ++LastPos;
	      ++i;
	    }
	    else if (i + 2 < TextBytes.Length) {
	      this.Text_ += String.Format("{0}Unknown Parameter (Type: {2:X2}) {3}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1], TextBytes[i + 2]);
	      ++LastPos;
	      ++i;
	    }
	    else
	      this.Text_ += String.Format("{0}Unknown Marker Type: {2:X2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i + 1]);
	    LastPos = i + 2;
	    ++i;
	  }
	  else if (TextBytes[i] == 0x7f || TextBytes[i] < 0x20) {
	    if (LastPos < i)
	      this.Text_ += E.GetString(TextBytes, LastPos, i - LastPos);
	    this.Text_ += String.Format("{0}Possible Special Code: {2:X2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[i]);
	    LastPos = i + 1;
	  }
	}
	if (LastPos < ByteCount)
	  this.Text_ += E.GetString(TextBytes, LastPos, ByteCount - LastPos);
	return (BR.ReadByte() == 0xff);
      } catch { return false; }
    }

    #endregion

  }

}
