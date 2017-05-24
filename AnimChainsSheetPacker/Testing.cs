using FlatRedBall.Content.AnimationChain;


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
