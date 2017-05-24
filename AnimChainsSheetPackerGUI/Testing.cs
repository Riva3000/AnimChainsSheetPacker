using AnimChainsSheetPacker;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsSheetPackerGUI
{
    internal class Testing
    {
        internal const string SpriteSheetPackerExeFilePath = @"E:\Program Files (x86)\amakaseev SpriteSheet Packer\";

        internal void RunPacking(float framesRelativeX, float framesRelativeY)
        {
            string mainWorkDir = @"W:\Programing\VisualStudio2015 Projects\AnimChainsSheetPacker\TestData\";
            string spriteImagesExportDir = @"W:\Programing\VisualStudio2015 Projects\AnimChainsSheetPacker\TestData\Sprites\";
            string inputAchxFilePath = Path.Combine(mainWorkDir, "Main.achx");

            var animChainListSave =
                //Main.LoadOriginalAchx( inputAchxFilePath );
                //AnimationChainListSave.FromFile( inputAchxFilePath );
                Packer.LoadtAchx(inputAchxFilePath);
            //Debug.WriteLine(" * animChainList: " + (animChainList != null ? "loaded Count: " + animChainList.AnimationChains.Count : "null"));

            string originalSpriteSheetDir;
            string originalSpriteSheetFileName;
            Bitmap originalSpriteSheetBmp = Packer.LoadOriginalSpriteSheets(
                animChainListSave,
                mainWorkDir, // achx dir
                out originalSpriteSheetDir,
                out originalSpriteSheetFileName
            );
            // debug
            Debug.WriteLine(
                " * originalSpriteSheetBmp: " + (originalSpriteSheetBmp != null ? "loaded size: " + originalSpriteSheetBmp.Width + ", " + originalSpriteSheetBmp.Height : "null")
                +
                "\n   originalSpriteSheetDir: " + originalSpriteSheetDir
                +
                "\n   originalSpriteSheetFileName: " + originalSpriteSheetFileName
            );

            var conversionData = Packer.ChopSpriteSheetToSpriteImages(
                animChainListSave,
                originalSpriteSheetBmp,
                spriteImagesExportDir
            );

            /*Debug.Write(
                Cmn.PrintList(
                    conversionData,
                    (anim) =>
                    {
                        return "Anim:\n" + Cmn.PrintList(
                            anim, 
                            (item) =>
                            {
                                return item == null ? "null" : item.ToString();
                            }
                            , null, "\t"
                        );
                    }
                )
            );*/

            //Main.RunPackerGui(_SpriteSheetPackerExeFilePath);
            Packer.RunPackerCommandline(
                AnimChainsSheetPackerGUI.Testing.SpriteSheetPackerExeFilePath, 
                spriteImagesExportDir, mainWorkDir,
                //originalSpriteSheetDir, originalSpriteSheetFileName,
                //@"W:\Programing\VisualStudio2015 Projects\AnimChainsSheetPacker\TestData\", "CaptainSpriteSheet.png",
                2, 2, true
            );

            var packedFramesData = Packer.LoadPackerJson( 
                Path.Combine(mainWorkDir, "Sprites.json")
            );

            System.Drawing.Size resultSheetSize = Packer.GetResultSheetSize(mainWorkDir);

            Packer.UpdateAnimChains(
                animChainListSave,
                conversionData,
                packedFramesData,
                resultSheetSize,
                new Vector2(framesRelativeX, framesRelativeY)
            );

            Packer.SaveAchx(
                animChainListSave, 
                Path.Combine(mainWorkDir, "Result.achx"),
                inputAchxFilePath
            );/**/
        }
    }
}
