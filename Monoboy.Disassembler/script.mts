import ops from "./ops.ts";
import type { Operator } from "./ops.ts";
// @ts-expect-error
import * as fs from "node:fs";

let lines: string[] = [];

let indentation = 0;
function line(line: string) {
    lines.push(" ".repeat(indentation * 4) + line);
}

function append(text: string) {
    lines[lines.length - 1] += text;
}

function scopeBegin() {
    line("{");
    indentation++;
}

function scopeEnd() {
    indentation--;
    line("}");
}

function generateOperators(
    name: string,
    ops: Record<string, Operator>,
    cb: boolean
) {
    line(`public static Dictionary<byte, Instruction> ${name} = new()`);
    scopeBegin();
    {
        for (const byte of Object.keys(ops)) {
            const opCode = Number(byte).toString(16).padStart(2, "0");
            const op = ops[byte];
            const mnemonic = op.mnemonic;
            const args = op.operands
                .map((o) => {
                    if (cb && !Number.isNaN(Number(o.name))) {
                        return `Bit${o.name}`;
                    } else if (o.name.startsWith("$")) {
                        return `RST${o.name.slice(1)}`;
                    } else {
                        return `${o.name}`;
                    }
                })
                .join(", ");

            line(
                `{ 0x${opCode}, new(${mnemonic}, ${op.bytes}, [${args}], ${byte}) },`
            );
        }
    }
    scopeEnd();
    append(";");
}

line("namespace Monoboy.Disassembler;");
line("");
line("using System.Collections.Generic;");
line("using static Operand;");
line("using static Mnemonic;");
line("");
line("#pragma warning disable CA2211");
line("");
line("public static class Ops");
scopeBegin();
{
    generateOperators("Unprefixed", ops.unprefixed, false);
    generateOperators("CBprefixed", ops.cbprefixed, true);
}
scopeEnd();

console.log(ops.unprefixed[0]);
console.log(ops.unprefixed[1]);
console.log(ops.unprefixed[2]);

fs.writeFileSync("./Ops.cs", lines.join("\n"));
