// Program: Sum1ToN.asm
// Computes: RAM[1] = 1 + 2 + ... + RAM[0]
// Usage: put a number (n) in RAM[0]
// Result: Show in RAM[1]

// Pseudo code
//
//    n = R0
//	  i = 1
//    sum = 0
// LOOP:
//    if i > n goto STOP
//	  sum = sum + i
//	  i = i + 1
//    goto LOOP
// STOP:
//    R1 = sum

   @R0
   D = M	// D = R0
   @n
   M = D	// n = R0
   @i
   M = 1	// i = 1
   @sum
   M = 0	// sum = 0

(LOOP)   
   @i
   D = M	// D = i
   @n
   D = D - M // D = i - n
   
   @STOP
   D;JGT	// if i > n goto STOP

   @i
   D = M	// D = i 
   @sum
   M = M + D
   @i
   M = M + 1	// i++
   @LOOP 
   0;JMP   
   
(STOP)
   @sum
   D = M
   @R1
   M = D   // R1 = sum
   
(END)   
   @END
   0;JMP
   
   
