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
using System.IO;
using System.Collections.Generic;

using PlayOnline.Core;

namespace PlayOnline.FFXI.FileTypes {

  public class DMSGStringTable : FileType {

    public override ThingList Load(BinaryReader BR, ProgressCallback ProgressCallback) {
    ThingList TL = new ThingList();
      if (ProgressCallback != null)
	ProgressCallback(I18N.GetText("FTM:CheckingFile"), 0);
      if (BR.BaseStream.Length < 0x38 || BR.BaseStream.Position != 0)
	return TL;
    FFXIEncoding E = new FFXIEncoding();
      // Skip (presumably) fixed portion of the header
      if ((E.GetString(BR.ReadBytes(8)) != "d_msg".PadRight(8, '\0')) || BR.ReadUInt16() != 1 || BR.ReadUInt32() != 0 || BR.ReadUInt16() != 2 || BR.ReadUInt32() != 3)
	return TL;
      // Read the useful header fields
    uint EntryCount = BR.ReadUInt32();
      if (BR.ReadUInt32() != 1)
	return TL;
    uint FileSize = BR.ReadUInt32();
      if (FileSize != BR.BaseStream.Length)
	return TL;
    uint HeaderSize = BR.ReadUInt32();
      if (HeaderSize != 0x38)
	return TL;
    uint EntryBytes = BR.ReadUInt32();
      if (EntryBytes != EntryCount * 36)
	return TL;
    uint DataBytes  = BR.ReadUInt32();
      if (FileSize != 0x38 + EntryBytes + DataBytes)
	return TL;
      // 12 NUL bytes
      if (BR.ReadUInt32() != 0 || BR.ReadUInt32() != 0 || BR.ReadUInt32() != 0)
	return TL;
      if (ProgressCallback != null)
	ProgressCallback(I18N.GetText("FTM:LoadingData"), 0);
      for (uint i = 0; i < EntryCount; ++i) {
      Things.DMSGStringTableEntry DSTE = new Things.DMSGStringTableEntry();
	if (!DSTE.Read(BR, E, i, EntryBytes, DataBytes)) {
	  TL.Clear();
	  break;
	}
	if (ProgressCallback != null)
	  ProgressCallback(null, (double) (i + 1) / EntryCount);
	TL.Add(DSTE);
      }
      return TL;
    }

  }

}
