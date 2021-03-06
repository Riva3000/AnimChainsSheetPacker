﻿//#define o // debug [o]utput

using System;
using System.Collections.Generic;
//using System.Linq;
//using FlatRedBall.Graphics.Animation;
using System.IO;
using System.Diagnostics; // Process = starting 3rd party program
//using System.Windows.Media.Imaging; // WPF bitmaps
using System.Drawing; // WinForms bitmaps
using System.Drawing.Imaging;
using FlatRedBall.Content.AnimationChain;
//using System.Web.Script.Serialization; // JavaScriptSerializer
using Newtonsoft.Json;
using Microsoft.Xna.Framework;


using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;
using Color = System.Drawing.Color;

using AnimChainsSheetPacker.DataTypes;



namespace AnimChainsSheetPacker
{
    /// <summary>Main and only class of AnimChainsSheetPacker lib.</summary>
    public sealed class Packer
    {
        public const string SPRITESHEETPACKER_EXE = "SpriteSheetPacker.exe";


        /// <summary>Takes .achx file, packs together sprites in .achx' sprite sheet and updates .achx file so all it's animations work the same as in original.</summary>
        /// <param name="inputAchxFilePath">Path w file name w ext.</param>
        /// <param name="spriteSheetPackerDir">Path to SpriteSheet Packer install dir (where the exe is).</param>
        /// <param name="outputDirectory">If ommited, original achx file and it's sprite sheet file will be overwritten with packed versions.</param>
        /// <param name="workDirectory">If you want to keep all the working files.</param>
        /// 
        /// <param name="sheetBorder">Adds transparent margin around the whole result sheet. (Good for preventing some rendering texture interpolation atifacts.)</param>
        /// <param name="spritesBorders">Adds transparent margin around every sprite in the result sheet. (Good for preventing sprites "bleeding" pixels into neibour sprites when rendered.)</param>
        /// <param name="sheetPowerOf2">Resutling sheet size will be power of 2. (Some hardwares and graphic libraries require this.)</param>
        /// <param name="maxSheetSize"></param>
        /// <param name="forceSquareSheet">(Some hardwares and graphic libraries require this.)</param>
        /// 
        /// <param name="offsetForAllFrames">Adds RelativeX,Y to all output Frames.</param>
        /// 
        /// <param name="resultSheetTransparentColor">SpriteSheetPacked messes transparent pixels RGB value, setting it to black, no matter what was original sheet color.
        /// If you require / prefer the color to be different, you can set it here.</param>
        public static void PackAchx(
                string inputAchxFilePath,

                string spriteSheetPackerDir,
                string outputDirectory = null,
                string workDirectory = null,

                uint sheetBorder = 0, uint spritesBorders = 0, bool sheetPowerOf2 = false, uint maxSheetSize = 8192, bool forceSquareSheet = false,

                Vector2 offsetForAllFrames = new Vector2(),
                Color? resultSheetTransparentColor = null
        )
        {
            // -- Preparation
            bool temporaryWorkDir = false;
            if (workDirectory == null)
            {
                workDirectory = Path.Combine(Path.GetTempPath(), "AnimChainsSheetPacker_" + Guid.NewGuid().ToString());
                temporaryWorkDir = true;
            }
            if (!Directory.Exists(workDirectory))
            {
                Directory.CreateDirectory(workDirectory);
            }

            string spriteImagesExportDir = Path.Combine(workDirectory, @"Sprites\");
            if (!Directory.Exists(spriteImagesExportDir))
            {
                Directory.CreateDirectory(spriteImagesExportDir);
            }

            bool overwriteInputFiles = outputDirectory == null;



            // -- Processing data
            //AnimationChainListSave animChainsListSave = AnimationChainListSave.FromFile( inputAchxFilePath );
            AnimationChainListSave animChainsListSave = LoadAchx(inputAchxFilePath);




            string originalSpriteSheetDir;
            string originalSpriteSheetFileName;
            Bitmap originalSpriteSheetBmp = LoadOriginalSpriteSheets(
                animChainsListSave, 
                Path.GetDirectoryName(inputAchxFilePath), // achx dir
                out originalSpriteSheetDir,
                out originalSpriteSheetFileName
            );

            PixelsFrame[][] pixelFrames = ChopSpriteSheetToSpriteImages(
                animChainsListSave, 
                originalSpriteSheetBmp, 
                spriteImagesExportDir
            );

            originalSpriteSheetBmp.Dispose();




            /* More options than plain commandline args, but crashes SpriteSheetPacker thanks to some bug in it (reported).
            Main.RunPackerCommandline(
                spriteSheetPackerExeFilePath, 
                spriteImagesExportDir, workDirectory,
                originalSpriteSheetDir, originalSpriteSheetFileName,
                sheetBorder, spritesBorders, sheetPowerOf2, maxSheetSize, forceSquareSheet
            );*/
            // Plain commandline args version
            RunPackerCommandline(
                spriteSheetPackerDir, 
                spriteImagesExportDir, workDirectory,
                sheetBorder, spritesBorders, sheetPowerOf2, maxSheetSize, forceSquareSheet
            );



            Dictionary<string, SSPFrame> packedFramesData = LoadPackerJson( 
                    //Path.Combine(workDirectory, "Result", Path.GetFileNameWithoutExtension(originalSpriteSheetFileName) + ".json" )
                    Path.Combine(workDirectory, "Sprites.json" )
                );



            Size resultSheetSize;
            if (resultSheetTransparentColor.HasValue)
                resultSheetSize = ChangeResultSheetTransparentColor(workDirectory, resultSheetTransparentColor.Value);
            else
                resultSheetSize = GetResultSheetSize(workDirectory);



            UpdateAnimChains(
                animChainsListSave,
                pixelFrames,
                packedFramesData,
                resultSheetSize,
                offsetForAllFrames
            );

            


            PlaceResultSpriteSheetFile(
                overwriteInputFiles, 
                originalSpriteSheetDir, originalSpriteSheetFileName, 
                workDirectory, 
                outputDirectory
            );

            if (overwriteInputFiles)
            {
                // Save achx to original achx path
                SaveAchx(animChainsListSave, inputAchxFilePath, inputAchxFilePath);
            }
            else // not overwriteInputFiles - means outputDirectory != null
            {
                SaveAchx(
                    animChainsListSave,
                    Path.Combine(outputDirectory, "Packed.achx"),
                    // Path.Combine(outputDirectory, Path.GetFileName(inputAchxFilePath))
                    inputAchxFilePath
                );
            }

            


            // -- Cleanup
            if (temporaryWorkDir)
            {
                Directory.Delete(workDirectory, true);
            }
        }




        // --- Source achx
        /*
            FlatRedBall.Graphics.Animation.AnimationChainList Main; = this is created from the Main.achx  = List<AnimationChain>
        
            AnimationChainList = List<AnimationChain> = this is created from .achx | does not contain SpriteSheets images file names or paths
            AnimationChain = List<AnimationFrame>
            AnimationFrame = contains SpriteSheet image file name (& path)
        */

        // - Load
        /// <summary>Loads .achx and checks it if it's valid for packing.</summary>
        public static AnimationChainListSave LoadAchx(string inputAchxFilePath)
        {
            var animChainListSave = AnimationChainListSave.FromFile( inputAchxFilePath );

            if (animChainListSave.AnimationChains.Count == 0)
                throw new AnimChainsSheetPackerException(
                    "LoadAchx(): animChainListSave is empty - has no AnimationChains.", 
                    AnimChainsSheetPackerErrorCode.AnimationChainList_Empty_NoAnims
                );

            int totalFramesCount = 0;
            foreach (var anim in animChainListSave.AnimationChains)
            {
                totalFramesCount += anim.Frames.Count;
            }
            if (totalFramesCount == 0)
                throw new AnimChainsSheetPackerException(
                    "LoadAchx(): animChainListSave is empty - has no Frames.", 
                    AnimChainsSheetPackerErrorCode.AnimationChainList_Empty_NoFrames
                );

            return animChainListSave;
        }
        




        // --- Source Sheet bitmap
        // - Load
        /// <summary>Loads (first) sprite sheet .achx references. Convert's it in-memory to 32bit format. Does second round of achx validity checks.</summary>
        public static Bitmap LoadOriginalSpriteSheets(
            AnimationChainListSave animChainListSave, string achxDir, 
            out string spriteSheetDir, out string spriteSheetFileName
        )
        {
            /*List<string> spriteSheetsFiles = new List<string>();

            foreach (var animationChain in animationChainList)
            {
                foreach (var frame in animationChain)
                {

                }
            }*/

            var frbFrame = animChainListSave.AnimationChains[0].Frames[0];
            string spriteSheetFilePath = GetSpriteSheetImageFilePaths(
                animChainListSave.FileRelativeTextures, 
                frbFrame.TextureName, 
                achxDir, 
                out spriteSheetDir, out spriteSheetFileName);

            foreach (var anim in animChainListSave.AnimationChains)
            {
                foreach (var frame in anim.Frames)
                {
                    if ( !String.Equals(spriteSheetFilePath, 
                                        _GetSpriteSheetImageFilePath(animChainListSave.FileRelativeTextures, frame.TextureName, achxDir), 
                                        StringComparison.OrdinalIgnoreCase) )
                        throw new AnimChainsSheetPackerException("LoadOriginalSpriteSheets(): AnimationChainListSave referencing multiple sprite / sprite sheet files. Not yet supported.", AnimChainsSheetPackerErrorCode.NotSupported_AnimationChainList_MutlipleImages);
                }
            }

            // v2: WinForms Bitmap

            Bitmap loadedSheetBmp = new Bitmap( Path.Combine(achxDir, spriteSheetFileName) );

            if (loadedSheetBmp.Width == 0 || loadedSheetBmp.Height == 0)
                throw new AnimChainsSheetPackerException("LoadOriginalSpriteSheets(): SpriteSheet has zero width or height.", AnimChainsSheetPackerErrorCode.InputImage_ZeroSize);

            Bitmap convertedSheetBmp = loadedSheetBmp.Clone(
                new Rectangle(0, 0, loadedSheetBmp.Width, loadedSheetBmp.Height), 
                PixelFormat.Format32bppArgb) 
                as Bitmap;
            loadedSheetBmp.Dispose();

            return convertedSheetBmp;
        }

        /// <returns>Absolute path of SpriteSheet image referenced by achx</returns>
        public static string GetSpriteSheetImageFilePaths(
            bool relativeToAchx, string frameFilePath, string achxDir, 
            out string spriteSheetDir, out string spriteSheetFileName
        )
        {
            if (relativeToAchx)
            {
                // all sprite sheet image files have relative paths, relative to location of parent .achx

                spriteSheetFileName = Path.GetFileName(frameFilePath);
                spriteSheetDir = Path.Combine(achxDir, Path.GetDirectoryName(frameFilePath));
            }
            else
            {
                // all sprite sheet image files have absolute paths

                spriteSheetFileName = Path.GetFileName(frameFilePath);
                spriteSheetDir = Path.GetDirectoryName(frameFilePath);
            }

            string filePath = Path.Combine(spriteSheetDir, spriteSheetFileName);
            if (!File.Exists(filePath))
                throw new AnimChainsSheetPackerException("GetSpriteSheetImageFilePaths(): Image file \"" + filePath + "\" not found.", AnimChainsSheetPackerErrorCode.InputImage_NotFound);

            return filePath;
        }

        private static string _GetSpriteSheetImageFilePath(bool relativeToAchx, string frameFilePath, string achxDir)
        {
            if (relativeToAchx)
            {
                // all sprite sheet image files have relative paths, relative to location of parent .achx

                string filePath = Path.Combine(achxDir, frameFilePath);

                if (!File.Exists(filePath))
                    throw new AnimChainsSheetPackerException("_GetSpriteSheetImageFilePath(): Image file \"" + filePath + "\" not found.", AnimChainsSheetPackerErrorCode.InputImage_NotFound);

                return filePath;
            }
            else
            {
                // all sprite sheet image files have absolute paths

                if (!File.Exists(frameFilePath))
                    throw new AnimChainsSheetPackerException("_GetSpriteSheetImageFilePath(): Image file \"" + frameFilePath + "\" not found.", AnimChainsSheetPackerErrorCode.InputImage_NotFound);

                return frameFilePath;
            }
        }






        // - Chop to sprite images - based on AnimFrames coordinates
        /// <summary>This function ALTERS animChainListSave passed to it. If it should be preserved pass in as deep clone.</summary>
        public static PixelsFrame[][] ChopSpriteSheetToSpriteImages(
            AnimationChainListSave animChainListSave, Bitmap spriteSheetBmp, string outputDir
        )
        {
#if o
            Debug.WriteLine(" * achx CoordinatesType: " + animChainListSave.CoordinateType);
#endif
            // Convert List<AnimationChainSave> to my structure

            List<AnimationChainSave> frbAnimChains = animChainListSave.AnimationChains;

            PixelsFrame[][] animsInPixels = new PixelsFrame[frbAnimChains.Count][];

            Bitmap outputSpriteBmp;
            AnimationFrameSave frbFrame;
            PixelsFrame pixelsFrame;

            //foreach (var anim in animChainListSave.AnimationChains)
            for (int animI = 0; animI < frbAnimChains.Count; animI++)
            {
#if o
                Debug.WriteLine(" Anim " + animI);
#endif
                animsInPixels[animI] = new PixelsFrame[frbAnimChains[animI].Frames.Count];

                for (int frameI = 0; frameI < frbAnimChains[animI].Frames.Count; frameI++)
                {
#if o
                    Debug.WriteLine("  Frame " + frameI);
#endif
                    frbFrame = frbAnimChains[animI].Frames[frameI];

                    // Is current Frame duplicate of other Frame ?
                    if ( _CheckAndSetDuplicity(frbAnimChains, animsInPixels, frbFrame, animI, frameI) )
                    {
                        // Is duplicate. skip.
                        continue;
                    }

                    // Not duplicate
#if o
                    Debug.WriteLine("   Not duplicate");
                    Debug.WriteLine(Testing.PrintFRBFrame(frbFrame, "     "));
#endif
                    // 1. Convert to pixels
                    if (animChainListSave.CoordinateType == FlatRedBall.Graphics.TextureCoordinateType.Pixel)
                    {
                        pixelsFrame = new PixelsFrame
                        {
                            Left = (ushort) frbFrame.LeftCoordinate,
                            Top = (ushort) frbFrame.TopCoordinate,
                            Right = (ushort) Math.Ceiling( frbFrame.RightCoordinate ), // Ceiling just to be sure
                            Bottom = (ushort) Math.Ceiling( frbFrame.BottomCoordinate )  // Ceiling just to be sure
                        };
                        pixelsFrame.HasZeroWidth = pixelsFrame.Left == pixelsFrame.Right;
                        pixelsFrame.HasZeroHeight = pixelsFrame.Top == pixelsFrame.Bottom;
#if o
                        Debug.WriteLine(
                            "   Converted to int pixels"
                            + '\n' +
                            pixelsFrame.ToString("     ")
                        );
#endif
                    }
                    else // TextureCoordinateType.UV
                    {
                        /*decimal decimalLeft = (decimal)frbFrame.LeftCoordinate * spriteSheetBmp.Width;
                        decimal decimalTop = (decimal)frbFrame.TopCoordinate * spriteSheetBmp.Height;
                        decimal decimalRight = ((decimal)frbFrame.RightCoordinate * spriteSheetBmp.Width) - decimalLeft;
                        decimal decimalBottom = ((decimal)frbFrame.BottomCoordinate * spriteSheetBmp.Height) - decimalTop;
                            
                        new PixelsFrame
                        {
                            DecimalLeft = decimalLeft,
                            Left = (ushort)decimalLeft,
                            DecimalTop = decimalTop,
                            Top = (ushort)decimalTop,
                            DecimalRight = decimalRight,
                            Right = (ushort)decimalRight,
                            DecimalBottom = decimalBottom,
                            Bottom = (ushort)decimalBottom
                        };*/

                        // FRB.RoundToInt() ?!!

                        pixelsFrame = new PixelsFrame
                        {
                            DecimalLeft = (decimal) frbFrame.LeftCoordinate * spriteSheetBmp.Width,
                            DecimalTop = (decimal) frbFrame.TopCoordinate * spriteSheetBmp.Height,
                            DecimalRight = (decimal) frbFrame.RightCoordinate * spriteSheetBmp.Width,
                            DecimalBottom = (decimal) frbFrame.BottomCoordinate * spriteSheetBmp.Height
                        };

                        // Push all coordinates to include more texture space, if they are fractional
                        pixelsFrame.Left = (ushort) pixelsFrame.DecimalLeft; 
                        pixelsFrame.Top = (ushort) pixelsFrame.DecimalTop;
                        pixelsFrame.Right = (ushort) Math.Ceiling( pixelsFrame.DecimalRight );
                        pixelsFrame.Bottom = (ushort) Math.Ceiling( pixelsFrame.DecimalBottom );

                        pixelsFrame.HasZeroWidth = pixelsFrame.Left == pixelsFrame.Right;
                        pixelsFrame.HasZeroHeight = pixelsFrame.Top == pixelsFrame.Bottom;
                        //pixelsFrame.HasZeroWidth = pixelsFrame.DecimalLeft == pixelsFrame.DecimalRight;
                        //pixelsFrame.HasZeroHeight = pixelsFrame.DecimalTop == pixelsFrame.DecimalBottom;
                    }

                    animsInPixels[animI][frameI] = pixelsFrame;

                    if ((!pixelsFrame.HasZeroWidth) && (!pixelsFrame.HasZeroHeight))
                    {
                        // 2. Create Sprite bitmap
                        outputSpriteBmp = spriteSheetBmp.Clone(
                            new Rectangle(
                                //(int)Math.Floor(frame.LeftCoordinate),
                                //(int)Math.Floor(frame.TopCoordinate),
                                pixelsFrame.Left,
                                pixelsFrame.Top,
                                pixelsFrame.Right - pixelsFrame.Left,
                                pixelsFrame.Bottom - pixelsFrame.Top
                            ),
                            PixelFormat.Format32bppArgb
                        );

                        // write sprite image file
                        outputSpriteBmp.Save(
                            Path.Combine(outputDir, _CreateFrameIdString(animI, frameI) + ".png"), 
                            ImageFormat.Png
                        );

                        outputSpriteBmp.Dispose();
                    }

                }// frame index
            }// anim index

            return animsInPixels;
        }

        /// <returns>True if Frame is a duplicate of any previous Frame.</returns>
        private static bool _CheckAndSetDuplicity(
            List<AnimationChainSave> animChainList, 
            PixelsFrame[][] duplicatesMapping, 
            AnimationFrameSave currentFrame, 
            int animIndex, int frameIndex
        )
        {
            AnimationFrameSave precedingFrame;
            //int animSearchStop = animIndex + 1;
            int frameSearchStop;

            for (int animI = 0; animI <= animIndex; animI++)
            {
                if (animI == animIndex)
                {
                    // If I'm at current frame's anim, set last checked frame to one before current frame

                    frameSearchStop = frameIndex;
                }
                else if (animChainList[animI].Frames.Count > 0)
                {
                    // Else if I'm at any preceding anim, and anim has frames, set last checked frame to last frame of anim

                    frameSearchStop = animChainList[animI].Frames.Count;
                }
                else
                {
                    // Else if anim has no frames, skip the anim

                    continue;
                }

                for (int frameI = 0; frameI < frameSearchStop; frameI++)
                {
                    precedingFrame = animChainList[animI].Frames[frameI];

                    if (precedingFrame != null && FramesEqual(currentFrame, precedingFrame))
                    {
#if o
                        Debug.WriteLine("   Duplicate of frame " + animI + " " + frameI);
#endif
                        //duplicatesMapping.Add(new DuplicateFramesMapping(animIndex, frameIndex, animI, frameI));
                        duplicatesMapping[animIndex][frameIndex] =
                            new PixelsFrame
                            {
                                MasterFRBFrame = precedingFrame,
                                MasterPixelsFrame = duplicatesMapping[animI][frameI],
                                FRBFrame = currentFrame
                            };

                        // Remove current Frame from animChainList = mark it as duplicate - to speed up further duplicates search
                        animChainList[animIndex].Frames[frameIndex] = null;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <returns>True if Frames have all parameters equal. 
        /// Not checking H / V Flip.</returns>
        public static bool FramesEqual(AnimationFrameSave frameA, AnimationFrameSave frameB)
        {
            //if ( ! String.Equals(frameA.TextureName, frameB.TextureName, StringComparison.OrdinalIgnoreCase) )
            //    return false;
            if (frameA.LeftCoordinate != frameB.LeftCoordinate)
                return false;
            if (frameA.RightCoordinate != frameB.RightCoordinate)
                return false;
            if (frameA.TopCoordinate != frameB.TopCoordinate)
                return false;
            if (frameA.BottomCoordinate != frameB.BottomCoordinate)
                return false;

            return true;
        }

        private static string _CreateFrameIdString(int animIndex, int frameIindex)
        {
            return "0_" + animIndex + "_" + frameIindex;
        }





        // --- "SpriteSheet Packer"
        // - Run
        /*/// <summary>This function blocks until SpriteSheet Packer finishes the job and returns.</summary>
        /// <param name="packerExePath"></param>
        /// <param name="inputDir">Path to folder with seaprated sprite images.</param>
        /// <param name="outputDir">Path to folder where resulting packed sprite sheet and data will be saved.</param>
        /// <param name="originalSpriteSheetDir"></param>
        /// <param name="originalSpriteSheetFileName"></param>
        /// <param name="sheetBorder">Adds transparent margin around the whole result sheet.</param>
        /// <param name="spritesBorders">Adds transparent margin around every sprite.</param>
        /// <param name="sheetPowerOf2">Resutling sheet size will be power of 2.</param>
        /// <param name="maxSheetSize"></param>
        /// <param name="forceSquareSheet"></param>
        public static void RunPackerCommandline(
                string packerExePath, string inputDir, string outputDir, 
                string originalSpriteSheetDir, string originalSpriteSheetFileName,
                uint sheetBorder = 0, uint spritesBorders = 0, bool sheetPowerOf2 = false, uint maxSheetSize = 8192, bool forceSquareSheet = false
            )
        {
            string originalSpriteSheetFileNameWOExt = Path.GetFileNameWithoutExtension(originalSpriteSheetFileName);

            _BackupOriginalSpriteSheetFile(outputDir, originalSpriteSheetDir, originalSpriteSheetFileName);

            // Create SpriteSheetPacker project file
            // the only way I found to set all the options we need
            var project = new SSPProject
            {
                destPath = ".",
                spriteSheetName = originalSpriteSheetFileNameWOExt,
                spriteBorder = spritesBorders,
                textureBorder = sheetBorder,
                scalingVariants = new ScalingVariant[]
                {
                    new ScalingVariant
                    {
                        forceSquared = forceSquareSheet,
                        maxTextureSize = maxSheetSize, // > 0 ? maxSheetSize : 8192,
                        //name = @"Result/",
                        pow2 = sheetPowerOf2
                    }
                }
            };
            project.srcList[0] =
                //Path.inputDir.TrimEnd('\\').Replace('\\', '/');
                "Temp";

            string projectFilePath = Path.Combine(outputDir, "TempProject.ssp");

            var serializer = new JavaScriptSerializer();
            var serializedResult = serializer.Serialize(project);
            File.WriteAllText(
                projectFilePath,
                serializedResult
            );


            // * SpriteSheetPacker [options] source destination
            //   source: Sprites for packing or --project file--
            //   destination: Destination --folder-- where saving the sprite sheet

            string args = 
                //'"' + projectFilePath + "\" \"" + outputDir + '"';
                '"' + projectFilePath + '"';

            var process = new Process {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(packerExePath, "SpriteSheetPacker.exe"),
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            //Process.EnterDebugMode();
            var standardOutput = process.StandardOutput;
            var standardError = process.StandardError;
            process.WaitForExit();

            Debug.Write(" * SpriteSheetPacker StandardOutput:\n" + standardOutput.ReadToEnd());
            Debug.Write(" * SpriteSheetPacker StandardError:\n" + standardError.ReadToEnd());

            MessageBox.Show("SpriteSheetPacker ExitCode: " + process.ExitCode);
        }*/
        /// <summary>This function blocks until SpriteSheet Packer finishes the job and returns.</summary>
        /// <param name="packerExeDir"></param>
        /// <param name="inputDir">Path to folder with seaprated sprite images.</param>
        /// <param name="outputDir">Path to folder where resulting packed sprite sheet and data will be saved.</param>
        /// <param name="sheetBorder">Adds transparent margin around the whole result sheet.</param>
        /// <param name="spritesBorders">Adds transparent margin around every sprite.</param>
        /// <param name="sheetPowerOf2">Resutling sheet size will be power of 2.</param>
        /// <param name="maxSheetSize"></param>
        /// <param name="forceSquareSheet"></param>
        public static void RunPackerCommandline(
                string packerExeDir, string inputDir, string outputDir,
                uint sheetBorder = 0, uint spritesBorders = 0, bool sheetPowerOf2 = false, uint maxSheetSize = 8192, bool forceSquareSheet = false
            )
        {
            // * SpriteSheetPacker [options] source destination
            //   source: Sprites for packing or project file
            //   destination: Destination --folder-- where saving the sprite sheet

            string args = " --format pixijs --png-opt-mode None ";

            if (sheetBorder > 0)
                args += " --texture-border " + sheetBorder;

            if (spritesBorders > 0)
                args += " --sprite-border " + spritesBorders;

            if (sheetPowerOf2)
                args += " --powerOf2";

            if (maxSheetSize > 0)
                args += " --max-size " + maxSheetSize;

            // --trimMode Rect  default 
            // --trim 1         default
            // --scale 1        default

            args +=
                $" \"{ inputDir.TrimEnd('\\') }\" " 
                +
                // doesn't work:
                //Path.Combine(outputDir, "Result.png");
                // works:
                $"\"{ outputDir.TrimEnd('\\') }\""
                ;
#if o
            Debug.WriteLine(" * SpriteSheetPacker args:\n\t" + args + '\n');
#endif
            var process = new Process {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(packerExeDir, SPRITESHEETPACKER_EXE),
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            //Process.EnterDebugMode();

            // This handling of standaerd outputs can lead to deadlock. Gt improve it with asynch reading.
            var standardOutputReader = process.StandardOutput;
            var standardErrorReader = process.StandardError;

            process.WaitForExit();

            var standardOutput = standardOutputReader.ReadToEnd();
            var standardError = standardErrorReader.ReadToEnd();
#if o
            Debug.WriteLine(" * SpriteSheetPacker StandardOutput:\n" + standardOutput);
            Debug.WriteLine(" * SpriteSheetPacker StandardError:\n" + standardError);
            Debug.WriteLine(" * SpriteSheetPacker ExitCode: " + process.ExitCode);
#endif
            // for SpriteSheetPacker apparently
            // 1 - no error = result files successfuly created
            // 0 - non-critical error = usualy something wrong with commandline params = no result files be created
            // <0 - hard crash with mistery error code

            if (process.ExitCode != 1)
            {
                throw new AnimChainsSheetPackerException(
                    "SpriteSheet Packer commandline program exited with error.\nStandardOutput: " + standardOutput
                    + "\n\nStandardError: " + standardError, 
                    AnimChainsSheetPackerErrorCode.SpriteSheetPacker_Error
                );
            }
        }

        private static void _BackupOriginalSpriteSheetFile(string outputDir, 
                string originalSpriteSheetDir, string originalSpriteSheetFileName)
        {
            // backup original sprite sheet image file
            //string originalSpriteSheetPath = Path.GetDirectoryName(originalSpriteSheetFilePath).TrimEnd('\\');
            if (string.Equals(outputDir.TrimEnd('\\'), originalSpriteSheetDir.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)) // output dir is the same as original sprite sheet dir
            {
                File.Move(
                    Path.Combine(originalSpriteSheetDir, originalSpriteSheetFileName),
                    //Path.Combine(
                    //    Path.GetDirectoryName(originalSpriteSheetFilePath), 
                    //    Path.GetFileName(originalSpriteSheetFilePath) + "_backup" + Path.GetExtension(originalSpriteSheetFilePath)
                    //)
                    Path.Combine(
                        originalSpriteSheetDir,
                        Path.GetFileNameWithoutExtension(originalSpriteSheetFileName) + "_backup" + Path.GetExtension(originalSpriteSheetFileName)
                    )
                );
            }
        }





        // - Swap sprite sheets ?





        // -- "SpriteSheet Packer" json
        // - Load to objects
        /// <summary>Deserializes Json file produced by SpriteSheet Packer</summary>
        /// <param name="jsonFilePath"></param>
        /// <returns></returns>
        public static Dictionary<string, SSPFrame> LoadPackerJson(string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            var testData = JsonConvert.DeserializeObject<SSPData>(json);
            return testData.frames;
        }





        // -- "SpriteSheet Packer" result sprite sheet
        // - Get sizes
        public static Size GetResultSheetSize(string outputDir)
        {
            Bitmap resultBmp = 
                //Bitmap.FromFile(Path.Combine(inputDir, "Sprites.png"));
                new Bitmap( Path.Combine(outputDir, "Sprites.png") );
            Size size = resultBmp.Size;
            resultBmp.Dispose();
            return size;
        }

        // Hack around Bitmap locking it's file on disk
        private static Bitmap LoadResultSheetNoLock(string imageFilePath, out string tempImageFilePath)
        {
            tempImageFilePath = Path.Combine(Path.GetDirectoryName(imageFilePath), "Sprites_temp.png");

            File.Move(
                imageFilePath,
                tempImageFilePath
            );

            return new Bitmap(tempImageFilePath);
        }




        // - Fix transparent color
        public static Size ChangeResultSheetTransparentColor(string workDir, Color color)
        {
            //                                                 since input directory (containing sprites) for 
            string resultSheetFilePath = Path.Combine(workDir, "Sprites.png");

            // v1: throsw GDI+ exception
            //Bitmap resultBmp = new Bitmap( resultSheetFilePath );
            // v2:
            string tempImageFilePath;
            Bitmap resultBmp = LoadResultSheetNoLock(resultSheetFilePath, out tempImageFilePath);

            Size resultSheetSize = resultBmp.Size;

            ChangeResultSheetTransparentColor(resultBmp, color);

            resultBmp.Save(resultSheetFilePath, ImageFormat.Png);
            resultBmp.Dispose();

            File.Delete(tempImageFilePath);

            return resultSheetSize;
        }
        public static void ChangeResultSheetTransparentColor(Bitmap resultBitmap, Color color)
        {
            BitmapManipulation.SetTransparentColor(resultBitmap, color);
        }




        // --- Update FRB Anims' Frames with new rects
        /// <summary>Takes all original AnimationFrames and updates them for new packed Sprite sheet.</summary>
        /// <param name="animChainListSave">Original AnimationChainListSave altered thru ChopSpriteSheetToSpriteImages()</param>
        /// <param name="animsInPixels">Data from ChopSpriteSheetToSpriteImages()</param>
        /// <param name="packerData">Data from SpriteSheet Packer result Json</param>
        /// <param name="resultSheetSize">Pixel size of packed Sprite sheet</param>
        /// <param name="offsetForAllFrames">Relative offset that will be added to all Frames</param>
        public static void UpdateAnimChains(
                AnimationChainListSave animChainListSave, 
                PixelsFrame[][] animsInPixels,
                Dictionary<string, SSPFrame> packerData,
                Size resultSheetSize,
                Vector2 offsetForAllFrames = new Vector2()
            )
        {
            List<AnimationChainSave> frbAnimChains = animChainListSave.AnimationChains;

            for (int animI = 0; animI < frbAnimChains.Count; animI++)
            {
                for (int frameI = 0; frameI < frbAnimChains[animI].Frames.Count; frameI++)
                {
#if o
                    Debug.WriteLine($"** Frame {animI} {frameI}");
#endif
                    AnimationFrameSave frbFrame = frbAnimChains[animI].Frames[frameI];

                    //KeyValuePair<string, SSPFrame> packerKV;

                    #region    --- frame is not duplicate
                    if (frbFrame != null)
                    {
#if o
                        Debug.WriteLine(" * Frame is not duplicate");
#endif
                        PixelsFrame frameConversionToPixelsData = animsInPixels[animI][frameI];

                        string frameIdString = "Sprites/" + _CreateFrameIdString(animI, frameI);

                        SSPFrame packerFrame;
                        if (packerData.TryGetValue(frameIdString, out packerFrame)) // frame doesn't have zero size
                        {
                            // Remove data from Dictionary - faster future search
                            packerData.Remove(frameIdString);

                            /*if (animChainListSave.CoordinateType == FlatRedBall.Graphics.TextureCoordinateType.Pixel)
                                originalFrameWidthInFractPixels = (decimal)frbFrame.RightCoordinate - (decimal)frbFrame.LeftCoordinate;
                            else // TextureCoordinateType.UV
                                originalFrameWidthInFractPixels = frameConversionToPixelsData.DecimalRight - frameConversionToPixelsData.DecimalLeft;*/
                            decimal originalFrameWidthInFractPixels = 
                                _CalculateOriginalFrameWidthInFractPixels(animChainListSave.CoordinateType, frbFrame, frameConversionToPixelsData);

                            /*if (animChainListSave.CoordinateType == FlatRedBall.Graphics.TextureCoordinateType.Pixel)
                                originalFrameHeightInFractPixels = (decimal)frbFrame.BottomCoordinate - (decimal)frbFrame.TopCoordinate;
                            else // TextureCoordinateType.UV
                                originalFrameHeightInFractPixels = frameConversionToPixelsData.DecimalBottom - frameConversionToPixelsData.DecimalTop;*/
                            decimal originalFrameHeightInFractPixels =
                                _CalculateOriginalFrameHeightInFractPixels(animChainListSave.CoordinateType, frbFrame, frameConversionToPixelsData);



                            #region    - Calculate updated frame coordinates in fract pixels
                            // - coordinates on bitmap in pixels - should be just updated to new coordinates from SSPacker
                            //   but FRB coordinates on bitmap can be fractional !

                            // -- x coordinate on texture
                            DecimalRect updatedFractPixelCoordinates = new DecimalRect();
                            decimal fractPart;
                            if (_TryGetFractPart(frbFrame.LeftCoordinate, out fractPart))
                            {
                                // previously was floored = moved left. now I have to move it back right by it's fractional part.

                                updatedFractPixelCoordinates.Left =
                                    // get fract part
                                    //(frame.LeftCoordinate - (int)frame.LeftCoordinate) + sspFrame.frame.x;

                                    packerFrame.frame.x + fractPart;
                            }
                            else
                            {
                                updatedFractPixelCoordinates.Left = packerFrame.frame.x;
                            }

                            if (_TryGetFractPart(frbFrame.RightCoordinate, out fractPart))
                            {
                                // previously was ceiled = moved right. now I have to move it back left by it's fractional part.

                                updatedFractPixelCoordinates.Right =
                                    //sspFrame.frame.x + sspFrame.frame.w - (frbFrame.RightCoordinate - (int)frbFrame.RightCoordinate);

                                    packerFrame.frame.x + packerFrame.frame.w - fractPart;
                            }
                            else
                            {
                                updatedFractPixelCoordinates.Right = packerFrame.frame.x + packerFrame.frame.w;
                            }


                            // -- y coordinate on texture
                            if (_TryGetFractPart(frbFrame.TopCoordinate, out fractPart))
                            {
                                // previously was floored = moved up. now I have to move it back down by it's fractional part.

                                updatedFractPixelCoordinates.Top =
                                    // get fract part
                                    //(frame.LeftCoordinate - (int)frame.LeftCoordinate) + sspFrame.frame.x;

                                    packerFrame.frame.y + fractPart;
                            }
                            else
                            {
                                updatedFractPixelCoordinates.Top = packerFrame.frame.y;
                            }

                            if (_TryGetFractPart(frbFrame.BottomCoordinate, out fractPart))
                            {
                                // previously was ceiled = moved down. now I have to move it back up by it's fractional part.

                                updatedFractPixelCoordinates.Bottom =
                                    //sspFrame.frame.x + sspFrame.frame.w - (frbFrame.RightCoordinate - (int)frbFrame.RightCoordinate);

                                    packerFrame.frame.y + packerFrame.frame.h - fractPart;
                            }
                            else
                            {
                                updatedFractPixelCoordinates.Bottom = packerFrame.frame.y + packerFrame.frame.h;
                            }
                            #endregion - Calculate updated frame coordinates in fract pixels END


                            #region    - Udate FRB Frame positions
                            if (animChainListSave.CoordinateType == FlatRedBall.Graphics.TextureCoordinateType.Pixel)
                            {
                                frbFrame.LeftCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Left);
                                frbFrame.RightCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Right);
                                frbFrame.TopCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Top);
                                frbFrame.BottomCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Bottom);
                            }
                            else // TextureCoordinateType.UV
                            {
                                // calculate UV coordinates from 

                                frbFrame.LeftCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Left / resultSheetSize.Width);
                                frbFrame.RightCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Right / resultSheetSize.Width);
                                frbFrame.TopCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Top / resultSheetSize.Height);
                                frbFrame.BottomCoordinate = Decimal.ToSingle(updatedFractPixelCoordinates.Bottom / resultSheetSize.Height);
                            }
                            #endregion - Udate FRB Frame positions END


                            #region    - Update FRB Frame offset
#if o
                            Debug.WriteLine(" * Updating FRB Frame offset");
#endif
                            // - Relative X Y - offset of frame in anim
                            // the sprite potentialy shrunk in packing. (it can never get bigger)

                            // did it shrink ?
                            int originalFrameSizeInIntPixels_Width = frameConversionToPixelsData.Right - frameConversionToPixelsData.Left;
                            int originalFrameSizeInIntPixels_Height = frameConversionToPixelsData.Bottom - frameConversionToPixelsData.Top;
                            //trimmedFrameSizeInIntPixels.Width = sspFrame.frame.w;
                            //trimmedFrameSizeInIntPixels.Height = sspFrame.frame.h;
                            if (originalFrameSizeInIntPixels_Width != packerFrame.frame.w)
                            {
                                // It did shrink on X
#if o
                                Debug.WriteLine("\t Sprite W shrinked");
                                if (frbFrame.FlipHorizontal)
                                    Debug.WriteLine("\t Fliped Horizontal");
                                Debug.Write("\n");
#endif
                                frameConversionToPixelsData.ShrunkInPacking_Width = true;

                                // calculate Frame X offset

                                /*// v1
                                decimal originalCenterX = originalFrameWidthInFractPixels / 2M;

                                decimal trimmedWidth = updatedFractPixelCoordinates.Right - updatedFractPixelCoordinates.Left;
                                decimal trimmedWrongCenterX = trimmedWidth / 2M;

                                decimal trimmedGoodCenterX = originalCenterX - packerFrame.spriteSourceSize.x;
                                decimal centerXoffset = -(trimmedGoodCenterX - trimmedWrongCenterX);

                                // Store calculated clean correction offset
                                frameConversionToPixelsData.PackingCorrectionOffsetX = Decimal.ToSingle(centerXoffset);
                                */
                                
                                // Store calculated clean correction offset
                                frameConversionToPixelsData.PackingCorrectionOffsetX = 
                                    // v2
                                    _CalculateCorrectionOffsetX(
                                        originalFrameWidthInFractPixels, updatedFractPixelCoordinates, 
                                        packerFrame.spriteSourceSize, frbFrame.FlipHorizontal );

                                // Acknowledge any previous offset (set on the Frame by anim creator)
                                frbFrame.RelativeX += frameConversionToPixelsData.PackingCorrectionOffsetX + offsetForAllFrames.X;
#if o
                                Debug.WriteLine(
                                    "\n\t * frbFrame.RelativeX: " + frbFrame.RelativeX
                                );
#endif
                            }
                            // It didn't shrink on X => no correction offset needed
#if o
                            Debug.Write("\n");
#endif
                            if (originalFrameSizeInIntPixels_Height != packerFrame.frame.h)
                            {
                                // It did shrink on Y
#if o
                                Debug.WriteLine("\t Sprite H shrinked");
                                if (frbFrame.FlipVertical)
                                    Debug.WriteLine("\t Fliped Vertical");
                                Debug.Write("\n");
#endif
                                frameConversionToPixelsData.ShrunkInPacking_Height = true;

                                // calculate Frame Y offset

                                /*// v1
                                decimal originalCenterY = originalFrameHeightInFractPixels / 2M;

                                decimal trimmedHeight = updatedFractPixelCoordinates.Bottom - updatedFractPixelCoordinates.Top;
                                decimal trimmedWrongCenterY = trimmedHeight / 2M;

                                decimal trimmedGoodCenterY = originalCenterY - packerFrame.spriteSourceSize.y;
                                decimal centerYoffset = trimmedGoodCenterY - trimmedWrongCenterY;

                                // Store calculated clean correction offset
                                frameConversionToPixelsData.PackingCorrectionOffsetY = Decimal.ToSingle(centerYoffset);
                                */

                                // Store calculated clean correction offset
                                frameConversionToPixelsData.PackingCorrectionOffsetY =
                                    // v2
                                    _CalculateCorrectionOffsetY(
                                        originalFrameHeightInFractPixels, updatedFractPixelCoordinates, 
                                        packerFrame.spriteSourceSize, frbFrame.FlipVertical );

                                // Acknowledge any previous offset (set on the Frame by anim creator)
                                frbFrame.RelativeY += frameConversionToPixelsData.PackingCorrectionOffsetY + offsetForAllFrames.Y;
#if o
                                Debug.WriteLine(
                                    "\n\t * frbFrame.RelativeY: " + frbFrame.RelativeY
                                );
#endif
                            }
                            // It didn't shrink on Y => no correction offset needed


                            // ??
                            // being trimmed from right doesn't matter
                            // have to compensate for trimming from left side by adding to RelativeX

                            #endregion - Update FRB Frame offset END
                        }
                        #region    - frame has zero size
                        else // frame has zero size
                        {
#if o
                            Debug.WriteLine("\n\t * frame has zero size");
#endif
                            if (frameConversionToPixelsData.HasZeroWidth)
                            {
                                // since it doesn't matter, place it to the left border of sheet

                                frbFrame.LeftCoordinate = 0f;
                                frbFrame.RightCoordinate = 0f;
                                frbFrame.BottomCoordinate = frbFrame.BottomCoordinate - frbFrame.TopCoordinate;
                                frbFrame.TopCoordinate = 0f;
                            }
                            else // HasZeroHeight
                            {
                                // since it doesn't matter, place it to the top border of sheet

                                frbFrame.TopCoordinate = 0f;
                                frbFrame.BottomCoordinate = 0f;
                                frbFrame.RightCoordinate = frbFrame.RightCoordinate - frbFrame.LeftCoordinate;
                                frbFrame.LeftCoordinate = 0f;
                            }
                        } 
                        #endregion - frame has zero size END
                    }
                    #endregion --- frame is not duplicate END
                    #region    --- frame is duplicate
                    else // frame is duplicate - of some previous frame
                    {
#if o
                        Debug.WriteLine(" * Frame IS duplicate");
#endif
                        // ** Dont forget frame is only duplicate in coordinates
                        //    Frames can still have different offsets, Frame times, V or H flips
                        //    V and/or H fliped frame will have different corrective Left, Top offsets than it's non flipped "master" frame !

                        PixelsFrame frameDuplicateData = animsInPixels[animI][frameI];
                        frbFrame = frameDuplicateData.FRBFrame;

                        // Return original FRB Frame to it's place in AnimChainsList - to preserve Frame time
                        frbAnimChains[animI].Frames[frameI] = frbFrame;

                        decimal originalFrameWidthInFractPixels =
                            _CalculateOriginalFrameWidthInFractPixels(
                                animChainListSave.CoordinateType, frbFrame, frameDuplicateData.MasterPixelsFrame);

                        decimal originalFrameHeightInFractPixels =
                            _CalculateOriginalFrameHeightInFractPixels(
                                animChainListSave.CoordinateType, frbFrame, frameDuplicateData.MasterPixelsFrame);

                        #region    - Udate FRB Frame positions
                        // From "master" frame. Position on new packed sheet is the same no matter flips etc.
                        frbFrame.LeftCoordinate =   frameDuplicateData.MasterFRBFrame.LeftCoordinate;
                        frbFrame.RightCoordinate =  frameDuplicateData.MasterFRBFrame.RightCoordinate;
                        frbFrame.TopCoordinate =    frameDuplicateData.MasterFRBFrame.TopCoordinate;
                        frbFrame.BottomCoordinate = frameDuplicateData.MasterFRBFrame.BottomCoordinate;
                        #endregion - Udate FRB Frame positions END

                        #region    - Update FRB Frame offset
#if o
                        Debug.WriteLine(" * Updating FRB Frame offset");
#endif
                        // Did it shrink ?
                        if (frameDuplicateData.MasterPixelsFrame.ShrunkInPacking_Width)
                        {
                            // It did shrink on X
#if o
                            Debug.WriteLine("\t Sprite W shrinked");
                            if (frbFrame.FlipHorizontal)
                                Debug.WriteLine("\t Fliped Horizontal");
                            Debug.Write("\n");
#endif
                            // Does duplicate have same flips as original frame ?
                            if (frameDuplicateData.FRBFrame.FlipHorizontal == frameDuplicateData.MasterFRBFrame.FlipHorizontal)
                            {
                                // Frames have same H (X) flip
                                // => use correction offset from master frame
#if o
                                Debug.WriteLine("\t Master and Duplicate have same Flip => using offset from Master \n");
#endif
                                frameDuplicateData.FRBFrame.RelativeX +=
                                    frameDuplicateData.MasterPixelsFrame.PackingCorrectionOffsetX + offsetForAllFrames.X;
                            }
                            else
                            {
                                // Frames have different H (X) flip
                                // => use correction offset from master frame, flip value's sign
#if o
                                Debug.WriteLine("\t Master and Duplicate have DIFFERENT Flip => use correction offset from master frame, flip value's sign \n");
#endif
                                frameDuplicateData.FRBFrame.RelativeX +=
                                    -frameDuplicateData.MasterPixelsFrame.PackingCorrectionOffsetX
                                    + offsetForAllFrames.X;
                            }
                        }
                        // It didn't shrink on X => no correction offset needed

#if o
                        Debug.Write("\n");
#endif

                        if (frameDuplicateData.MasterPixelsFrame.ShrunkInPacking_Width)
                        {
                            // It did shrink on Y
#if o
                            Debug.WriteLine("\t Sprite H shrinked");
                            if (frbFrame.FlipVertical)
                                Debug.WriteLine("\t Fliped Vertical");
                            Debug.Write("\n");
#endif
                            if (frameDuplicateData.FRBFrame.FlipVertical == frameDuplicateData.MasterFRBFrame.FlipVertical)
                            {
                                // Frames have same V (Y) flip
                                // => use correction offset from master frame
#if o
                                Debug.WriteLine("\t Master and Duplicate have same Flip => using offset from Master \n");
#endif
                                frameDuplicateData.FRBFrame.RelativeY +=
                                    frameDuplicateData.MasterPixelsFrame.PackingCorrectionOffsetY + offsetForAllFrames.Y;
                            }
                            else
                            {
                                // Frames have different V (Y) flip
                                // => use correction offset from master frame, flip value's sign
#if o
                                Debug.WriteLine("\t Master and Duplicate have DIFFERENT Flip => use correction offset from master frame, flip value's sign \n");
#endif
                                // ..calculate
                                frameDuplicateData.FRBFrame.RelativeY +=
                                    -frameDuplicateData.MasterPixelsFrame.PackingCorrectionOffsetY
                                    + offsetForAllFrames.X;
                            }
                        }
                        // It didn't shrink on Y => no correction offset needed
                        // .. ?
                        #endregion - Update FRB Frame offset END

                    }
                    #endregion --- frame is duplicate END
#if o
                    Debug.Write("\n");
#endif
                }
            }
        }

