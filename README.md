<h1 align="center">OpenTS2</h1>
OpenTS2 is an Open-Source Reimplementation of The Sims 2, using the Unity game engine. Also aims to provide a number of modular libraries for working with TS2 formats in a C# environment.

## Progress
Currently a basic main menu with a neighborhood chooser is implemented. Neighborhood info can be previewed and a basic neighborhood view can be accessed.
![image](https://github.com/LazyDuchess/OpenTS2/assets/42678262/7ec4b21f-8987-41e6-b780-cca694698354)


## Acknowledgements
* [InvertedTomato.CRC](https://github.com/invertedtomato/crc)
* [TGA Image Reader](https://www.codeproject.com/Articles/31702/NET-Targa-Image-Reader)
* [Hardware Cursor Plugin](https://forum.unity.com/threads/hardware-cursor-plugin.70163/)

## Similar Projects
* [FreeSO](https://github.com/RHY3756547/FreeSO) - Open Source reimplementation of The Sims Online using C# and Monogame. OpenTS2 borrows a lot of code and structure from this project.
* [Las Marionetas](https://github.com/OmniBlade/LasMarionetas) - Similar project, aiming to reimplement The Sims 2 by reverse engineering its binary code into C/C++ source code.
* [SimUnity2](https://github.com/LazyDuchess/SimUnity2) - Earlier attempt at a TS2 reimplementation in the Unity engine. Abandoned, succeeded by this project.
* [OpenTPW](https://github.com/ThemeParkWorld/OpenTPW) - Open Source reimplementation of Sim Theme Park / Theme Park World.
* [OpenRCT2](https://github.com/OpenRCT2/OpenRCT2) - Open Source reimplementation of Rollercoaster Tycoon 2.

## License
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.

# Development

## Prerequisites
* [Unity 2020.3.32f1](https://unity3d.com/get-unity/download/archive) - Can be found under "Unity 2020.x", you could also download the Unity Hub and install from there. Unity version is subject to change, please keep an eye on this!
* [Visual Studio 2019](https://visualstudio.microsoft.com/vs/)
* A copy of The Sims 2 Ultimate Collection

## Setup
1. You need to have a "config.json" file in the root folder that provides paths to your Sims 2 UC installation and user directories. Copy the "config.example.json" file and rename it to "config.json" to start off, and type in your own paths.
2. In Unity, make sure Edit > Preferences > External Tools > External Script Editor is set to Visual Studio. By default it opens files separately as opposed to in a solution.

## Project Structure

We follow the layout of a normal Unity project except:

* `Assets/Scripts/OpenTS2` - Contains the bulk of the C# code that deals with TS2 formats and files.
* `Assets/Scripts/OpenTS2/Engine` - Unity *specific* code tends to live in here to keep it
  separate from the more language-independent code in the directory.
* `Assets/Tests/OpenTS2/` - Unit tests following the same directory structure as the `Scripts` folder.

## Testing

We currently use the [Unity Test Runner](https://docs.unity3d.com/2020.3/Documentation/Manual/testing-editortestsrunner.html)
for unit testing code. These tests can be run inside of unity through the test runner tab or if you use Rider as your C#
editor, inside of it.
