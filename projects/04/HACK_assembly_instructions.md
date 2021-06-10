**A-instruction:**

*@value* // A = value

where *value* is either a constant or a symbol referring to such a constant

**C-instruction:**

*dest = comp ; jump* (both *dest* and *jump* are optional)

*comp* = 0, 1, -1, D, A, !D, !A, -D, -A, D + 1, A + 1, D - 1, A - 1, D + A, D - A, A - D, D&A, D|A, M, !M, -M, M + 1, M - 1, D + M, D - M, M - D, D&M, D|M

*dest* = null, M, D, MD, A, AM, AD, AMD

*jump* = null, JGT, JEQ, JGE, JLT, JNE,JLE, JMP

**Semantics:**
- Computes the value of *comp*
- Stores the result in *dest*
- If the Boolean expression (*comp jump* 0) is true, jumps to execute the instruction stored in ROM[A]

**Language Convention**
- SCREEN: base address of the screen memory map (16,384)
- KBD: address of the keyboard memory map (24,576)