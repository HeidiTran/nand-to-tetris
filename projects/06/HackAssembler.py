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

def getIOfilePath(args):
    inputFilePath = args[0]  # Expect input to be HackAssember.py path\file.asm
    fields = inputFilePath.split('\\')
    inputFileName = fields[-1]
    inputFileNameWithoutExt = inputFileName.split('.')[0]
    fields[-1] = inputFileNameWithoutExt + ".hack"
    outputFilePath = '\\'.join(fields)
    return (inputFilePath, outputFilePath)

if __name__ == '__main__':
    args = sys.argv[1:]
    inputFilePath, outputFilePath = getIOfilePath(args)

    res = []
    try:
        with open(inputFilePath, "r") as f:
            lines = [line.rstrip() for line in f]
            for line in lines:
                if line == '' or line.startswith('//'):
                    continue
                
                if line.startswith('@'):
                    instruction = translateAInstruction(line)
                else:
                    instruction = translateCInstruction(line)
                res.append(instruction)
    except IOError:
        print(f'{inputFilePath} does not exist!')
    
    with open(outputFilePath, "w") as f:
        f.write('\n'.join(res))

    