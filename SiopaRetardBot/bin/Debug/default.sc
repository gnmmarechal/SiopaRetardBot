# Pokémon Insurgence Starter Shiny Hunter Script/ SiopaScriptv2 Test Script

# Config
TITLE SiopaRetardBot
PRINTLN SiopaRetardBot v.InsurgenceRetard2.0
PROCESS Game
RESET 0x7B
KILL 0x14
PAUSE 0x90
WAITKEY any
PRINTLN Reading original colour...
POINT ogColorPoint setPoint 50
COLOR ogColor getColorAt ogColorPoint
WAITKEY any
PRINTLN Reading shiny colour...
POINT shinyColorPoint setPoint 50
COLOR shinyColor getColorAt shinyColorPoint
WAITKEY any
PRINTLN Reading X/Y...
POINT targetPoint setPoint 50

# Loop
START

KEYSPAM 0x0D

COLOR currentColor getColorAt targetPoint
IF COLORMATCH shinyColor currentColor THEN END
IF COLORMATCH ogColor currentColor THEN DORESET

PRINT LOOP:
PRINTLOOP

PRINTLN .

LOOP

CONFIGRESET

PRINTLN Colour mismatch! Resetting.

PRINT Reset Number: 
PRINTRESET

PRINTLN .

ENDRESET


END
PRINTLN Found match after loop: 
PRINTLOOP
