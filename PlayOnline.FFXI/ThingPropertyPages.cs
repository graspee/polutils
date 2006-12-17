using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PlayOnline.FFXI {

  public partial class ThingPropertyPages: Form {

    private int DeltaW;
    private int DeltaH;

    public ThingPropertyPages(Things.IThing T) {
      this.InitializeComponent();
      // Use the dummy page to get size deltas, then discard it
      this.DeltaW = this.Width  - this.tabDummy.Width;
      this.DeltaH = this.Height - this.tabDummy.Height;
      this.tabPages.TabPages.Clear();
      this.tabDummy.Dispose();
      this.tabDummy = null;
      // Add pages as needed
      foreach (PropertyPages.IThing P in T.GetPropertyPages()) {
	P.Left = 0;
	P.Top  = 0;
	if (this.tabPages.TabPages.Count == 0) { // Resize to match the first page (even if not fixed-size)
	  this.Width  = P.Width  + this.DeltaW;
	  this.Height = P.Height + this.DeltaH;
	}
	if (!P.IsFixedSize)
	  P.Dock = DockStyle.Fill;
      TabPage TP = new ThemedTabPage(P.TabName);
	TP.UseVisualStyleBackColor = true;
	TP.Controls.Add(P);
	TP.Tag = P;
	this.tabPages.TabPages.Add(TP);
      }
      this.AdjustSize();
    }

    private void AdjustSize() {
      if (this.tabPages.SelectedTab == null)
	return;
    PropertyPages.IThing PP = this.tabPages.SelectedTab.Tag as PropertyPages.IThing;
      if (PP.IsFixedSize) { // Size change required
	this.Width  = PP.Width  + this.DeltaW;
	this.Height = PP.Height + this.DeltaH;
      }
    }

    private void btnClose_Click(object sender, EventArgs e) {
      this.Close();
    }

    private void tabPages_SelectedIndexChanged(object sender, EventArgs e) {
      this.AdjustSize();
    }

  }

}