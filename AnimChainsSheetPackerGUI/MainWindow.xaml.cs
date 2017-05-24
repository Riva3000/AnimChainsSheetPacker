using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        private int _MaxSheetSize = 2048;
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
        private string _OutputDir;
        public string OutputDir
        {
            get { return _OutputDir; }
            set { SetField(ref _OutputDir, value, "OutputDir"); }
        }

        private string _SSPDir;
        public string SSPDir
        {
            get { return _SSPDir; }
            set { SetField(ref _SSPDir, value, "SSPDir"); }
        }

        private string _SourceAchx;
        public string SourceAchx
        {
            get { return _SourceAchx; }
            set { SetField(ref _SourceAchx, value, "SourceAchx"); }
        }

        private string _WorkDir;
        public string WorkDir
        {
            get { return _WorkDir; }
            set { SetField(ref _WorkDir, value, "WorkDir"); }
        }
        #endregion -- Paths END

        // debug
        private const string _SpriteSheetPackerExeFilePath = @"E:\Program Files (x86)\amakaseev SpriteSheet Packer\";





        public MainWindow()
        {
            ContentRendered += _This_ContentRendered;

            DataContext = this;

            InitializeComponent();

            //VisualTreeHelper.
            //FDSVMessages
        }





        private void _This_ContentRendered(object sender, EventArgs e)
        {
            //var animChainListSave = Main.LoadOriginalAchx( @"W:\Programing\VisualStudio2015 Projects\AnimChainsSheetPacker\TestData\Main.achx" );
            //Debug.WriteLine(" * animChainListSave: " + (animChainListSave != null ? "loaded Count: " + animChainListSave.AnimationChains.Count : "null"));

            //_RunTexturePackerImport();

            //Testing.SerializeJson();

            //var packedFramesData = Main.LoadPackerJson( @"W:\Programing\VisualStudio2015 Projects\TexturePackerImport\TestData\sspResult.json" );
            //MessageBox.Show(Cmn.PrintList(packedFramesData.Take(2)));
            // works !
            //MessageBox.Show(Path.GetFileName( packedFramesData.Keys.First() ));

            // float rounding error !
            //float f = 20.345f;
            //Debug.WriteLine((f - (int)f).ToString());
            //Debug.WriteLine((f - (int)f)); // not 0.345 but 0.3449993
            //Debug.WriteLine((f - Math.Floor(f))); // returns double: 0,344999313354492 :)
            //Debug.WriteLine((f - (float)Math.Floor(f))); // 0,3449993 :)
            /*// works
            decimal decimalF = (decimal)f;
            Debug.WriteLine((decimalF - (int)(decimalF))); */


            //_AddMsg("Bla blaba aaaaaaaa", Brushes.Magenta);
            //_AddMsg("Bla blaba aaaaaaaa", Brushes.Blue);

            //Cmn.Msg("Done");
            //MessageBox.Show("Done");
        }

        private void _RunPacking()
        {
            string mainWorkDir = @"W:\Programing\VisualStudio2015 Projects\AnimChainsSheetPacker\TestData\";
            string spriteImagesExportDir = @"W:\Programing\VisualStudio2015 Projects\AnimChainsSheetPacker\TestData\Sprites\";
            string inputAchxFilePath = Path.Combine(mainWorkDir, "Main.achx");

            var animChainListSave =
                //Main.LoadOriginalAchx( inputAchxFilePath );
                //AnimationChainListSave.FromFile( inputAchxFilePath );
                Packer.LoadtAchx(inputAchxFilePath);
            //Debug.WriteLine(" * animChainList: " + (animChainList != null ? "loaded Count: " + animChainList.AnimationChains.Count : "null"));

            string originalSpriteSheetDir;
            string originalSpriteSheetFileName;
            Bitmap originalSpriteSheetBmp = Packer.LoadOriginalSpriteSheets(
                animChainListSave,
                mainWorkDir, // achx dir
                out originalSpriteSheetDir,
                out originalSpriteSheetFileName
            );
            // debug
            Debug.WriteLine(
                " * originalSpriteSheetBmp: " + (originalSpriteSheetBmp != null ? "loaded size: " + originalSpriteSheetBmp.Width + ", " + originalSpriteSheetBmp.Height : "null")
                +
                "\n   originalSpriteSheetDir: " + originalSpriteSheetDir
                +
                "\n   originalSpriteSheetFileName: " + originalSpriteSheetFileName
            );

            var conversionData = Packer.ChopSpriteSheetToSpriteImages(
                animChainListSave,
                originalSpriteSheetBmp,
                spriteImagesExportDir
            );

            /*Debug.Write(
                Cmn.PrintList(
                    conversionData,
                    (anim) =>
                    {
                        return "Anim:\n" + Cmn.PrintList(
                            anim, 
                            (item) =>
                            {
                                return item == null ? "null" : item.ToString();
                            }
                            , null, "\t"
                        );
                    }
                )
            );*/

            //Main.RunPackerGui(_SpriteSheetPackerExeFilePath);
            Packer.RunPackerCommandline(
                _SpriteSheetPackerExeFilePath, 
                spriteImagesExportDir, mainWorkDir,
                //originalSpriteSheetDir, originalSpriteSheetFileName,
                //@"W:\Programing\VisualStudio2015 Projects\AnimChainsSheetPacker\TestData\", "CaptainSpriteSheet.png",
                2, 2, true
            );

            var packedFramesData = Packer.LoadPackerJson( 
                Path.Combine(mainWorkDir, "Sprites.json")
            );

            System.Drawing.Size resultSheetSize = Packer.GetResultSheetSize(mainWorkDir);

            Packer.UpdateAnimChains(
                animChainListSave,
                conversionData,
                packedFramesData,
                resultSheetSize,
                new Vector2(_FramesRelativeX, _FramesRelativeY)
            );

            Packer.SaveAchx(
                animChainListSave, 
                Path.Combine(mainWorkDir, "Result.achx"),
                inputAchxFilePath
            );/**/
        }




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
                SSPDir = path;
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
                    _SourceAchx, _SSPDir, _OutputDir, null,
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

                throw;
            }
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

        private void ButSheetMaxSize_Click(object sender, RoutedEventArgs e)
        {
            MaxSheetSize = (int)(sender as Button).Tag;
        }
    }
}
