//using Newtonsoft.Json.Schema;

namespace AnimChainsSheetPacker.DataTypes
{
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
