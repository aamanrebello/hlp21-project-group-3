# HLP21 Group 3 Main Repo

This repo contains our final deliverable code for the group project of ELEC96019 - High Level Programming, 2021. It is an F# Elmish MVU implementation of the Draw2D JavaScript library used in [ISSIE](https://github.com/tomcl/ISSIE), which implements many advanced features in addition to those found in the current ISSIE application. 

## Prerequisites to Get the Project Running:

The following need to be installed (if you already installed, just check the version constraints):

- [Dotnet Core SDK](https://dotnet.microsoft.com/download). Version >= 3.1

For Mac and Linux users, download and install [Mono](http://www.mono-project.com/download/stable/) from the official website (the version from brew is incomplete, may lead to MSB error later).

- [Node.js v12](https://nodejs.org/en/download/). Version >= 12

Installing Node.js will automatically install the npm package manager. The latest LTS version of Node is v14, which will almost certainly work here.

## Running the Code

When running for the first time, enter on the command line `build` on Windows or `build.sh` on Linux/MacOS from within this root directory. This allows the package dependencies to be installed.  Subsequent runs can be initiated by the command `npm run dev` from within this root directory.

## External Interface 

Documentation may be found in the [*doc*](doc) folder:
- [*Features.md*](doc/Features.md) lists out the different features of our implementation of the Draw2D library as well as the user interface for each feature. **This is the single page summary of features desired for the coursework assessment**.
- [*External_Interfaces.md*](doc/External_Interfaces.md) is useful for future integration with ISSIE/further development of code. It explains how the program could integrate with the existing ISSIE code e.g. the widthInferrer.
- The folder [*Internal_Interfaces*](doc/Internal_Interfaces) contains the interfaces between the modules *Symbol.fs*, *BusWire.fs* and *Sheet.fs*. This is for reference.

## Individual Code (Related to Coursework)

Initial code written by the group members for their individual modules may be found by navigating to the `indiv_code` branch (link [here](https://github.com/aamanrebello/hlp21-project-group-3/tree/indiv_code)). This code is now highly outdated compared to the latest commits in this repo.
