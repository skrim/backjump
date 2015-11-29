# Backjump
​
Ever needed a quick way to review building plans on-site and share comments with teams?
​
With Backjump, you can instantly locate yourself and visualize the plans around you in 3D. You can switch different view layers containing walls, structures, piping, cabling, equipment or furnishing. By pointing and tapping, a pinned remark can be attached to any point in 3D model.
​
Backjump uses Google Tango augmented reality technology to track user movement on-site and superimpose 3D building information models (BIM) on camera viewfinder.
​
## Requirements
​
  - Tested with Unity 5.2.3f1
​
## Usage
​
  - Start the app. It should end in config mode with the 3D model floating in front of you
  - Select 1st point on model: Move the device and tap Select to confirm
  - Select 1st point in real world. Move the device and tap Select to confirm. 1st point in model and real world are aligned.
  - Select 2st point on model: Move the device and tap Select to confirm
  - Select 2nd point in real world. Move the device and tap Select to confirm. The model is now aligned to real world. 
​
  - Tap "Mode" button toggle between layers. Note that layers except for Architecture are empty in example model.
  - Tap "Select" or "Pin" to place an annotation
​
## Preparing your own model for Backjump
​
  - You may use any model Unity supports
  - Tested toolchain: Rhinoceros 3D --> export .fbx as meshes
  - See next chapter for layer names if you wish to toggle between display modes
​
## Using Backjump to visualize your own model
​
  - Prepare a model with HVACE elements on layers "IV, Vesi, 3D 2D Viivat"
  - Prepare a model with STRUCTURAL elements on layers "Laatat, Pilarit, Ikkunat"
  - ARCHITECTURAL mode displays all layers
  - Alternatively, modify layer names defined in Assets/Backjump/BackjumpSceneManagerBehavior.cs:
​
```sh
@"ARK: *
HVACE: IV, Vesi, 3D 2D Viivat
STRUCTURAL: Laatat, Pilarit, Ikkunat"
```
​
  - Open Unity
  - Open Scenes/BackjumpTango scene
  - Drag the new model (preferably in .fbx format) from file manager to BackjumpTango scene.
  - You may wish to modify the alpha value of "Albeido" material property to 50% to make the model half-transparent
  - Link your model to "Model" property of BackjumpSceneManager
  - You may wish to remove "Wooden Frame" from scene
​
## Compiling
​
  - Open "Build Settings"
  - Select "Android" and "Switch Platform"
  - Uncheck "Development Build" in "Build Settings"
  - Connect device and hit "Build and Run"
​
## Known issues
​
  - 3D Model is hardcoded
  - Config button does not recalibrate model. Workaround: Close and restart app instead.
  - Annotations cannot be edited. Instead, hardcoded marker texts are used
  - Function of Select and Pin buttons should be reiterated