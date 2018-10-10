# Pokémon Insurgence Starter Shiny Hunter Script/ SiopaScriptv2 Test Script

# Config
CLEAR # Clear the console window
TITLE Pokémon Insurgence Static Event Shiny Hunter Script # Sets console window title
PRINTLN SiopaRetardBot v.InsurgenceRetard2.0 # Prints line to console
PROCESS Game # Process name for Insurgence
RESET 0x7B # Set Reset key to F12
KILL 0x14 # Set Kill toggle to CAPSLOCK
PAUSE 0x90 # Set Pause toggle to NUMLOCK
PRINTLN Place the cursor over the original Pokémon colour and hit ENTER.
WAITKEY any # Waits for any key input
PRINTLN Reading original colour...
POINT ogColorPoint setPoint 50 # Sets the POINT variable ogColorPoint to the current position of the mouse after a delay of 50ms
COLOR ogColor getColorAt ogColorPoint # Sets the COLOR variable ogColor to the colour at the position of ogColorPoint
PRINTLN Done!
PRINTLN Place the cursor over the shiny Pokémon colour and hit ENTER.
WAITKEY any
PRINTLN Reading shiny colour...
POINT shinyColorPoint setPoint 50
COLOR shinyColor getColorAt shinyColorPoint
PRINTLN Place the cursor over the target position and hit ENTER.
WAITKEY any
PRINTLN Reading X/Y...
POINT targetPoint setPoint 50
PRINTLN Done!

# Loop
START # Starts the loop section

	KEYSPAM 0x0D # Spam ENTER

	COLOR currentColor getColorAt targetPoint
	IF COLORMATCH shinyColor currentColor THEN END # Matches currentColor to shinyColor and jumps to END if they match
	IF COLORMATCH ogColor currentColor THEN DORESET # Matches currentColor to ogColor and jumps to RESET if they match

	PRINT Loop Count: # Prints to console window
	PRINTLOOP # Prints the loop counter

	PRINTLN .

LOOP # Closes the loop section

CONFIGRESET # Starts the reset section

	PRINTLN Colour mismatch! Resetting.

	PRINT Reset Count: 
	PRINTRESET # Prints the reset counter

	PRINTLN .

ENDRESET # Closes the reset section and actually resets back to START


END
PRINTLN Found match after loop: 
PRINTLOOP
