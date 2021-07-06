# A-instruction

> *@value* // A = value

where *value* is either a constant or a symbol referring to such a constant

**Binary syntax**: *0valueInBinary*
- eg: @21 will be translated to 0000000000010101

### Type of symbols
- Variable symbols: represent memory locations where the programmer wants to maintain values
- Label symbols: represent destinations of goto instructions
- Pre-defined symbols: represent special memory locations
	- In the Hack programming language, there are 23 pre-defined symbols

# C-instruction
> *dest = comp ; jump* (both *dest* and *jump* are optional)

**Binary syntax**: 1 1 1 a c1 c2 c3 c4 c5 c6 d1 d2 d3 j1 j2 j3

| comp | comp | c1 c2 c3 c4 c5 c6 |
|:---:|:---:|:---:|
| 0 |   | 101010 |
| 1 |   | 111111 |
| -1 |   | 111010 |
| D |   | 001100 |
| A | M | 110000 |
| !D |   | 001111 |
| !A | !M | 110001 |
| -D |   | 001111 |
| -A | -M | 110011 |
| D + 1 |   | 011111 |
| A + 1 | M + 1 | 110111 |
| D - 1 |   | 001110 |
| A - 1 | M - 1 | 110010 |
| D + A | D + M | 000010 |
| D - A | D - M | 010011 |
| A - D | M - D | 000111 |
| D & A | D & M | 000000 |
| D \| A | D \| M | 010101 |
| a = 0 | a = 1 |   |


| dest | d1 d2 d3 | effect: the value is stored in |
|:-:|:-:|---|
| null | 000 | The value is not stored |
| M | 001 | RAM[A] |
| D | 010 | D register |
| MD | 011 | RAM[A] and D register |
| A | 100 | A register |
| AM | 101 | A register and RAM[A] |
| AD | 110 | A register and D register |
| AMD | 111 | A register, RAM[A] and D register |

| jump | j1 j2 j3 | effect: the value is stored in |
|:-:|:-:|---|
| null | 000 | no jump |
| JGT | 001 | if out > 0 jump |
| JEQ | 010 | if out = 0 jump |
| JGE | 011 | if out >= 0 jump |
| JLT | 100 | if out < 0 jump |
| JNE | 101 | if out != 0 jump |
| JLE | 110 | if out <= 0 jump |
| JMP | 111 | Unconditional jump |

# Semantics
- Computes the value of *comp*
- Stores the result in *dest*
- If the Boolean expression (*comp jump* 0) is true, jumps to execute the instruction stored in ROM[A]

# Language Convention
- SCREEN: base address of the screen memory map (16,384)
- KBD: address of the keyboard memory map (24,576)

# Architecture
Check out `./Architecture.svg` to zoom into details
![](./Architecture.svg)