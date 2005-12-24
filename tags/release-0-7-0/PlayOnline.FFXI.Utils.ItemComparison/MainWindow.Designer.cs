namespace PlayOnline.FFXI.Utils.ItemComparison {

  public partial class MainWindow {

    #region Controls

    private PlayOnline.FFXI.FFXIItemEditor ieLeft;
    private PlayOnline.FFXI.FFXIItemEditor ieRight;
    private System.Windows.Forms.Button btnLoadItemSet1;
    private System.Windows.Forms.Button btnLoadItemSet2;
    private System.Windows.Forms.OpenFileDialog dlgLoadItems;
    private System.Windows.Forms.Button btnPrevious;
    private System.Windows.Forms.Button btnNext;
    private System.Windows.Forms.Button btnRemoveUnchanged;

    private System.ComponentModel.Container components = null;

    #endregion

    #region Windows Form Designer generated code

    protected override void Dispose(bool disposing) {
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent() {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
      this.ieLeft = new PlayOnline.FFXI.FFXIItemEditor();
      this.ieRight = new PlayOnline.FFXI.FFXIItemEditor();
      this.btnLoadItemSet1 = new System.Windows.Forms.Button();
      this.btnLoadItemSet2 = new System.Windows.Forms.Button();
      this.dlgLoadItems = new System.Windows.Forms.OpenFileDialog();
      this.btnPrevious = new System.Windows.Forms.Button();
      this.btnNext = new System.Windows.Forms.Button();
      this.btnRemoveUnchanged = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // ieLeft
      // 
      this.ieLeft.BackColor = System.Drawing.Color.Transparent;
      this.ieLeft.Item = null;
      resources.ApplyResources(this.ieLeft, "ieLeft");
      this.ieLeft.Name = "ieLeft";
      this.ieLeft.SizeChanged += new System.EventHandler(this.ItemViewerSizeChanged);
      // 
      // ieRight
      // 
      resources.ApplyResources(this.ieRight, "ieRight");
      this.ieRight.BackColor = System.Drawing.Color.Transparent;
      this.ieRight.Item = null;
      this.ieRight.Name = "ieRight";
      this.ieRight.SizeChanged += new System.EventHandler(this.ItemViewerSizeChanged);
      // 
      // btnLoadItemSet1
      // 
      resources.ApplyResources(this.btnLoadItemSet1, "btnLoadItemSet1");
      this.btnLoadItemSet1.Name = "btnLoadItemSet1";
      this.btnLoadItemSet1.Click += new System.EventHandler(this.btnLoadItemSet1_Click);
      // 
      // btnLoadItemSet2
      // 
      resources.ApplyResources(this.btnLoadItemSet2, "btnLoadItemSet2");
      this.btnLoadItemSet2.Name = "btnLoadItemSet2";
      this.btnLoadItemSet2.Click += new System.EventHandler(this.btnLoadItemSet2_Click);
      // 
      // dlgLoadItems
      // 
      resources.ApplyResources(this.dlgLoadItems, "dlgLoadItems");
      // 
      // btnPrevious
      // 
      resources.ApplyResources(this.btnPrevious, "btnPrevious");
      this.btnPrevious.Name = "btnPrevious";
      this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
      // 
      // btnNext
      // 
      resources.ApplyResources(this.btnNext, "btnNext");
      this.btnNext.Name = "btnNext";
      this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
      // 
      // btnRemoveUnchanged
      // 
      resources.ApplyResources(this.btnRemoveUnchanged, "btnRemoveUnchanged");
      this.btnRemoveUnchanged.Name = "btnRemoveUnchanged";
      this.btnRemoveUnchanged.Click += new System.EventHandler(this.btnRemoveUnchanged_Click);
      // 
      // MainWindow
      // 
      resources.ApplyResources(this, "$this");
      this.Controls.Add(this.btnRemoveUnchanged);
      this.Controls.Add(this.btnNext);
      this.Controls.Add(this.btnPrevious);
      this.Controls.Add(this.btnLoadItemSet2);
      this.Controls.Add(this.btnLoadItemSet1);
      this.Controls.Add(this.ieLeft);
      this.Controls.Add(this.ieRight);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.Name = "MainWindow";
      this.ResumeLayout(false);

    }

    #endregion

  }

}
