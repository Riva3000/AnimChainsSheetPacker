Utility for packing sprite sheets coupled with FlatRedBall engine's AnimChainList anim files.  
https://github.com/vchelaru/FlatRedBall  
  
Uses amakaseev's SpriteSheet Packer commandline tool for the packing operations.  
https://github.com/amakaseev/sprite-sheet-packer  
The tool has to be present on the computer for AnimChainsSheetPacker to work.  
AnimChainsSheetPacker is tested with amakaseev's SpriteSheet Packer version 1.0.8 only.

--- Parts:  
- AnimChainsSheetPacker.dll  
    Contain all the packing and achx generation functionality.
    
    For simple use include in your project and use Packer.PackAchx(..) static method.
  
- AnimChainsSheetPackerGUI  
    Windows GUI frontend for the library.  
    Not fully finished. May be unstable.
    
  
