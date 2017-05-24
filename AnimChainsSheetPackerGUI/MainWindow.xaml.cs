using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
//using System.Windows.Shapes;
using System.IO;
using FlatRedBall.Content.AnimationChain;
using System.Drawing;
using System.ComponentModel;
using Microsoft.Win32;
using Microsoft.Xna.Framework;

using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

// debug
using System.Diagnostics;

namespace AnimChainsSheetPacker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //<!--sheetBorder = 0, uint spritesBorders = 0, bool sheetPowerOf2 = false, uint maxSheetSize = 8192, bool forceSquareSheet-->

        #region    -- SpriteSheet params
        private int _SheetBorder = 0;
        public int SheetBorder
        {
            get { return _SheetBorder; }
            set { SetField(ref _SheetBorder, value, "SheetBorder"); }
        }

        private int _SpritesBorders = 0;
        public int SpritesBorders
        {
            get { return _SpritesBorders; }
            set { SetField(ref _SpritesBorders, value, "SpritesBorders"); }
        }

        private int _MaxSheetSize = 8000;
        public int MaxSheetSize
        {
            get { return _MaxSheetSize; }
            set { SetField(ref _MaxSheetSize, value, "MaxSheetSize"); }
        }

        private bool _SheetPowerOf2;
        public bool SheetPowerOf2
        {
            get { return _SheetPowerOf2; }
            set { SetField(ref _SheetPowerOf2, value, "SheetPowerOf2"); }
        }

        private bool _ForceSquareSheet;
        public bool ForceSquareSheet
        {
            get { return _ForceSquareSheet; }
            set { SetField(ref _ForceSquareSheet, value, "ForceSquareSheet"); }
        }
        #endregion -- SpriteSheet params END

        #region    -- Output achx params

        private float _FramesRelativeX;
        public float FramesRelativeX
        {
            get { return _FramesRelativeX; }
            set { SetField(ref _FramesRelativeX, value, "FramesRelativeX"); }
        }

        private float _FramesRelativeY;
        public float FramesRelativeY
        {
            get { return _FramesRelativeY; }
            set { SetField(ref _FramesRelativeY, value, "FramesRelativeY"); }
        }
        #endregion -- Output achx params END

        #region    -- Paths
        private string _OutputDir = @"W:\Programing\VisualStudio2015 Projects\TexturePackerFRBImport_misc\TestData\";
        public string OutputDir
        {
            get { return _OutputDir; }
            set { SetField(ref _OutputDir, value, "OutputDir"); }
        }

        private string _SSPDir = @"E:\Program Files (x86)\amakaseev SpriteSheet Packer\";
        public string SSPDir
        {
            get { return _SSPDir; }
            set { SetField(ref _SSPDir, value, "SSPDir"); }
        }

        private string _SourceAchx = @"W:\Programing\VisualStudio2015 Projects\TexturePackerFRBImport_misc\TestData\Input\MinimalTest01.achx";
        public string SourceAchx
        {
            get { return _SourceAchx; }
            set { SetField(ref _SourceAchx, value, "SourceAchx"); }
        }

        private string _WorkDir = @"W:\Programing\VisualStudio2015 Projects\TexturePackerFRBImport_misc\TestData\";
        public string WorkDir
        {
            get { return _WorkDir; }
            set { SetField(ref _WorkDir, value, "WorkDir"); }
        }
        #endregion -- Paths END







        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();

            //VisualTreeHelper.
            //FDSVMessages
        }






        #region    --- INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            // else
            field = value;

            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion --- INotifyPropertyChanged implementation END


        private bool _ShowOpenFileDialog(string title, string filter, out string resultFilePathName)
        {
            Debug.WriteLine(" * ShowStandardOpenFileDialog()");

            var dialog = new OpenFileDialog
            {
                DereferenceLinks = true, // default is false
                CheckFileExists = true,
                CheckPathExists = true,
                //Multiselect = multipleImages, // default is false
                // openFileDialog.ValidateNames ??

                Title = title,

                // Set filter for file extension and default file extension 
                //DefaultExt = ".vpm",
                //Filter = "JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif"
                //Filter = "PNG File (*.png)",
                Filter = filter,
                //FilterIndex = 0,

                //FileName = System.IO.Path.GetFileName(filePathName),
                //InitialDirectory = path //System.IO.Path.GetDirectoryName(path)
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            //&& !String.IsNullOrWhiteSpace(dialog.FileName))
            {
                resultFilePathName = dialog.FileName;
                return true;
            }

            resultFilePathName = null;
            return false;
        }

        private void _AddMsg(string msg, Brush textColor = null)
        {
            //FlowDocument
            //FDMessagesStack.Blocks.Add(

            if (textColor == null)
                textColor = Brushes.White;

            FDMessagesStack.Inlines.Add(
                new Run
                {
                    Text = msg,
                    Foreground = textColor,
                }
            );
            FDMessagesStack.Inlines.Add(
                new LineBreak()
            );

            //FDSVMessages.Scroll
        }

        private void SpriteSheetPackerLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/amakaseev/sprite-sheet-packer");
        }
        /*private void SpriteSheetPackerSourceLink_Click(object sender, RoutedEventArgs e)
        {

        }*/

        private void ButBrowseAchx_Click(object sender, RoutedEventArgs e)
        {
            string path;
            if (_ShowOpenFileDialog("Select source FRB AnimatinChains file", "AnimatinChains (*.achx)|*.achx", out path))
            {
                SourceAchx = path;
            }
        }

        private void ButBrowseSSPDir_Click(object sender, RoutedEventArgs e)
        {
            string path;
            if (_ShowOpenFileDialog("Select SpriteSheet Packer directory", "Executable (*.exe)|*.exe", out path))
            {
                SSPDir = Path.GetDirectoryName(path);
            }
        }

        private void ButBrowseOutputDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                DereferenceLinks = true, // default is false
                //CheckFileExists = true,
                CheckPathExists = true,
                //Multiselect = multipleImages, // default is false
                // openFileDialog.ValidateNames ??

                Title = "Select output directory for result achx",

                // Set filter for file extension and default file extension 
                //DefaultExt = ".vpm",
                //Filter = "JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif"
                //Filter = "PNG File (*.png)",
                Filter = "All files (*.*)|*.*",
                //FilterIndex = 0,

                //FileName = System.IO.Path.GetFileName(filePathName),
                //InitialDirectory = path //System.IO.Path.GetDirectoryName(path)
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            //&& !String.IsNullOrWhiteSpace(dialog.FileName))
            {
                OutputDir = dialog.FileName;
            }
        }
        private void ButClearOutputDir_Click(object sender, RoutedEventArgs e)
        {
            OutputDir = null;
        }

        private void ButBrowseWorkDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                DereferenceLinks = true, // default is false
                //CheckFileExists = true,
                CheckPathExists = true,
                //Multiselect = multipleImages, // default is false
                // openFileDialog.ValidateNames ??

                Title = "Select work directory for temporary files",

                // Set filter for file extension and default file extension 
                //DefaultExt = ".vpm",
                //Filter = "JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif"
                //Filter = "PNG File (*.png)",
                Filter = "All files (*.*)|*.*",
                //FilterIndex = 0,

                //FileName = System.IO.Path.GetFileName(filePathName),
                //InitialDirectory = path //System.IO.Path.GetDirectoryName(path)
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            //&& !String.IsNullOrWhiteSpace(dialog.FileName))
            {
                WorkDir = dialog.FileName;
            }
        }
        private void ButClearWorkDir_Click(object sender, RoutedEventArgs e)
        {
            WorkDir = null;
        }

        private void ButStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Packer.PackAchx(
                    // input .achx file path name ext 
                    _SourceAchx,
                    // SpriteSheet Packer dir
                    _SSPDir,
                    // Output dir
                    _OutputDir, 
                    // Work (temp) dir
                    // !! throw GDI error when null = Windows Temp dir - investigate
                    _OutputDir,

                    (uint)_SheetBorder, (uint)_SpritesBorders, _SheetPowerOf2, (uint)_MaxSheetSize, _ForceSquareSheet
                );
            }
            catch (Exception ex)
            {
                if (ex is AnimChainsSheetPackerException)
                {
                    var myEx = ex as AnimChainsSheetPackerException;
                    switch (myEx.ErrorCode)
                    {
                        case AnimChainsSheetPackerErrorCode.AnimationChainList_Empty_NoAnims:
                            _AddMsg("Achx has no AnimationChains.", Brushes.Yellow);
                            return;
                        case AnimChainsSheetPackerErrorCode.AnimationChainList_Empty_NoFrames:
                            _AddMsg("Achx has no AnimationFrames.", Brushes.Yellow);
                            return;
                        case AnimChainsSheetPackerErrorCode.NotSupported_AnimationChainList_MutlipleImages:
                            _AddMsg("Achx uses multiple sprite / sprite sheet images. Not yet supported.", Brushes.Yellow);
                            return;
                        case AnimChainsSheetPackerErrorCode.NotSupported_AnimationFrame_Flipped:
                            _AddMsg("Some AnimationFrames in Achx use Horizontal / Vertical flipping. Not yet supported.", Brushes.Yellow);
                            return;
                        case AnimChainsSheetPackerErrorCode.InputImage_ZeroSize:
                            _AddMsg("Sprite sheet inage used by achx has zero width or height.", Brushes.Yellow);
                            return;
                        case AnimChainsSheetPackerErrorCode.InputImage_NotFound:
                            _AddMsg("Sprite sheet inage file used by achx not found.", Brushes.Yellow);
                            return;
                        case AnimChainsSheetPackerErrorCode.SpriteSheetPacker_Error:
                            _AddMsg("Uknown SpriteSheet Packer error.", Brushes.Red);
                            return;

                        default:
                            throw new Exception("Unknown AnimChainsSheetPackerErrorCode (" + (byte)myEx.ErrorCode + ')', myEx);
                    }
                }

                if (ex is IOException)
                {
                    var ioEx = ex as IOException;
                    _AddMsg("Uknown file system error.", Brushes.Red);
                    return;
                }

                _AddMsg("Uknown error:" + ex.Message, Brushes.Red);
                return;
            }
        }

        private void ButSheetMaxSize_Click(object sender, RoutedEventArgs e)
        {
            MaxSheetSize = (int)(sender as Button).Tag;
        }
    }
}
