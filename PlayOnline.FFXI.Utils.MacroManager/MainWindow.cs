// $Id$

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using PlayOnline.Core;

namespace PlayOnline.FFXI.Utils.MacroManager {

  public class MainWindow : System.Windows.Forms.Form {

    private string MacroLibraryFile;

    private class ATMenuItem : MenuItem {

      public ATMenuItem(uint ResourceID, EventHandler OnClick) : base(FFXIResourceManager.GetResourceString(ResourceID), OnClick) {
	this.ResourceID_ = ResourceID;
      }

      private uint ResourceID_;
      public  uint ResourceID { get { return this.ResourceID_;  } }

    }

    #region Controls

    private System.Windows.Forms.TreeView tvMacroTree;
    private System.Windows.Forms.ImageList ilBrowserIcons;
    private System.Windows.Forms.Panel pnlProperties;
    private System.Windows.Forms.ContextMenu mnuTreeContext;
    private System.Windows.Forms.TextBox txtCommand1;
    private System.Windows.Forms.TextBox txtCommand2;
    private System.Windows.Forms.TextBox txtCommand3;
    private System.Windows.Forms.TextBox txtCommand6;
    private System.Windows.Forms.TextBox txtCommand5;
    private System.Windows.Forms.TextBox txtCommand4;
    private System.Windows.Forms.GroupBox grpMacroCommands;
    private System.Windows.Forms.ContextMenu mnuTextContext;
    private System.Windows.Forms.MenuItem mnuTextCopy;
    private System.Windows.Forms.MenuItem mnuTextCut;
    private System.Windows.Forms.MenuItem mnuTextPaste;
    private System.Windows.Forms.MenuItem mnuTextSep;
    private System.Windows.Forms.MenuItem mnuTreeRename;
    private System.Windows.Forms.MenuItem mnuTreeClear;
    private System.Windows.Forms.MenuItem mnuTreeDelete;
    private System.Windows.Forms.MenuItem mnuTreeNew;
    private System.Windows.Forms.MenuItem mnuTreeNewFolder;
    private System.Windows.Forms.MenuItem mnuTreeNewMacro;
    private System.Windows.Forms.MenuItem mnuTreeUndo;
    private System.Windows.Forms.MenuItem mnuTreeSave;
    private System.Windows.Forms.MenuItem mnuTreeSep2;
    private System.Windows.Forms.MenuItem mnuTreeSep;
    private System.Windows.Forms.MenuItem mnuTreeCollapse;
    private System.Windows.Forms.MenuItem mnuTreeExpand;
    private System.Windows.Forms.MenuItem mnuTreeExpandAll;
    private System.Windows.Forms.MenuItem mnuTextClear;
    private System.Windows.Forms.MenuItem mnuTextInsert;
    private System.ComponentModel.IContainer components;

    #endregion

    public MainWindow() {
      this.InitializeComponent();
      // Set Up Icons
      this.Icon = Icons.Joystick;
      if (this.Icon == null)
	this.Icon = Icons.CheckedPage;
      try {
	this.ilBrowserIcons.Images.Add(Icons.DocFolder);
	this.ilBrowserIcons.Images.Add(Icons.People);
	this.ilBrowserIcons.Images.Add(Icons.FolderOpen);
	this.ilBrowserIcons.Images.Add(Icons.FolderClosed);
	this.ilBrowserIcons.Images.Add(Icons.ConfigFile);
	this.ilBrowserIcons.Images.Add(Icons.TextFile);
      }
      catch (Exception E) {
	Console.WriteLine(E.ToString());
	this.tvMacroTree.ImageList = null;
      }
      // Prepare textbox context menu
      this.CreateTextInsertMenu();
      // Build macro library file name
    string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    string SettingsDir = Path.Combine(LocalAppData, Path.Combine("Pebbles", "POLUtils"));
      if (!Directory.Exists(SettingsDir))
	Directory.CreateDirectory(SettingsDir);
      this.MacroLibraryFile = Path.Combine(SettingsDir, "macro-library.xml");
      // Set Up Library & Character Trees
      this.AddMacroLibraryNode(this.tvMacroTree);
      Game.Clear(); // Force fresh data
      foreach (Character C in Game.Characters)
	this.AddCharacterNode(C, this.tvMacroTree);
    }

