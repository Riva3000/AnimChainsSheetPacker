//#define dbgPrefill
//#define o // debug [o]utput

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
//using Microsoft.Xna.Framework;
using System.Windows.Media;
using System.Diagnostics; // Process !

using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Size = System.Drawing.Size;
using Color = System.Drawing.Color;

using AnimChainsSheetPacker.DataTypes;
using AnimChainsSheetPacker.ProjectDataTypes;

// debug
using System.Text;
using System.Xml.Serialization;

namespace AnimChainsSheetPacker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //<!--sheetBorder = 0, uint spritesBorders = 0, bool sheetPowerOf2 = false, uint maxSheetSize = 8192, bool forceSquareSheet-->

        private Project _Project = new Project();
        public Project Project
        {
            get { return _Project; }
            set { SetField(ref _Project, value, "Project"); }
        }

#if dbgPrefill
        private string _SSPDir =
            //@"E:\Program Files (x86)\amakaseev SpriteSheet Packer\";
            @"C:\Program Files (x86)\SpriteSheet Packer\";
#else
        private string _SSPDir;
#endif
        public string SSPDir
        {
            get { return _SSPDir; }
            set { SetField(ref _SSPDir, value, "SSPDir"); }
        }

        private readonly ColorData[] _PredefinedColors;
        public ColorData[] PredefinedColors { get { return _PredefinedColors; } }

        private const string _SETTING_FILE_NAME = "settings.txt";





        public MainWindow()
        {
            _LoadSettings();

            _PredefinedColors = _BuildKnownColors();
            //MessageBox.Show("_PredefinedColors.Length " + _PredefinedColors.Length + "  _PredefinedColors[0].Name: " + _PredefinedColors[0].Name);

            Closing += _This_Closing;

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
#if o
            Debug.WriteLine(" * ShowStandardOpenFileDialog()");
#endif
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
                //Filter = "PNG File (*.png)|*.png",
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

        private bool _ShowSaveFileDialog(string title, string filter, out string resultFilePathName)
        {
#if o
            Debug.WriteLine(" * ShowStandardOpenFileDialog()");
#endif
            var dialog = new SaveFileDialog
            {
                DereferenceLinks = true, // default is false
                //CheckFileExists = true,
                CheckPathExists = true,
                //Multiselect = multipleImages, // default is false
                // openFileDialog.ValidateNames ??

                Title = title,

                // Set filter for file extension and default file extension 
                //DefaultExt = ".vpm",
                //Filter = "JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif"
                //Filter = "PNG File (*.png)|*.png",
                Filter = filter,
                //FilterIndex = 0,

                //FileName = System.IO.Path.GetFileName(filePathName),
                //InitialDirectory = path //System.IO.Path.GetDirectoryName(path)
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value && !String.IsNullOrWhiteSpace(dialog.FileName))
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

        private void _RunPacking()
        {
            //Color? resultSheetTransparentColor = null;
            string workDirectory;

            // ------------------ Packer code
            // -- Preparation
            bool temporaryWorkDir = false;
            if (_Project.WorkDir == null)
            {
                workDirectory = Path.Combine(Path.GetTempPath(), "AnimChainsSheetPacker_" + Guid.NewGuid().ToString());
                temporaryWorkDir = true;
            }
            else
            {
                workDirectory = _Project.WorkDir;
            }
            if (!Directory.Exists(workDirectory))
            {
                Directory.CreateDirectory(workDirectory);
            }

            string spriteImagesExportWorkDir = Path.Combine(workDirectory, @"Sprites\");
            if (!Directory.Exists(spriteImagesExportWorkDir))
            {
                Directory.CreateDirectory(spriteImagesExportWorkDir);
            }

            // debug
            //File.WriteAllText(Path.Combine(workDirectory, "test.txt"), "test");
            //Process.Start(workDirectory);
            //return;

            bool overwriteInputFiles = _Project.OutputDir == null;



            // -- Processing data
#if o
            Debug.WriteLine("\n--------- Loading Source achx ---------\n");
#endif
            _AddMsg(" - Loading Source achx - ");

            var animChainListSave = Packer.LoadAchx(_Project.SourceAchx);
            //Debug.WriteLine(" * animChainList: " + (animChainList != null ? "loaded Count: " + animChainList.AnimationChains.Count : "null"));






#if o
            Debug.WriteLine("\n--------- Loading Original SpriteSheet ---------\n");
#endif
            _AddMsg(" - Loading original SpriteSheet - ");

            string originalSpriteSheetDir;
            string originalSpriteSheetFileName;
            Bitmap originalSpriteSheetBmp = Packer.LoadOriginalSpriteSheets(
                animChainListSave,
                Path.GetDirectoryName(_Project.SourceAchx), // achx dir   NOT path file name
                out originalSpriteSheetDir,
                out originalSpriteSheetFileName
            );
            // debug
#if o
            Debug.WriteLine(
                " * originalSpriteSheetBmp: " + (originalSpriteSheetBmp != null ? "loaded size: " + originalSpriteSheetBmp.Width + ", " + originalSpriteSheetBmp.Height : "null")
                +
                "\n   originalSpriteSheetDir: " + originalSpriteSheetDir
                +
                "\n   originalSpriteSheetFileName: " + originalSpriteSheetFileName
            );
#endif



#if o
            Debug.WriteLine("\n--------- Chopping Original SpriteSheet ---------\n");
#endif
            _AddMsg(" - Chopping original SpriteSheet - ");

            var pixelAnims = Packer.ChopSpriteSheetToSpriteImages(
                animChainListSave,
                originalSpriteSheetBmp,
                spriteImagesExportWorkDir
            );

            originalSpriteSheetBmp.Dispose();

            /*Debug.WriteLine(
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




#if o
            Debug.WriteLine("\n--------- Runing Packer commandline ---------\n");
#endif
            _AddMsg(" - Runing SpriteSheet Packer commandline - ");

            //Main.RunPackerGui(_SpriteSheetPackerExeFilePath);
            Packer.RunPackerCommandline(
                _SSPDir,                    // packerExeDir

                spriteImagesExportWorkDir,  // inputDir

                workDirectory,              // outputDir

                //2, 2, true

                (uint)_Project.SheetBorder, (uint)_Project.SpritesBorders, _Project.SheetPowerOf2, (uint)_Project.MaxSheetSize, _Project.ForceSquareSheet
            );




#if o
            Debug.WriteLine("\n--------- Loading Packer Json ---------\n");
#endif
            _AddMsg(" - Loading result SpriteSheet Packer data (Json) - ");

            Dictionary<string, SSPFrame> packedFramesData = Packer.LoadPackerJson( 
                Path.Combine(workDirectory, "Sprites.json")
            );

            /*// debug
            var packedFramesDataSorted = packedFramesData.ToArray();
            Array.Sort(
                packedFramesDataSorted, 
                (kvA, kvB) => { return string.Compare(kvA.Key, kvB.Key); }
            );
            Debug.WriteLine(
                Cmn.PrintList( packedFramesDataSorted, null, "\t" )
            );*/



#if o
            Debug.WriteLine("\n--------- Geting Result SpriteSheet size ---------\n");
#endif
            _AddMsg(" - Loading result SpriteSheet - ");

            Size resultSheetSize;
            if (_Project.SheetBGColor.HasValue)
            {
                _AddMsg("Updating result SpriteSheet");
                resultSheetSize = Packer.ChangeResultSheetTransparentColor(workDirectory, _Project.SheetBGColor.Value);
            }
            else
                resultSheetSize = Packer.GetResultSheetSize(workDirectory);
#if o
            Debug.WriteLine("ResultSheetSize: " + resultSheetSize);
#endif


#if o
            Debug.WriteLine("\n--------- Updating Original AnimChains ---------\n");
#endif
            _AddMsg(" - Updating AnimChains - ");

            Packer.UpdateAnimChains(
                animChainListSave,
                pixelAnims,
                packedFramesData,
                resultSheetSize, 
                new Microsoft.Xna.Framework.Vector2(_Project.FramesRelativeX, _Project.FramesRelativeY)  // offsetForAllFrames
            );




#if o
            Debug.WriteLine("\n--------- Renaming (and moving) Result SpriteSheet file ---------\n");
#endif
            _AddMsg("Moving result SpriteSheet to output directory");
            Packer.PlaceResultSpriteSheetFile(
                overwriteInputFiles, 
                originalSpriteSheetDir, originalSpriteSheetFileName, 
                workDirectory, 
                _Project.OutputDir
            );

#if o
            Debug.WriteLine("\n--------- Saving Result achx ---------\n");
#endif
            _AddMsg(" - Saving result achx - ");
            if (overwriteInputFiles)
            {
                // Save achx to original achx path
                Packer.SaveAchx(animChainListSave, _Project.SourceAchx, _Project.SourceAchx);
            }
            else // not overwriteInputFiles - means outputDirectory != null
            {
                Packer.SaveAchx(
                    animChainListSave,
                    Path.Combine(_Project.OutputDir, "Packed.achx"),
                    // Path.Combine(outputDirectory, Path.GetFileName(inputAchxFilePath))
                    _Project.SourceAchx
                );
            }

            _AddMsg(" -- Packing successfuly finished -- ", Brushes.YellowGreen);

#if o
            Debug.WriteLine("\n--------- Cleanup temp dir ---------\n");
#endif
            if (temporaryWorkDir)
            {
                _AddMsg(" - Cleaning temp work dir - ");
                Directory.Delete(workDirectory, true);
            }
            _AddMsg(" - All done - ");
        }

        private ColorData[] _BuildKnownColors()
        {
            //List<Color> allColors = new List<Color>();

            var colorPropInfos = typeof(Color).GetProperties();

            // skip last 9 = empty colors
            int wantedColorsCount = colorPropInfos.Length - 9;

            ColorData[] knownColors = new ColorData[wantedColorsCount + 1];

            // skip first = Transparent
            //foreach (System.Reflection.PropertyInfo property in colorPropInfos.Skip(1))
            System.Reflection.PropertyInfo property;
            for (int i = 1, ii = 2; i < wantedColorsCount; i++, ii++)
            {
                property = colorPropInfos[i];
                if (property.PropertyType == typeof(Color))
                {
                    knownColors[ii] = new ColorData(
                            (Color)property.GetValue(null),
                            null
                        );
                }
            }

            knownColors[0] = new ColorData(new Color(), "- No change -");
            knownColors[1] = new ColorData(new Color(), "- Custom color >>");

            return knownColors;
        }

        private void _LoadSettings()
        {
            // Does settings file exist ? (in app root)
            if (File.Exists( _SETTING_FILE_NAME ))
            {
                System.IO.StreamReader file = new System.IO.StreamReader( _SETTING_FILE_NAME );

                string line = file.ReadLine();
                if (line != null)
                {
                    SSPDir = line;
                }

                file.Close();
            }
        }

        private void _SaveSettings()
        {
            if (! String.IsNullOrWhiteSpace(_SSPDir) )
                File.WriteAllText( _SETTING_FILE_NAME, _SSPDir );
        }




        private void _This_Closing(object sender, CancelEventArgs e)
        {
            _SaveSettings();
        }

        private void SpriteSheetPackerLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://github.com/amakaseev/sprite-sheet-packer");
        }
        /*private void SpriteSheetPackerSourceLink_Click(object sender, RoutedEventArgs e)
        {

        }*/

        private void ButBrowseAchx_Click(object sender, RoutedEventArgs e)
        {
            string path;
            if (_ShowOpenFileDialog("Select source FRB AnimatinChains file", "AnimatinChains (*.achx)|*.achx", out path))
            {
                _Project.SourceAchx = path;
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
                CheckFileExists = false,
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

                FileName = "none",
                //InitialDirectory = path //System.IO.Path.GetDirectoryName(path)
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            //&& !String.IsNullOrWhiteSpace(dialog.FileName))
            {
                _Project.OutputDir = Path.GetDirectoryName( dialog.FileName );
            }
        }
        private void ButClearOutputDir_Click(object sender, RoutedEventArgs e)
        {
            _Project.OutputDir = null;
        }

        private void ButBrowseWorkDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                DereferenceLinks = true, // default is false
                CheckFileExists = false,
                CheckPathExists = true,
                //Multiselect = multipleImages, // default is false
                // openFileDialog.ValidateNames ??

                Title = "Select directory for temporary files",

                // Set filter for file extension and default file extension 
                //DefaultExt = ".vpm",
                //Filter = "JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif"
                //Filter = "PNG File (*.png)",
                Filter = "All files (*.*)|*.*",
                //FilterIndex = 0,

                FileName = "none",
                //InitialDirectory = path //System.IO.Path.GetDirectoryName(path)
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            //&& !String.IsNullOrWhiteSpace(dialog.FileName))
            {
                _Project.WorkDir = Path.GetDirectoryName( dialog.FileName );
            }
        }
        private void ButClearWorkDir_Click(object sender, RoutedEventArgs e)
        {
            _Project.WorkDir = null;
        }

        private void ButStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region    ------------------ inputs error checking

                if (String.IsNullOrWhiteSpace(_Project.SourceAchx))
                {
                    _AddMsg("Select input .achx file.", Brushes.Yellow);
                    return;
                }
                else if (!File.Exists(_Project.SourceAchx))
                {
                    _AddMsg("Can not find input .achx file.", Brushes.Yellow);
                    return;
                }

                if (String.IsNullOrWhiteSpace(_SSPDir))
                {
                    _AddMsg("Select SpriteSheet Packer directory.", Brushes.Yellow);
                    return;
                }
                else if (!File.Exists(Path.Combine(_SSPDir, Packer.SPRITESHEETPACKER_EXE)))
                {
                    _AddMsg("Can not find SpriteSheet Packer in select directory.", Brushes.Yellow);
                    return;
                }

                //
                if (_Project.OutputDir == null)
                {
                    var userChoice = MessageBox.Show("Really overwrite input .achx and it's sprite sheet image with resulting packed versions ?\n\nIf you do not want to overwrite source files, choose output directory (that is different than that of source .achx file).", "Overwrite", MessageBoxButton.YesNo);
                    if (userChoice != MessageBoxResult.Yes)
                        return;
                }
                else if (String.IsNullOrWhiteSpace(_Project.OutputDir))
                {
                    _AddMsg("Wrong output directory path selected.", Brushes.Yellow);
                    return;
                }

                if (_Project.WorkDir != null && String.IsNullOrWhiteSpace(_Project.WorkDir))
                {
                    _AddMsg("Wrong work directory path selected.", Brushes.Yellow);
                    return;
                }
                #endregion ------------------ inputs error checking END


                /*Packer.PackAchx(
                    // input .achx file path name ext 
                    _SourceAchx,
                    // SpriteSheet Packer dir
                    _SSPDir,
                    // Output dir
                    _OutputDir, 
                    // Work (temp) dir
                    // !! throw GDI error when null = Windows Temp dir - investigate
                    _WorkDir,

                    (uint)_SheetBorder, (uint)_SpritesBorders, _SheetPowerOf2, (uint)_MaxSheetSize, _ForceSquareSheet
                );*/

                _RunPacking();
            }
#if o
            catch (AnimChainsSheetPackerException ex)
            {
                switch (ex.ErrorCode)
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
                        throw;
                }
            }
