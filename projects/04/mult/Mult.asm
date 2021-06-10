// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)
//
// This program only needs to handle arguments that satisfy
// R0 >= 0, R1 >= 0, and R0*R1 < 32768.

// Pseudo code:
//
// a = R0
// b = R1
// res = 0
//  
// LOOP: 
//		if b == 0 goto STOP
//		res += a
//		b -= 1
//		goto LOOP
// STOP:
//  	R2 = res
// END:
//		goto END 

   @R0
   D = M
   @a 
   M = D	// a = R0
   
   @R1
   D = M
   @b 
   M = D	// b = R1
   
   @res 
   M = 0	// res = 0
   
(LOOP)
   @b 
   D = M 
   @STOP 
   D;JEQ	// if b == 0 goto STOP 
   
   @a 
   D = M 
   @res 
   M = M + D	// res += a 
   
   @b 
   M = M - 1	// b -= 1
   
   @LOOP
   0;JMP
   
(STOP)
   @res 
   D = M
   @R2
   M = D   
   
(END)
   @END
   0;JMP 