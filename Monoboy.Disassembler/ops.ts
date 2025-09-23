export default {
    unprefixed: {
        "0": {
            mnemonic: "NOP",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "1": {
            mnemonic: "LD",
            bytes: 3,
            cycles: [12],
            operands: [
                {
                    name: "BC",
                },
                {
                    name: "n16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "2": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "BC",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "3": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "BC",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "4": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "5": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "6": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "7": {
            mnemonic: "RLCA",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "0",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "8": {
            mnemonic: "LD",
            bytes: 3,
            cycles: [20],
            operands: [
                {
                    name: "a16",
                    bytes: 2,
                },
                {
                    name: "SP",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "9": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "BC",
                },
            ],
            flags: {
                Z: "-",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "10": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "BC",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "11": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "BC",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "12": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "13": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "14": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "15": {
            mnemonic: "RRCA",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "0",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "16": {
            mnemonic: "STOP",
            bytes: 2,
            cycles: [4],
            operands: [
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "17": {
            mnemonic: "LD",
            bytes: 3,
            cycles: [12],
            operands: [
                {
                    name: "DE",
                },
                {
                    name: "n16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "18": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "DE",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "19": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "DE",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "20": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "21": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "22": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "23": {
            mnemonic: "RLA",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "0",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "24": {
            mnemonic: "JR",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "e8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "25": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "DE",
                },
            ],
            flags: {
                Z: "-",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "26": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "DE",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "27": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "DE",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "28": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "29": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "30": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "31": {
            mnemonic: "RRA",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "0",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "32": {
            mnemonic: "JR",
            bytes: 2,
            cycles: [12, 8],
            operands: [
                {
                    name: "NZ",
                },
                {
                    name: "e8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "33": {
            mnemonic: "LD",
            bytes: 3,
            cycles: [12],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "n16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "34": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                    increment: true,
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "35": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "36": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "37": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "38": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "39": {
            mnemonic: "DAA",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "Z",
                N: "-",
                H: "0",
                C: "C",
            },
        },
        "40": {
            mnemonic: "JR",
            bytes: 2,
            cycles: [12, 8],
            operands: [
                {
                    name: "Z",
                },
                {
                    name: "e8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "41": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "42": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                    increment: true,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "43": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "44": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "45": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "46": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "47": {
            mnemonic: "CPL",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "1",
                H: "1",
                C: "-",
            },
        },
        "48": {
            mnemonic: "JR",
            bytes: 2,
            cycles: [12, 8],
            operands: [
                {
                    name: "NC",
                },
                {
                    name: "e8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "49": {
            mnemonic: "LD",
            bytes: 3,
            cycles: [12],
            operands: [
                {
                    name: "SP",
                },
                {
                    name: "n16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "50": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                    decrement: true,
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "51": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "SP",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "52": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [12],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "53": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [12],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "54": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "55": {
            mnemonic: "SCF",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "0",
                H: "0",
                C: "1",
            },
        },
        "56": {
            mnemonic: "JR",
            bytes: 2,
            cycles: [12, 8],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "e8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "57": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "SP",
                },
            ],
            flags: {
                Z: "-",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "58": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                    decrement: true,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "59": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "SP",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "60": {
            mnemonic: "INC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "-",
            },
        },
        "61": {
            mnemonic: "DEC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "62": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "63": {
            mnemonic: "CCF",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "64": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "65": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "66": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "67": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "68": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "69": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "70": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "71": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "B",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "72": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "73": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "74": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "75": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "76": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "77": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "78": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "79": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "80": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "81": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "82": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "83": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "84": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "85": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "86": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "87": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "D",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "88": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "89": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "90": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "91": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "92": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "93": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "94": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "95": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "E",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "96": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "97": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "98": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "99": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "100": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "101": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "102": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "103": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "H",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "104": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "105": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "106": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "107": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "108": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "109": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "110": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "111": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "L",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "112": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "113": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "114": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "115": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "116": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "117": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "118": {
            mnemonic: "HALT",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "119": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "120": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "121": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "122": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "123": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "124": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "125": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "126": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "127": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "128": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "129": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "130": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "131": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "132": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "133": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "134": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "135": {
            mnemonic: "ADD",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "136": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "137": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "138": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "139": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "140": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "141": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "142": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "143": {
            mnemonic: "ADC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "144": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "145": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "146": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "147": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "148": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "149": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "150": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "151": {
            mnemonic: "SUB",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "1",
                N: "1",
                H: "0",
                C: "0",
            },
        },
        "152": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "153": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "154": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "155": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "156": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "157": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "158": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "159": {
            mnemonic: "SBC",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "-",
            },
        },
        "160": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "161": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "162": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "163": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "164": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "165": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "166": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "167": {
            mnemonic: "AND",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "168": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "169": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "170": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "171": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "172": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "173": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "174": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "175": {
            mnemonic: "XOR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "1",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "176": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "177": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "178": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "179": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "180": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "181": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "182": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "183": {
            mnemonic: "OR",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "184": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "185": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "186": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "187": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "188": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "189": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "190": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "191": {
            mnemonic: "CP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "1",
                N: "1",
                H: "0",
                C: "0",
            },
        },
        "192": {
            mnemonic: "RET",
            bytes: 1,
            cycles: [20, 8],
            operands: [
                {
                    name: "NZ",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "193": {
            mnemonic: "POP",
            bytes: 1,
            cycles: [12],
            operands: [
                {
                    name: "BC",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "194": {
            mnemonic: "JP",
            bytes: 3,
            cycles: [16, 12],
            operands: [
                {
                    name: "NZ",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "195": {
            mnemonic: "JP",
            bytes: 3,
            cycles: [16],
            operands: [
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "196": {
            mnemonic: "CALL",
            bytes: 3,
            cycles: [24, 12],
            operands: [
                {
                    name: "NZ",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "197": {
            mnemonic: "PUSH",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "BC",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "198": {
            mnemonic: "ADD",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "199": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$00",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "200": {
            mnemonic: "RET",
            bytes: 1,
            cycles: [20, 8],
            operands: [
                {
                    name: "Z",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "201": {
            mnemonic: "RET",
            bytes: 1,
            cycles: [16],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "202": {
            mnemonic: "JP",
            bytes: 3,
            cycles: [16, 12],
            operands: [
                {
                    name: "Z",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "203": {
            mnemonic: "PREFIX",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "204": {
            mnemonic: "CALL",
            bytes: 3,
            cycles: [24, 12],
            operands: [
                {
                    name: "Z",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "205": {
            mnemonic: "CALL",
            bytes: 3,
            cycles: [24],
            operands: [
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "206": {
            mnemonic: "ADC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "207": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$08",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "208": {
            mnemonic: "RET",
            bytes: 1,
            cycles: [20, 8],
            operands: [
                {
                    name: "NC",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "209": {
            mnemonic: "POP",
            bytes: 1,
            cycles: [12],
            operands: [
                {
                    name: "DE",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "210": {
            mnemonic: "JP",
            bytes: 3,
            cycles: [16, 12],
            operands: [
                {
                    name: "NC",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "211": {
            mnemonic: "ILLEGAL_D3",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "212": {
            mnemonic: "CALL",
            bytes: 3,
            cycles: [24, 12],
            operands: [
                {
                    name: "NC",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "213": {
            mnemonic: "PUSH",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "DE",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "214": {
            mnemonic: "SUB",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "215": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$10",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "216": {
            mnemonic: "RET",
            bytes: 1,
            cycles: [20, 8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "217": {
            mnemonic: "RETI",
            bytes: 1,
            cycles: [16],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "218": {
            mnemonic: "JP",
            bytes: 3,
            cycles: [16, 12],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "219": {
            mnemonic: "ILLEGAL_DB",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "220": {
            mnemonic: "CALL",
            bytes: 3,
            cycles: [24, 12],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "221": {
            mnemonic: "ILLEGAL_DD",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "222": {
            mnemonic: "SBC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "223": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$18",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "224": {
            mnemonic: "LDH",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "a8",
                    bytes: 1,
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "225": {
            mnemonic: "POP",
            bytes: 1,
            cycles: [12],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "226": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "227": {
            mnemonic: "ILLEGAL_E3",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "228": {
            mnemonic: "ILLEGAL_E4",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "229": {
            mnemonic: "PUSH",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "230": {
            mnemonic: "AND",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "0",
            },
        },
        "231": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$20",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "232": {
            mnemonic: "ADD",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "SP",
                },
                {
                    name: "e8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "0",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "233": {
            mnemonic: "JP",
            bytes: 1,
            cycles: [4],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "234": {
            mnemonic: "LD",
            bytes: 3,
            cycles: [16],
            operands: [
                {
                    name: "a16",
                    bytes: 2,
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "235": {
            mnemonic: "ILLEGAL_EB",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "236": {
            mnemonic: "ILLEGAL_EC",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "237": {
            mnemonic: "ILLEGAL_ED",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "238": {
            mnemonic: "XOR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "239": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$28",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "240": {
            mnemonic: "LDH",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "a8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "241": {
            mnemonic: "POP",
            bytes: 1,
            cycles: [12],
            operands: [
                {
                    name: "AF",
                },
            ],
            flags: {
                Z: "Z",
                N: "N",
                H: "H",
                C: "C",
            },
        },
        "242": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "243": {
            mnemonic: "DI",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "244": {
            mnemonic: "ILLEGAL_F4",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "245": {
            mnemonic: "PUSH",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "AF",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "246": {
            mnemonic: "OR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "247": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$30",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "248": {
            mnemonic: "LD",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "HL",
                },
                {
                    name: "SP",
                    increment: true,
                },
                {
                    name: "e8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "0",
                N: "0",
                H: "H",
                C: "C",
            },
        },
        "249": {
            mnemonic: "LD",
            bytes: 1,
            cycles: [8],
            operands: [
                {
                    name: "SP",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "250": {
            mnemonic: "LD",
            bytes: 3,
            cycles: [16],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "a16",
                    bytes: 2,
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "251": {
            mnemonic: "EI",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "252": {
            mnemonic: "ILLEGAL_FC",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "253": {
            mnemonic: "ILLEGAL_FD",
            bytes: 1,
            cycles: [4],
            operands: [],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "254": {
            mnemonic: "CP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
                {
                    name: "n8",
                    bytes: 1,
                },
            ],
            flags: {
                Z: "Z",
                N: "1",
                H: "H",
                C: "C",
            },
        },
        "255": {
            mnemonic: "RST",
            bytes: 1,
            cycles: [16],
            operands: [
                {
                    name: "$38",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
    },
    cbprefixed: {
        "0": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "1": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "2": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "3": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "4": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "5": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "6": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "7": {
            mnemonic: "RLC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "8": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "9": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "10": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "11": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "12": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "13": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "14": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "15": {
            mnemonic: "RRC",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "16": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "17": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "18": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "19": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "20": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "21": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "22": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "23": {
            mnemonic: "RL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "24": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "25": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "26": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "27": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "28": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "29": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "30": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "31": {
            mnemonic: "RR",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "32": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "33": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "34": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "35": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "36": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "37": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "38": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "39": {
            mnemonic: "SLA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "40": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "41": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "42": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "43": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "44": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "45": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "46": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "47": {
            mnemonic: "SRA",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "48": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "49": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "50": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "51": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "52": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "53": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "54": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "55": {
            mnemonic: "SWAP",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "0",
            },
        },
        "56": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "57": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "58": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "59": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "60": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "61": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "62": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "63": {
            mnemonic: "SRL",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "0",
                C: "C",
            },
        },
        "64": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "65": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "66": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "67": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "68": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "69": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "70": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "71": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "72": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "73": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "74": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "75": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "76": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "77": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "78": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "79": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "80": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "81": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "82": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "83": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "84": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "85": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "86": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "87": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "88": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "89": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "90": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "91": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "92": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "93": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "94": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "95": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "96": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "97": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "98": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "99": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "100": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "101": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "102": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "103": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "104": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "105": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "106": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "107": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "108": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "109": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "110": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "111": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "112": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "113": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "114": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "115": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "116": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "117": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "118": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "119": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "120": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "121": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "122": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "123": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "124": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "125": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "126": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [12],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "127": {
            mnemonic: "BIT",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "Z",
                N: "0",
                H: "1",
                C: "-",
            },
        },
        "128": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "129": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "130": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "131": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "132": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "133": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "134": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "135": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "136": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "137": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "138": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "139": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "140": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "141": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "142": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "143": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "144": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "145": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "146": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "147": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "148": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "149": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "150": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "151": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "152": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "153": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "154": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "155": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "156": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "157": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "158": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "159": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "160": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "161": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "162": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "163": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "164": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "165": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "166": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "167": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "168": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "169": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "170": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "171": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "172": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "173": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "174": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "175": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "176": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "177": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "178": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "179": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "180": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "181": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "182": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "183": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "184": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "185": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "186": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "187": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "188": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "189": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "190": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "191": {
            mnemonic: "RES",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "192": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "193": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "194": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "195": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "196": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "197": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "198": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "199": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "0",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "200": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "201": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "202": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "203": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "204": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "205": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "206": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "207": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "1",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "208": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "209": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "210": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "211": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "212": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "213": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "214": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "215": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "2",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "216": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "217": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "218": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "219": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "220": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "221": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "222": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "223": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "3",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "224": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "225": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "226": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "227": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "228": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "229": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "230": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "231": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "4",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "232": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "233": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "234": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "235": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "236": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "237": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "238": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "239": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "5",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "240": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "241": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "242": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "243": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "244": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "245": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "246": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "247": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "6",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "248": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "B",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "249": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "C",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "250": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "D",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "251": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "E",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "252": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "H",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "253": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "L",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "254": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [16],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "HL",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
        "255": {
            mnemonic: "SET",
            bytes: 2,
            cycles: [8],
            operands: [
                {
                    name: "7",
                },
                {
                    name: "A",
                },
            ],
            flags: {
                Z: "-",
                N: "-",
                H: "-",
                C: "-",
            },
        },
    },
} as Operators;

export interface Operators {
    unprefixed: { [key: string]: Operator };
    cbprefixed: { [key: string]: Operator };
}

export interface Flags {
    Z: string;
    N: string;
    H: string;
    C: string;
}

export interface Operator {
    mnemonic: string;
    bytes: number;
    cycles: number[];
    operands: OperatorOperand[];
    flags: Flags;
}

export interface OperatorOperand {
    name: string;
    bytes?: number;
    increment?: boolean;
    decrement?: boolean;
}
