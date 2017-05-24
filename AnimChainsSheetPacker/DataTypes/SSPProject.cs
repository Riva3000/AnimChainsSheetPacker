using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimChainsSheetPacker.DataTypes
{
    public class SSPProject
    {
        public string algorithm { get { return "Rect"; } }
        public string dataFormat { get { return "pixijs"; } }

        /// <summary>Output dir, relative to project file location.</summary>
        public string destPath { get; set; } = ".";

        // needed ?
        public string encryptionKey { get { return ""; } }
        public uint epsilon { get { return 15; } }

        public bool heuristicMask { get { return false; } }
        public string imageFormat { get { return "*.png"; } }

        // needed ?
        public ushort jpgQuality { get { return 80; } }

        public string pixelFormat { get { return "ARGB8888"; } }
        public ushort pngOptLevel { get { return 1; } }
        public string pngOptMode { get { return "Lossless"; } }

        // needed ?
        public bool premultiplied { get { return false; } }

        public bool prependSmartFolderName { get { return false; } }

        public ScalingVariant[] scalingVariants; // { get; } = new ScalingVariant[1];

        public uint spriteBorder { get; set; }

        /// <summary>File name of output sprite sheet (without extension).</summary>
        public string spriteSheetName { get; set; }

        //private string[] _srcList =
        // Has to be path relative to project file ?
        /// <summary>Single item with relative path to input dir (where the separate sprite images are), 
        /// <para>with unix path separators, without terminating separator.</para></summary>
        public string[] srcList { get; } = new string[1];

        public uint textureBorder { get; set; }

        public string trimMode { get { return "Rect"; } }

        public bool trimSpriteNames { get { return true; } }
        public ushort trimThreshold { get { return 1; } }
        public ushort webpQuality { get { return 80; } }
    }

    public class ScalingVariant
    {
        public bool forceSquared { get; set; }
        public uint maxTextureSize { get; set; }
        /// <summary>ScalingVariant sub-folder name in output dir.</summary>
        public string name { get; set; } = string.Empty;
        /// <summary>Enforce Power of 2 ?</summary>
        public bool pow2 { get; set; }
        //public float scale { get { return 1; } }
        public ushort scale { get { return 1; } }
    }
}
