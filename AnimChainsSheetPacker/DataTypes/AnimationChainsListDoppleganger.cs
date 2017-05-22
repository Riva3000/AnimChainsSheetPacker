using FlatRedBall.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TexturePackerImport.DataTypes
{
    [XmlRoot("AnimationChainArraySave")]
    public class AnimationChainsListDoppleganger : List<AnimationChain>
    {
        public bool FileRelativeTextures { get; set; }
        //public bool TimeMeasurementUnit { get; set; }
        public eCoordinateType CoordinateType { get; set; }
    }

    public enum eCoordinateType : byte
    {
        Unset = 0,
        Pixel,
        TextureCoordinate,
        SpriteSheet
    }
}
