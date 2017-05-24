namespace AnimChainsSheetPacker.DataTypes
{
    public class SSPRectSprite
    {
        /// <summary>offsets from original sprite Left</summary>
        public int x;
        /// <summary>offsets from original sprite Top</summary>
        public int y;
        public int w;
        public int h;

        public override string ToString()
        {
            return "x: " + x + " y: " + y + " w: " + w + " h: " + h;
        }
    }
}
