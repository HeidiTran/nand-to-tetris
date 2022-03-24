// push constant 17
   @17
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 17
   @17
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// eq
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_0
   D;JEQ
   D=0
   @FINALLY_0
   0;JMP
(IF_0)
   D=-1
(FINALLY_0)
   @SP
   A=M-1
   M=D
// push constant 17
   @17
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 16
   @16
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// eq
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_1
   D;JEQ
   D=0
   @FINALLY_1
   0;JMP
(IF_1)
   D=-1
(FINALLY_1)
   @SP
   A=M-1
   M=D
// push constant 16
   @16
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 17
   @17
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// eq
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_2
   D;JEQ
   D=0
   @FINALLY_2
   0;JMP
(IF_2)
   D=-1
(FINALLY_2)
   @SP
   A=M-1
   M=D
// push constant 892
   @892
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 891
   @891
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// lt
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_3
   D;JLT
   D=0
   @FINALLY_3
   0;JMP
(IF_3)
   D=-1
(FINALLY_3)
   @SP
   A=M-1
   M=D
// push constant 891
   @891
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 892
   @892
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// lt
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_4
   D;JLT
   D=0
   @FINALLY_4
   0;JMP
(IF_4)
   D=-1
(FINALLY_4)
   @SP
   A=M-1
   M=D
// push constant 891
   @891
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 891
   @891
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// lt
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_5
   D;JLT
   D=0
   @FINALLY_5
   0;JMP
(IF_5)
   D=-1
(FINALLY_5)
   @SP
   A=M-1
   M=D
// push constant 32767
   @32767
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 32766
   @32766
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// gt
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_6
   D;JGT
   D=0
   @FINALLY_6
   0;JMP
(IF_6)
   D=-1
(FINALLY_6)
   @SP
   A=M-1
   M=D
// push constant 32766
   @32766
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 32767
   @32767
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// gt
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_7
   D;JGT
   D=0
   @FINALLY_7
   0;JMP
(IF_7)
   D=-1
(FINALLY_7)
   @SP
   A=M-1
   M=D
// push constant 32766
   @32766
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 32766
   @32766
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// gt
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   D=M-D
   @IF_8
   D;JGT
   D=0
   @FINALLY_8
   0;JMP
(IF_8)
   D=-1
(FINALLY_8)
   @SP
   A=M-1
   M=D
// push constant 57
   @57
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 31
   @31
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// push constant 53
   @53
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// add
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   M=M+D
// push constant 112
   @112
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// sub
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   M=M-D
// neg
   @SP
   A=M-1
   M=-M
// and
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   M=M&D
// push constant 82
   @82
   D=A
   @SP
   A=M
   M=D
   @SP
   M=M+1
// or
   @SP
   M=M-1
   A=M
   D=M
   A=A-1
   M=M|D
// not
   @SP
   A=M-1
   M=!M