#else
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
                            //throw new Exception("Unknown AnimChainsSheetPackerErrorCode (" + (byte)myEx.ErrorCode + ')', myEx);
                            _AddMsg("Unknown AnimChainsSheetPackerErrorCode (" + (byte)myEx.ErrorCode + ')', Brushes.Red);
                            return;
                    }
                }

                if (ex is IOException)
                {
                    var ioEx = ex as IOException;
                    _AddMsg("File system error:" + ex.Message, Brushes.Red);
                    return;
                }

                _AddMsg("Uknown error:" + ex.Message, Brushes.Red);
                return;
            }
#endif
        }

        private void ButSheetMaxSize_Click(object sender, RoutedEventArgs e)
        {
            _Project.MaxSheetSize = (int)(sender as Button).Tag;
        }

        private int _ComboBoxColorLastIndex = 0;
        private void ComboBoxColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // special cases:
            if (ComboBoxColor.SelectedIndex == 0) // Color = None
            {
                _Project.SheetBGColor = null;
                _ComboBoxColorLastIndex = ComboBoxColor.SelectedIndex;
            }
            else if (ComboBoxColor.SelectedIndex == 1) // Use custom color
            {
                if (NumColorR.Value.HasValue && NumColorG.Value.HasValue && NumColorB.Value.HasValue)
                {
                    _Project.SheetBGColor = Color.FromArgb(NumColorR.Value.Value, NumColorG.Value.Value, NumColorG.Value.Value);
                    _ComboBoxColorLastIndex = ComboBoxColor.SelectedIndex;
                }
                else
                {
                    ComboBoxColor.SelectedIndex = _ComboBoxColorLastIndex;
                }
            }
            // Predefined named colors
            else
            {
                _Project.SheetBGColor = _PredefinedColors[ComboBoxColor.SelectedIndex].Color;
                _ComboBoxColorLastIndex = ComboBoxColor.SelectedIndex;
            }
        }

        private void ButtonSaveProject_Click(object sender, RoutedEventArgs e)
        {
            string filePath; // = "TestProject.acspx";
            if (_ShowSaveFileDialog("File name for Project", "project file (*.acspx)|*.acspx", out filePath))
            {
                ProjectSerialization.SaveProject(_Project, filePath);
            }
        }
        private void ButtonLoadProject_Click(object sender, RoutedEventArgs e)
        {
            string filePath; // = "TestProject.acspx";
            if (_ShowOpenFileDialog("Project file to load", "project file (*.acspx)|*.acspx", out filePath))
            {
                Project = ProjectSerialization.LoadProject(filePath);
            }
        }
    }
}
