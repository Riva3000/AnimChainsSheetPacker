using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsSheetPacker.DataTypes
{
    public struct DuplicateFramesMapping
    {
        public readonly int DuplicateAnimIndex;
        public readonly int DuplicateFrameIndex;

        public readonly int OriginalAnimIndex;
        public readonly int OriginalFrameIndex;

        public DuplicateFramesMapping(int duplicateAnimIndex, int duplicateFrameIndex, int originalAnimIndex, int originalFrameIndex)
        {
            DuplicateAnimIndex = duplicateAnimIndex;
            DuplicateFrameIndex = duplicateFrameIndex;

            OriginalAnimIndex = originalAnimIndex;
            OriginalFrameIndex = originalFrameIndex;
        }
    }
}
