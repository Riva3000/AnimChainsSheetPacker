using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsSheetPacker
{
    public class AnimChainsSheetPackerException : Exception
    {
        public readonly AnimChainsSheetPackerErrorCode ErrorCode;

        /*public AnimChainsSheetPackerException()
        {
        }*/

        public AnimChainsSheetPackerException(string message, AnimChainsSheetPackerErrorCode errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public AnimChainsSheetPackerException(string message, AnimChainsSheetPackerErrorCode errorCode,  Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /*protected AnimChainsSheetPackerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }*/
    }

    public enum AnimChainsSheetPackerErrorCode : byte
    {
        AnimationChainList_Empty_NoAnims,
        AnimationChainList_Empty_NoFrames,
        NotSupported_AnimationChainList_MutlipleImages,
        NotSupported_AnimationFrame_Flipped,
        InputImage_ZeroSize,
        InputImage_NotFound,
        SpriteSheetPacker_Error,
        OutputImage_Error
    }
}
