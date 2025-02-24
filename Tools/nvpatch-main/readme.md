# nvpatch

A simple command line utitly to patch existing x64 executables to include the 
export symbols `NvOptimusEnablement` and `AmdPowerXpressRequestHighPerformance` 
as required to enable the discreet GPU on some machines (mainly laptops)

Originally written to be used with the game [Sector's Edge](https://sectorsedge.com) 
it should work with any x64 Windows application.

## Experimental Status

At this stage, this project should be considered experimental - I've only just 
got it working and have only done some very basic testing. I'm keen 
to hear how it works for others.

If you find problems or have suggestions, please [log an issue](https://github.com/toptensoftware/nvpatch/issues)

Or, if it works for you, please drop me an [email](https://www.toptensoftware.com/contact)
and let me know.


## Installation and Usage

To install:

> dotnet tool install -g Topten.nvpatch

(requires .NET 5 installed)

One installed you can patch an existing executable with the following command:

> nvpatch --enable MyProgram.exe

Currently on x64 executables are supported.


## How it Works

See [this blog post](https://www.toptensoftware.com/blog/nvpatch-how-it-works/).


## More Options

```
nvpatch v0.1.103.0
Copyright c 2021 Topten Software. All Rights Reserved

Usage: nvpatch [options] <inputfile.exe> [<outputfile.exe]

Adds, updates or queries the export symbols 'NvOptimusEnablement'
and 'AmdPowerXpressRequestHighPerformance' in an existing .exe

Options:
  --enable       sets GPU export symbols to 1 (adding if missing
  --disable      sets GPU export symbols to 0 (if it exists)
  --status       shows the current NvOptimusEnablement status
  --help         show this help, or help for a command
  --version      show version information
```


## License

Copyright © 2014-2021 Topten Software.  
All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License") you may not use this
product except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under
the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.</p>
