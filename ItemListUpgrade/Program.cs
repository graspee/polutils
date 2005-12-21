// $Id$

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;

namespace ItemListUpgrade {

  internal class Program {

    private OpenFileDialog dlgOldFile = new OpenFileDialog();
    private SaveFileDialog dlgNewFile = new SaveFileDialog();

    private Program() {
      // Prepare dialogs
      this.dlgOldFile.DefaultExt = "xml";
      this.dlgOldFile.Filter = this.GetText("FileFilter");
      this.dlgOldFile.Title = this.GetText("Title:OldFile");
      this.dlgNewFile.DefaultExt = "xml";
      this.dlgNewFile.Filter = this.GetText("FileFilter");
      this.dlgNewFile.Title = this.GetText("Title:NewFile");
    }

    private void Run() {
     if (this.dlgOldFile.ShowDialog() != DialogResult.OK)
	return;
      if (this.dlgNewFile.ShowDialog() != DialogResult.OK)
	return;
      this.PerformUpgrade(this.dlgOldFile.FileName, this.dlgNewFile.FileName);
    }

    #region I18N

    private ResourceManager Resources = new ResourceManager("Messages", Assembly.GetExecutingAssembly());

    private string GetText(string Name) {
    string ResourceString = this.Resources.GetObject(Name, CultureInfo.CurrentUICulture) as string;
      if (ResourceString == null)
	ResourceString = this.Resources.GetObject(Name, CultureInfo.InvariantCulture) as string;
      if (ResourceString == null)
	return Name;
      else
	return ResourceString;
    }

    #endregion

    #region Applying the XSLT transform

    private XslCompiledTransform UpgradeTransform = null;

    private void PrepareTransform() {
      if (this.UpgradeTransform != null)
	return;
      try {
	this.UpgradeTransform = new XslCompiledTransform();
      XmlReader XR = new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ItemListUpgrade.xslt"));
	this.UpgradeTransform.Load(XR);
	XR.Close();
      } catch {
	this.UpgradeTransform = null;
      }
    }

    private void PerformUpgrade(string OldListFile, string NewListFile) {
      this.PrepareTransform();
      if (this.UpgradeTransform != null) {
	try {
	XmlDocument XD = new XmlDocument();
	  XD.Load(OldListFile);
	XmlWriter XW = XmlTextWriter.Create(NewListFile, this.UpgradeTransform.OutputSettings);
	  this.UpgradeTransform.Transform(XD, XW);
	  XW.Close();
	  MessageBox.Show(null, this.GetText("UpgradeSuccess"), this.GetText("Title:UpgradeComplete"), MessageBoxButtons.OK, MessageBoxIcon.Information);
	} catch (Exception E) {
	  MessageBox.Show(null, String.Format(this.GetText("UpgradeFailed"), E.Message), this.GetText("Title:UpgradeFailed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
      }
      else
	MessageBox.Show(null, this.GetText("PrepareFailed"), this.GetText("Title:UpgradeFailed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    #endregion

    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
    Program P = new Program();
      P.Run();
    }

  }

}