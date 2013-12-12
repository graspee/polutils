// $Id$

// Copyright � 2004-2010 Tim Van Holder
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS"
// BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.IO;
using System.Threading;
using System.Windows.Forms;
using PlayOnline.Core;

namespace PlayOnline.FFXI.Utils.DataBrowser {

  internal class FileScanner {

    private FileScanDialog FSD = new FileScanDialog();

    public ThingList FileContents = null;

    public void ScanFile(IWin32Window ParentForm, string FileName) {
      lock (this.FSD) {
        if (FileName != null && File.Exists(FileName)) {
          this.FSD = new FileScanDialog();
          var T = new Thread(() => {
            try {
              Application.DoEvents();
              while (!this.FSD.Visible) {
                Thread.Sleep(0);
                Application.DoEvents();
              }
              this.FSD.Invoke(new AnonymousMethod(() => this.FSD.ResetProgress()));
              this.FileContents = FileType.LoadAll(FileName, (msg, pct) => this.FSD.Invoke(new AnonymousMethod(() => this.FSD.SetProgress(msg, pct))));
              this.FSD.Invoke(new AnonymousMethod(() => this.FSD.Finish()));
            } catch {
              this.FileContents = null;
            }
          });
          T.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
          T.Start();
          if (this.FSD.ShowDialog(ParentForm) == DialogResult.Abort) {
            this.FSD.Finish();
            this.FileContents = null;
          }
          if (T.IsAlive)
            T.Abort();
          this.FSD.Dispose();
          this.FSD = null;
        }
      }
    }

  }

}
