Utility for packing sprite sheets referenced by FlatRedBall engine AnimChainList anim files.  
https://github.com/vchelaru/FlatRedBall  
  
Uses amakaseev's SpriteSheet Packer commandline tool for the packing operations.  
https://github.com/amakaseev/sprite-sheet-packer  
The tool has to be present on the computer for AnimChainsSheetPacker to work.  
  
--- Parts:  
- AnimChainsSheetPacker.dll  
    Contain all the packing and achx generation functionality.
    
    For simple use include in your project and use Packer.PackAchx(..) static method.
  
- AnimChainsSheetPackerGUI  
    GUI frontend for the library.  
    Currently unfinished and probably unstable. Do not use on live data.
    
  
