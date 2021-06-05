# SFTool
Tool for working with models from Sketchfab
<br />
<br />
At the moment, only downloading models is supported. In the future, it may be possible to add support for converting to Autodesk FBX format.
<br />
<br />
<hr />

### Usage:
* SFTool.exe model_hash output_dir

##### Where:
* model_hash - the whole URL or last 26/27/32 characters from URL (length depends on model; some models have unique hashes).
* output_dir *(optional)* - path where output files should be located

<hr />

### Requirements:

* .NET Framework 4.7.2

<hr />

To import the downloaded model, look for the Blender script on Google. Perhaps later, when I implement the conversion of the model into an editable format, this item will be removed.
<br />

This is my first serious application in C#, so the code can be crappy in places or not fully optimized. I originally wrote it for .NET 5.0, but later decided to switch to .NET Framework 4.8 for better compatibility.
<br />

### Thanks
* @StrixG - for help

