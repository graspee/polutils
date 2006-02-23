using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using PlayOnline.Core;

namespace PlayOnline.FFXI {

  public abstract class FileType {

    public delegate void ProgressCallback(string Message, double PercentCompleted);

    public static List<FileType> AllTypes {
      get {
      List<FileType> Results = new List<FileType>();
	Results.Add(new FileTypes.DialogTable());
	Results.Add(new FileTypes.XIStringTable());
	Results.Add(new FileTypes.SimpleStringTable());
	Results.Add(new FileTypes.SpellInfo());
	Results.Add(new FileTypes.AbilityInfo());
	Results.Add(new FileTypes.StatusInfo());
	Results.Add(new FileTypes.QuestInfo());
	Results.Add(new FileTypes.ItemData());
	Results.Add(new FileTypes.Images());
	return Results;
      }
    }

    public virtual string Name {
      get {
      string MessageID = String.Format("FT:{0}", this.GetType().Name);
      string Result = MessageID;
	try {
	  Result = I18N.GetText(MessageID, this.GetType().Assembly);
	} catch { }
	if (Result == MessageID)
	  Result = this.GetType().Name;
	return Result;
      }
    }

    public abstract ThingList Load(BinaryReader BR, ProgressCallback ProgressCallback);

    public virtual ThingList Load(string FileName, ProgressCallback ProgressCallback) {
      ProgressCallback(I18N.GetText("FTM:OpeningFile"), 0);
    BinaryReader BR = null;
      try {
	BR = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read), Encoding.ASCII);
      } catch { }
      if (BR == null || BR.BaseStream == null)
	return null;
    ThingList Results = this.Load(BR, ProgressCallback);
      BR.Close();
      return Results;
    }

    public static ThingList LoadAll(string FileName, ProgressCallback ProgressCallback, bool FirstMatchOnly) {
    ThingList Results = new ThingList();
      ProgressCallback(I18N.GetText("FTM:OpeningFile"), 0);
    BinaryReader BR = null;
      try {
	BR = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read), Encoding.ASCII);
      } catch { }
      if (BR == null || BR.BaseStream == null)
	return Results;
      foreach (FileType FT in FileType.AllTypes) {
      ProgressCallback SubCallback = new ProgressCallback(delegate (string Message, double PercentCompleted) {
	string SubMessage = null;
	  if (Message != null)
	    SubMessage = String.Format("[{0}] {1}", FT.Name, Message);
	  ProgressCallback(SubMessage, PercentCompleted);
	});
      ThingList SubResults = FT.Load(BR, SubCallback);
	if (SubResults != null) {
	  Results.AddRange(SubResults);
	  if (FirstMatchOnly && Results.Count > 0)
	    break;
	}
	BR.BaseStream.Seek(0, SeekOrigin.Begin);
      }
      return Results;
    }

    public static ThingList LoadAll(string FileName, ProgressCallback ProgressCallback) {
      return FileType.LoadAll(FileName, ProgressCallback, true);
    }

  }

}
