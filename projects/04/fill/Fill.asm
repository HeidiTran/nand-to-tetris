// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed. 
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Pseudo code
//  while (true) {
//  	if (keyboard == 0) {
//  		set color to draw to white
//  	} else {
//  		set color to draw to black
//  	}
//  	
//  	// Draw the whole screen 
//  	addr = SCREEN
//  	row = 256
//  	while row > 0:
//  		row -=1
//  		col = 32
//  		while col > 0:
//  			RAM[addr] = color	// Draw with color = color 
//  			addr = addr + 1
//  			col -= 1
//  }
//  
//  MAINLOOP:
//  	if key NOT pressed goto WHITE
//  	color = -1
//  	goto DRAW
//  	
//  WHITE:
//  	color = 0
//  
//  DRAW:
//  	addr = SCREEN
//  	row = 256	
//  	DRAWLOOP:
//  		if row == 0 goto MAINLOOP
//  		row -=1
//  		col = 32
//  		
//  		ROWLOOP:
//  			if col == 0 goto DRAWLOOP
//  			RAM[addr] = color 
//  			addr = addr + 1
//  			col -= 1
//  			goto ROWLOOP

(MAINLOOP)
   @KBD
   D = M 
   @WHITE 
   D;JEQ	// if key NOT pressed goto WHITE
   
   @color
   M = -1	// set color to black 
   @DRAW
   0;JMP 

(WHITE)
   @color
   M = 0	// set color to white 

// Draw the whole screen 
(DRAW)   
   @SCREEN
   D = A 
   @addr 
   M = D	// addr = SCREEN
   
   @256
   D = A
   @row 
   M = D 	// row = 256
   
(DRAWLOOP)
   @row 
   D = M 
   @MAINLOOP
   D;JEQ	// if row == 0 goto MAINLOOP
   
   @row 
   M = M - 1	// row -= 1
   @32
   D = A
   @col 
   M = D 		// col = 32
   
(ROWLOOP)
   @col 
   D = M 
   @DRAWLOOP
   D;JEQ 	// if col == 0 goto DRAWLOOP  
   
   @color 
   D = M 
   @addr 
   A = M
   M = D 	// RAM[addr] = color 
   
   @addr
   M = M + 1	// addr = addr + 1
   
   @col 
   M = M - 1	// col -= 1
   
   @ROWLOOP
   0;JMP 