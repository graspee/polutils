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
using System.Text;
using System.Windows.Forms;
using PlayOnline.Core.Audio;

namespace PlayOnline.Utils.AudioManager {

  public partial class WaveWriter : Form {

    public WaveWriter(AudioFile AF, string TargetFile) {
      InitializeComponent();
      this.prbBytesWritten.Select();
      this.txtSource.Text = AF.Path;
      this.txtTarget.Text = TargetFile;
      this.AF  = AF;
      this.AFS = AF.OpenStream(false);
      this.FS  = new FileStream(TargetFile, FileMode.Create, FileAccess.Write);
    }

    private AudioFile       AF;
    private AudioFileStream AFS;
    private FileStream      FS;

    private readonly int ChunkSize = 16 * 1024;

    private void WriteWave() {
      this.prbBytesWritten.Maximum = (int) this.AFS.Length;
      this.prbBytesWritten.Maximum += 0x2c;
      if (this.AF.Looped)
        this.prbBytesWritten.Maximum += 0x48; // For the "Loop Start" cue marker
      { // Write WAV header
      var BW = new BinaryWriter(this.FS, Encoding.ASCII);
        // File Header
        BW.Write("RIFF".ToCharArray());
        BW.Write(this.prbBytesWritten.Maximum);
        // Wave Format Header
        BW.Write("WAVEfmt ".ToCharArray());
        BW.Write((int) 0x10);
        // Wave Format Data
        BW.Write((short) 1); // PCM
        BW.Write((short) this.AF.Channels);
        BW.Write((int)   this.AF.SampleRate);
        BW.Write((int)   (2 * this.AF.Channels * this.AF.SampleRate)); // bytes per second
        BW.Write((short) (2 * this.AF.Channels)); // bytes per sample
        BW.Write((short) 16); // bits
        // Wave Data Header
        BW.Write("data".ToCharArray());
        BW.Write((int) this.AFS.Length);
      }
      this.prbBytesWritten.Value = 0x2c;
      // Write PCM data
    var data = new byte[this.ChunkSize];
      while (true) {
      int read = AFS.Read(data, 0, this.ChunkSize);
        this.prbBytesWritten.Value += read;
        FS.Write(data, 0, read);
        if (read != this.ChunkSize)
          break;
      }
      AFS.Close();
      // Write "Loop Start" cue marker
      if (this.AF.Looped) {
      var BW = new BinaryWriter(this.FS, Encoding.ASCII);
        BW.Write("cue ".ToCharArray());
        BW.Write((int) 0x1c);
        BW.Write((int) 1);
        BW.Write((int) 1);
        BW.Write((int) (this.AF.LoopStart * this.AF.SampleRate));
        BW.Write("data".ToCharArray());
        BW.Write((int) 0);
        BW.Write((int) 0);
        BW.Write((int) (this.AF.LoopStart * this.AF.SampleRate));
        BW.Write("LIST".ToCharArray());
        BW.Write((int) 0x10 + 12);
        BW.Write("adtl".ToCharArray());
        BW.Write("labl".ToCharArray());
        BW.Write((int) 0x04 + 12);
        BW.Write((int) 1);
        BW.Write("Loop Start\0\0".ToCharArray());
        this.prbBytesWritten.Value += 0x48;
      }
      FS.Close();
    }

    private void WaveWriter_Activated(object sender, System.EventArgs e) {
      this.Refresh();
      Application.DoEvents();
      if (this.AFS == null)
        MessageBox.Show(this, "Could not create input audio stream", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      else if (this.FS == null)
        MessageBox.Show(this, "Could not create output file stream", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      else
        this.WriteWave();
      this.Close();
    }

  }

}
