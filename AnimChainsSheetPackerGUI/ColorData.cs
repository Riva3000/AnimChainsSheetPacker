using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsSheetPacker
{
    internal class ColorData
    {
        public readonly Color Color;
        public readonly string _Name;
        public string Name
        {
            get
            {
                if (_Name != null)
                    return _Name;

                return Color.Name;
            }
        }
    }
}
