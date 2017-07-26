using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsSheetPacker
{
    public struct ColorData
    {
        public readonly Color Color;
        private readonly string _Name;
        public string Name
        {
            get
            {
                if (_Name != null)
                    return _Name;

                return Color.Name;
            }
        }

        public ColorData(Color color, string name)
        {
            Color = color;
            _Name = name;
        }
    }
}
