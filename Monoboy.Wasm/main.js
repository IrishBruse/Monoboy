// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
import { dotnet } from './dotnet.js'

globalThis.Dotnet = await dotnet.create();

await dotnet.run()

let body = document.querySelector("body");
let viewport = document.getElementById("viewport");
let ctx = viewport.getContext("2d");

let image;

let emulator;
let paused = true;

viewport.addEventListener("drop", async (e) =>
{
    e.stopPropagation();
    e.preventDefault();
    e.dataTransfer.dropEffect = "copy";

    let array = await e.dataTransfer.files[0].arrayBuffer()
    let data = new Uint8Array(array);
    emulator.OpenFile(data);
    paused = false;
});

const pickerOpts = {
    types: [
        {
            description: 'Gameboy Rom',
            accept: {
                'application/gb': ['.gb', '.gbc']
            }
        },
    ],
    excludeAcceptAllOption: true,
    multiple: false
};

viewport.addEventListener("click", async (e) =>
{
    window.showOpenFilePicker(pickerOpts).then(async (files) =>
    {
        let file = await files[0].getFile();
        console.log(file);
        let array = await file.arrayBuffer()
        let data = new Uint8Array(array);
        emulator.OpenFile(data);
        paused = false;
    }).catch()
});

viewport.addEventListener("dragover", (e) =>
{
    e.stopPropagation();
    e.preventDefault();
    e.dataTransfer.dropEffect = "copy"
});

body.addEventListener("drop", async (e) =>
{
    e.stopPropagation();
    e.preventDefault();
    e.dataTransfer.dropEffect = "none"
});

body.addEventListener("dragover", (e) =>
{
    e.stopPropagation();
    e.preventDefault();
    e.dataTransfer.dropEffect = "none"
});

document.addEventListener('keydown', (event) => input(event, true));
document.addEventListener('keyup', (event) => input(event, false));

function input(event, isDown)
{
    let key = event.key.toLowerCase();
    switch (key)
    {
        case "arrowdown": case "s":
            emulator.SetButtonState(0b00001000, isDown);
            break;
        case "arrowup": case "w":
            emulator.SetButtonState(0b00000100, isDown);
            break;
        case "arrowleft": case "a":
            emulator.SetButtonState(0b00000010, isDown);
            break;
        case "arrowright": case "d":
            emulator.SetButtonState(0b00000001, isDown);
            break;

        case "enter":
            emulator.SetButtonState(0b10000000, isDown);
            break;
        case "backspace":
            emulator.SetButtonState(0b01000000, isDown);
            break;
        case " ":
            emulator.SetButtonState(0b00010000, isDown);
            break;
        case "shift":
            emulator.SetButtonState(0b00100000, isDown);
            break;
    }
}

async function invokeCSMethod()
{
    var interop = await Dotnet.getAssemblyExports("Monoboy.Wasm");

    emulator = interop.Monoboy.Wasm.Program;

    let framebuffer = emulator.GetFramebuffer();
    let pixels = new Uint8ClampedArray(Dotnet.Module.HEAPU8.buffer, framebuffer._pointer, framebuffer._length);
    image = new ImageData(pixels, 160, 144);

    window.requestAnimationFrame(loop);
}

function loop()
{
    if (!paused)
    {
        let start = performance.now();
        emulator.RunFrame();
        let end = performance.now();
        document.title = "Monoboy - " + (Math.round((end - start) * 10000) / 10000) + "ms"
    }

    ctx.putImageData(image, 0, 0);

    window.requestAnimationFrame(loop);
}

addEventListener("resize", resize);

function resize(e)
{
    let hSize = Math.floor(window.innerHeight / 144);
    let wSize = Math.floor(window.innerWidth / 160);
    viewport.style.scale = Math.min(hSize, wSize);
}

invokeCSMethod();
resize();
