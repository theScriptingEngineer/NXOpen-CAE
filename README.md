# NXOpen-CAE

This is a repository with NXOpen code for Siemens SimCenter 3D (aka NX CAE) in C#
If you’re using my scripts in your daily work, saving you a lot of work and time, buy me a coffe so I can continue developing awesome scripts.
[Buy me a coffe](https://www.buymeacoffee.com/theScriptingEngineer)

## Learning NXOpen
I currently have 2 courses available for learning NXOpen. A beginner course in python to get you started by setting up your developer environment and teach some basic concepts:

[Siemens NX beginner NXOpen course (Python)](https://www.udemy.com/course/siemens-nx-beginner-nxopen-course-python/?referralCode=DEE8FAB445765802FEDC)

An in depth course walking you through a complete structural analysis, step by step, towardw a fully automated analysis from load/bc application till postprocessing:

[SimCenter 3D basic NXOpen course (C#)](https://www.udemy.com/course/simcenter3d-basic-nxopen-course/?referralCode=4ABC27CFD7D2C57D220B%20)

>Use the code **NXOPEN_PYTHON_SEP24** or **NXOPEN_CSHARP_SEP24** to get the current best price (replace *SEP* with the current 3 letter month eg. DEC if the current month is December)
>

If you’re using my scripts in your daily work, saving you a lot of work and time, buy me a coffe so I can continue developing awesome scripts.
[Buy me a coffe](https://www.buymeacoffee.com/theScriptingEngineer)


## Documentation of the journals in this repository

[NXOpen C# documentation](https://nxopencsdocumentation.thescriptingengineer.com/)

## Setup

I'm using Visual Studio Code for creating my journals.

### SimCenter dll's for intellisense

Because of copyright concerns I did not add the dll's required for intellisense to this repository. Also, you want to use the dll's from the specific SimCenter version that you are using.

In order to have intellisense do either of the following:
 1. Copy the following SimCenter dll's into the dlls folder:
   * NXOpen.dll
   * NXOpen.Utilities.dll
   * NXOpen.UF.dll
   * NXOpenUI.dll
 2. Update the references in NXOpenCAE.csproj to point to the dll's for the specific version that you are using.

 ### VSCode +.NET framework

 This repository and the course on Udemy focus on journals only, meaning none of the code is compiled.
 Every file can run as a journal, without additional licenses (provided you have the licenses to perform the same operations in the GUI)
 Since I'm writing the journals on a linux machine, I'm referencing the netcoreapp3.1 as the targetframework, so I get a working intellisense.

 For windows users please visit my [course]((https://www.udemy.com/course/simcenter3d-basic-nxopen-course/?referralCode=4ABC27CFD7D2C57D220B%20)) or the excellent blog from [Jan Boetcher](https://www.ib-boettcher.de/en/post/nxopen-vscode/)
