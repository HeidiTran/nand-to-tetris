// Program: Rectangle.asm
// Draws a filled rectangle at the screen's top left corner
// The rectangle's width is 16 pixels, and its height is RAM[0]
// Usage: put a non-negative number (rectangle's height) in RAM[0]

// Pseudo code
// for (i = 0; i < n; i++) {
// 		draw 16 black pixels at the beginning of row i 
// }	

// addr = SCREEN
// n = RAM[0]
// i = 0
//
// LOOP:
//		if i > n goto END
//		RAM[addr] = -1	// 1111111111111111 (16 ones)
//		// advances to the next row
//		addr = addr + 32 (the screen's width is 512 bits = 32 columns of 16 bits)
// 		i = i + 1
//		goto LOOP
//
// END:
//		goto END 

   @R0
   D = M 
   @n 
   M = D 	// n = rectangle's height
   
   @i 
   M = 0	// i = 0
   
   @SCREEN
   D = A
   @addr
   M = D 	// address = 16384 (base address of the Hack screen)
   
(LOOP)
   @i 
   D = M 	// D = i 
   @n 
   D = D - M 
   @END
   D;JGT 	// if i > n goto END
   
   @addr
   A = M 
   M = -1	// RAM[addr] = 1111111111111111
   
   @i
   M = M + 1	// i++
   @32
   D = A
   @addr
   M = D + M 	// addr += 32
   @LOOP 
   0;JMP		// goto LOOP
   
(END)
   @END		// program's end
   0;JMP 	// infinite loop   