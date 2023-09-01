![logo](md_img/decraft_logo.png)
# DECRAFT
Advanced launcher for very old Minecraft.
![ui_image_1](md_img/ui_1.png)

## About the project
Jar files for very old (preclassic to Indev) versions of Minecraft can be hard to get running. The goal of this project is to make a launcher that can easily make them work without modifying them.

## Running

DECRAFT is currently in a work-in-progress state, so currently, the preferred way is to build it yourself. You will need Visual Studio 2019-2022 with `.NET desktop development` tools installed.

## Features

Contributions through PRs are welcome.

- Scanning all classes for entry points
- Launching through `public static void main(String[])`
- Launching `Applet`s through a wrapper
- Simple old login server emulation
- Custom instance directories