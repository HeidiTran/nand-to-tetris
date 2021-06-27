# Architecture
- `Parser`: unpacks each instruction into its underlying fields
- `Code`: translates each field into its corresponding binary value
- `SymbolTable`: manages the symbol table
- `Main`: initializes the I/O files and drives the process

# Basic Logic 
Repeat until end of file:
- Read each line of the Assembly language command
- Break the line into different fields it's composed of
- Lookup the binary code for the field
- Combine these binary codes into a single machine language command
- Output the machine language command

### Symbol Table
- Assembler use it to lookup addresses that map to names