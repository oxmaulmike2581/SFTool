# SFTool
Tool for working with models from Sketchfab
<hr />
<h3>This tool is officially deprecated. No more updates or support.</h3>
This is caused because every day the number of models stored in binz format is growing rapidly.<br />
I know about all methods to convert .binz back to .osgjs/.bin, but I don't want to use it.<br />
I don't want to add methods that execute code from the browser, whether in the user's default browser or in a sandboxed one.<br />
A time ago I didn't find any documenation about structure of .binz format, so I can't make a proper parser.<br />
Understand me, all these methods with launching separate browsers, transferring data from one application to another, and combinations of utilities that are different in their original purpose are terrible crutches that can only be used as a temporary solution.<br />
So I decided to officially stop the support of my tool and mark it as archived.<br />
I have not received any requests to stop development or remove the utility, and I hope I never will.<br />
You can continue to use my utility however you want, but I cannot guarantee that it will work. Its code is open, so if you want to improve it - your time has come.<br />
This was and remains my first serious application and experience in their development, and I want to move further in this direction.<br />
I will not leave links to similar software here, but I assure you, it is easy to find it through Google.<br />
<hr />
At the moment, only downloading models is supported. In the future, it may be possible to add support for converting to Autodesk FBX format.
<br />
If you want to download all models of specified author, first you need to get the list of model uids. You can do that using my second tool named <a href="https://github.com/DARKSIDEOPG/SFUserParser">SFUserParser</a>.

<hr />

### Usage:
* SFTool.exe -h <em>model_hash</em> -o <em>output_dir</em>
* SFTool.exe -l <em>links_file</em> -o <em>output_dir</em>

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
The BINZ format is new to Sketchfab. It looks like it's just an additional layer of encryption on top of the existing OSGJS and BIN files instead of GZip compression. Work is underway to study it and create a decoder, but it is too early to talk about the timing of its implementation. 
<br />

I know about many scripts and utilities similar to mine, including those that are somehow capable of bypassing the .binz format.
Unfortunately, so far not a single author has been able (at least publicly) to draw up a specification of this format.
Add scripts for browsers or separate ready-made .exe / .py / etc. in my utility I do not want. From the point of view of an integrated approach, the best solution would be to add a full-fledged parser for both binz and osgjs formats.
<br />

To import the downloaded model, look for the Blender script on Google. Perhaps later, when I implement the conversion of the model into an editable format, this item will be removed.
<br />

This is my first serious application in C#, so the code can be crappy in places or not fully optimized. I originally wrote it for .NET 5.0, but later decided to switch to .NET Framework 4.7.2 for better compatibility.

<hr />

### Thanks:
* <a href="https://github.com/StrixG/" target="_blank">@StrixG</a> - for help

<hr />

### Changelog: <br />
<strong>1.0.0.4:</strong>
<ol>
  <li>Another fix for BINZ downloading (previous fix wasn't work correctly, sorry about that)</li>
</ol>
<br />
<strong>1.0.0.3:</strong>
<ol>
  <li>Fixed saving error when trying to download a model in the new encrypted BINZ format</li>
  <li>Added a warning if the downloaded model is in BINZ format</li>
</ol>
<p>
  The BINZ format is new to Sketchfab. It looks like it's just an additional layer of encryption on top of the existing OSGJS and BIN files instead of GZip compression. Work is underway to study it and create a decoder, but it is too early to talk about the timing of its implementation.
</p>
<br />
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
