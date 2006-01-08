// $Id$

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using PlayOnline.Core;

namespace PlayOnline.FFXI.Utils.DataBrowser {

  internal partial class FileScanDialog : Form {

    private string FileName;

    public static bool AllowAbort = false;
    public static bool ShowProgressDetails = false;

    public ArrayList          StringTableEntries = new ArrayList();
    public ThingList<Graphic> Images             = new ThingList<Graphic>();
    public ThingList<Item>    Items              = new ThingList<Item>();

    public FileScanDialog(string FileName) {
      InitializeComponent();
      this.FileName = FileName;
      this.DialogResult = DialogResult.None;
      this.ControlBox = FileScanDialog.AllowAbort;
    }

    #region Scanners

    #region Main Scanning Functionality

    private void ScanFile() {
      if (this.FileName != null && File.Exists(this.FileName)) {
	this.prbScanProgress.Value = 0;
	this.prbScanProgress.Visible = true;
      BinaryReader BR = null;
	try {
          BR = new BinaryReader(new FileStream(this.FileName, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.ASCII);
	} catch { }
	if (BR != null && BR.BaseStream.CanSeek) {
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanDialogFile(BR);
	  if (this.StringTableEntries.Count != 0)
	    goto Done;
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanXIStringFile(BR);
	  if (this.StringTableEntries.Count != 0)
	    goto Done;
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanSimpleStringFile(BR);
	  if (this.StringTableEntries.Count != 0)
	    goto Done;
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanSpellFile(BR);
	  if (this.StringTableEntries.Count != 0)
	    goto Done;
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanAbilityFile(BR);
	  if (this.StringTableEntries.Count != 0)
	    goto Done;
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanStatusFile(BR);
	  if (this.StringTableEntries.Count != 0)
	    goto Done;
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanQuestData(BR);
	  if (this.StringTableEntries.Count != 0)
	    goto Done;
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanItemData(BR);
	  if (this.Items.Count != 0)
	    goto Done;
	  // No specific format recognized - scan for embedded image data
  	  Application.DoEvents();
	  BR.BaseStream.Seek(0, SeekOrigin.Begin);
	  this.ScanImages(BR);
	Done:
	  BR.Close();
	}
      }
      this.Scanning = false;
      this.ScanThread = null;
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void SetProgress(long Current, long Max) {
      this.prbScanProgress.Value = (int) (Math.Min((decimal) Current / Max, 1.0M) * this.prbScanProgress.Maximum);
      Application.DoEvents();
    }

    #endregion

    #region Abilities

    private void ScanAbilityFile(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("AbilityCheck");
      this.SetProgress(0, 1);
      BR.BaseStream.Seek(0, SeekOrigin.Begin);
      if ((BR.BaseStream.Length % 0x400) != 0)
	return;
    ColumnHeader[] InfoColumns = new ColumnHeader[6];
      InfoColumns[0]           = new ColumnHeader();
      InfoColumns[0].Text      = I18N.GetText("ColumnHeader:Name");
      InfoColumns[0].TextAlign = HorizontalAlignment.Left;
      InfoColumns[0].Width     = 80;
      InfoColumns[1]           = new ColumnHeader();
      InfoColumns[1].Text      = I18N.GetText("ColumnHeader:MPCost");
      InfoColumns[1].TextAlign = HorizontalAlignment.Left;
      InfoColumns[1].Width     = 30;
      InfoColumns[2]           = new ColumnHeader();
      InfoColumns[2].Text      = I18N.GetText("ColumnHeader:CoolDown");
      InfoColumns[2].TextAlign = HorizontalAlignment.Left;
      InfoColumns[2].Width     = 50;
      InfoColumns[3]           = new ColumnHeader();
      InfoColumns[3].Text      = I18N.GetText("ColumnHeader:ValidTargets");
      InfoColumns[3].TextAlign = HorizontalAlignment.Left;
      InfoColumns[3].Width     = 80;
      InfoColumns[4]           = new ColumnHeader();
      InfoColumns[4].Text      = I18N.GetText("ColumnHeader:Description");
      InfoColumns[4].TextAlign = HorizontalAlignment.Left;
      InfoColumns[4].Width     = 120;
      InfoColumns[5]           = new ColumnHeader();
      InfoColumns[5].Text      = I18N.GetText("ColumnHeader:Unknown");
      InfoColumns[5].TextAlign = HorizontalAlignment.Left;
      InfoColumns[5].Width     = 120;
      this.StringTableEntries.Add(InfoColumns);
    long EntryCount = BR.BaseStream.Length / 0x400;
    Encoding E = new FFXIEncoding();
      this.lblScanProgress.Text = I18N.GetText("AbilityLoad");
      // Block Layout:
      // 000-001 U16 Index
      // 002-003 U16 Unknown
      // 004-005 U16 MP Cost
      // 006-007 U16 Cooldown
      // 008-009 U16 Valid Targets
      // 00a-029 TXT Name
      // 02a-129 TXT Description (exact length unknown)
      // 12a-3fe U8  Padding (NULs)
      // 3ff-3ff U8  End marker (0xff)
      for (int i = 0; i < EntryCount; ++i) {
      byte[] Bytes = BR.ReadBytes(0x400);
	if (Bytes[0x3ff] != 0xff || Bytes[9] != 0x00 || !FFXIEncryption.DecodeDataBlock(Bytes))
	  goto BadFormat;
      ArrayList Fields = new ArrayList(5);
	Fields.Add(E.GetString(Bytes, 0x0a, 32).TrimEnd('\0'));
	Fields.Add(String.Format("{0}", Bytes[0x04] + (Bytes[0x05] << 8)));
	Fields.Add(String.Format("{0}", new TimeSpan(0, 0, Bytes[0x06] + (Bytes[0x07] << 8))));
	Fields.Add(String.Format("{0}", (ValidTarget) Bytes[8]));
	Fields.Add(E.GetString(Bytes, 42, 256).TrimEnd('\0'));
	Fields.Add(String.Format("{0} ({0:X2})", Bytes[0x02] + (Bytes[0x03] << 8)));
	this.StringTableEntries.Add(Fields.ToArray());
	if (FileScanDialog.ShowProgressDetails)
	  this.lblScanProgress.Text = String.Format(I18N.GetText("AbilityLoadProgress"), i + 1, EntryCount);
	this.SetProgress(i + 1, EntryCount);
      }
      return;
    BadFormat:
      this.StringTableEntries.Clear();
    }

    #endregion

    #region Dialog Files

    private void ScanDialogFile(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("DialogCheck");
      this.SetProgress(0, 1);
      BR.BaseStream.Seek(0, SeekOrigin.Begin);
      if (BR.BaseStream.Length < 4)
	return;
    uint FileSizeMaybe = BR.ReadUInt32();
      if (FileSizeMaybe != (0x10000000 + BR.BaseStream.Length - 4))
	return;
    uint FirstTextPos = (BR.ReadUInt32() ^ 0x80808080);
      if ((FirstTextPos % 4) != 0 || FirstTextPos > BR.BaseStream.Length || FirstTextPos < 8)
	return;
    ColumnHeader[] InfoColumns = new ColumnHeader[1];
      InfoColumns[0]           = new ColumnHeader();
      InfoColumns[0].Text      = I18N.GetText("ColumnHeader:Text");
      InfoColumns[0].TextAlign = HorizontalAlignment.Left;
      InfoColumns[0].Width     = 100;
      this.StringTableEntries.Add(InfoColumns);
    uint EntryCount = FirstTextPos / 4;
    Encoding E = new FFXIEncoding();
      this.lblScanProgress.Text = I18N.GetText("DialogLoad");
      for (int i = 0; i < EntryCount; ++i) {
	BR.BaseStream.Seek(4 + 4 * i, SeekOrigin.Begin);
      long Offset = (BR.ReadUInt32() ^ 0x80808080);
      long NextOffset = (((i + 1) == EntryCount) ? BR.BaseStream.Length : (BR.ReadUInt32() ^ 0x80808080));
	if (NextOffset < Offset || NextOffset > (Offset + 1024)) // Sanity check - the 1024 is arbitrary
	  goto BadFormat;
	BR.BaseStream.Seek(4 + Offset, SeekOrigin.Begin);
      byte[] TextBytes = BR.ReadBytes((int) (NextOffset - Offset));
	for (int j = 0; j < TextBytes.Length; ++j)
	  TextBytes[j] ^= 0x80;
      string Entry = String.Empty;
      int LastPos = 0;
	for (int j = 0; j < TextBytes.Length; ++j) {
	  if (TextBytes[j] == 0x07) { // Line Break
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}NewLine{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 1;
	  }
	  else if (TextBytes[j] == 0x08) { // Character Name (You)
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Player Name{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 1;
	  }
	  else if (TextBytes[j] == 0x09) { // Character Name (They)
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Speaker Name{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 1;
	  }
	  else if (TextBytes[j] == 0x0a && j + 1 < TextBytes.Length) {
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Numeric Parameter {1}{2}", FFXIEncoding.SpecialMarkerStart, TextBytes[j + 1], FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 2;
	    ++j;
	  }
	  else if (TextBytes[j] == 0x0b) { // Indicates that the lines after this are in a prompt window
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Selection Dialog{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 1;
	  }
	  else if (TextBytes[j] == 0x0c && j + 1 < TextBytes.Length) {
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Multiple Choice (Parameter {1}){2}", FFXIEncoding.SpecialMarkerStart, TextBytes[j + 1], FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 2;
	    ++j;
	  }
	  else if (TextBytes[j] == 0x19 && j + 1 < TextBytes.Length) {
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Item Parameter {1}{2}", FFXIEncoding.SpecialMarkerStart, TextBytes[j + 1], FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 2;
	    ++j;
	  }
	  else if (TextBytes[j] == 0x1a && j + 1 < TextBytes.Length) {
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Marker: {1:X2}{2:X2}{3}", FFXIEncoding.SpecialMarkerStart, TextBytes[j + 0], TextBytes[j + 1], FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 2;
	    ++j;
	  }
	  else if (TextBytes[j] == 0x1e && j + 1 < TextBytes.Length) {
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Set Color #{1}{2}", FFXIEncoding.SpecialMarkerStart, TextBytes[j + 1], FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 2;
	    ++j;
	  }
	  else if (TextBytes[j] == 0x7f && j + 2 < TextBytes.Length) { // Unknown Type of Text Substitution
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Marker: {1:X2}{2:X2}{3:X2}{4}", FFXIEncoding.SpecialMarkerStart, TextBytes[j + 0], TextBytes[j + 1], TextBytes[j + 2], FFXIEncoding.SpecialMarkerEnd);
	    LastPos = j + 3;
	    j += 2;
	  }
#if DEBUG
	  else if (TextBytes[j] < 0x20) {
	    if (LastPos < j)
	      Entry += E.GetString(TextBytes, LastPos, j - LastPos);
	    Entry += String.Format("{0}Possible Special Code: {2:X2}{1}", FFXIEncoding.SpecialMarkerStart, FFXIEncoding.SpecialMarkerEnd, TextBytes[j]);
	    LastPos = j + 1;
	  }
#endif
	}
	if (LastPos < TextBytes.Length)
	  Entry += E.GetString(TextBytes, LastPos, TextBytes.Length - LastPos);
	this.StringTableEntries.Add(new string[] { Entry });
	if (FileScanDialog.ShowProgressDetails)
	  this.lblScanProgress.Text = String.Format(I18N.GetText("DialogLoadProgress"), i + 1, EntryCount);
	this.SetProgress(i + 1, EntryCount);
      }
      return;
    BadFormat:
      this.StringTableEntries.Clear();
    }

    #endregion

    #region Images

    private void ScanImages(BinaryReader BR) {
    int ImageCount = 0;
      this.lblScanProgress.Text = String.Format(I18N.GetText("ImageScan"), ImageCount);
      this.SetProgress(0, 1);
    Graphic G = new Graphic();
      while (BR.BaseStream.Position < BR.BaseStream.Length) {
      long Pos = BR.BaseStream.Position; // Save Position (G.Read() will advance it an unknown amount
	if (G.Read(BR)) {
	  this.Images.Add(G);
	  G = new Graphic();
	  this.SetProgress(BR.BaseStream.Position, BR.BaseStream.Length);
	  if (FileScanDialog.ShowProgressDetails)
	    this.lblScanProgress.Text = String.Format(I18N.GetText("ImageScanProgress"), ++ImageCount);
	}
	else {
	  BR.BaseStream.Seek(Pos + 1, SeekOrigin.Begin);
	  if (BR.BaseStream.Position == BR.BaseStream.Length || (BR.BaseStream.Position % 1024) == 0)
	    this.SetProgress(BR.BaseStream.Position, BR.BaseStream.Length);
	}
      }
      G = null;
    }

    #endregion

    #region Item Data

    private void ScanItemData(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("ItemCheck");
      this.SetProgress(0, 1);
      if (BR == null || BR.BaseStream == null)
	goto BadFormat;
      if ((BR.BaseStream.Length % 0xC00) != 0)
	goto BadFormat;
      // First deduce the type of item data is in the file.
    Item.Language L;
    Item.Type T;
      Item.DeduceLanguageAndType(BR, out L, out T);
      // Now read the items
    long ItemCount = BR.BaseStream.Length / 0xc00;
      this.lblScanProgress.Text = I18N.GetText("ItemLoad");
      for (long i = 0; i < ItemCount; ++i) {
      Item I = new Item();
	if (!I.Read(BR, L, T))
	  goto BadFormat;
	this.Items.Add(I);
	this.Images.Add(I.GetFieldValue("icon") as Graphic);
	if (FileScanDialog.ShowProgressDetails)
	  this.lblScanProgress.Text = String.Format(I18N.GetText("ItemLoadProgress"), i + 1, ItemCount);
	this.SetProgress(i + 1, ItemCount);
      }
      return;
    BadFormat:
      this.Items.Clear();
      this.Images.Clear();
    }

    #endregion

    #region Quests / Missions / Key Items

    private void ScanQuestData(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("ItemCheck");
      this.SetProgress(0, 1);
      if (Encoding.ASCII.GetString(BR.ReadBytes(4)) != "menu")
	return;
      if (BR.ReadInt32() != 0x101)
	return;
      if (BR.ReadInt64() != 0x000)
	return;
      if (BR.ReadInt64() != 0)
	return;
      if (BR.ReadInt64() != 0)
	return;
    string MenuNameStart = Encoding.ASCII.GetString(BR.ReadBytes(4));
      BR.ReadUInt32(); // unknown
      if (BR.ReadInt64() != 0)
	return;
      // Now we're ready to start reading menus
    ColumnHeader[] InfoColumns = new ColumnHeader[03 + 12];
      InfoColumns[00]           = new ColumnHeader();
      InfoColumns[00].Text      = I18N.GetText("ColumnHeader:Section");
      InfoColumns[00].TextAlign = HorizontalAlignment.Left;
      InfoColumns[00].Width     = 50;
      InfoColumns[01]           = new ColumnHeader();
      InfoColumns[01].Text      = I18N.GetText("ColumnHeader:Name");
      InfoColumns[01].TextAlign = HorizontalAlignment.Left;
      InfoColumns[01].Width     = 50;
      InfoColumns[02]           = new ColumnHeader();
      InfoColumns[02].Text      = String.Format(I18N.GetText("ColumnHeader:LineCount"), I18N.GetText("ColumnHeader:Text"));
      InfoColumns[02].TextAlign = HorizontalAlignment.Right;
      InfoColumns[02].Width     = 30;
      for (int i = 0; i < 12; ++i) {
	InfoColumns[03 + i]           = new ColumnHeader();
	InfoColumns[03 + i].Text      = String.Format(I18N.GetText("ColumnHeader:LineNumber"), I18N.GetText("ColumnHeader:Text"), i + 1);
	InfoColumns[03 + i].TextAlign = HorizontalAlignment.Left;
	InfoColumns[03 + i].Width     = 100;
      }
      this.StringTableEntries.Add(InfoColumns);
    uint MenuCount = 0;
    FFXIEncoding E = new FFXIEncoding();
      this.lblScanProgress.Text = I18N.GetText("QuestLoad");
      while (BR.BaseStream.Position < BR.BaseStream.Length) {
      string Marker = Encoding.ASCII.GetString(BR.ReadBytes(4));
      string Filler = Encoding.ASCII.GetString(BR.ReadBytes(4));
      string MenuName = Encoding.ASCII.GetString(BR.ReadBytes(8));
	if (Marker.StartsWith("end"))
	  break;
	if (Marker != "menu" || Filler != "    ")
	  continue;
	if (MenuCount++ == 0 && MenuName.Substring(0, 4) != MenuNameStart)
	  continue;
	// TODO: verify the menu name; would not be future-proof tho.  Perhaps just check for _qs or _ms?
	if (BR.ReadInt32() != 0) {
	  BR.BaseStream.Seek(-4, SeekOrigin.Current);
	  continue;
	}
      int EntryCount = BR.ReadInt32();
      long MenuStart = BR.BaseStream.Position - 0x18;
      long MaxMenuPos = MenuStart;
	for (int i = 0; i < EntryCount; ++i) {
	int  Index     = BR.ReadInt32();
	long NameStart = MenuStart + BR.ReadInt32();
	long NameEnd   = MenuStart + BR.ReadInt32();
	long BodyStart = MenuStart + BR.ReadInt32();
	long BodyEnd   = MenuStart + BR.ReadInt32();
	  if (NameEnd > MaxMenuPos)
	    MaxMenuPos = NameEnd;
	ArrayList Fields = new ArrayList();
	  Fields.Add(MenuName);
	  {
	  long CurPos = BR.BaseStream.Position;
	    BR.BaseStream.Seek(NameStart, SeekOrigin.Begin);
	    { // Read entry name
	    string EntryName = FFXIEncryption.ReadEncodedString(BR, E);
	      if (MenuName == "sc_item_") {
		BR.BaseStream.Seek(NameEnd, SeekOrigin.Begin);
	      string X = EntryName;
		if (X != String.Empty && X != "X")
		  EntryName = String.Format("({0}) ", X);
		else
		  EntryName = String.Empty;
		EntryName += FFXIEncryption.ReadEncodedString(BR, E);
	      }
	      Fields.Add(EntryName);
	    }
	    BR.BaseStream.Seek(BodyStart, SeekOrigin.Begin);
	    { // Read entry description lines
	    int LineCount = BR.ReadInt32();
	      Fields.Add(LineCount.ToString());
	    long[] LineStart = new long[LineCount];
	      for (int j = 0; j < LineCount; ++j) {
		LineStart[j] = MenuStart + BR.ReadInt32();
		if (LineStart[j] > MaxMenuPos)
		  MaxMenuPos = LineStart[j];
	      }
	      for (int j = 0; j < LineCount; ++j) {
		BR.BaseStream.Seek(LineStart[j], SeekOrigin.Begin);
		Fields.Add(FFXIEncryption.ReadEncodedString(BR, E));
	      }
	    }
	    BR.BaseStream.Seek(CurPos, SeekOrigin.Begin);
	  }
	  this.StringTableEntries.Add(Fields.ToArray());
	  if (FileScanDialog.ShowProgressDetails)
	    this.lblScanProgress.Text = String.Format(I18N.GetText("QuestLoadProgress"), MenuName, i + 1, EntryCount);
	}
	if ((MaxMenuPos % 16) != 0)
	  MaxMenuPos += 16 - (MaxMenuPos % 16);
	if (MaxMenuPos < BR.BaseStream.Length)
	  BR.BaseStream.Seek(MaxMenuPos, SeekOrigin.Begin);
	else
	  break;
      }
    }

    #endregion

    #region Spell Info

    // Same general layout as the ability info, different data
    private void ScanSpellFile(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("SpellCheck");
      this.SetProgress(0, 1);
      BR.BaseStream.Seek(0, SeekOrigin.Begin);
      if ((BR.BaseStream.Length % 0x400) != 0)
	return;
    ColumnHeader[] InfoColumns = new ColumnHeader[12];
      InfoColumns[00]           = new ColumnHeader();
      InfoColumns[00].Text      = I18N.GetText("ColumnHeader:Type");
      InfoColumns[00].TextAlign = HorizontalAlignment.Left;
      InfoColumns[00].Width     = 50;
      InfoColumns[01]           = new ColumnHeader();
      InfoColumns[01].Text      = I18N.GetText("ColumnHeader:EnglishName");
      InfoColumns[01].TextAlign = HorizontalAlignment.Left;
      InfoColumns[01].Width     = 50;
      InfoColumns[02]           = new ColumnHeader();
      InfoColumns[02].Text      = I18N.GetText("ColumnHeader:JapaneseName");
      InfoColumns[02].TextAlign = HorizontalAlignment.Left;
      InfoColumns[02].Width     = 50;
      InfoColumns[03]           = new ColumnHeader();
      InfoColumns[03].Text      = I18N.GetText("ColumnHeader:Skill");
      InfoColumns[03].TextAlign = HorizontalAlignment.Left;
      InfoColumns[03].Width     = 50;
      InfoColumns[04]           = new ColumnHeader();
      InfoColumns[04].Text      = I18N.GetText("ColumnHeader:Element");
      InfoColumns[04].TextAlign = HorizontalAlignment.Left;
      InfoColumns[04].Width     = 50;
      InfoColumns[05]           = new ColumnHeader();
      InfoColumns[05].Text      = I18N.GetText("ColumnHeader:Jobs");
      InfoColumns[05].TextAlign = HorizontalAlignment.Left;
      InfoColumns[05].Width     = 50;
      InfoColumns[06]           = new ColumnHeader();
      InfoColumns[06].Text      = I18N.GetText("ColumnHeader:MPCost");
      InfoColumns[06].TextAlign = HorizontalAlignment.Left;
      InfoColumns[06].Width     = 50;
      InfoColumns[07]           = new ColumnHeader();
      InfoColumns[07].Text      = I18N.GetText("ColumnHeader:CastTime");
      InfoColumns[07].TextAlign = HorizontalAlignment.Left;
      InfoColumns[07].Width     = 50;
      InfoColumns[08]           = new ColumnHeader();
      InfoColumns[08].Text      = I18N.GetText("ColumnHeader:RecastTime");
      InfoColumns[08].TextAlign = HorizontalAlignment.Left;
      InfoColumns[08].Width     = 50;
      InfoColumns[09]           = new ColumnHeader();
      InfoColumns[09].Text      = I18N.GetText("ColumnHeader:ValidTargets");
      InfoColumns[09].TextAlign = HorizontalAlignment.Left;
      InfoColumns[09].Width     = 50;
      InfoColumns[10]           = new ColumnHeader();
      InfoColumns[10].Text      = I18N.GetText("ColumnHeader:EnglishDescription");
      InfoColumns[10].TextAlign = HorizontalAlignment.Left;
      InfoColumns[10].Width     = 120;
      InfoColumns[11]           = new ColumnHeader();
      InfoColumns[11].Text      = I18N.GetText("ColumnHeader:JapaneseDescription");
      InfoColumns[11].TextAlign = HorizontalAlignment.Left;
      InfoColumns[11].Width     = 120;
      this.StringTableEntries.Add(InfoColumns);
    long EntryCount = BR.BaseStream.Length / 0x400;
    Encoding E = new FFXIEncoding();
      this.lblScanProgress.Text = I18N.GetText("SpellLoad");
      // Block Layout:
      // 000-001 U16 Index
      // 002-003 U16 Spell Type (1/2/3/4/5 - White/Black/Summon/Ninja/Bard)
      // 004-005 U16 Element
      // 006-007 U16 Valid Targets
      // 008-009 U16 Skill
      // 00a-00b U16 MP Cost
      // 00c-00c U8  Cast Time (1/4 second)
      // 00d-00d U8  Recast Time (1/4 second)
      // 00f-026 U8  Level required (1 byte per job, 0xff if not learnable; first is for the NUL job, so always 0xff; only 24 slots despite 32 possible job flags)
      // 027-028 U16 Unknown (Merit Required?)
      // 029-03c TXT Japanese Name
      // 03d-050 TXT English Name
      // 051-0D0 TXT Japanese Description
      // 0D1-150 TXT English Description
      // 151-3fe U8  Padding (NULs)
      // 3ff-3ff U8  End marker (0xff)
      for (int i = 0; i < EntryCount; ++i) {
      byte[] Bytes = BR.ReadBytes(0x400);
	if (Bytes[0x3] != 0x00 || Bytes[0x5] != 0x00 || Bytes[0x7] != 0x00 || Bytes[0x9] != 0x00 || Bytes[0xf] != 0xff || Bytes[0x3ff] != 0xff)
	  goto BadFormat;
	if (!FFXIEncryption.DecodeDataBlock(Bytes))
	  goto BadFormat;
      ArrayList Fields = new ArrayList(9);
	Fields.Add(String.Format("{0}", (MagicType) Bytes[0x02]));
	Fields.Add(E.GetString(Bytes, 0x3d, 20).TrimEnd('\0'));
	Fields.Add(E.GetString(Bytes, 0x29, 20).TrimEnd('\0'));
	Fields.Add(String.Format("{0}", (Skill) Bytes[0x08]));
	Fields.Add(String.Format("{0}", (Element) Bytes[0x04]));
	{ // Minimum Required Job Level (x24)
	string JobInfo = String.Empty;
	  for (int j = 1; j < 24; ++j) {
	    if (Bytes[0x00e + j] != 0xFF) {
	      if (JobInfo != String.Empty) JobInfo += ", ";
	      JobInfo += Bytes[0x00e + j].ToString();
	      JobInfo += ((Job) (1 << j)).ToString();
	    }
	  }
	  Fields.Add(JobInfo);
	}
	Fields.Add(String.Format("{0}",  Bytes[0x0a] + (Bytes[0x0b] << 8)));
	Fields.Add(String.Format("{0}s", Bytes[0x0c] / 4.0));
	Fields.Add(String.Format("{0}s", Bytes[0x0d] / 4.0));
	Fields.Add(String.Format("{0}", (ValidTarget) Bytes[0x06]));
	Fields.Add(E.GetString(Bytes, 0xd1, 128).TrimEnd('\0'));
	Fields.Add(E.GetString(Bytes, 0x51, 128).TrimEnd('\0'));
	this.StringTableEntries.Add(Fields.ToArray());
	if (FileScanDialog.ShowProgressDetails)
	  this.lblScanProgress.Text = String.Format(I18N.GetText("SpellLoadProgress"), i + 1, EntryCount);
	this.SetProgress(i + 1, EntryCount);
      }
      return;
    BadFormat:
      this.StringTableEntries.Clear();
    }

    #endregion

    #region Status Effects

    // Same general layout as the item info, but only the first 0x200 bytes are encoded (and in the ability/spell style)
    private void ScanStatusFile(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("StatusCheck");
      this.SetProgress(0, 1);
      BR.BaseStream.Seek(0, SeekOrigin.Begin);
      if ((BR.BaseStream.Length % 0xc00) != 0)
	return;
    ColumnHeader[] InfoColumns = new ColumnHeader[3];
      InfoColumns[0]           = new ColumnHeader();
      InfoColumns[0].Text      = I18N.GetText("ColumnHeader:Name");
      InfoColumns[0].TextAlign = HorizontalAlignment.Left;
      InfoColumns[0].Width     = 100;
      InfoColumns[1]           = new ColumnHeader();
      InfoColumns[1].Text      = I18N.GetText("ColumnHeader:Status");
      InfoColumns[1].TextAlign = HorizontalAlignment.Left;
      InfoColumns[1].Width     = 100;
      InfoColumns[2]           = new ColumnHeader();
      InfoColumns[2].Text      = I18N.GetText("ColumnHeader:Description");
      InfoColumns[2].TextAlign = HorizontalAlignment.Left;
      InfoColumns[2].Width     = 100;
      this.StringTableEntries.Add(InfoColumns);
    long EntryCount = BR.BaseStream.Length / 0xc00;
    Encoding E = new FFXIEncoding();
      this.lblScanProgress.Text = I18N.GetText("StatusLoad");
      // Block Layout:
      // 000-001 U16 Index
      // 002-021 TXT Name
      // 022-041 TXT Status
      // 042-141 TXT Description
      // 200-201 U16 Icon Size
      // 202-bff IMG Icon (+ padding)
      for (int i = 0; i < EntryCount; ++i) {
      byte[] Bytes = BR.ReadBytes(0x200);
	if (!FFXIEncryption.DecodeDataBlock(Bytes))
	  goto BadFormat;
      byte[] IconBytes = BR.ReadBytes(0xa00);
	if (IconBytes[0x9ff] != 0xff)
	  goto BadFormat;
	{ // Verify that the icon info is valid
	Graphic StatusIcon = new Graphic();
	BinaryReader IconBR = new BinaryReader(new MemoryStream(IconBytes, 0, 0xa00, false));
	int IconSize = IconBR.ReadInt32();
	  if (IconSize > 0 && IconSize <= 0x9fb) {
	    if (!StatusIcon.Read(IconBR) || IconBR.BaseStream.Position != 4 + IconSize)
	      goto BadFormat;
	  }
	  IconBR.Close();
	  if (StatusIcon == null)
	    goto BadFormat;
	  this.Images.Add(StatusIcon);
	}
	this.StringTableEntries.Add(new string[] { E.GetString(Bytes, 2, 32).TrimEnd('\0'), E.GetString(Bytes, 34, 32).TrimEnd('\0'), E.GetString(Bytes, 66, 128).TrimEnd('\0') });
	if (FileScanDialog.ShowProgressDetails)
	  this.lblScanProgress.Text = String.Format(I18N.GetText("StatusLoadProgress"), i + 1, EntryCount);
	this.SetProgress(i + 1, EntryCount);
      }
      return;
    BadFormat:
      this.Images.Clear();
      this.StringTableEntries.Clear();
    }

    #endregion

    #region String Tables

    private void ScanSimpleStringFile(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("StringCheck");
      this.SetProgress(0, 1);
      BR.BaseStream.Seek(0, SeekOrigin.Begin);
      if ((BR.BaseStream.Length % 0x40) != 0)
	return;
    ColumnHeader[] InfoColumns = new ColumnHeader[1];
      InfoColumns[0]           = new ColumnHeader();
      InfoColumns[0].Text      = I18N.GetText("ColumnHeader:Text");
      InfoColumns[0].TextAlign = HorizontalAlignment.Left;
      InfoColumns[0].Width     = 100;
      this.StringTableEntries.Add(InfoColumns);
    long EntryCount = BR.BaseStream.Length / 0x40;
    Encoding E = new FFXIEncoding();
      this.lblScanProgress.Text = I18N.GetText("StringLoad");
      for (int i = 0; i < EntryCount; ++i) {
      uint EntryIndex = BR.ReadUInt32();
      byte[] EntryBytes = BR.ReadBytes(0x3c);
	if (EntryIndex != i || EntryBytes[0x3b] != 0xff)
	  goto BadFormat;
	this.StringTableEntries.Add(new string[] { E.GetString(EntryBytes, 0, 0x3b).TrimEnd('\0') });
	if (FileScanDialog.ShowProgressDetails)
	  this.lblScanProgress.Text = String.Format(I18N.GetText("StringLoadProgress"), i + 1, EntryCount);
	this.SetProgress(i + 1, EntryCount);
      }
      return;
    BadFormat:
      this.StringTableEntries.Clear();
    }

    private void ScanXIStringFile(BinaryReader BR) {
      this.lblScanProgress.Text = I18N.GetText("StringCheck");
      this.SetProgress(0, 1);
      BR.BaseStream.Seek(0, SeekOrigin.Begin);
      if (BR.BaseStream.Length < 0x38)
	return;
    Encoding E = new FFXIEncoding();
      // Read past the marker (32 bytes)
      if ((E.GetString(BR.ReadBytes(10)) != "XISTRING".PadRight(10, '\0')) || BR.ReadUInt16() != 2)
	return;
      foreach (byte B in BR.ReadBytes(20)) {
	if (B != 0)
	  return;
      }
      // Read The Header
    uint FileSize = BR.ReadUInt32();
      if (FileSize != BR.BaseStream.Length)
	return;
    uint EntryCount = BR.ReadUInt32();
    uint EntryBytes = BR.ReadUInt32();
    uint DataBytes  = BR.ReadUInt32();
      BR.ReadUInt32(); // Unknown
      BR.ReadUInt32(); // Unknown
      if (EntryBytes != EntryCount * 12 || FileSize != 0x38 + EntryBytes + DataBytes)
	return;
      this.lblScanProgress.Text = I18N.GetText("StringLoad");
      {
      ColumnHeader[] InfoColumns = new ColumnHeader[1];
	InfoColumns[0]           = new ColumnHeader();
	InfoColumns[0].Text      = I18N.GetText("ColumnHeader:Text");
	InfoColumns[0].TextAlign = HorizontalAlignment.Left;
	InfoColumns[0].Width     = 100;
	this.StringTableEntries.Add(InfoColumns);
      }
      for (int i = 0; i < EntryCount; ++i) {
      uint  Offset = BR.ReadUInt32();
      short Size = BR.ReadInt16();
        BR.ReadUInt16(); // Unknown (0 or 1; so probably a flag of some sort)
	BR.ReadUInt32(); // Unknown
	if (Size > 0 && Offset + Size <= DataBytes) {
	long IndexPos = BR.BaseStream.Position;
	  BR.BaseStream.Seek(0x38 + EntryBytes + Offset, SeekOrigin.Begin);
	string Text = E.GetString(BR.ReadBytes(Size)).TrimEnd('\0');
	  BR.BaseStream.Seek(IndexPos, SeekOrigin.Begin);
	  this.StringTableEntries.Add(new string[] { Text });
	}
	else
	  this.StringTableEntries.Add(new string[] { I18N.GetText("InvalidEntry") });
	if (FileScanDialog.ShowProgressDetails)
	  this.lblScanProgress.Text = String.Format(I18N.GetText("StringLoadProgress"), i + 1, EntryCount);
	this.SetProgress(i + 1, EntryCount);
      }
    }

    #endregion

    #endregion

    private bool   Scanning   = false;
    private Thread ScanThread = null;

    private void FileScanDialog_Activated(object sender, System.EventArgs e) {
      lock (this) {
	if (this.Scanning)
	  return;
	this.Scanning = true;
	if (FileScanDialog.AllowAbort) {
	  this.ScanThread = new Thread(new ThreadStart(this.ScanFile));
	  this.ScanThread.Start();
	}
	else
	  this.ScanFile();
      }
    }

    private void FileScanDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      if (FileScanDialog.AllowAbort && this.ScanThread != null) {
	this.ScanThread.Abort();
	this.ScanThread = null;
	this.StringTableEntries.Clear();
	this.Images.Clear();
	this.Items.Clear();
	this.DialogResult = DialogResult.Abort;
      }
    }

  }

}