        private static bool _TryGetFractPart(decimal value, out decimal fractPart)
        {
            // return (value - (int)value);

            //return ( value - Math.Truncate(value) );

            decimal integralPart = Math.Truncate(value);
            if (value == integralPart)
            {
                fractPart = 0M;
                return false;
            }
            else
            {
                fractPart = value - integralPart;
                return true;
            }
        }

        private static bool _TryGetFractPart(float value, out decimal fractPart)
        {
            // return (value - (int)value);

            //return ( value - Math.Truncate(value) );
            decimal decimalValue = (decimal)value;
            decimal integralPart = Math.Truncate(decimalValue);
            if (decimalValue == integralPart)
            {
                fractPart = 0M;
                return false;
            }
            else
            {
                fractPart = decimalValue - integralPart;
                return true;
            }
        }


        private static float _CalculateCorrectionOffsetX(
            decimal originalFrameWidthInFractPixels, DecimalRect updatedFractPixelCoordinates, 
            SSPRectSprite offsetsFromOriginalSprite, bool flipped )
        {
            // old big size 205 x 205
            // old center
            // [old big size] / 2
            decimal originalCenterX = originalFrameWidthInFractPixels / 2M;

            // new trim excentric SIZE
            decimal trimmedWidth = updatedFractPixelCoordinates.Right - updatedFractPixelCoordinates.Left;

            // new trim wrong excentric center 
            // [trimmed width or height] / 2
            decimal trimmedWrongCenterX = trimmedWidth / 2M;

            // new trim good center 
            // [old center] - [trim offset]
            // decimal trimmedGoodCenterX = originalCenterX - packerFrame.spriteSourceSize.x;
            /*decimal trimmedGoodCenterX;
            if (flipped)
            {
                // Use offset from right, instead of left
                decimal offsetFromRight = originalFrameWidthInFractPixels - trimmedWidth - offsetsFromOriginalSprite.x;
                trimmedGoodCenterX = originalCenterX - offsetFromRight; // + / - ?
            }
            else
            {
                // Use offset from left
                trimmedGoodCenterX = originalCenterX - offsetsFromOriginalSprite.x;
            }*/
            // Use offset from left
            decimal trimmedGoodCenterX = originalCenterX - offsetsFromOriginalSprite.x;

            decimal centerXoffset;
            if (flipped)
                centerXoffset = trimmedGoodCenterX - trimmedWrongCenterX;
            else
                centerXoffset = -(trimmedGoodCenterX - trimmedWrongCenterX);
#if o
            Debug.WriteLine(
                "\t originalWidth: " + originalFrameWidthInFractPixels
                +
                "\n\t originalCenterX: " + originalCenterX
                +
                "\n\t trimmedWidth: " + trimmedWidth
                +
                "\n\t trimmedWrongCenterX: " + trimmedWrongCenterX
                +
                "\n\t trimmedGoodCenterX: " + trimmedGoodCenterX
                +
                "\n\t centerXoffset: " + centerXoffset
            );
#endif
            return Decimal.ToSingle(centerXoffset);
        }

