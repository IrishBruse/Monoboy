# Monoboy

<p align="center">
<img src="./Icon.ico">
</p>

Reference: https://gbdev.io/pandocs/

## Gameboy Controls

| Gameboy | Keyboard  | Gamepad | Alternative       |
| ------- | --------- | ------- | ----------------- |
| Right   | D         | Stick   | Dpad / Arrow Keys |
| Left    | A         | Stick   | Dpad / Arrow Keys |
| Up      | W         | Stick   | Dpad / Arrow Keys |
| Down    | S         | Stick   | Dpad / Arrow Keys |
| A       | Space     | A       |                   |
| B       | ShiftLeft | B       |                   |
| Start   | Enter     | Start   |                   |
| Select  | Escape    | Select  |                   |

## Emulator Controls

| Action                | Shortcut | Alternative           |
| --------------------- | -------- | --------------------- |
| Speedup 5 time        | F        |                       |
| Open Rom              | Ctrl+O   | Drop file into window |
| Screenshot            | F2       |                       |
| Dump Memory           | F5       |                       |
| Dump Background Image | F6       |                       |
| Dump Tilemap Image    | F7       |                       |

## Screenshots

<p align="center">
<img src="Images/cpu_instrs.png">
<br/>
blargg's cpu_instrs passing
</p>
<br/>

<p align="center">
<img src="Images/Tetris.png">
<br/>
Tetris
</p>
<br/>

<p align="center">
<img src="Images/Super Marioland.png">
<br/>
Super Marioland
</p>
<br/>

### Linux notes
veldrid cant find libdl.so
Fix is to make a sym link
```
sudo ln -s /usr/lib/libdl.so.2 /usr/lib/libdl.so
```
