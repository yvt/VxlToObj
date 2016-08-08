using System;
using System.Windows.Forms;
using System.Threading;
using VxlToObj.Core;
using System.IO;
using System.Collections.Generic;

namespace VxlToObj.Shell.Gui
{
    public partial class MainForm : Form
    {
        enum InputFormatSelection
        {
            Auto,
            Kv6,
            Vxl
        }
        enum OutputFormatSelection
        {
            WavefrontObj
        }

        public MainForm()
        {
            InitializeComponent();
        }

        sealed class ConversionOptions
        {
            public InputFormatSelection InputFormat { get; set; }
            public OutputFormatSelection OutputFormat { get; set; }
            public string OutputDirectory { get; set; }

            public InputFormatSelection? DetectFileFormatForFileName(string filename)
            {
                var fmt = InputFormat;
                if (fmt == InputFormatSelection.Auto)
                {
                    switch (System.IO.Path.GetExtension(filename).ToLowerInvariant())
                    {
                        case ".kv6":
                            fmt = InputFormatSelection.Kv6;
                            break;
                        case ".vxl":
                            fmt = InputFormatSelection.Vxl;
                            break;
                        default:
                            return null;
                    }
                }
                return fmt;
            }

            public string GetOutputPathForInputPath(string filename)
            {
                // Make the path absolute (normalization)
                filename = Path.GetFullPath(filename);

                // Decompose the path
                var basename = Path.GetFileName(filename);

                // Change the file extension
                switch (OutputFormat)
                {
                    case OutputFormatSelection.WavefrontObj:
                        basename = Path.GetFileNameWithoutExtension(basename) + ".obj";
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                // Reconstruct the path
                var dir = OutputDirectory != null ? OutputDirectory :
                    Path.GetDirectoryName(filename);

                return Path.Combine(dir, basename);
            }
        }

        /// <summary>
        /// Retrieves the current conversion options specified by the user.
        /// Must be called from the GUI thread.
        /// </summary>
        /// <returns>The current conversion options.</returns>
        ConversionOptions GetConversionOptions()
        {
            return new ConversionOptions()
            {
                InputFormat = (InputFormatSelection) inputFormatComboBox.SelectedIndex,
                OutputFormat = (OutputFormatSelection) outputFormatComboBox.SelectedIndex,
                OutputDirectory = outputDirectorySameAsInputCheckBox.Checked ? null :
                    outputDirectoryTextBox.Text
            };
        }
    
        bool working = false;
        Thread workerThread;
        
        private void ProcessingStarted()
        {
            Invoke(new Action(() =>
            {
                working = true;
                choosePanel.Visible = false;
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
            }));
        }

        private void ProcessingEnded(string error)
        {
            Invoke(new Action(() =>
            {
                workerThread = null;
                working = false;
                choosePanel.Visible = true;
                progressBar.Visible = false;
                if (error != null)
                {
                    MessageBox.Show(this, error, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }));
        }

        private void ReportProgress(float value)
        {
            Invoke(new Action(() =>
            {
                if (float.IsNaN(value))
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                } else
                {
                    progressBar.Value = (int)Math.Max(Math.Min(value * 100f, 100f), 0f);
                    progressBar.Style = ProgressBarStyle.Continuous;
                }
            }));
        }

        private void ProcessOneFile(string file, ConversionOptions opts)
        {
            var infmt = opts.DetectFileFormatForFileName(file).Value;
            var outfmt = opts.OutputFormat;

            VoxelModel model;
            try {
                switch (infmt)
                {
                    case InputFormatSelection.Kv6:
                        model = new Kv6VoxelModelLoader().LoadVoxelModel(
                            System.IO.File.ReadAllBytes(file));
                        break;
                    case InputFormatSelection.Vxl:
                        model = new VxlVoxelModelLoader().LoadVoxelModel(
                            System.IO.File.ReadAllBytes(file));
                        break;
                    case InputFormatSelection.Auto:
                    default:
                        throw new InvalidOperationException();
                }
            } catch (Exception ex)
            {
                throw new Exception("Failed to load the input file (I/O error or file type mismatch?).", ex);
            }

            var slices = new SimpleMeshSliceGenerator().GenerateSlices(model);

            System.Drawing.Bitmap bmp;
            new SimpleMeshTextureGenerator().GenerateTextureAndUV(model, slices, out bmp);

            var outpath = opts.GetOutputPathForInputPath(file);

            switch (outfmt)
            {
                case OutputFormatSelection.WavefrontObj:
                    new ObjWriter().Save(slices, bmp, outpath);
                    break;
            }
        }

        /// <summary>
        /// Starts the conversion on the specified file(s).
        /// Must be called from the GUI thread.
        /// </summary>
        /// <param name="files">The list of file pathes to be processed.</param>
        private void StartConversion(string[] files)
        {
            if (workerThread != null || working)
            {
                throw new InvalidOperationException();
            }

            ProcessingStarted();

            // Retrive the conversion options.
            // (GUI cannot be accessed directly from the worker thread.)
            var opts = GetConversionOptions();

            workerThread = new Thread(() =>
            {
                if (files.Length > 1)
                {
                    ReportProgress(0f);
                }
                for (int i = 0; i < files.Length; ++i)
                {
                    var file = files[i];
                    try
                    {
                        ProcessOneFile(file, opts);
                    }
                    catch (Exception ex)
                    {
                        var fn = System.IO.Path.GetFileName(file);
                        ProcessingEnded($"Conversion failed while processing {fn}:\n\n{ex.ToString()}");
                        return;
                    }
                    if (files.Length > 1)
                    {
                        ReportProgress((float)(i + 1) / files.Length);
                    }
                }

                ProcessingEnded(null);
            });
            workerThread.Start();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            inputFormatComboBox.SelectedIndex = (int)InputFormatSelection.Auto;
            outputFormatComboBox.SelectedIndex = (int)OutputFormatSelection.WavefrontObj;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var opts = GetConversionOptions();
                var files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                foreach (var file in files)
                {
                    if (opts.DetectFileFormatForFileName(file) == null)
                    {
                        // Cannot deduce the file type
                        return;
                    }
                }

                if (working)
                {
                    // Busy
                    return;
                }

                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var opts = GetConversionOptions();
                var files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                foreach (var file in files)
                {
                    if (opts.DetectFileFormatForFileName(file) == null)
                    {
                        // Cannot deduce the file type
                        return;
                    }
                }

                if (working)
                {
                    // Busy
                    return;
                }

                StartConversion(files);
            }
        }

        private void browseOutputDirectoryButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Choose the output directory.";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    outputDirectoryTextBox.Text = dlg.SelectedPath;
                }
            }
        }

        private void outputDirectorySameAsInputCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            outputDirectoryTextBox.Enabled = !outputDirectorySameAsInputCheckBox.Checked;
        }

        private void chooseButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                var opts = GetConversionOptions();
                string filter;
                switch (opts.InputFormat)
                {
                    case InputFormatSelection.Auto:
                        filter = "Supported Files (*.kv6;*.vxl)|*.kv6;*.vxl";
                        break;
                    case InputFormatSelection.Kv6:
                        filter = "VOXLAP Sprite Files (*.kv6)|*.kv6";
                        break;
                    case InputFormatSelection.Vxl:
                        filter = "VOXLAP Worldmap Files (*.vxl)|*.vxl";
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                dlg.Filter = filter + "|All Files|*.*";
                dlg.Multiselect = true;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    StartConversion(dlg.FileNames);
                }
            }
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new AboutForm())
            {
                dlg.ShowDialog(this);
            }
        }
    }
}
