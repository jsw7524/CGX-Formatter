A Recursive Descendant Parser for https://www.codingame.com/training/hard/cgx-formatter
Grammer:
Start -> ELEMENT
ELEMENT -> BLOCK | PRIMITIVE_TYPE | KEY_VALUE
BLOCK -> ( ELEMENT SEQUENCE ) | ()
SEQUENCE -> ; ELEMENT SEQUENCE | EMPTY
PRIMITIVE_TYPE -> NUMBER | BOOLEAN | STRING | NULL
KEY_VALUE -> STRING = BLOCK | STRING = PRIMITIVE_TYPE