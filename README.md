# LightshipExtensions
Includes a demo scene and scripts with some new extended functionality to help with creating Lightship ARDK projects.
Developed by Divij Sood, R&D Fellow at InGAME.
InGAME is the UK's dedicated centre for innovation in games and media technology. It is funded by the AHRC as part of the UK Industrial Strategy.

Requires Lightship ARDK 2.1.0
You can upgrade the project but will need to update the WayspotAnchorController script based on update notes from Lightship ARDK.

## Scripts changed:
ObjectHolderController
WayspotAnchorController
PlacementController
LightshipTemplateFactory

## New scripts:
ObjectHolderControllerMult
LookAtTarget script
DirectionalArrow
TakePhoto
Requires Asset: Native Gallery for Android & iOS by Yasirkula
https://assetstore.unity.com/packages/tools/integration/native-gallery-for-android-ios-112630
SerializableTypes based on answers by Cherno and JimmyCushnie at 
https://answers.unity.com/questions/956047/serialize-quaternion-or-vector3.html

## Main features:
In addition to placed object anchor positions, anchor’s child object’s rotation is also saved
The object id for each anchor is saved, so when it loads, the correct object is spawned at each anchor
Function for gaze input added to both placement scripts
DirectionalArrow is a screen-space floating pointer that helps guide the player to an AR object
TakePhoto uses the Native Gallery Plugin to take a screenshot and save it on an Android or Apple device

## Instructions:
Place different types of objects as children under ARController
Assign them to the list ObjectHolders in the inspector, on the ObjectHolderControllerMult component of ARController
Test the scene - Choose which object to spawn by assigning the objectId on the text box above the "Place Object" button. 
Click "Place Object" and then tap the screen over a detected plane to place the object
Repeat for other types of objects
Test the Save, Clear, and Load buttons next
The directional arrow's target is assigned by the placement scripts to show how this works
Click "Take Photo" to take a screenshot and store it on-device

## Coming soon:
Gaze-based interaction: Object glow brighter as you align your view with it
InteractiveObject interface
