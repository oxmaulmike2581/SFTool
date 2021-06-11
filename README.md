# SFTool
Tool for working with models from Sketchfab
<br />
<br />
At the moment, only downloading models is supported. In the future, it may be possible to add support for converting to Autodesk FBX format.

<hr />

### Usage:
* SFTool.exe -h model_hash -o output_dir
* SFTool.exe -l links_file -o output_dir

<strong>Arguments:</strong>
<ul>
  <li>-h:<br />
  &#9;Model hash or whole link.
  </li>
  <li>-l:<br />
  &#9;Path to the file with list of links to batch download.
  </li>
  <li>-o <em>(optional)</em>: <br />
  &#9;Path to the output directory. If does not specified, the current directory will used.
  </li>
</ul>

<hr />

### Requirements:
* <a href="https://dotnet.microsoft.com/download/dotnet-framework/net472" target="_blank">.NET Framework 4.7.2</a>

<hr />

### Notes:

To import the downloaded model, look for the Blender script on Google. Perhaps later, when I implement the conversion of the model into an editable format, this item will be removed.
<br />

This is my first serious application in C#, so the code can be crappy in places or not fully optimized. I originally wrote it for .NET 5.0, but later decided to switch to .NET Framework 4.7.2 for better compatibility.

<hr />

### Thanks:
* <a href="https://github.com/StrixG/" target="_blank">@StrixG</a> - for help

<hr />

### Changelog:
<strong>1.0.0.2:</strong>
<ol>
  <li>Replaced the exception with the message to the console, in the case when the output folder already exists</li>
  <li>Removed waiting for pressing any key after processing the model</li>
  <li>Added a greeting logo</li>
  <li>Added help when called without arguments instead of exception</li>
  <li>Added 3 modes of operation:<br />
    <ul>
      <li>with the "-h" key: one link or hash of the model;</li>
      <li>with the "-l" key: path to the file containing the list of links and / or hashes (one per line);</li>
      <li>by the presence of the file "sftool_links.txt" in the folder with the program, this overrides the two keys above and automatically activates the batch mode;</li>
    </ul>
  </li>
  <li>Changed the default namespace, now it matches the name of the output exe file</li>
  <li>Moved the project files to a separate folder so that there is no mess after the build</li>
  <li>Updated version of the program</li>
  <li>Removed unnecessary files and folders in the repository</li>
  <li>The work on the parser of the OSGJS format has begun (it will appear in the next versions)</li>
</ol>
