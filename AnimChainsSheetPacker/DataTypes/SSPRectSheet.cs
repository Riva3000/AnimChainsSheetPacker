using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsSheetPacker.DataTypes
{
    public class SSPRectSheet
    {
        /// <summary>position of croped sprite in result sheet</summary>
        public int x;
        /// <summary>position of croped sprite in result sheet</summary>
        public int y;
        /// <summary>size of result trimed sprite</summary>
        public int w;
        /// <summary>size of result trimed sprite</summary>
        public int h;

        public override string ToString()
        {
            return "x: " + x + " y: " + y + " w: " + w + " h: " + h;
        }
    }
}
