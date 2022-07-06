# Monoboy

<p align="center">
<img src="./Icon.ico">
</p>

Reference: https://gbdev.io/pandocs/

## Gameboy Controls

| Gameboy | Keyboard  | Gamepad    |
| ------- | --------- | ---------- |
| Right   | D         | Stick/Dpad |
| Left    | A         | Stick/Dpad |
| Up      | W         | Stick/Dpad |
| Down    | S         | Stick/Dpad |
| A       | ShiftLeft | A / X      |
| B       | Space     | B / Circle |
| Start   | Escape    | Start      |
| Select  | Enter     | Select     |

## Emulator Controls

| Action     | Shortcut |
| ---------- | -------- |
| Open Rom   | Ctrl+O   |
| Screenshot | F2       |

### Linux notes
veldrid cant find libdl.so
Fix is to make a sym link
```
sudo ln -s /usr/lib/libdl.so.2 /usr/lib/libdl.so
```