        private static float _CalculateCorrectionOffsetY(
            decimal originalFrameHeightInFractPixels, DecimalRect updatedFractPixelCoordinates, 
            SSPRectSprite offsetsFromOriginalSprite, bool flipped )
        {
            decimal originalCenterY = originalFrameHeightInFractPixels / 2M;

            decimal trimmedHeight = updatedFractPixelCoordinates.Bottom - updatedFractPixelCoordinates.Top;
            decimal trimmedWrongCenterY = trimmedHeight / 2M;

            // Use offset from top
            //decimal trimmedGoodCenterY = originalCenterY - packerFrame.spriteSourceSize.y;
            decimal trimmedGoodCenterY = originalCenterY - offsetsFromOriginalSprite.y;

            decimal centerYoffset;
            if (flipped)
                centerYoffset = -(trimmedGoodCenterY - trimmedWrongCenterY);
            else
                centerYoffset = trimmedGoodCenterY - trimmedWrongCenterY;

#if o
            Debug.WriteLine(
                "\t originalHeight: " + originalFrameHeightInFractPixels
                +
                "\n\t originalCenterY: " + originalCenterY
                +
                "\n\t trimmedHeight: " + trimmedHeight
                +
                "\n\t trimmedWrongCenterY: " + trimmedWrongCenterY
                +
                "\n\t trimmedGoodCenterY: " + trimmedGoodCenterY
                +
                "\n\t centerYoffset: " + centerYoffset
            );
#endif
            return Decimal.ToSingle(centerYoffset);
        }


