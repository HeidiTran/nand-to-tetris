// Program: Pointers.asm
//	for (i = 0; i < n; i++) {
//		arr[i] = -1
//	}

	// Suppose that arr = 100 (pointer to arr's address is at address 100)
	// Suppose n = 10
	
   @100
   D = A	// D = 100
   @arr
   M = D	// arr = D = 100
   
   @10
   D = A	// D = 10
   @n
   M = D	// n = 10
   
   // initialize i
   @i 
   M = 0
   
(LOOP)
   // if (i == n) goto END
   @i 
   D = M	// D = i 
   @n 
   D = D - M // D = i - n 
   @END
   D;JEQ
   
   // RAM[arr + i] = -1
   @arr
   D = M 	// D = arr -> get the address of the array
   @i 
   A = D + M 	// Update the address register A to (arr's address + i's address)
   M = -1		// arr[i] = -1
   
   // i++
   @i 
   M = M + 1
   
   @LOOP
   0;JMP
   
(END)
   @END
   0;JMP 
   
   
   