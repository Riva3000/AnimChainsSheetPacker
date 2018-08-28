using FlatRedBall.Content.AnimationChain;
using System;

namespace AnimChainsSheetPacker.DataTypes
{
    /// <summary>FRB Frame coordinates converted (from UV coordinates or Fractional-Pixel coordinates) to Integral-Pixel coordinates. = Rounded.</summary>
    public class PixelsFrame
    {
        /// <summary>FRB Frame coordinates converted (from UV coordinates or Fractional-Pixel coordinates) to Integral-Pixel coordinates. = Rounded down.</summary>
        public ushort Left;
        /// <summary>FRB Frame coordinates converted (from UV coordinates or Fractional-Pixel coordinates) to Integral-Pixel coordinates. = Rounded down.</summary>
        public ushort Right;
        /// <summary>FRB Frame coordinates converted (from UV coordinates or Fractional-Pixel coordinates) to Integral-Pixel coordinates. = Rounded up.</summary>
        public ushort Top;
        /// <summary>FRB Frame coordinates converted (from UV coordinates or Fractional-Pixel coordinates) to Integral-Pixel coordinates. = Rounded up.</summary>
        public ushort Bottom;

        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalLeft;
        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalRight;
        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalTop;
        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalBottom;


        #region    -- Duplicate / Master data
        // - For Duplicate
        /// <summary>
        /// Frame of which this Frame is "duplicate" = has same coordinates on original sprite sheet.
        /// Only assigned if this frame is "duplicate" of other Frame.
        /// </summary>
        public PixelsFrame MasterPixelsFrame;

        /// <summary>
        /// Frame of which this Frame is "duplicate" = has same coordinates on original sprite sheet.
        /// Only assigned if this frame is "duplicate" of other Frame.
        /// </summary>
        public AnimationFrameSave MasterFRBFrame;

        /// <summary>
        /// "Original" FRB Frame of this PixelsFrame. Only assigned if this frame is "duplicate" of other Frame.
        /// </summary>
        public AnimationFrameSave FRBFrame;


        // - For Master
        public SSPFrame PackerFrame;

        /// <summary>
        /// For possible "duplicates", if this Frame is "Master"
        /// </summary>
        public float PackingCorrectionOffsetX;

         /// <summary>
        /// For possible "duplicates", if this Frame is "Master"
        /// </summary>
        public float PackingCorrectionOffsetY;

        public bool ShrunkInPacking_Width;
        public bool ShrunkInPacking_Height;
        #endregion -- Duplicate / Master data END



        public bool HasZeroWidth;
        public bool HasZeroHeight;

        public string ToString(string lineIndent)
        {
            string str = lineIndent + "PixelsFrame:";

            if (MasterPixelsFrame == null)
            {
                str +=
                    '\n' +
                    lineIndent + "  " + Left.ToString()
                    + '\n' +
                    lineIndent + "  " + Top
                    + '\n' +
                    lineIndent + "  " + Right
                    + '\n' +
                    lineIndent + "  " + Bottom;
            }
            else
            {
                str += " Duplicate";
            }

            return str;
                
        }
        public override string ToString()
        {
            return ToString(String.Empty);
        }
    }
}
