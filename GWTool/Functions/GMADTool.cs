using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AridityTeam.IO;

namespace GWTool.Functions
{
    public static class GMADTool
    {
        public static int Extract(string gmaFile, string outputDir,
            IProgress<ExtractProgress> progress = null) =>
            ThreadHelper.Current.Run(() =>
                ExtractAsync(gmaFile, outputDir, CancellationToken.None, progress));

        public static async Task<int> ExtractAsync(string gmaFile, string outputDir, CancellationToken token,
            IProgress<ExtractProgress> progress = null)
        {
            if (token.IsCancellationRequested)
                throw new TaskCanceledException();

            using (var fs = new FileStream(
                gmaFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                useAsync: true))
            using (var reader = new BinaryReader(fs, Encoding.GetEncoding(1252)))
            {
                uint magic = reader.ReadUInt32();
                if (magic != 0x44414D47)
                    return 1;

                fs.Seek(18, SeekOrigin.Current);

                string addonName = ReadString(reader);
                string addonDesc = ReadString(reader);
                string addonAuthor = ReadString(reader);

                fs.Seek(4, SeekOrigin.Current);

                var files = new List<GmaFileEntry>();
                while (true)
                {
                    uint filenum = reader.ReadUInt32();
                    if (filenum == 0)
                        break;

                    files.Add(new GmaFileEntry
                    {
                        Path = ReadString(reader),
                        Size = reader.ReadUInt32()
                    });

                    fs.Seek(8, SeekOrigin.Current);
                }

                if (files.Count == 0)
                    return 2;

                string addonDir = Path.Combine(outputDir, ScrubFileName(addonName));
                Directory.CreateDirectory(addonDir);

                var progressState = new ExtractProgress
                {
                    TotalFiles = files.Count
                };

                byte[] buffer = new byte[81920];

                for (int i = 0; i < files.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var entry = files[i];
                    string fullPath = Path.Combine(addonDir, entry.Path);

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    using (var outStream = new FileStream(
                        fullPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        4096,
                        useAsync: true))
                    {
                        uint remaining = entry.Size;

                        while (remaining > 0)
                        {
                            int toRead = (int)Math.Min(buffer.Length, remaining);
                            int read = await fs.ReadAsync(buffer, 0, toRead, token)
                                               .ConfigureAwait(false);

                            if (read == 0)
                                throw new EndOfStreamException();

                            await outStream.WriteAsync(buffer, 0, read, token)
                                           .ConfigureAwait(false);

                            remaining -= (uint)read;
                        }
                    }

                    progressState.FilesProcessed = i + 1;
                    progressState.CurrentFile = entry.Path;
                    progress?.Report(progressState);
                }

                string infoFilePath = Path.Combine(addonDir, "addon.txt");

                string infoText =
                    "\"AddonInfo\"\r\n{\r\n" +
                    "\t\"name\" \"" + addonName + "\"\r\n" +
                    "\t\"author_name\" \"" + addonAuthor + "\"\r\n" +
                    "\t\"info\" \"" + addonDesc + "\"\r\n}";

                using (var sw = new StreamWriter(infoFilePath))
                {
                    await sw.WriteAsync(infoText, token);
                    await sw.FlushAsync(token);
                }

                return 0;
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            string result = string.Empty;

            while (true)
            {
                char Char = reader.ReadChar();

                if (Char == '\0')
                    break;

                result += Char;
            }

            return result;
        }

        public static string ScrubFileName(string value)
        {
            StringBuilder sb = new StringBuilder(value);
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char item in invalid)
            {
                sb.Replace(item.ToString(), "");
            }
            return sb.ToString();
        }
    }
}