    private void AddMacroBar(string FileName, TreeNode Parent) {
      try {
      MacroFolder MF = MacroFolder.LoadFromMacroBar(FileName);
	MF.Name = "Dropped Macro Bar";
	if (MF == null)
	  throw new InvalidOperationException("Only valid macro bar files (mcr*.dat) can be dropped onto the macro tree.");
	this.AddMacroFolderNode(MF, Parent);
      MacroFolder ParentFolder = Parent.Tag as MacroFolder;
	ParentFolder.Folders.Add(MF);
	this.tvMacroTree.SelectedNode = Parent;
	Parent.ExpandAll();
      }
      catch (Exception E) {
	MessageBox.Show(this, E.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void RefreshMacroNode(TreeNode MacroNode) {
    Macro M = MacroNode.Tag as Macro;
      MacroNode.Text = M.Name;
      if (this.tvMacroTree.SelectedNode == MacroNode)
	this.ShowProperties(M);
    }

    private bool IsClearAllowed(TreeNode TN) {
      if (TN.Tag is Character || TN.Tag is MacroFolder || TN.Tag is Macro)
	return true;
      return false;
    }

    private bool IsDeleteAllowed(TreeNode TN) {
      if (TN.Tag is MacroFolder)
	return (TN.Parent != null && !(TN.Tag as MacroFolder).Locked);
      else if (TN.Tag is Macro) {
      MacroFolder OwnerFolder = TN.Parent.Tag as MacroFolder;
	return (OwnerFolder != null && !OwnerFolder.Locked);
      }
      return false;
    }

    private bool IsRenameAllowed(TreeNode TN) {
      if (TN.Tag is Character || TN.Tag is Macro)
	return true;
      else if (TN.Tag is MacroFolder)
	return !(TN.Tag as MacroFolder).Locked;
      return false;
    }

    private bool IsSaveAllowed(TreeNode TN) {
      if (TN.Tag is Character) // Allow saving all of a character's macro bars at once
	return true;
      else if (TN.Tag is MacroFolder) { // Allow saving the macro library, and character macro bars
	if (TN.Parent == null || TN.Parent.Tag is Character)
	  return true;
      }
      return false;
    }

    private void ShowProperties(Macro M) {
      this.txtCommand1.Enabled = (M != null);
      this.txtCommand2.Enabled = (M != null);
      this.txtCommand3.Enabled = (M != null);
      this.txtCommand4.Enabled = (M != null);
      this.txtCommand5.Enabled = (M != null);
      this.txtCommand6.Enabled = (M != null);
      if (M == null) {
	this.txtCommand1.Clear();
	this.txtCommand2.Clear();
	this.txtCommand3.Clear();
	this.txtCommand4.Clear();
	this.txtCommand5.Clear();
	this.txtCommand6.Clear();
      }
      else {
	this.txtCommand1.Text = M.Commands[0];
	this.txtCommand2.Text = M.Commands[1];
	this.txtCommand3.Text = M.Commands[2];
	this.txtCommand4.Text = M.Commands[3];
	this.txtCommand5.Text = M.Commands[4];
	this.txtCommand6.Text = M.Commands[5];
      }
    }

    #region TreeNode Actions

    private void DoClear(TreeNode TN, bool Confirm) {
      if (TN.Tag is Character) {
      Character C = TN.Tag as Character;
	if (!Confirm || MessageBox.Show(this, String.Format("Are you sure you want to clear all macro bars for {0}?", C.Name), "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
	  foreach (TreeNode SubNode in TN.Nodes)
	    this.DoClear(SubNode, false);
	}
      }
      else if (TN.Tag is MacroFolder) {
      MacroFolder MF = TN.Tag as MacroFolder;
	if (!Confirm || MessageBox.Show(this, String.Format("Are you sure you want to clear the '{0}' macro folder (and all its contents)?", MF.Name), "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
	  if (!MF.Locked) {
	    TN.Text = String.Empty;
	    MF.Name = String.Empty;
	  }
	  foreach (TreeNode SubNode in TN.Nodes)
	    this.DoClear(SubNode, false);
	}
      }
      else if (TN.Tag is Macro) {
      Macro M = TN.Tag as Macro;
	if (!Confirm || M.Empty || MessageBox.Show(this, String.Format("Are you sure you want to clear the '{0}' macro?", M.Name), "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
	  M.Clear();
	  this.RefreshMacroNode(TN);
	}
      }
    }

    private void DoDelete(TreeNode TN, bool Confirm) {
      if (TN.Tag is Macro) {
      Macro M = TN.Tag as Macro;
	if (!Confirm || M.Empty || MessageBox.Show(this, String.Format("Are you sure you want to delete the '{0}' macro?", M.Name), "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
	TreeNode ParentNode = TN.Parent;
	  (ParentNode.Tag as MacroFolder).Macros.Remove(M);
	  ParentNode.Nodes.Remove(TN);
	}
      }
      else if (TN.Tag is MacroFolder) {
      MacroFolder MF = TN.Tag as MacroFolder;
	if (!Confirm || MessageBox.Show(this, String.Format("Are you sure you want to delete the '{0}' macro folder (and all its contents)?", MF.Name), "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
	TreeNode ParentNode = TN.Parent;
	  (ParentNode.Tag as MacroFolder).Folders.Remove(MF);
	  ParentNode.Nodes.Remove(TN);
	}
      }
    }

    private void DoNewFolder(TreeNode TN, bool Confirm) {
    MacroFolder NewFolder = new MacroFolder("New Folder");
      (TN.Tag as MacroFolder).Folders.Add(NewFolder);
      this.tvMacroTree.SelectedNode = this.AddMacroFolderNode(NewFolder, TN);
      this.tvMacroTree.SelectedNode.BeginEdit();
    }

    private void DoNewMacro(TreeNode TN, bool Confirm) {
    Macro NewMacro = new Macro("New Macro");
      (TN.Tag as MacroFolder).Macros.Add(NewMacro);
      this.tvMacroTree.SelectedNode = this.AddMacroNode(NewMacro, TN);
      this.tvMacroTree.SelectedNode.BeginEdit();
    }

    private void DoSave(TreeNode TN, bool Confirm) {
      if (TN.Tag is Character) {
      Character C = TN.Tag as Character;
	for (int i = 0; i < 10; ++i)
	  C.SaveMacroBar(i);
      }
      else if (TN.Tag is MacroFolder) {
      MacroFolder MF = TN.Tag as MacroFolder;
	if (!MF.Locked)
	  MF.WriteToXml(this.MacroLibraryFile);
	else {
	Character C = TN.Parent.Tag as Character;
	  if (C != null)
	    C.SaveMacroBar(C.MacroBars.IndexOf(MF));
	}
      }
    }

    private void DoUndo(TreeNode TN, bool Confirm) {
    }

    #endregion

    #region TreeView Creation

    private void AddMacroLibraryNode(TreeView Parent) {
      if (File.Exists(this.MacroLibraryFile))
	this.AddMacroFolderNode(MacroFolder.LoadFromXml(MacroLibraryFile), this.tvMacroTree);
      else
	this.AddMacroFolderNode(new MacroFolder("Macro Library"), this.tvMacroTree);
    }

    private void AddCharacterNode(Character C, TreeView Parent) {
    TreeNode CharacterNode = new TreeNode(C.Name, 1, 1);
      CharacterNode.Tag = C;
      foreach (MacroFolder MF in C.MacroBars)
	this.AddMacroFolderNode(MF, CharacterNode);
      Parent.Nodes.Add(CharacterNode);
    }

    private TreeNode AddMacroFolderNode(MacroFolder Folder, TreeNode Parent) {
    TreeNode MacroFolderNode = new TreeNode(Folder.Name, 3, 2);
      MacroFolderNode.Tag = Folder;
      foreach (MacroFolder MF in Folder.Folders) this.AddMacroFolderNode(MF, MacroFolderNode);
      foreach (Macro       M  in Folder.Macros)  this.AddMacroNode      (M,  MacroFolderNode);
      Parent.Nodes.Add(MacroFolderNode);
      return MacroFolderNode;
    }

    private void AddMacroFolderNode(MacroFolder Folder, TreeView Parent) {
    TreeNode MacroFolderNode = new TreeNode(Folder.Name, 0, 0);
      MacroFolderNode.Tag = Folder;
      foreach (MacroFolder MF in Folder.Folders) this.AddMacroFolderNode(MF, MacroFolderNode);
      foreach (Macro       M  in Folder.Macros)  this.AddMacroNode      (M,  MacroFolderNode);
      Parent.Nodes.Add(MacroFolderNode);
    }

    private TreeNode AddMacroNode(Macro M, TreeNode Parent) {
    TreeNode MacroNode = new TreeNode(M.Name, 4, 4);
      MacroNode.Tag = M;
      Parent.Nodes.Add(MacroNode);
      return MacroNode;
    }

    #endregion

    #region TextBox Context Menu Creation

    private void CreateTextInsertMenu() {
    MenuItem InsertMenu = this.mnuTextContext.MenuItems.Add("&Insert");
      this.AddAutoTransMenuItems  (InsertMenu.MenuItems.Add("&Auto-Translator Text"));
      this.AddFacesMenuItems      (InsertMenu.MenuItems.Add("&Faces"));
      this.AddAlphabetMenuItems   (InsertMenu.MenuItems.Add("&Alphabets"));
      this.AddSpecialCharMenuItems(InsertMenu.MenuItems.Add("&Special Characters"));
    }

    private void AddAutoTransMenuItems(MenuItem ParentMenu) {
    Hashtable CategoryMenus = new Hashtable();
      if (AutoTranslator.Data != null) {
	foreach (AutoTranslator.MessageGroup MG in AutoTranslator.Data) {
	MenuItem CategoryMenu = CategoryMenus[MG.Category] as MenuItem;
	  if (CategoryMenu == null) {
	    CategoryMenu = ParentMenu.MenuItems.Add("&Unknown Category");
	    switch (MG.Category) {
	      case 0x0104: CategoryMenu.Text = String.Format("&Japanese Messages", MG.Category); break;
	      case 0x0202: CategoryMenu.Text = String.Format("&English Messages",  MG.Category); break;
	    }
	    CategoryMenus[MG.Category] = CategoryMenu;
	  }
	MenuItem GroupMenu = CategoryMenu.MenuItems.Add(MG.Title);
	  GroupMenu.MenuItems.Add(MG.Description).Enabled = false;
	  foreach (AutoTranslator.Message M in MG.Messages) {
	  uint ResID = (uint) ((uint) (M.Category << 16) + (ushort) (M.ParentGroup << 8) + M.ID);
	    GroupMenu.MenuItems.Add(new ATMenuItem(ResID, new EventHandler(this.AutoTransMenuItem_Click)));
	  }
	}
      }
    }

    private void AddFacesMenuItems(MenuItem ParentMenu) {
      ParentMenu.Enabled = false;
    }

    private void AddAlphabetMenuItems(MenuItem ParentMenu) {
      { // Greek
      MenuItem MI = ParentMenu.MenuItems.Add("&Greek");
	{
	MenuItem MI2 = MI.MenuItems.Add("&Lowercase");
	  for (int i = 0x03B1; i <= 0x03C1; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  // Skip second form of sigma (not in shift-jis)
	  for (int i = 0x03C3; i <= 0x03C9; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Uppercase");
	  for (int i = 0x0391; i <= 0x03A1; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  // Skip second form of sigma (not in shift-jis)
	  for (int i = 0x03A3; i <= 0x03A9; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
      }
      { // Japanese
      MenuItem MI = ParentMenu.MenuItems.Add("&Japanese");
	{
	MenuItem MI2 = MI.MenuItems.Add("&Hiragana");
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Katakana");
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Punctuation && Symbols");
	  for (int i = 0x3000; i <= 0x3003; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  for (int i = 0x3005; i <= 0x3015; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x301D", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x301F", new EventHandler(this.InsertMenuItem_Click));
	}
      }
      { // Latin
      MenuItem MI = ParentMenu.MenuItems.Add("&Latin");
	{
	MenuItem MI2 = MI.MenuItems.Add("&Lowercase");
	  for (int i = 0x0061; i <= 0x007A; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Uppercase");
	  for (int i = 0x0041; i <= 0x005A; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Numbers");
	  for (int i = 0x0030; i <= 0x0039; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Punctuation && Symbols");
	  for (int i = 0x0020; i <= 0x002F; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  for (int i = 0x003A; i <= 0x0040; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  for (int i = 0x005B; i <= 0x0060; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  for (int i = 0x007B; i <= 0x007E; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  foreach (MenuItem MI3 in MI2.MenuItems) { // Avoid activating the special Menu caption formatting
	    if (MI3.Text == "-")
	      MI3.Text = "- ";
	    else if (MI3.Text == "&")
	      MI3.Text = "&&";
	  }
	  MI2.MenuItems.Add("\x2010", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2015", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2018", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2019", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x201C", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x201D", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2020", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2021", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2025", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2026", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2030", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2032", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2033", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x203B", new EventHandler(this.InsertMenuItem_Click));
	}
      }
      { // Wide ASCII
      MenuItem MI = ParentMenu.MenuItems.Add("&Wide ASCII");
	{
	MenuItem MI2 = MI.MenuItems.Add("&Lowercase");
	  for (int i = 0xFF41; i <= 0xFF5A; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Uppercase");
	  for (int i = 0xFF21; i <= 0xFF3A; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Numbers");
	  for (int i = 0xFF10; i <= 0xFF19; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Punctuation && Symbols");
	  for (int i = 0xFF01; i <= 0xFF0F; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  for (int i = 0xFF1A; i <= 0xFF20; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  for (int i = 0xFF3B; i <= 0xFF40; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	  for (int i = 0xFF5B; i <= 0xFF5E; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
      }
    }

    private void AddSpecialCharMenuItems(MenuItem ParentMenu) {
      { // Circled Numbers
      MenuItem MI = ParentMenu.MenuItems.Add("&Circled Numbers");
	for (int i = 0x2460; i <= 0x2473; ++i)
	  MI.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
      }
      { // Dingbats
      MenuItem MI = ParentMenu.MenuItems.Add("&Dingbats");
	MI.MenuItems.Add("\x25A0", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25A1", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25B2", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25B3", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25BC", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25BD", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25C6", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25C7", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25CB", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25CE", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25CF", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x25EF", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2605", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2606", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2640", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2642", new EventHandler(this.InsertMenuItem_Click));
      }
      { // Line Drawing
      MenuItem MI = ParentMenu.MenuItems.Add("&Line Drawing");
	MI.MenuItems.Add("\x2500", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2501", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2502", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2503", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x250C", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x250F", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2510", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2513", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2514", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2517", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2518", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x251B", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x251C", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x251D", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2520", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2523", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2524", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2525", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2528", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x252B", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x252C", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x252F", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2530", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2533", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2534", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2537", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2538", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x253B", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x253C", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x253F", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x2542", new EventHandler(this.InsertMenuItem_Click));
	MI.MenuItems.Add("\x254B", new EventHandler(this.InsertMenuItem_Click));
      }
      { // Mathematical & Musical Symbols
      MenuItem MI = ParentMenu.MenuItems.Add("&Math && Music");
	{ // Math Operators
	MenuItem MI2 = MI.MenuItems.Add("&Operators");
	  MI2.MenuItems.Add("\x2200", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2202", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2203", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2207", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2208", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x220B", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2211", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x221A", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x221D", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x221E", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x221F", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2220", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2225", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2227", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2228", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2229", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x222A", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x222B", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x222C", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x222E", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2234", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2235", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x223D", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2252", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2260", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2261", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2266", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2267", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x226A", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x226B", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2282", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2283", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2286", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x2287", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x22A5", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x22BF", new EventHandler(this.InsertMenuItem_Click));
	}
	{ // Arrows
	MenuItem MI2 = MI.MenuItems.Add("&Arrows");
	  MI2.MenuItems.Add("\x2190", new EventHandler(this.InsertMenuItem_Click)); // Left
	  MI2.MenuItems.Add("\x2191", new EventHandler(this.InsertMenuItem_Click)); // Up
	  MI2.MenuItems.Add("\x2192", new EventHandler(this.InsertMenuItem_Click)); // Right
	  MI2.MenuItems.Add("\x2193", new EventHandler(this.InsertMenuItem_Click)); // Down
	  MI2.MenuItems.Add("\x21D2", new EventHandler(this.InsertMenuItem_Click)); // =>
	  MI2.MenuItems.Add("\x21D4", new EventHandler(this.InsertMenuItem_Click)); // <=>
	}
	{ // Musical Symbols
	MenuItem MI2 = MI.MenuItems.Add("&Musical Symbols");
	  MI2.MenuItems.Add("\x266A", new EventHandler(this.InsertMenuItem_Click)); // Single note
	  MI2.MenuItems.Add("\x266D", new EventHandler(this.InsertMenuItem_Click)); // Flat
	  MI2.MenuItems.Add("\x266F", new EventHandler(this.InsertMenuItem_Click)); // Sharp
	}
      }
      { // Roman Numerals
      MenuItem MI = ParentMenu.MenuItems.Add("&Roman Numerals");
	{
	MenuItem MI2 = MI.MenuItems.Add("&Lowercase");
	  for (int i = 0x2170; i <= 0x2179; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Uppercase");
	  for (int i = 0x2160; i <= 0x2169; ++i)
	    MI2.MenuItems.Add(new string((char) i, 1), new EventHandler(this.InsertMenuItem_Click));
	}
      }
      { // Unit Indicators
      MenuItem MI = ParentMenu.MenuItems.Add("&Unit Indicators");
	MI.MenuItems.Add("\x2103", new EventHandler(this.InsertMenuItem_Click)); // �C
	MI.MenuItems.Add("\x338E", new EventHandler(this.InsertMenuItem_Click)); // mg
	MI.MenuItems.Add("\x338F", new EventHandler(this.InsertMenuItem_Click)); // kg
	MI.MenuItems.Add("\x339C", new EventHandler(this.InsertMenuItem_Click)); // mm
	MI.MenuItems.Add("\x339D", new EventHandler(this.InsertMenuItem_Click)); // cm
	MI.MenuItems.Add("\x339E", new EventHandler(this.InsertMenuItem_Click)); // km
	MI.MenuItems.Add("\x33A1", new EventHandler(this.InsertMenuItem_Click)); // m�
	MI.MenuItems.Add("\x33C4", new EventHandler(this.InsertMenuItem_Click)); // cc
	MI.MenuItems.Add("\x33CD", new EventHandler(this.InsertMenuItem_Click)); // KK
      }
      { // Various
      MenuItem MI = ParentMenu.MenuItems.Add("&Various");
	{
	MenuItem MI2 = MI.MenuItems.Add("&Asian Symbols");
	  MI2.MenuItems.Add("\x3231", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3232", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3239", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x32A4", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x32A5", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x32A6", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x32A7", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x32A8", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3303", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x330D", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3314", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3318", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3322", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3323", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3326", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3327", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x332B", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3336", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x333B", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3349", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x334A", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x334D", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3351", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x3357", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x337B", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x337C", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x337D", new EventHandler(this.InsertMenuItem_Click));
	  MI2.MenuItems.Add("\x337E", new EventHandler(this.InsertMenuItem_Click));
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Combined Letters");
	  MI2.MenuItems.Add("\x2116", new EventHandler(this.InsertMenuItem_Click)); // "numero"
	  MI2.MenuItems.Add("\x2121", new EventHandler(this.InsertMenuItem_Click)); // "telephone"
	}
	{
	MenuItem MI2 = MI.MenuItems.Add("&Other");
	  MI2.MenuItems.Add("\x2312", new EventHandler(this.InsertMenuItem_Click));
	}
      }
    }

    #endregion

    #region Windows Form Designer generated code

    protected override void Dispose(bool disposing) {
      if (disposing && components != null)
	components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.tvMacroTree = new System.Windows.Forms.TreeView();
      this.mnuTreeContext = new System.Windows.Forms.ContextMenu();
      this.mnuTreeClear = new System.Windows.Forms.MenuItem();
      this.mnuTreeDelete = new System.Windows.Forms.MenuItem();
      this.mnuTreeRename = new System.Windows.Forms.MenuItem();
      this.mnuTreeSave = new System.Windows.Forms.MenuItem();
      this.mnuTreeUndo = new System.Windows.Forms.MenuItem();
      this.mnuTreeSep = new System.Windows.Forms.MenuItem();
      this.mnuTreeCollapse = new System.Windows.Forms.MenuItem();
      this.mnuTreeExpand = new System.Windows.Forms.MenuItem();
      this.mnuTreeExpandAll = new System.Windows.Forms.MenuItem();
      this.mnuTreeSep2 = new System.Windows.Forms.MenuItem();
      this.mnuTreeNew = new System.Windows.Forms.MenuItem();
      this.mnuTreeNewFolder = new System.Windows.Forms.MenuItem();
      this.mnuTreeNewMacro = new System.Windows.Forms.MenuItem();
      this.ilBrowserIcons = new System.Windows.Forms.ImageList(this.components);
      this.pnlProperties = new System.Windows.Forms.Panel();
      this.grpMacroCommands = new System.Windows.Forms.GroupBox();
      this.txtCommand6 = new System.Windows.Forms.TextBox();
      this.mnuTextContext = new System.Windows.Forms.ContextMenu();
      this.mnuTextClear = new System.Windows.Forms.MenuItem();
      this.mnuTextCopy = new System.Windows.Forms.MenuItem();
      this.mnuTextCut = new System.Windows.Forms.MenuItem();
      this.mnuTextPaste = new System.Windows.Forms.MenuItem();
      this.mnuTextSep = new System.Windows.Forms.MenuItem();
      this.mnuTextInsert = new System.Windows.Forms.MenuItem();
      this.txtCommand5 = new System.Windows.Forms.TextBox();
      this.txtCommand4 = new System.Windows.Forms.TextBox();
      this.txtCommand3 = new System.Windows.Forms.TextBox();
      this.txtCommand2 = new System.Windows.Forms.TextBox();
      this.txtCommand1 = new System.Windows.Forms.TextBox();
      this.pnlProperties.SuspendLayout();
      this.grpMacroCommands.SuspendLayout();
      this.SuspendLayout();
      // 
      // tvMacroTree
      // 
      this.tvMacroTree.AllowDrop = true;
      this.tvMacroTree.ContextMenu = this.mnuTreeContext;
      this.tvMacroTree.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvMacroTree.HideSelection = false;
      this.tvMacroTree.HotTracking = true;
      this.tvMacroTree.ImageList = this.ilBrowserIcons;
      this.tvMacroTree.LabelEdit = true;
      this.tvMacroTree.Location = new System.Drawing.Point(0, 0);
      this.tvMacroTree.Name = "tvMacroTree";
      this.tvMacroTree.PathSeparator = "::";
      this.tvMacroTree.Size = new System.Drawing.Size(394, 412);
      this.tvMacroTree.TabIndex = 0;
      this.tvMacroTree.DragOver += new System.Windows.Forms.DragEventHandler(this.tvMacroTree_DragOver);
      this.tvMacroTree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tvMacroTree_KeyUp);
      this.tvMacroTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvMacroTree_AfterSelect);
      this.tvMacroTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvMacroTree_BeforeSelect);
      this.tvMacroTree.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvMacroTree_AfterLabelEdit);
      this.tvMacroTree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvMacroTree_ItemDrag);
      this.tvMacroTree.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvMacroTree_BeforeLabelEdit);
      this.tvMacroTree.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvMacroTree_DragDrop);
      // 
      // mnuTreeContext
      // 
      this.mnuTreeContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
										   this.mnuTreeClear,
										   this.mnuTreeDelete,
										   this.mnuTreeRename,
										   this.mnuTreeSave,
										   this.mnuTreeUndo,
										   this.mnuTreeSep,
										   this.mnuTreeCollapse,
										   this.mnuTreeExpand,
										   this.mnuTreeExpandAll,
										   this.mnuTreeSep2,
										   this.mnuTreeNew});
      this.mnuTreeContext.Popup += new System.EventHandler(this.mnuTreeContext_Popup);
      // 
      // mnuTreeClear
      // 
      this.mnuTreeClear.Index = 0;
      this.mnuTreeClear.Text = "&Clear";
      this.mnuTreeClear.Click += new System.EventHandler(this.mnuTreeClear_Click);
      // 
      // mnuTreeDelete
      // 
      this.mnuTreeDelete.Index = 1;
      this.mnuTreeDelete.Text = "&Delete";
      this.mnuTreeDelete.Click += new System.EventHandler(this.mnuTreeDelete_Click);
      // 
      // mnuTreeRename
      // 
      this.mnuTreeRename.Index = 2;
      this.mnuTreeRename.Text = "&Rename";
      this.mnuTreeRename.Click += new System.EventHandler(this.mnuTreeRename_Click);
      // 
      // mnuTreeSave
      // 
      this.mnuTreeSave.Index = 3;
      this.mnuTreeSave.Text = "&Save Changes";
      this.mnuTreeSave.Click += new System.EventHandler(this.mnuTreeSave_Click);
      // 
      // mnuTreeUndo
      // 
      this.mnuTreeUndo.Enabled = false;
      this.mnuTreeUndo.Index = 4;
      this.mnuTreeUndo.Text = "&Undo Changes";
      this.mnuTreeUndo.Click += new System.EventHandler(this.mnuTreeUndo_Click);
      // 
      // mnuTreeSep
      // 
      this.mnuTreeSep.Index = 5;
      this.mnuTreeSep.Text = "-";
      // 
      // mnuTreeCollapse
      // 
      this.mnuTreeCollapse.Index = 6;
      this.mnuTreeCollapse.Text = "Co&llapse";
      this.mnuTreeCollapse.Click += new System.EventHandler(this.mnuTreeCollapse_Click);
      // 
      // mnuTreeExpand
      // 
      this.mnuTreeExpand.Index = 7;
      this.mnuTreeExpand.Text = "&Expand";
      this.mnuTreeExpand.Click += new System.EventHandler(this.mnuTreeExpand_Click);
      // 
      // mnuTreeExpandAll
      // 
      this.mnuTreeExpandAll.Index = 8;
      this.mnuTreeExpandAll.Text = "E&xpand All";
      this.mnuTreeExpandAll.Click += new System.EventHandler(this.mnuTreeExpandAll_Click);
      // 
      // mnuTreeSep2
      // 
      this.mnuTreeSep2.Index = 9;
      this.mnuTreeSep2.Text = "-";
      this.mnuTreeSep2.Visible = false;
      // 
      // mnuTreeNew
      // 
      this.mnuTreeNew.Index = 10;
      this.mnuTreeNew.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
									       this.mnuTreeNewFolder,
									       this.mnuTreeNewMacro});
      this.mnuTreeNew.Text = "&New";
      this.mnuTreeNew.Visible = false;
      // 
      // mnuTreeNewFolder
      // 
      this.mnuTreeNewFolder.Index = 0;
      this.mnuTreeNewFolder.Text = "&Folder";
      this.mnuTreeNewFolder.Click += new System.EventHandler(this.mnuTreeNewFolder_Click);
      // 
      // mnuTreeNewMacro
      // 
      this.mnuTreeNewMacro.Index = 1;
      this.mnuTreeNewMacro.Text = "&Macro";
      this.mnuTreeNewMacro.Click += new System.EventHandler(this.mnuTreeNewMacro_Click);
      // 
      // ilBrowserIcons
      // 
      this.ilBrowserIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
      this.ilBrowserIcons.ImageSize = new System.Drawing.Size(16, 16);
      this.ilBrowserIcons.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // pnlProperties
      // 
      this.pnlProperties.Controls.Add(this.grpMacroCommands);
      this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Right;
      this.pnlProperties.Location = new System.Drawing.Point(394, 0);
      this.pnlProperties.Name = "pnlProperties";
      this.pnlProperties.Size = new System.Drawing.Size(396, 412);
      this.pnlProperties.TabIndex = 1;
      // 
      // grpMacroCommands
      // 
      this.grpMacroCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
	| System.Windows.Forms.AnchorStyles.Right)));
      this.grpMacroCommands.Controls.Add(this.txtCommand6);
      this.grpMacroCommands.Controls.Add(this.txtCommand5);
      this.grpMacroCommands.Controls.Add(this.txtCommand4);
      this.grpMacroCommands.Controls.Add(this.txtCommand3);
      this.grpMacroCommands.Controls.Add(this.txtCommand2);
      this.grpMacroCommands.Controls.Add(this.txtCommand1);
      this.grpMacroCommands.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.grpMacroCommands.Location = new System.Drawing.Point(8, 4);
      this.grpMacroCommands.Name = "grpMacroCommands";
      this.grpMacroCommands.Size = new System.Drawing.Size(380, 216);
      this.grpMacroCommands.TabIndex = 0;
      this.grpMacroCommands.TabStop = false;
      this.grpMacroCommands.Text = "Macro Commands";
      // 
      // txtCommand6
      // 
      this.txtCommand6.AcceptsTab = true;
      this.txtCommand6.ContextMenu = this.mnuTextContext;
      this.txtCommand6.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.txtCommand6.Location = new System.Drawing.Point(8, 180);
      this.txtCommand6.Name = "txtCommand6";
      this.txtCommand6.Size = new System.Drawing.Size(364, 26);
      this.txtCommand6.TabIndex = 5;
      this.txtCommand6.Text = "";
      this.txtCommand6.TextChanged += new System.EventHandler(this.txtCommand_TextChanged);
      // 
      // mnuTextContext
      // 
      this.mnuTextContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
										   this.mnuTextClear,
										   this.mnuTextCopy,
										   this.mnuTextCut,
										   this.mnuTextPaste,
										   this.mnuTextSep,
										   this.mnuTextInsert});
      this.mnuTextContext.Popup += new System.EventHandler(this.mnuTextContext_Popup);
      // 
      // mnuTextClear
      // 
      this.mnuTextClear.Index = 0;
      this.mnuTextClear.Text = "C&lear";
      this.mnuTextClear.Click += new System.EventHandler(this.mnuTextClear_Click);
      // 
      // mnuTextCopy
      // 
      this.mnuTextCopy.Index = 1;
      this.mnuTextCopy.Text = "&Copy";
      this.mnuTextCopy.Click += new System.EventHandler(this.mnuTextCopy_Click);
      // 
      // mnuTextCut
      // 
      this.mnuTextCut.Index = 2;
      this.mnuTextCut.Text = "Cu&t";
      this.mnuTextCut.Click += new System.EventHandler(this.mnuTextCut_Click);
      // 
      // mnuTextPaste
      // 
      this.mnuTextPaste.Index = 3;
      this.mnuTextPaste.Text = "&Paste";
      this.mnuTextPaste.Click += new System.EventHandler(this.mnuTextPaste_Click);
      // 
      // mnuTextSep
      // 
      this.mnuTextSep.Index = 4;
      this.mnuTextSep.Text = "-";
      // 
      // mnuTextInsert
      // 
      this.mnuTextInsert.Index = 5;
      this.mnuTextInsert.Text = "&Insert...";
      this.mnuTextInsert.Visible = false;
      this.mnuTextInsert.Click += new System.EventHandler(this.mnuTextInsert_Click);
      // 
      // txtCommand5
      // 
      this.txtCommand5.AcceptsTab = true;
      this.txtCommand5.ContextMenu = this.mnuTextContext;
      this.txtCommand5.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.txtCommand5.Location = new System.Drawing.Point(8, 148);
      this.txtCommand5.Name = "txtCommand5";
      this.txtCommand5.Size = new System.Drawing.Size(364, 26);
      this.txtCommand5.TabIndex = 4;
      this.txtCommand5.Text = "";
      this.txtCommand5.TextChanged += new System.EventHandler(this.txtCommand_TextChanged);
      // 
      // txtCommand4
      // 
      this.txtCommand4.AcceptsTab = true;
      this.txtCommand4.ContextMenu = this.mnuTextContext;
      this.txtCommand4.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.txtCommand4.Location = new System.Drawing.Point(8, 116);
      this.txtCommand4.Name = "txtCommand4";
      this.txtCommand4.Size = new System.Drawing.Size(364, 26);
      this.txtCommand4.TabIndex = 3;
      this.txtCommand4.Text = "";
      this.txtCommand4.TextChanged += new System.EventHandler(this.txtCommand_TextChanged);
      // 
      // txtCommand3
      // 
      this.txtCommand3.AcceptsTab = true;
      this.txtCommand3.ContextMenu = this.mnuTextContext;
      this.txtCommand3.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.txtCommand3.Location = new System.Drawing.Point(8, 84);
      this.txtCommand3.Name = "txtCommand3";
      this.txtCommand3.Size = new System.Drawing.Size(364, 26);
      this.txtCommand3.TabIndex = 2;
      this.txtCommand3.Text = "";
      this.txtCommand3.TextChanged += new System.EventHandler(this.txtCommand_TextChanged);
      // 
      // txtCommand2
      // 
      this.txtCommand2.AcceptsTab = true;
      this.txtCommand2.ContextMenu = this.mnuTextContext;
      this.txtCommand2.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.txtCommand2.Location = new System.Drawing.Point(8, 52);
      this.txtCommand2.Name = "txtCommand2";
      this.txtCommand2.Size = new System.Drawing.Size(364, 26);
      this.txtCommand2.TabIndex = 1;
      this.txtCommand2.Tag = "";
      this.txtCommand2.Text = "";
      this.txtCommand2.TextChanged += new System.EventHandler(this.txtCommand_TextChanged);
      // 
      // txtCommand1
      // 
      this.txtCommand1.AcceptsTab = true;
      this.txtCommand1.ContextMenu = this.mnuTextContext;
      this.txtCommand1.Font = new System.Drawing.Font("Lucida Sans Unicode", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.txtCommand1.Location = new System.Drawing.Point(8, 20);
      this.txtCommand1.Name = "txtCommand1";
      this.txtCommand1.Size = new System.Drawing.Size(364, 26);
      this.txtCommand1.TabIndex = 0;
      this.txtCommand1.Tag = "";
      this.txtCommand1.Text = "";
      this.txtCommand1.TextChanged += new System.EventHandler(this.txtCommand_TextChanged);
      // 
      // MainWindow
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(790, 412);
      this.Controls.Add(this.tvMacroTree);
      this.Controls.Add(this.pnlProperties);
      this.MaximizeBox = false;
      this.Name = "MainWindow";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "FFXI Macro Manager";
      this.pnlProperties.ResumeLayout(false);
      this.grpMacroCommands.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    #region Menu Events

    private void AutoTransMenuItem_Click(object sender, EventArgs e) {
    ATMenuItem MI = sender as ATMenuItem;
      if (MI != null) {
      ContextMenu TopMenu = MI.GetContextMenu();
	if (TopMenu != null) {
	TextBoxBase Owner = TopMenu.SourceControl as TextBoxBase;
	  if (Owner != null) {
	  string NewText = Owner.Text.Substring(0, Owner.SelectionStart);
	    NewText += String.Format("{0}[{1:X8}] {2}{3}", FFXIEncoding.SpecialMarkerStart, MI.ResourceID, MI.Text, FFXIEncoding.SpecialMarkerEnd);
	  int caretpos = NewText.Length;
	    NewText += Owner.Text.Substring(Owner.SelectionStart + Owner.SelectionLength);
	    Owner.Text = NewText;
	    Owner.SelectionStart = caretpos;
	    Owner.SelectionLength = 0;
	  }
	}
      }
    }

    private void InsertMenuItem_Click(object sender, EventArgs e) {
    MenuItem MI = sender as MenuItem;
      if (MI != null) {
      ContextMenu TopMenu = MI.GetContextMenu();
	if (TopMenu != null) {
	TextBoxBase Owner = TopMenu.SourceControl as TextBoxBase;
	  if (Owner != null) {
	  string MenuText = MI.Text;
	    if (MenuText == "&&" || MenuText == "- ")
	      MenuText = MenuText.Substring(0, 1);
	  string NewText = Owner.Text.Substring(0, Owner.SelectionStart) + MenuText;
	  int caretpos = NewText.Length;
	    NewText += Owner.Text.Substring(Owner.SelectionStart + Owner.SelectionLength);
	    Owner.Text = NewText;
	    Owner.SelectionStart = caretpos;
	    Owner.SelectionLength = 0;
	  }
	}
      }
    }

    private void mnuTextContext_Popup(object sender, System.EventArgs e) {
      // Currently, there's nothing to dynamically alter in this menu
    }

    private void mnuTextClear_Click(object sender, System.EventArgs e) {
    MenuItem MI = sender as MenuItem;
      if (MI != null) {
      ContextMenu TopMenu = MI.GetContextMenu();
	if (TopMenu != null) {
	TextBoxBase Owner = TopMenu.SourceControl as TextBoxBase;
	  if (Owner != null)
	    Owner.Clear();
	}
      }
    }

    private void mnuTextCopy_Click(object sender, System.EventArgs e) {
    MenuItem MI = sender as MenuItem;
      if (MI != null) {
      ContextMenu TopMenu = MI.GetContextMenu();
	if (TopMenu != null) {
	TextBoxBase Owner = TopMenu.SourceControl as TextBoxBase;
	  if (Owner != null)
	    Clipboard.SetDataObject(Owner.SelectedText, true);
	}
      }
    }

    private void mnuTextCut_Click(object sender, System.EventArgs e) {
    MenuItem MI = sender as MenuItem;
      if (MI != null) {
      ContextMenu TopMenu = MI.GetContextMenu();
	if (TopMenu != null) {
	TextBoxBase Owner = TopMenu.SourceControl as TextBoxBase;
	  if (Owner != null) {
	    Clipboard.SetDataObject(Owner.SelectedText, true);
	    if (Owner.SelectedText != String.Empty) {
	    string NewText = Owner.Text.Substring(0, Owner.SelectionStart);
	      NewText += Owner.Text.Substring(Owner.SelectionStart + Owner.SelectionLength);
	    int caretpos = Owner.SelectionStart;
	      Owner.Text = NewText;
	      Owner.SelectionStart = caretpos;
	      Owner.SelectionLength = 0;
	    }
	  }
	}
      }
    }

    private void mnuTextPaste_Click(object sender, System.EventArgs e) {
    MenuItem MI = sender as MenuItem;
      if (MI != null) {
      ContextMenu TopMenu = MI.GetContextMenu();
	if (TopMenu != null) {
	TextBoxBase Owner = TopMenu.SourceControl as TextBoxBase;
	  if (Owner != null) {
	  string NewText = Owner.Text.Substring(0, Owner.SelectionStart);
	    NewText += Clipboard.GetDataObject().GetData(typeof(String));
	  int caretpos = NewText.Length;
	    NewText += Owner.Text.Substring(Owner.SelectionStart + Owner.SelectionLength);
	    Owner.Text = NewText;
	    Owner.SelectionStart = caretpos;
	    Owner.SelectionLength = 0;
	  }
	}
      }
    }

    private TreeNode ContextNode = null;

    private void mnuTreeContext_Popup(object sender, System.EventArgs e) {
      this.ContextNode = this.tvMacroTree.GetNodeAt(this.tvMacroTree.PointToClient(Control.MousePosition));
      this.mnuTreeRename.Enabled = this.IsRenameAllowed(this.ContextNode);
      this.mnuTreeClear.Enabled  = this.IsClearAllowed (this.ContextNode);
      this.mnuTreeDelete.Enabled = this.IsDeleteAllowed(this.ContextNode);
      this.mnuTreeSave.Enabled   = this.IsSaveAllowed  (this.ContextNode);
      //this.mnuTreeUndo.Enabled   = this.mnuTreeSave.Enabled;
      // The "New" portion is optional; use Visible rather than Enabled
      this.mnuTreeSep2.Visible = this.mnuTreeNew.Visible = (this.ContextNode.Tag is MacroFolder && !(this.ContextNode.Tag as MacroFolder).Locked);
    }

    private void mnuTreeClear_Click(object sender, System.EventArgs e) {
      this.DoClear(this.ContextNode, true);
    }

    private void mnuTreeCollapse_Click(object sender, System.EventArgs e) {
      this.ContextNode.Collapse();
    }

    private void mnuTreeDelete_Click(object sender, System.EventArgs e) {
      this.DoDelete(this.ContextNode, true);
    }

    private void mnuTreeExpand_Click(object sender, System.EventArgs e) {
      this.ContextNode.Expand();
    }

    private void mnuTreeExpandAll_Click(object sender, System.EventArgs e) {
      this.ContextNode.ExpandAll();
    }

    private void mnuTreeNewFolder_Click(object sender, System.EventArgs e) {
      this.DoNewFolder(this.ContextNode, true);
    }

    private void mnuTreeNewMacro_Click(object sender, System.EventArgs e) {
      this.DoNewMacro(this.ContextNode, true);
    }

    private void mnuTreeRename_Click(object sender, System.EventArgs e) {
      this.ContextNode.BeginEdit();
    }

    private void mnuTreeSave_Click(object sender, System.EventArgs e) {
      this.DoSave(this.ContextNode, true);
    }

    private void mnuTreeUndo_Click(object sender, System.EventArgs e) {
      this.DoUndo(this.ContextNode, true);
    }

    #endregion

    #region TreeView Events

    private void tvMacroTree_BeforeLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e) {
      if (!this.IsRenameAllowed(e.Node))
	e.CancelEdit = true;
    }

    private void tvMacroTree_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e) {
      if (e.Label == null) // Use made no changes to the label text
	return;
    Character   C  = e.Node.Tag as Character;   if (C  != null) { C.Name  = e.Label; return; }
    MacroFolder MF = e.Node.Tag as MacroFolder; if (MF != null) { MF.Name = e.Label; return; }
    Macro       M  = e.Node.Tag as Macro;       if (M  != null) { M.Name  = e.Label; return; }
    }

    private void tvMacroTree_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e) {
    }

    private void tvMacroTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {
      if (this.tvMacroTree.SelectedNode != null)
	this.ShowProperties(this.tvMacroTree.SelectedNode.Tag as Macro);
    }

    private void tvMacroTree_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
      if (this.tvMacroTree.SelectedNode != null) {
	if (!e.Shift && !e.Control && !e.Alt) { // Unmodified Keys
	  if (e.KeyData == Keys.F2) // F2 = Rename
	    this.tvMacroTree.SelectedNode.BeginEdit();
	  else if (e.KeyData == Keys.Delete) {
	    if (this.IsDeleteAllowed(this.tvMacroTree.SelectedNode))
	      this.DoDelete(this.tvMacroTree.SelectedNode, true);
	    else if (this.IsClearAllowed(this.tvMacroTree.SelectedNode))
	      this.DoClear(this.tvMacroTree.SelectedNode, true);
	  }
	}
      }
    }

    #endregion

    #region Drag & Drop Events

    private void tvMacroTree_DragDrop(object sender, System.Windows.Forms.DragEventArgs e) {
    TreeNode DropNode = this.tvMacroTree.GetNodeAt(this.tvMacroTree.PointToClient(new Point(e.X, e.Y)));
      if (e.Data.GetDataPresent(typeof(TreeNode))) {
      TreeNode DragNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;
	if (DropNode.Tag is MacroFolder) {
	MacroFolder DropFolder = DropNode.Tag as MacroFolder;
	  if (e.Effect == DragDropEffects.Move) {
	    if (DragNode.Tag is MacroFolder) {
	    MacroFolder DragFolder = DragNode.Tag as MacroFolder;
	      (DragNode.Parent.Tag as MacroFolder).Folders.Remove(DragFolder);
	      DropFolder.Folders.Add(DragFolder);
	    }
	    else if (DragNode.Tag is Macro) {
	    Macro DragMacro = DragNode.Tag as Macro;
	      (DragNode.Parent.Tag as MacroFolder).Macros.Remove(DragMacro);
	      DropFolder.Macros.Add(DragMacro);
	    }
	    // Relocate the tree node
	    DragNode.Parent.Nodes.Remove(DragNode);
	    DropNode.Nodes.Add(DragNode);
	    this.tvMacroTree.SelectedNode = DragNode;
	  }
	  else { // Insert copy of dragged item under the target folder
	    if (DragNode.Tag is Macro) {
	    Macro DragMacro = (DragNode.Tag as Macro);
	    Macro CopiedMacro = DragMacro.Clone();
	      DropFolder.Macros.Add(CopiedMacro);
	      this.tvMacroTree.SelectedNode = this.AddMacroNode(CopiedMacro, DropNode);
	    }
	    else if (DragNode.Tag is MacroFolder) {
	    MacroFolder DragFolder = (DragNode.Tag as MacroFolder);
	    MacroFolder CopiedFolder = DragFolder.Clone();
	      DropFolder.Folders.Add(CopiedFolder);
	      this.tvMacroTree.SelectedNode = this.AddMacroFolderNode(CopiedFolder, DropNode);
	    }
	    else if (DragNode.Tag is Character) {
	    Character C = DragNode.Tag as Character;
	    MacroFolder CopiedCharacter = new MacroFolder(String.Format("Macros for {0}", C.Name));
	      foreach (MacroFolder Bar in C.MacroBars)
		CopiedCharacter.Folders.Add(Bar.Clone());
	      DropFolder.Folders.Add(CopiedCharacter);
	      this.tvMacroTree.SelectedNode = this.AddMacroFolderNode(CopiedCharacter, DropNode);
	    }
	  }
	}
	else if (DropNode.Tag is Macro) { // Replace macro contents
	Macro Source = DragNode.Tag as Macro;
	Macro Target = DropNode.Tag as Macro;
	  if (Target.Empty || MessageBox.Show(this, String.Format("Are you sure you want to replace the '{0}' macro with the '{1}' macro?", Target.Name, Source.Name), "Confirm Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
	    Target.Name = Source.Name;
	    for (int i = 0; i < 6; ++i)
	      Target.Commands[i] = Source.Commands[i];
	    this.RefreshMacroNode(DropNode);
	    this.tvMacroTree.SelectedNode = DropNode;
	  }
	}
      }
      else if (e.Data.GetDataPresent("FileDrop")) { // allow dropping macro bars onto the macro library
	try {
	String[] FileNames = e.Data.GetData("FileDrop") as String[];
	  foreach (String MacroBarFile in FileNames)
	    this.AddMacroBar(MacroBarFile, DropNode);
	}
	catch { }
      }
    }

    private void tvMacroTree_DragOver(object sender, System.Windows.Forms.DragEventArgs e) {
      e.Effect = DragDropEffects.None;
    TreeNode DropNode = this.tvMacroTree.GetNodeAt(this.tvMacroTree.PointToClient(new Point(e.X, e.Y)));
      if (e.Data.GetDataPresent(typeof(TreeNode))) {
      TreeNode DragNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;
	if (DropNode != null && DragNode != DropNode) {
	  if (DropNode.Tag is MacroFolder) {
	    // We currently don't handle dropping anything on a locked folder.
	    if (!(DropNode.Tag as MacroFolder).Locked) {
	      e.Effect = DragDropEffects.Copy;
	      if ((e.AllowedEffect & DragDropEffects.Move) != 0 && (e.KeyState & 0x2c) != 0x08)
		e.Effect = DragDropEffects.Move;
	    }
	  }
	  else if (DropNode.Tag is Macro && DragNode.Tag is Macro)
	    e.Effect = DragDropEffects.Copy;
	}
      }
      else if (e.Data.GetDataPresent("FileDrop")) {
	if (DropNode != null && DropNode.Tag is MacroFolder && !(DropNode.Tag as MacroFolder).Locked)
	  e.Effect = DragDropEffects.Copy;
      }
    }

    private void tvMacroTree_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e) {
    TreeNode DragNode = e.Item as TreeNode;
      if (DragNode == null)
	return;
      if (DragNode.Tag is Character || DragNode.Tag is Macro || (DragNode.Tag is MacroFolder && DragNode.Parent != null)) {
      DragDropEffects AllowedEffects = DragDropEffects.Copy;
	{ // If the source folder isn't locked, allow moves
	TreeNode SourceFolderNode = DragNode;
	  if (DragNode.Tag is Macro)
	    SourceFolderNode = DragNode.Parent;
	  if (SourceFolderNode.Tag is MacroFolder && !(SourceFolderNode.Tag as MacroFolder).Locked)
	    AllowedEffects |= DragDropEffects.Move;
	}
	this.DoDragDrop(DragNode, AllowedEffects);
      }
    }

    #endregion

    #region Other Events

    private void txtCommand_TextChanged(object sender, System.EventArgs e) {
    TextBoxBase TBB = sender as TextBoxBase;
      if (TBB != null && TBB.Enabled) {
      Macro M = this.tvMacroTree.SelectedNode.Tag as Macro;
	if (M != null)
	  M.Commands[TBB.TabIndex] = TBB.Text;
      }
    }

    #endregion

    private InsertDialog InsertDialog;

    private void mnuTextInsert_Click(object sender, System.EventArgs e) {
      if (this.InsertDialog == null)
	this.InsertDialog = new InsertDialog();
      //this.InsertDialog.Reset();
      this.InsertDialog.ShowDialog(this);
    }

  }
}