        private static decimal _CalculateOriginalFrameWidthInFractPixels(
            FlatRedBall.Graphics.TextureCoordinateType textureCoordinateType, AnimationFrameSave frbFrame, PixelsFrame frameConversionToPixelsData)
        {
            if (textureCoordinateType == FlatRedBall.Graphics.TextureCoordinateType.Pixel)
                return (decimal)frbFrame.RightCoordinate - (decimal)frbFrame.LeftCoordinate;
            else // TextureCoordinateType.UV
                return frameConversionToPixelsData.DecimalRight - frameConversionToPixelsData.DecimalLeft;
        }

        private static decimal _CalculateOriginalFrameHeightInFractPixels(
            FlatRedBall.Graphics.TextureCoordinateType textureCoordinateType, AnimationFrameSave frbFrame, PixelsFrame frameConversionToPixelsData)
        {
            if (textureCoordinateType == FlatRedBall.Graphics.TextureCoordinateType.Pixel)
                return (decimal)frbFrame.BottomCoordinate - (decimal)frbFrame.TopCoordinate;
            else // TextureCoordinateType.UV
                return frameConversionToPixelsData.DecimalBottom - frameConversionToPixelsData.DecimalTop;
        }



        // -- Result achx
        public static void SaveAchx(AnimationChainListSave animChainListSave, string outputAchxFilePath, string originalAchxFilePath)
        {
            if (string.Equals(outputAchxFilePath, originalAchxFilePath, StringComparison.OrdinalIgnoreCase)) // output dir and file name is the same as of original file
            {
                // backup the original file

                File.Move(
                    originalAchxFilePath,
                    Path.Combine(
                        Path.GetDirectoryName(originalAchxFilePath),
                        Path.GetFileNameWithoutExtension(originalAchxFilePath) + "_backup" + Path.GetExtension(originalAchxFilePath)
                    )
                );
            }

            // needs changing ?
            //animChainListData.CoordinateType = FlatRedBall.Graphics.TextureCoordinateType.?

            animChainListSave.Save(outputAchxFilePath);
        }

