using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private async Task ExtractPacksFromFolderAsync()
    {
        string initial = "";
        try { if (File.Exists(ClientExe)) initial = Path.Combine(Path.GetDirectoryName(ClientExe), "pack"); } catch { }
        using (var sourceDialog = new FolderBrowserDialog { Description = "İçinde .idx ve .pak dosyaları bulunan pack klasörünü seçin", SelectedPath = Directory.Exists(initial) ? initial : "" })
        {
            if (sourceDialog.ShowDialog(this) != DialogResult.OK) return;
            string packDir = sourceDialog.SelectedPath;
            var packNames = Directory.GetFiles(packDir, "*.idx")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => File.Exists(Path.Combine(packDir, n + ".pak")))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
            if (packNames.Count == 0)
            {
                MessageBox.Show(this, "Bu klasörde eşleşen .idx ve .pak dosyası bulunamadı.", "Packları çıkar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var selected = ChoosePacks(packNames);
            if (selected == null || selected.Count == 0) return;
            using (var outputDialog = new FolderBrowserDialog { Description = "Çıkarılan pack klasörlerinin yazılacağı yeri seçin", SelectedPath = Directory.GetParent(packDir)?.FullName ?? packDir })
            {
                if (outputDialog.ShowDialog(this) != DialogResult.OK) return;
                string outputDir = Path.Combine(outputDialog.SelectedPath, "Cikarilan-Packlar");
                var process = memory.Field_0197?.Property_0268;
                if (process == null || process.HasExited)
                {
                    MessageBox.Show(this, "Pack dizinleri şifreli olduğu için önce oyunu açın ve uygulamadaki Oyuna bağlan düğmesine basın.", "Packları çıkar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                await ExtractSelectedPacksAsync(process, packDir, selected, outputDir);
            }
        }
    }

    private List<string> ChoosePacks(IList<string> packNames)
    {
        using (var dialog = new Form
        {
            Text = "Çıkarılacak packları seç",
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(520, 570),
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            BackColor = StudioSurface,
            ForeColor = StudioText
        })
        {
            var info = new Label { Text = "Çıkarmak istediğiniz paketleri işaretleyin.", Dock = DockStyle.Top, Height = 38, Padding = new Padding(12, 10, 0, 0), ForeColor = StudioMuted };
            var list = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, BackColor = StudioRaised, ForeColor = StudioText, BorderStyle = BorderStyle.FixedSingle };
            foreach (var name in packNames) list.Items.Add(name, true);
            var buttons = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = StudioSurface };
            var all = new Button { Text = "Tümünü seç", Left = 12, Top = 12, Width = 105, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = StudioRaised, ForeColor = StudioText };
            var none = new Button { Text = "Temizle", Left = 126, Top = 12, Width = 90, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = StudioRaised, ForeColor = StudioText };
            var cancel = new Button { Text = "İptal", Left = 318, Top = 12, Width = 85, Height = 32, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = StudioRaised, ForeColor = StudioText };
            var ok = new Button { Text = "Çıkar", Left = 412, Top = 12, Width = 90, Height = 32, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = StudioAccent, ForeColor = Color.Black };
            all.Click += (s, e) => { for (int i = 0; i < list.Items.Count; i++) list.SetItemChecked(i, true); };
            none.Click += (s, e) => { for (int i = 0; i < list.Items.Count; i++) list.SetItemChecked(i, false); };
            buttons.Controls.AddRange(new Control[] { all, none, cancel, ok });
            dialog.Controls.Add(list); dialog.Controls.Add(buttons); dialog.Controls.Add(info);
            dialog.AcceptButton = ok; dialog.CancelButton = cancel;
            if (dialog.ShowDialog(this) != DialogResult.OK) return null;
            return list.CheckedItems.Cast<string>().ToList();
        }
    }

    private async Task ExtractSelectedPacksAsync(Process process, string packDir, List<string> selected, string outputDir)
    {
        compatibilityPackExtractButton.Enabled = false;
        compatibilityScanButton.Enabled = false;
        compatibilityLiveScanButton.Enabled = false;
        var lines = new List<string>();
        Action<string> line = text =>
        {
            lines.Add(text);
            compatibilityStatus.Text = string.Join(Environment.NewLine, lines.Skip(Math.Max(0, lines.Count - 18)));
        };
        try
        {
            Directory.CreateDirectory(outputDir);
            line("Pack dizinleri açık oyundan okunuyor…");
            IProgress<string> progress = new Progress<string>(line);
            var indexes = await Task.Run(() =>
            {
                var current = PackExtractor.DumpIndexes(process, packDir, progress.Report);
                foreach (var old in PackExtractor.LoadIndexes(PackIndexFolder))
                {
                    string idx = Path.Combine(packDir, old.Key + ".idx");
                    string pak = Path.Combine(packDir, old.Key + ".pak");
                    if (!current.ContainsKey(old.Key) && File.Exists(idx) && (new FileInfo(idx).Length - 32) / 288 == old.Value.Count && PackExtractor.ValidateIndex(pak, old.Value)) current[old.Key] = old.Value;
                }
                PackExtractor.SaveIndexes(current, PackIndexFolder);
                return current;
            });
            line("Seçilen " + selected.Count + " pack doğru klasör adlarıyla çıkarılıyor…");
            var result = await Task.Run(() => PackExtractor.ExtractSelectedPacks(packDir, indexes, selected, outputDir, progress.Report));
            int clientDone = 0, clientFailed = 0;
            var clientErrors = new List<string>();
            if (result.Pending.Count > 0)
            {
                line(result.Pending.Count + " ek şifreli dosya oyunun kendi pack okuyucusuyla çözülüyor…");
                var clientResult = await ExtractEncryptedPackEntriesAsync(result.Pending, line, clientErrors);
                clientDone = clientResult[0]; clientFailed = clientResult[1];
            }
            var errors = result.Errors.Concat(clientErrors).ToList();
            if (errors.Count > 0) File.WriteAllLines(Path.Combine(outputDir, "_pack-cikarma-hatalari.txt"), errors, Encoding.UTF8);
            int total = result.Written + clientDone;
            line("Bitti · " + total + " dosya çıkarıldı" + (result.Failed + clientFailed > 0 ? " · " + (result.Failed + clientFailed) + " dosya çıkarılamadı" : ""));
            MessageBox.Show(this,
                total + " dosya çıkarıldı." + Environment.NewLine + "Çıktı: " + outputDir + (errors.Count > 0 ? Environment.NewLine + "Hata listesi çıktı klasörüne yazıldı." : ""),
                "Packları çıkar", MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            line("✗ Pack çıkarma tamamlanamadı: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Packları çıkar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            compatibilityPackExtractButton.Enabled = true;
            compatibilityScanButton.Enabled = true;
            compatibilityLiveScanButton.Enabled = true;
        }
    }

    private async Task<int[]> ExtractEncryptedPackEntriesAsync(List<PackExtractor.PendingEntry> pending, Action<string> report, List<string> errors)
    {
        int done = 0, failed = 0;
        const int batchSize = 8;
        for (int start = 0; start < pending.Count; start += batchSize)
        {
            var batch = pending.Skip(start).Take(batchSize).ToList();
            foreach (var job in batch) Directory.CreateDirectory(Path.GetDirectoryName(job.TargetPath));
            string jobs = "[" + string.Join(",", batch.Select(j => "(" + PythonLiteral(j.Item.Name) + "," + PythonLiteral(j.TargetPath) + ")")) + "]";
            string body =
                "import os,sys\n" +
                "w=globals().get('old_open',open);p=sys.modules.get('pack');ok=0;bad=[]\n" +
                "def _pack_read(n):\n" +
                " if p is not None and hasattr(p,'Get'):\n" +
                "  try:\n" +
                "   d=p.Get(n)\n" +
                "   if d is not None:return d if isinstance(d,(bytes,bytearray)) else str(d).encode('latin-1')\n" +
                "  except Exception:pass\n" +
                " with open(n,'rb') as s:return s.read()\n" +
                "for i,(n,t) in enumerate(" + jobs + "):\n" +
                " try:\n" +
                "  d=_pack_read(n)\n" +
                "  with w(t,'wb') as o:o.write(d)\n" +
                "  ok+=1\n" +
                " except Exception:bad.append(str(i))\n" +
                "f.write(str(ok)+'|'+','.join(bad))";
            string response;
            try { response = await GamePythonAsync(body, CancellationToken.None); }
            catch (Exception ex)
            {
                response = null;
                foreach (var job in batch) errors.Add(job.PackName + " / " + job.Item.Name + ": " + ex.Message);
                failed += batch.Count;
            }
            if (!string.IsNullOrEmpty(response))
            {
                var parts = response.Split('|'); int batchDone;
                if (parts.Length == 2 && int.TryParse(parts[0], out batchDone))
                {
                    done += batchDone;
                    var bad = new HashSet<int>();
                    int parsed;
                    foreach (var value in parts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) if (int.TryParse(value, out parsed)) bad.Add(parsed);
                    failed += bad.Count;
                    foreach (var index in bad) if (index >= 0 && index < batch.Count) errors.Add(batch[index].PackName + " / " + batch[index].Item.Name + ": istemci dosyayı açamadı");
                }
                else
                {
                    failed += batch.Count;
                    foreach (var job in batch) errors.Add(job.PackName + " / " + job.Item.Name + ": istemci yanıtı geçersiz");
                }
            }
            else if (response != null)
            {
                failed += batch.Count;
                foreach (var job in batch) errors.Add(job.PackName + " / " + job.Item.Name + ": istemciden boş yanıt");
            }
            report(Math.Min(start + batch.Count, pending.Count) + "/" + pending.Count + " ek şifreli dosya işlendi");
        }
        return new[] { done, failed };
    }
}
