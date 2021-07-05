import sys

# List of computations with a = 0
COM_A_ZERO = set([
    '0',
    '1',
    '-1',
    'D',
    'A',
    '!D',
    '!A',
    '-D',
    '-A',
    'D+1',
    'A+1',
    'D-1',
    'A-1',
    'D+A',
    'D-A',
    'A-D',
    'D&A',
    'D|A'])

COMP_MAP = {
    '0': '101010',
    '1': '111111',
    '-1': '111010',
    'D': '001100',
    'A': '110000',
    '!D': '001101',
    '!A': '110001',
    '-D': '001111',
    '-A': '110011',
    'D+1': '011111',
    'A+1': '110111',
    'D-1': '001110',
    'A-1': '110010',
    'D+A': '000010',
    'D-A': '010011',
    'A-D': '000111',
    'D&A': '000000',
    'D|A': '010101',
    'M': '110000',
    '!M': '110001',
    '-M': '110011',
    'M+1': '110111',
    'M-1': '110010',
    'D+M': '000010',
    'D-M': '010011',
    'M-D': '000111',
    'D&M': '000000',
    'D|M': '010101'
}

DEST_MAP = {
    'null' : '000',
    'M' : '001',
    'D' : '010',
    'MD' : '011',
    'A' : '100',
    'AM' : '101',
    'AD' : '110',
    'AMD' : '111'
}

JUMP_MAP = {
    'null' : '000',
    'JGT' : '001',
    'JEQ' : '010',
    'JGE' : '011',
    'JLT' : '100',
    'JNE' : '101',
    'JLE' : '110',
    'JMP' : '111'
}

PRE_DEF_SYMBOLS = {
    'R0': 0,
    'R1': 1,
    'R2': 2,
    'R3': 3,
    'R4': 4,
    'R5': 5,
    'R6': 6,
    'R7': 7,
    'R8': 8,
    'R9': 9,
    'R10': 10,
    'R11': 11,
    'R12': 12,
    'R13': 13,
    'R14': 14,
    'R15': 15,
    'SCREEN': 16384,
    'KBD': 24576,
    'SP': 0,
    'LCL': 1,
    'ARG': 2,
    'THIS': 3,
    'THAT': 4
}

def removeEmptyLineAndCommentsFromProgram(lines):
    res = []
    for line in lines:
        if not line.startswith('//') and line != '':
            # Remove inline comment
            fields = line.split(' ')
            res.append(fields[0])
    return res

def buildSymbolTableWithLabelSymbols(lines):
    '''
    Identify all labels in the program and build the symbol table 
    containing those labels
    '''
    symbolTable = {}
    i, counter = 0, 0
    while i < len(lines):
        line = lines[i]
        if line.startswith('('):
            label = line[1:-1]
            symbolTable[label] = counter
        else:
            counter += 1
        i += 1
    return symbolTable

def buildSymbolTableWithVariableSymbols(lines, symbolTable):
    '''
    Identify all variables in the program and add them to the existing
    symbol table -> return a new symbol table
    '''
    
    variables = set()   # contains the name of all variables in the program
    # Each variable is assigned a unique memory address, starting at 16
    memAddress = 16 
    for line in lines:
        if not line.startswith('@'):
            continue
        
        val = line[1:]
        try:
            _ = int(val)
        except ValueError:
            if val not in PRE_DEF_SYMBOLS and val not in symbolTable and val not in variables:
                symbolTable[val] = memAddress
                variables.add(val)
                memAddress += 1
    return symbolTable

def removeSymbolsFromProgram(lines, symbolTable):
    res = []
    for line in lines:
        if line.startswith('('):
            continue

        if line.startswith('@'):
            val = line[1:]
            if val in symbolTable:
                res.append('@' + str(symbolTable[val]))
            elif val in PRE_DEF_SYMBOLS:
                res.append('@' + str(PRE_DEF_SYMBOLS[val]))
            else:
                res.append(line)
        else:
            res.append(line)
    return res

def translateAInstruction(line):
    binary = format(int(line[1:]), "b")
    return '0' * (16 - len(binary)) + binary

def translateCInstruction(line):
    instruction = line.split(' ')[0]
    if ';' in instruction:
        dest = 'null'
        comp, jump = instruction.split(';')
    else:
        jump = 'null'
        dest, comp = instruction.split('=')

    a = '0' if comp in COM_A_ZERO else '1'
    destBin = DEST_MAP[dest]
    compBin = COMP_MAP[comp]
    jumpBin = JUMP_MAP[jump]
    return '111' + a + compBin + destBin + jumpBin

def translate(lines):
    '''
    This function will translate symbol-less HACK program into machine code
    '''
    res = []
    for line in lines:
        if line.startswith('@'):
            instruction = translateAInstruction(line)
        else:
            instruction = translateCInstruction(line)
        res.append(instruction)
    return res

def getIOfilePath(args):
    inputFilePath = args[0]  # Expect input to be HackAssembler.py path\file.asm
    fields = inputFilePath.split('\\')
    inputFileName = fields[-1]
    inputFileNameWithoutExt = inputFileName.split('.')[0]
    fields[-1] = inputFileNameWithoutExt + ".hack"
    outputFilePath = '\\'.join(fields)
    return (inputFilePath, outputFilePath)

if __name__ == '__main__':
    args = sys.argv[1:]
    inputFilePath, outputFilePath = getIOfilePath(args)

    try:
        with open(inputFilePath, "r") as f:
            lines = [line.strip() for line in f]
            lines = removeEmptyLineAndCommentsFromProgram(lines)
            
            symbolTable = buildSymbolTableWithLabelSymbols(lines)
            symbolTable1 = buildSymbolTableWithVariableSymbols(lines, symbolTable)
            lines = removeSymbolsFromProgram(lines, symbolTable1)
            
            res = translate(lines)
            with open(outputFilePath, "w") as f:
                f.write('\n'.join(res))
    except IOError:
        print(f'{inputFilePath} does not exist!')

    