        public static void PlaceResultSpriteSheetFile(bool overwriteInputFile, 
                                                      string originalSpriteSheetDir, string originalSpriteSheetFileName, 
                                                      string workDirectory, string outputDirectory)
        {
            if (overwriteInputFile)
            {
                string originalSpriteSheetFilePath = Path.Combine(originalSpriteSheetDir, originalSpriteSheetFileName);

                // Delete original sheet file
                //File.Delete(originalSpriteSheetFilePath);

                // Backup original sheet file
                File.Move(
                    originalSpriteSheetFilePath,
                    Path.Combine(originalSpriteSheetDir, Path.GetFileNameWithoutExtension(originalSpriteSheetFileName) + "_backup" + Path.GetExtension(originalSpriteSheetFileName))
                );

                // Move result sheet file to original sheet file dir with original sheet file name
                File.Move(
                    Path.Combine(workDirectory, "Sprites.png"),
                    Path.Combine(originalSpriteSheetDir, Path.GetFileNameWithoutExtension(originalSpriteSheetFileName) + ".png")
                );
            }
            else // not overwriteInputFiles - means outputDirectory != null
            {
                string resultSheetFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(originalSpriteSheetFileName) + ".png");

                // Move result sheet file to output dir with original sheet file name
                if (File.Exists(resultSheetFilePath))
                {
                    // Backup exiting sheet file
                    File.Move(
                        resultSheetFilePath,
                        Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(originalSpriteSheetFileName) + "_backup.png")
                    );
                }

                File.Move(
                    Path.Combine(workDirectory, "Sprites.png"),
                    resultSheetFilePath
                );
            }
        }
    }
}