using FlatRedBall.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Web.Script.Serialization; // JavaScriptSerializer
using FlatRedBall.Content.AnimationChain;

using AnimChainsSheetPacker.DataTypes;


namespace AnimChainsSheetPacker
{
    public class Testing
    {
        public static string PrintFRBFrame(AnimationFrameSave frame, string lineIndent)
        {
            return
                lineIndent + frame.LeftCoordinate.ToString()
                + '\n' +
                lineIndent + frame.TopCoordinate
                + '\n' +
                lineIndent + frame.RightCoordinate
                + '\n' +
                lineIndent + frame.BottomCoordinate;
        }
    }
}
