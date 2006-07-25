using System;
using System.IO;
using System.Collections.Generic;

using PlayOnline.Core;

namespace PlayOnline.FFXI.FileTypes {

  public class SimpleStringTable : FileType {

    public override ThingList Load(BinaryReader BR, ProgressCallback ProgressCallback) {
    ThingList TL = new ThingList();
      ProgressCallback(I18N.GetText("FTM:CheckingFile"), 0);
      if ((BR.BaseStream.Length % 0x40) != 0 || BR.BaseStream.Position != 0)
	return TL;
    long EntryCount = BR.BaseStream.Length / 0x40;
      ProgressCallback(I18N.GetText("FTM:LoadingData"), 0);
      for (int i = 0; i < EntryCount; ++i) {
      FFXI.SimpleStringTableEntry SSTE = new FFXI.SimpleStringTableEntry();
	if (!SSTE.Read(BR)) {
	  TL.Clear();
	  break;
	}
	ProgressCallback(null, (double) (i + 1) / EntryCount);
	TL.Add(SSTE);
      }
      return TL;
    }

  }

}
