// Modification of GWTool

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using GWTool.Functions;

using Microsoft.VisualStudio.Threading;

namespace GWTool.UI
{
    public partial class Main : Form
    {
        private readonly string[] args = Environment.GetCommandLineArgs();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly IContainer components = (IContainer)null;
        private bool _enableDragDrop = false;
        private Label lblInfo;
        private Label lblResult;
        private Button btnOpenFile;
        private OpenFileDialog openFileDialog1;
        private Label lblVersion;
        private Button btnCancel;
        private ProgressBar progressBar;
        private JoinableTask _task;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = _enableDragDrop ? DragDropEffects.All : DragDropEffects.None;
        }

        private void Main_DragDrop(object sender, DragEventArgs e)
        {
            if (!_enableDragDrop)
                return;

            _task = ThreadHelper.Current.RunAsync(async delegate
            {
                await TaskScheduler.Default;
                foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
                    await HandleFileAsync(file, _cts.Token);
            });
        }

        private void SetStatus(string text) =>
            SetStatus(text, Color.Black);
        private void SetStatus(string text, Color color) =>
            this.InvokeIfRequired(() =>
            {
                this.lblResult.ForeColor = color;
                this.lblResult.Text = text;
            });

        private async Task HandleFileAsync(string file, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var type = await AnalyzeFileAsync(file);
            if (type == FileType.Unknown)
            {
                await this.InvokeAsyncIfRequired(() =>
                    SetStatus("I don't know what type this file is :(", Color.Red));
            }
            else
            {
                await this.InvokeAsyncIfRequired(delegate
                {
                    _enableDragDrop = false;
                    this.btnOpenFile.Enabled = false;
                    this.btnCancel.Visible = true;
                    this.progressBar.Visible = true;
                    SetStatus("The file is a " + type.ToString() + " file!\r\n");
                });
                var extension = GetExtension(type);
                if (new FileInfo(file).Extension != extension)
                {
                    var destFileName = file + extension;
                    File.Move(file, destFileName);
                    file = destFileName;
                    await this.InvokeAsyncIfRequired(() =>
                        SetStatus("Added proper extension to the file", Color.Green));
                }
                switch (type)
                {
                    case FileType.GMAD:
                        await this.InvokeAsyncIfRequired(() =>
                            SetStatus("Extracting GMAD file..."));
                        try
                        {
                            await GMADTool.ExtractAsync(file, Path.GetDirectoryName(file), token,
                                new Progress<ExtractProgress>(progress =>
                                {
                                    this.InvokeIfRequired(delegate
                                    {
                                        SetStatus($"Extracting GMAD file... ({progress.FilesProcessed}/{progress.TotalFiles})");
                                        this.progressBar.Style = ProgressBarStyle.Blocks;
                                        this.progressBar.Maximum = progress.TotalFiles;
                                        this.progressBar.Value = progress.FilesProcessed;
                                    });
                                }));
                        }
                        catch (TaskCanceledException)
                        {
                            await this.InvokeAsyncIfRequired(() =>
                                SetStatus("The operation has been canceled by the user.", Color.Red));
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            await this.InvokeAsyncIfRequired(() =>
                                SetStatus("The operation has been canceled by the user.", Color.Red));
                            break;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Exception occured");

                            await this.InvokeAsyncIfRequired(() =>
                                SetStatus("Failed to extract the GMAD file.", Color.Red));
                            break;
                        }
                        await this.InvokeAsyncIfRequired(() =>
                            SetStatus("Successfully extracted the GMAD file!", Color.Green));
                        break;
                    case FileType.LZMA:
                        await this.InvokeAsyncIfRequired(() =>
                            SetStatus("This GMA file seems to be compressed (LZMA). Please extract this with 7-Zip and drag the extracted GMA file here.", Color.Yellow));
                        break;
                    default:
                        await this.InvokeAsyncIfRequired(() =>
                            SetStatus("Now just put this file in the correct directory in GMOD!\n(ex. garrysmod/addons)"));
                        break;
                }

                await this.InvokeAsyncIfRequired(delegate
                {
                    _enableDragDrop = true;
                    this.btnOpenFile.Enabled = true;
                    this.btnCancel.Visible = false;
                    this.progressBar.Visible = false;
                    this.progressBar.Style = ProgressBarStyle.Marquee;
                    this.progressBar.Maximum = 100;
                    this.progressBar.Value = 0;
                });
            }
        }

        private static string GetExtension(FileType type)
        {
            switch (type)
            {
                case FileType.GMAD:
                    return ".gma";
                case FileType.LZMA:
                    return ".7z";
                default:
                    return "." + type.ToString().ToLower();
            }
        }

        private static async Task<FileType> AnalyzeFileAsync(string file)
        {
            var buffer = new byte[3];
            string str;
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                _ = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                str = new SoapHexBinary(buffer).ToString();
            }
            switch (str)
            {
                case "5D0000":
                    return FileType.LZMA;
                case "474D41":
                    return FileType.GMAD;
                case "445550":
                    return FileType.DUPE;
                case "474D53":
                    return FileType.GMS;
                default:
                    return FileType.Unknown;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(21, 69);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(175, 39);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "Drop an addon file anywhere\r\non this window to extract it\nor press the Open GMAD " +
    "file button";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblResult
            // 
            this.lblResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.lblResult.Location = new System.Drawing.Point(12, 108);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(190, 40);
            this.lblResult.TabIndex = 1;
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblVersion.AutoSize = true;
            this.lblVersion.Enabled = false;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblVersion.Location = new System.Drawing.Point(93, 206);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(28, 13);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "v0.5";
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnOpenFile.Location = new System.Drawing.Point(49, 12);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(105, 23);
            this.btnOpenFile.TabIndex = 3;
            this.btnOpenFile.Text = "Open GMAD file";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "GMOD addon files|*.gma";
            this.openFileDialog1.Title = "Select an GMAD file";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(72, 180);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.progressBar.Location = new System.Drawing.Point(12, 151);
            this.progressBar.MarqueeAnimationSpeed = 10;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(190, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // Main
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(214, 227);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GWTool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Main_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Main_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private enum FileType
        {
            GMAD,
            GMS,
            DUPE,
            LZMA,
            Unknown,
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
            if (this.openFileDialog1.FileName.Length != 0)
            {
                if (File.Exists(this.openFileDialog1.FileName))
                {
                    _task = ThreadHelper.Current.RunAsync(async delegate
                    {
                        await TaskScheduler.Default;
                        foreach (var file in this.openFileDialog1.FileNames)
                            await HandleFileAsync(file, _cts.Token);
                    });
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if(args.Length < 2)
            {
                return;
            }

            var file = args[1];
            if (File.Exists(file))
                ThreadHelper.Current.Run(() => HandleFileAsync(file, _cts.Token));
        }

        private void Main_FormClosing(object sender, CancelEventArgs e)
        {
            if (!(_task is null))
                ThreadHelper.Current.Run(() => _task.JoinAsync());

            _cts.Cancel();
            _cts.Dispose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts.Cancel();
            _cts.Dispose();

            _cts = new CancellationTokenSource();
        }
    }
}
