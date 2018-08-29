//#define dbgPrefill
//#define o // debug [o]utput

using System;
using System.Collections.Generic;
using System.Linq;
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

// debug
using System.Text;


namespace AnimChainsSheetPacker.ProjectDataTypes
{
    public class Project : INotifyPropertyChanged
    {
        public byte FileFormatVersion = 1;

        #region    -- SpriteSheet params
        private int _SheetBorder = 0;
        public int SheetBorder
        {
            get { return _SheetBorder; }
            set { SetField(ref _SheetBorder, value, "SheetBorder"); }
        }

#if dbgPrefill
        private int _SpritesBorders = 1;
#else
        private int _SpritesBorders = 0;
#endif
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


        private Color? _SheetBGColor = null;
        public Color? SheetBGColor
        {
            get { return _SheetBGColor; }
            set { SetField(ref _SheetBGColor, value, "SheetBGColor"); }
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
#if dbgPrefill
        private string _OutputDir = 
            //@"W:\Programing\VisualStudio2015 Projects\FRBAnimChainsSheetPacker_misc\TestData\Output\";
            @"W:\Programing\VisualStudio2015 Projects\FRBAnimChainsSheetPacker_misc\TestData fliped frames\Output\";
#else
        private string _OutputDir;
#endif
        public string OutputDir
        {
            get { return _OutputDir; }
            set { SetField(ref _OutputDir, value, "OutputDir"); }
        }







#if dbgPrefill
        private string _SourceAchx = 
            // MinimalTest01.achx - achx with one anim with one frame
            //@"W:\Programing\VisualStudio2015 Projects\FRBAnimChainsSheetPacker_misc\TestData\Input\MinimalTest01.achx";
            @"W:\Programing\VisualStudio2015 Projects\FRBAnimChainsSheetPacker_misc\TestData fliped frames\Input\TestArrow.achx";
#else
        private string _SourceAchx;
#endif
        public string SourceAchx
        {
            get { return _SourceAchx; }
            set { SetField(ref _SourceAchx, value, "SourceAchx"); }
        }


#if dbgPrefill
        private string _WorkDir = 
            //@"W:\Programing\VisualStudio2015 Projects\FRBAnimChainsSheetPacker_misc\TestData\Work\";
            @"W:\Programing\VisualStudio2015 Projects\FRBAnimChainsSheetPacker_misc\TestData fliped frames\Work\";
#else
        private string _WorkDir;
#endif
        public string WorkDir
        {
            get { return _WorkDir; }
            set { SetField(ref _WorkDir, value, "WorkDir"); }
        }
        #endregion -- Paths END







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
    }
}
