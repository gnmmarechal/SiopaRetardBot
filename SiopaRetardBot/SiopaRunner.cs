using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

// Siopa Retard Bot v2
//
// Interpreter de SiopaScriptv2
namespace SiopaRetardBot
{
    class SiopaRunner
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        // Main parser
        static void Main(String[] args)
        {

            // Get script
            List<String> fileContents = readFile("insurgence.sc");
            foreach (String line in fileContents)
            {
                Console.WriteLine(line);
            }
            Console.ReadLine();

            // Parser
            bool runningLoop = true;

            // Parse and run config part of the file
            String processName = "";
            bool processIsSet = false;
            int resetKey;
            int killToggle = 0x14;
            int maxLoop = int.MaxValue;
            int[] mainLoopIndex = { fileContents.IndexOf("START"), fileContents.IndexOf("LOOP") }; // Gets indexes of main loop
            int[] resetIndex = { fileContents.IndexOf("CONFIGRESET"), fileContents.IndexOf("ENDRESET") }; // Gets indexes of reset loop
            int endIndex = fileContents.IndexOf("END"); // Gets index of END
            int loopNumber = 0;


            // Variable Tables
            Dictionary<String, Color> colourMap = new Dictionary<String, Color>();
            Dictionary<String, POINT> pointMap = new Dictionary<String, POINT>();

            // Read config
            
            /*for (int j = 0; j < mainLoopIndex[0]; j++)
            {
                String command = fileContents.ElementAt(j);
                String[] commandArray = command.Split(' ');
                if (commandArray.Length == 0) // Quit if no command is found
                {
                    runningLoop = false;
                    goto jumpToDie;
                }
                switch (commandArray[0])
                {
                    case "DELAY":
                        sleep(int.Parse(commandArray[1]));
                        break;
                    case "PROCESS": // Process name
                        if (!processIsSet)
                        {
                            commandArray = commandArray.Skip(1).ToArray();
                            for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PROCESS)
                            {
                                if (i == commandArray.Length - 1)
                                {
                                    processName += commandArray[i];
                                }
                                else
                                {
                                    processName += commandArray[i] + " ";
                                }
                            }
                            processIsSet = true;
                            Console.WriteLine("Target Process: " + processName);
                        }
                        else
                        {
                            Console.WriteLine("Error. Script attempted to set process twice.");
                            runningLoop = false;
                            goto jumpToDie;
                        }
                        break;

                    case "PRINT": // Print Text to Console
                        commandArray = commandArray.Skip(1).ToArray();
                        String toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.Write(toPrint);
                        break;
                    case "PRINTLN": // Print Text to Console (Line)
                        commandArray = commandArray.Skip(1).ToArray();
                        toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINTLN)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.WriteLine(toPrint);
                        break;

                    case "KILL": // Define the kill toggle
                        killToggle = Convert.ToInt32(commandArray[1], 16);
                        Console.WriteLine("Kill Toggle: " + killToggle);
                        break;
                    case "RESET": // Define the reset key
                        resetKey = Convert.ToInt32(commandArray[1], 16);
                        Console.WriteLine("Reset Key: " + resetKey);
                        break;
                    case "MAXLOOP": // Define the maximum loop number
                        maxLoop = Convert.ToInt32(commandArray[1], 10);
                        Console.WriteLine("Max Loop: " + maxLoop);
                        break;
                    case "WAITKEY": // Waits for key input
                        if (commandArray[1].Equals("any")) { Console.ReadLine(); break; }
                        bool correctKey = false;
                        while (!correctKey)
                        {
                            // To Do (Specific WAITKEY)
                        }
                        break;
                    case "POINT": // POINT variable-related info
                        String pointName = commandArray[1]; // Variable Name
                        // Other argument
                        if (commandArray[2].Equals("setPoint"))
                        {
                            sleep(int.Parse(commandArray[3])); // Pausa em ms
                            POINT tempPoint;
                            GetCursorPos(out tempPoint); // Obtém as coordenadas XY
                            pointMap[pointName] = tempPoint; // Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("X") && commandArray[3].StartsWith("Y"))
                        {
                            // Definir o ponto pelas coordenadas
                            POINT tempPoint;
                            tempPoint.X = int.Parse(commandArray[2].TrimStart('X'));
                            tempPoint.Y = int.Parse(commandArray[3].TrimStart('Y'));

                            pointMap[pointName] = tempPoint;
                        }
                        break;
                    case "COLOR": // COLOR variable-related info
                        String colorName = commandArray[1]; // Variable Name
                        // Other argument
                        if (commandArray[2].Equals("getColorAt"))
                        {
                            POINT cPoint;
                            cPoint.X = 0;
                            cPoint.Y = 0;
                            if (pointMap.ContainsKey(commandArray[3]))
                            {
                                cPoint = pointMap[commandArray[3]]; // Gets the point from the variable list
                            }
                            Color tempColor = GetColorAt(cPoint.X, cPoint.Y);
                            //Console.WriteLine(tempColor);
                            
                            colourMap[colorName] = tempColor; // Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("R") && commandArray[3].StartsWith("G") && commandArray[4].StartsWith("B"))
                        {
                            // Definir o ponto pelas coordenadas
                            Color tempColor = Color.FromArgb(255, int.Parse(commandArray[2].TrimStart('R')), int.Parse(commandArray[2].TrimStart('G')), int.Parse(commandArray[2].TrimStart('B')));

                            colourMap[colorName] = tempColor;
                        }
                        break;
                    case "PRINTLOOP": // Prints the current loop
                        Console.Write(loopNumber);
                        break;
                    default:
                        break;
                }
            }*/

            if (!processIsSet)
            {
                runningLoop = false;
                return;
            }

            
            jumpToDie: // MAIN LOOP
            // Die if process isn't running
            runningLoop = processIsRunning(processName);
            if (!runningLoop) goto End;

            // Kill if KILL key is toggled
            InputSimulator a = new InputSimulator();
            if (a.InputDeviceState.IsTogglingKeyInEffect(VirtualKeyCode.CAPITAL))
            {
                Console.WriteLine("Terminated!");
                Console.ReadLine();
                runningLoop = false;
                goto End;
            }
            // Parse loop stuff
            Process p2 = Process.GetProcessesByName(processName).FirstOrDefault();
            for (int j = mainLoopIndex[0]; j < mainLoopIndex[1]; j++)
            {
                String command = fileContents.ElementAt(j);
                String[] commandArray = command.Split(' ');
                if (commandArray.Length == 0) // Quit if no command is found
                {
                    runningLoop = false;
                    goto End;
                }
                switch (commandArray[0])
                {
                    case "DELAY":
                        sleep(int.Parse(commandArray[1]));
                        break;
                    case "PRINT": // Print Text to Console
                        commandArray = commandArray.Skip(1).ToArray();
                        String toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.Write(toPrint);
                        break;
                    case "PRINTLN": // Print Text to Console (Line)
                        commandArray = commandArray.Skip(1).ToArray();
                        toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINTLN)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.WriteLine(toPrint);
                        break;
                    case "WAITKEY": // Waits for key input
                        if (commandArray[1].Equals("any")) { Console.ReadLine(); break; }
                        bool correctKey = false;
                        while (!correctKey)
                        {
                            // To Do (Specific WAITKEY)
                        }
                        break;
                    case "POINT": // POINT variable-related info
                        String pointName = commandArray[1]; // Variable Name
                                                            // Other argument
                        if (commandArray[2].Equals("setPoint"))
                        {
                            sleep(int.Parse(commandArray[3])); // Pausa em ms
                            POINT tempPoint;
                            GetCursorPos(out tempPoint); // Obtém as coordenadas XY
                            pointMap[pointName] = tempPoint; // Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("X") && commandArray[3].StartsWith("Y"))
                        {
                            // Definir o ponto pelas coordenadas
                            POINT tempPoint;
                            tempPoint.X = int.Parse(commandArray[2].TrimStart('X'));
                            tempPoint.Y = int.Parse(commandArray[3].TrimStart('Y'));

                            pointMap[pointName] = tempPoint;
                        }
                        break;
                    case "COLOR": // COLOR variable-related info
                        String colorName = commandArray[1]; // Variable Name
                                                            // Other argument
                        if (commandArray[2].Equals("getColorAt"))
                        {
                            POINT cPoint;
                            cPoint.X = 0;
                            cPoint.Y = 0;
                            if (pointMap.ContainsKey(commandArray[3]))
                            {
                                cPoint = pointMap[commandArray[3]]; // Gets the point from the variable list
                            }
                            Color tempColor = GetColorAt(cPoint.X, cPoint.Y);
                            //Console.WriteLine(tempColor);

                            colourMap[colorName] = tempColor; // Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("R") && commandArray[3].StartsWith("G") && commandArray[4].StartsWith("B"))
                        {
                            // Definir o ponto pelas coordenadas
                            Color tempColor = Color.FromArgb(255, int.Parse(commandArray[2].TrimStart('R')), int.Parse(commandArray[2].TrimStart('G')), int.Parse(commandArray[2].TrimStart('B')));

                            colourMap[colorName] = tempColor;
                        }
                        break;
                    case "KEYSPAM": // Spams a key
                        Console.WriteLine("Keypress (Spam Mode): " + Convert.ToInt32(commandArray[1], 16));
                        SetForegroundWindow(p2.MainWindowHandle);
                        InputSimulator inp = new InputSimulator();
                        for (int i = 0; i < 200; i++)
                        {
                            inp.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                        }
                        break;

                    case "KEYPRESS": // Presses a single key
                        Console.WriteLine("Keypress: " + Convert.ToInt32(commandArray[1], 16));
                        Process p21 = Process.GetProcessesByName(processName).FirstOrDefault();
                        SetForegroundWindow(p21.MainWindowHandle);
                        InputSimulator inp1 = new InputSimulator();
                        inp1.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                        break;
                    case "IF": // Branch conditions

                        // TO-DO --- Remove hardcoded ops no if
                        if (commandArray[1].Equals("COLORMATCH"))
                        {
                            Color c1 = colourMap[commandArray[2]];
                            Color c2 = colourMap[commandArray[3]];
                            if (c1.Equals(c2))
                            {
                                // Run operation
                                switch (commandArray[5])
                                {
                                    case "END":
                                        goto jumpToEnd;
                                        break;
                                    case "DORESET":
                                        goto jumpToReset;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        if (commandArray[1].Equals("COLORMISMATCH"))
                        {
                            Color c1 = colourMap[commandArray[2]];
                            Color c2 = colourMap[commandArray[3]];
                            if (!c1.Equals(c2))
                            {
                                // Run operation
                                switch (commandArray[5])
                                {
                                    case "END":
                                        goto jumpToEnd;
                                        break;
                                    case "DORESET":
                                        goto jumpToReset;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                    case "PRINTLOOP": // Prints the current loop
                        Console.Write(loopNumber);
                        break;
                    default:
                        break;
                }
            }

            // Increment loop counter
            loopNumber++;

            // Kill process if loop max is set
            if (loopNumber == maxLoop)
            {
                runningLoop = false;
                Console.WriteLine("Maximum loop count reached (" + maxLoop + ").");
                goto End;
            }
            goto jumpToDie;


            // Pre-Reset Stuff
            jumpToReset:
            for (int j = resetIndex[0]; j < resetIndex[1]; j++)
            {
                String command = fileContents.ElementAt(j);
                String[] commandArray = command.Split(' ');
                if (commandArray.Length == 0) // Quit if no command is found
                {
                    runningLoop = false;
                    goto End;
                }
                switch (commandArray[0])
                {
                    case "DELAY":
                        sleep(int.Parse(commandArray[1]));
                        break;
                    case "PRINT": // Print Text to Console
                        commandArray = commandArray.Skip(1).ToArray();
                        String toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.Write(toPrint);
                        break;
                    case "PRINTLN": // Print Text to Console (Line)
                        commandArray = commandArray.Skip(1).ToArray();
                        toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINTLN)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.WriteLine(toPrint);
                        break;
                    case "WAITKEY": // Waits for key input
                        if (commandArray[1].Equals("any")) { Console.ReadLine(); break; }
                        bool correctKey = false;
                        while (!correctKey)
                        {
                            // To Do (Specific WAITKEY)
                        }
                        break;
                    case "POINT": // POINT variable-related info
                        String pointName = commandArray[1]; // Variable Name
                        // Other argument
                        if (commandArray[2].Equals("setPoint"))
                        {
                            sleep(int.Parse(commandArray[3])); // Pausa em ms
                            POINT tempPoint;
                            GetCursorPos(out tempPoint); // Obtém as coordenadas XY
                            pointMap[pointName] = tempPoint; // Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("X") && commandArray[3].StartsWith("Y"))
                        {
                            // Definir o ponto pelas coordenadas
                            POINT tempPoint;
                            tempPoint.X = int.Parse(commandArray[2].TrimStart('X'));
                            tempPoint.Y = int.Parse(commandArray[3].TrimStart('Y'));

                            pointMap[pointName] = tempPoint;
                        }
                        break;
                    case "COLOR": // COLOR variable-related info
                        String colorName = commandArray[1]; // Variable Name
                        // Other argument
                        if (commandArray[2].Equals("getColorAt"))
                        {
                            POINT cPoint;
                            cPoint.X = 0;
                            cPoint.Y = 0;
                            if (pointMap.ContainsKey(commandArray[3]))
                            {
                                cPoint = pointMap[commandArray[3]]; // Gets the point from the variable list
                            }
                            Color tempColor = GetColorAt(cPoint.X, cPoint.Y);
                            //Console.WriteLine(tempColor);

                            colourMap[colorName] = tempColor;  // Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("R") && commandArray[3].StartsWith("G") && commandArray[4].StartsWith("B"))
                        {
                            // Definir o ponto pelas coordenadas
                            Color tempColor = Color.FromArgb(255, int.Parse(commandArray[2].TrimStart('R')), int.Parse(commandArray[2].TrimStart('G')), int.Parse(commandArray[2].TrimStart('B')));

                            colourMap[colorName] = tempColor;
                        }
                        break;
                    case "KEYSPAM": // Spams a key
                        Console.WriteLine("Keypress (Spam Mode): " + Convert.ToInt32(commandArray[1], 16));
                        SetForegroundWindow(p2.MainWindowHandle);
                        InputSimulator inp = new InputSimulator();
                        for (int i = 0; i < 200; i++)
                        {
                            inp.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                        }
                        break;

                    case "KEYPRESS": // Presses a single key
                        Console.WriteLine("Keypress: " + Convert.ToInt32(commandArray[1], 16));
                        Process p21 = Process.GetProcessesByName(processName).FirstOrDefault();
                        SetForegroundWindow(p21.MainWindowHandle);
                        InputSimulator inp1 = new InputSimulator();
                        inp1.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                        break;
                    case "IF": // Branch conditions

                        // TO-DO --- Remove hardcoded ops no if
                        if (commandArray[1].Equals("COLORMATCH"))
                        {
                            Color c1 = colourMap[commandArray[2]];
                            Color c2 = colourMap[commandArray[3]];
                            if (c1.Equals(c2))
                            {
                                // Run operation
                                switch (commandArray[5])
                                {
                                    case "END":
                                        goto jumpToEnd;
                                        break;
                                    case "DORESET":
                                        goto jumpToReset;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        if (commandArray[1].Equals("COLORMISMATCH"))
                        {
                            Color c1 = colourMap[commandArray[2]];
                            Color c2 = colourMap[commandArray[3]];
                            if (!c1.Equals(c2))
                            {
                                // Run operation
                                switch (commandArray[5])
                                {
                                    case "END":
                                        goto jumpToEnd;
                                        break;
                                    case "DORESET":
                                        goto jumpToReset;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                    case "PRINTLOOP": // Prints the current loop
                        Console.Write(loopNumber);
                        break;
                    default:
                        break;
                }
            }

            goto jumpToDie;
            // End Stuff
            jumpToEnd:
            for (int j = endIndex; j < fileContents.Count(); j++)
            {
                String command = fileContents.ElementAt(j);
                String[] commandArray = command.Split(' ');
                if (commandArray.Length == 0) // Quit if no command is found
                {
                    runningLoop = false;
                    goto End;
                }
                switch (commandArray[0])
                {
                    case "DELAY":
                        sleep(int.Parse(commandArray[1]));
                        break;
                    case "PRINT": // Print Text to Console
                        commandArray = commandArray.Skip(1).ToArray();
                        String toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.Write(toPrint);
                        break;
                    case "PRINTLN": // Print Text to Console (Line)
                        commandArray = commandArray.Skip(1).ToArray();
                        toPrint = "";
                        for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINTLN)
                        {
                            if (i == commandArray.Length - 1)
                            {
                                toPrint += commandArray[i];
                            }
                            else
                            {
                                toPrint += commandArray[i] + " ";
                            }
                        }
                        // Print do texto
                        Console.WriteLine(toPrint);
                        break;
                    case "WAITKEY": // Waits for key input
                        if (commandArray[1].Equals("any")) { Console.ReadLine(); break; }
                        bool correctKey = false;
                        while (!correctKey)
                        {
                            // To Do (Specific WAITKEY)
                        }
                        break;
                    case "POINT": // POINT variable-related info
                        String pointName = commandArray[1]; // Variable Name
                        // Other argument
                        if (commandArray[2].Equals("setPoint"))
                        {
                            sleep(int.Parse(commandArray[3])); // Pausa em ms
                            POINT tempPoint;
                            GetCursorPos(out tempPoint); // Obtém as coordenadas XY
                            pointMap[pointName] = tempPoint;// Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("X") && commandArray[3].StartsWith("Y"))
                        {
                            // Definir o ponto pelas coordenadas
                            POINT tempPoint;
                            tempPoint.X = int.Parse(commandArray[2].TrimStart('X'));
                            tempPoint.Y = int.Parse(commandArray[3].TrimStart('Y'));

                            pointMap[pointName] = tempPoint;
                        }
                        break;
                    case "COLOR": // COLOR variable-related info
                        String colorName = commandArray[1]; // Variable Name
                        // Other argument
                        if (commandArray[2].Equals("getColorAt"))
                        {
                            POINT cPoint;
                            cPoint.X = 0;
                            cPoint.Y = 0;
                            if (pointMap.ContainsKey(commandArray[3]))
                            {
                                cPoint = pointMap[commandArray[3]]; // Gets the point from the variable list
                            }
                            Color tempColor = GetColorAt(cPoint.X, cPoint.Y);
                            //Console.WriteLine(tempColor);

                            colourMap[colorName] = tempColor;  // Adiciona o ponto à lista
                        }
                        else if (commandArray[2].StartsWith("R") && commandArray[3].StartsWith("G") && commandArray[4].StartsWith("B"))
                        {
                            // Definir o ponto pelas coordenadas
                            Color tempColor = Color.FromArgb(255, int.Parse(commandArray[2].TrimStart('R')), int.Parse(commandArray[2].TrimStart('G')), int.Parse(commandArray[2].TrimStart('B')));

                            colourMap[colorName] = tempColor;
                        }
                        break;
                    case "KEYSPAM": // Spams a key
                        Console.WriteLine("Keypress (Spam Mode): " + Convert.ToInt32(commandArray[1], 16));
                        SetForegroundWindow(p2.MainWindowHandle);
                        InputSimulator inp = new InputSimulator();
                        for (int i = 0; i < 200; i++)
                        {
                            inp.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                        }
                        break;

                    case "KEYPRESS": // Presses a single key
                        Console.WriteLine("Keypress: " + Convert.ToInt32(commandArray[1], 16));
                        Process p21 = Process.GetProcessesByName(processName).FirstOrDefault();
                        SetForegroundWindow(p21.MainWindowHandle);
                        InputSimulator inp1 = new InputSimulator();
                        inp1.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                        break;
                    case "PRINTLOOP": // Prints the current loop
                        Console.Write(loopNumber);
                        break;
                    default:
                        break;
                }
            }

            End:

            // Main Loop

            int readMode = 0; // 0 = CONFIG, 1 = LOOP, 2 = RESET, 3 = END

            while (runningLoop)
            {
                if (readMode == 0)
                {
                    // Read Config
                    foreach (String command in fileContents) //int j = 0; j < mainLoopIndex[0]; j++
                    {
                        //String command = fileContents.ElementAt(j);
                        String[] commandArray = command.Split(' ');
                        if (commandArray.Length == 0) // Quit if no command is found
                        {
                            runningLoop = false;
                            goto jumpToDie;
                        }
                        switch (commandArray[0])
                        {
                            case "START":
                                readMode = 1;
                                break;
                            case "DELAY":
                                sleep(int.Parse(commandArray[1]));
                                break;
                            case "PROCESS": // Process name
                                if (!processIsSet)
                                {
                                    commandArray = commandArray.Skip(1).ToArray();
                                    for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PROCESS)
                                    {
                                        if (i == commandArray.Length - 1)
                                        {
                                            processName += commandArray[i];
                                        }
                                        else
                                        {
                                            processName += commandArray[i] + " ";
                                        }
                                    }
                                    processIsSet = true;
                                    Console.WriteLine("Target Process: " + processName);
                                }
                                else
                                {
                                    Console.WriteLine("Error. Script attempted to set process twice.");
                                    runningLoop = false;
                                    goto jumpToDie;
                                }
                                break;

                            case "PRINT": // Print Text to Console
                                commandArray = commandArray.Skip(1).ToArray();
                                String toPrint = "";
                                for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                                {
                                    if (i == commandArray.Length - 1)
                                    {
                                        toPrint += commandArray[i];
                                    }
                                    else
                                    {
                                        toPrint += commandArray[i] + " ";
                                    }
                                }
                                // Print do texto
                                Console.Write(toPrint);
                                break;
                            case "PRINTLN": // Print Text to Console (Line)
                                commandArray = commandArray.Skip(1).ToArray();
                                toPrint = "";
                                for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINTLN)
                                {
                                    if (i == commandArray.Length - 1)
                                    {
                                        toPrint += commandArray[i];
                                    }
                                    else
                                    {
                                        toPrint += commandArray[i] + " ";
                                    }
                                }
                                // Print do texto
                                Console.WriteLine(toPrint);
                                break;

                            case "KILL": // Define the kill toggle
                                killToggle = Convert.ToInt32(commandArray[1], 16);
                                Console.WriteLine("Kill Toggle: " + killToggle);
                                break;
                            case "RESET": // Define the reset key
                                resetKey = Convert.ToInt32(commandArray[1], 16);
                                Console.WriteLine("Reset Key: " + resetKey);
                                break;
                            case "MAXLOOP": // Define the maximum loop number
                                maxLoop = Convert.ToInt32(commandArray[1], 10);
                                Console.WriteLine("Max Loop: " + maxLoop);
                                break;
                            case "WAITKEY": // Waits for key input
                                if (commandArray[1].Equals("any")) { Console.ReadLine(); break; }
                                bool correctKey = false;
                                while (!correctKey)
                                {
                                    // To Do (Specific WAITKEY)
                                }
                                break;
                            case "POINT": // POINT variable-related info
                                String pointName = commandArray[1]; // Variable Name
                                                                    // Other argument
                                if (commandArray[2].Equals("setPoint"))
                                {
                                    sleep(int.Parse(commandArray[3])); // Pausa em ms
                                    POINT tempPoint;
                                    GetCursorPos(out tempPoint); // Obtém as coordenadas XY
                                    pointMap[pointName] = tempPoint; // Adiciona o ponto à lista
                                }
                                else if (commandArray[2].StartsWith("X") && commandArray[3].StartsWith("Y"))
                                {
                                    // Definir o ponto pelas coordenadas
                                    POINT tempPoint;
                                    tempPoint.X = int.Parse(commandArray[2].TrimStart('X'));
                                    tempPoint.Y = int.Parse(commandArray[3].TrimStart('Y'));

                                    pointMap[pointName] = tempPoint;
                                }
                                break;
                            case "COLOR": // COLOR variable-related info
                                String colorName = commandArray[1]; // Variable Name
                                                                    // Other argument
                                if (commandArray[2].Equals("getColorAt"))
                                {
                                    POINT cPoint;
                                    cPoint.X = 0;
                                    cPoint.Y = 0;
                                    if (pointMap.ContainsKey(commandArray[3]))
                                    {
                                        cPoint = pointMap[commandArray[3]]; // Gets the point from the variable list
                                    }
                                    Color tempColor = GetColorAt(cPoint.X, cPoint.Y);
                                    //Console.WriteLine(tempColor);

                                    colourMap[colorName] = tempColor; // Adiciona o ponto à lista
                                }
                                else if (commandArray[2].StartsWith("R") && commandArray[3].StartsWith("G") && commandArray[4].StartsWith("B"))
                                {
                                    // Definir o ponto pelas coordenadas
                                    Color tempColor = Color.FromArgb(255, int.Parse(commandArray[2].TrimStart('R')), int.Parse(commandArray[2].TrimStart('G')), int.Parse(commandArray[2].TrimStart('B')));

                                    colourMap[colorName] = tempColor;
                                }
                                break;
                            case "PRINTLOOP": // Prints the current loop
                                Console.Write(loopNumber);
                                break;
                            default:
                                break;
                        }
                    }
                    // After reading START, move to readMode 1
                }
                else if (readMode == 1)
                {
                    // Main Loop Mode

                    // Increment Loop Counter
                    loopNumber++;
                }
                else if (readMode == 2)
                {
                    // Reset Mode

                    // Change Back to Mode 1
                    readMode = 1;
                }
                else if (readMode == 3)
                {
                    // End Mode

                    // Die
                    runningLoop = false;
                    break;
                }
            }

            Console.ReadLine();
            return;
            
        }

        // File Reader (Also removes empty lines and comments)
        static List<String> readFile(String fileName)
        {
            List<String> retList = new List<string>();

            // Ler ficheiro para a lista
            String line;
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("#") && !String.IsNullOrWhiteSpace(line))
                    retList.Add(line);
            }
            return retList;
        }

        // Checks if process is running
        static bool processIsRunning(String processName)
        {
            Process p2 = Process.GetProcessesByName(processName).FirstOrDefault(); // Adicionar escolha de processo?

            if (p2 == null)
            {
                Console.WriteLine("Null process error (" + processName + ").");
                return false;
            }
            return true;
        }

        // Sleep
        static void sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        // Get colour at pixel
        static Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }
    }
}
