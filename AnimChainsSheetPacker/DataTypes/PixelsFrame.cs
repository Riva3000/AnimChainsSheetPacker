using FlatRedBall.Content.AnimationChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsSheetPacker.DataTypes
{
    public class PixelsFrame
    {
        /// <summary>FRB Frame coordinates converted (from UV coordinates or Fractional-Pixel coordinates) to Integral-Pixel coordinates. = Rounded.</summary>
        public ushort Left;
        public ushort Right;
        public ushort Top;
        public ushort Bottom;

        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalLeft;
        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalRight;
        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalTop;
        /// <summary>FRB Frame coordinates converted from UV coordinates to Fractional-Pixel coordinates. Not rounded to Integral-Pixel.</summary>
        public decimal DecimalBottom;

        public PixelsFrame DuplicateOfPixelsFrame;
        public AnimationFrameSave DuplicateOfFRBFrame;

        public bool HasZeroWidth;
        public bool HasZeroHeight;

        public string ToString(string lineIndent)
        {
            string str = lineIndent + "PixelsFrame:";

            if (DuplicateOfPixelsFrame == null)
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
