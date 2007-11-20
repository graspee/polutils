// $Id$

using System;
using System.Collections;

namespace PlayOnline.FFXI {

  public class MacroFolderCollection : CollectionBase {

    public void Add     (MacroFolder MF) {        this.InnerList.Add     (MF); }
    public bool Contains(MacroFolder MF) { return this.InnerList.Contains(MF); }
    public int  IndexOf (MacroFolder MF) { return this.InnerList.IndexOf (MF); }
    public void Remove  (MacroFolder MF) {        this.InnerList.Remove  (MF); }

    public MacroFolder this[int Index] {
      get { return this.InnerList[Index] as MacroFolder; }
      set { this.InnerList[Index] = value; }
    }

  }

  public class MacroCollection : CollectionBase {

    public void Add     (Macro M) {        this.InnerList.Add     (M); }
    public bool Contains(Macro M) { return this.InnerList.Contains(M); }
    public int  IndexOf (Macro M) { return this.InnerList.IndexOf (M); }
    public void Remove  (Macro M) {        this.InnerList.Remove  (M); }

    public Macro this[int Index] {
      get { return this.InnerList[Index] as Macro; }
      set { this.InnerList[Index] = value; }
    }

  }

}
