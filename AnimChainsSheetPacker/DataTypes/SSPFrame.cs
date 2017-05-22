using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
//using Newtonsoft.Json.Schema;

namespace AnimChainsSheetPacker.DataTypes
{
    //[ Newtonsoft.Json. JsonObject()]
    public class SSPFrame
    {
        /// <summary>Resulting trimmed sprite position/size in result sprite sheet.</summary>
        public SSPRectSheet frame;
        /// <summary>Resulting trimmed sprite position/size in original sprite.</summary>
        public SSPRectSprite spriteSourceSize;

        // not needed
        //public bool rotated;
        //public bool trimmed;
        //public SSPRect sourceSize;

        public override string ToString()
        {
            return 
                "SSPFrame: " + frame + "\tspriteSourceSize: " + spriteSourceSize;
        }
    }
}
