using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            String fileName = "default.sc";
            if (args.Length > 0)
            {
                fileName = args[0];
            }
            // Get script
            List<String> fileContents = readFile(fileName);
            foreach (String line in fileContents)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("Press ENTER to run script.");
            Console.ReadLine();

            Dictionary<int, String> specialKeys = new Dictionary<int, String>();
            int[] keyCodeList = { 0x08, 0x03, 0x14, 0x2E, 0x28, 0x23, 0x2F, 0x24, 0x2D, 0x25, 0x90, 0x22, 0x21, 0x2C, 0x27, 0x91, 0x09, 0x26, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F, 0x6B, 0x6D, 0x6A, 0x6F };
            String[] keyStringList = { "{BS}", "{BREAK}", "{CAPSLOCK}", "{DEL}", "{DOWN}", "{END}", "{HELP}", "{HOME}", "{INS}", "{LEFT}", "{NUMLOCK}", "{PGDN}", "{PGUP}", "{PRTSC}", "{RIGHT}", "{SCROLLLOCK}", "{TAB}", "{UP}", "{F1}", "{F2}", "{F3}", "{F4}", "{F5}", "{F6}", "{F7}", "{F8}", "{F9}", "{F10}", "{F11}", "{F12}", "{F13}", "{F14}", "{F15}", "{F16}", "{ADD}", "{SUBTRACT}", "{MULTIPLY}", "{DIVIDE}" };
            for (int i = 0; i < keyCodeList.Length; i++)
            {
                specialKeys.Add(keyCodeList[i], keyStringList[i]);
                //Console.WriteLine(keyCodeList[i] + " = " + keyStringList[i]);
            }
            //specialKeys.Add(0x07B, "{F12}");

            // Parser
            bool runningLoop = true;

            // Parse and run config part of the file
            String processName = "";
            int resetKey = 0x00;
            int killToggle = 0x14;
            int pauseKey = 0x90;
            int maxLoop = int.MaxValue;
            int[] mainLoopIndex = { fileContents.IndexOf("START"), fileContents.IndexOf("LOOP") }; // Gets indexes of main loop
            int[] resetIndex = { fileContents.IndexOf("CONFIGRESET"), fileContents.IndexOf("ENDRESET") }; // Gets indexes of reset loop
            int endIndex = fileContents.IndexOf("END"); // Gets index of END
            int loopNumber = 0;
            int resetCounter = 0;


            // Variable Tables
            Dictionary<String, Color> colourMap = new Dictionary<String, Color>();
            Dictionary<String, POINT> pointMap = new Dictionary<String, POINT>();
            Process p2 = null;

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
                            break;
                        }
                        switch (commandArray[0])
                        {
                            case "START":
                                readMode = 1;
                                break;
                            case "DELAY":
                                Console.WriteLine("Delay: " + commandArray[1]);
                                sleep(int.Parse(commandArray[1]));
                                break;
                            case "PROCESS": // Process name
                                if (true)
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
                                    
                                    Console.WriteLine("Target Process: " + processName);
                                }
                                break;
                            case "TITLE":
                                Console.WriteLine("Title: " + commandArray[1]);
                                commandArray = commandArray.Skip(1).ToArray();
                                String toTitle = "";
                                for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                                {
                                    if (i == commandArray.Length - 1)
                                    {
                                        toTitle += commandArray[i];
                                    }
                                    else
                                    {
                                        toTitle += commandArray[i] + " ";
                                    }
                                }
                                // Console Title
                                Console.Title = toTitle;
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
                            case "PAUSE": // Define pause toggle
                                pauseKey = Convert.ToInt32(commandArray[1], 16);
                                Console.WriteLine("Pause Toggle: " + resetKey);
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
                    p2 = Process.GetProcessesByName(processName).FirstOrDefault();
                    if (p2 == null)
                    {
                        runningLoop = false;
                        Console.WriteLine("Null process error (" + processName + ").");
                        break;
                    }
                    InputSimulator a = new InputSimulator();
                    if (a.InputDeviceState.IsTogglingKeyInEffect((VirtualKeyCode)killToggle))
                    {
                        Console.WriteLine("Terminated!");
                        Console.ReadLine();
                        runningLoop = false;
                        break;
                    }
                    while (a.InputDeviceState.IsTogglingKeyInEffect((VirtualKeyCode)pauseKey))
                    {
                        Console.WriteLine("Paused!");
                        Console.ReadLine();
                    }
                    if (loopNumber >= maxLoop)
                    {
                        runningLoop = false;
                        Console.WriteLine("Maximum loop count reached (" + maxLoop + ").");
                        break;
                    }

                    for (int j = mainLoopIndex[0]; j < fileContents.Count(); j++)
                    {
                        String command = fileContents.ElementAt(j);
                        String[] commandArray = command.Split(' ');
                        if (commandArray.Length == 0) // Quit if no command is found
                        {
                            runningLoop = false;
                            break;
                        }

                        if (commandArray[0].Equals("LOOP"))
                        {
                            //Console.WriteLine("Looping...");
                            j = mainLoopIndex[0];

                            break;
                        }
                        if (commandArray[0].Equals("CONFIGRESET"))
                        {
                            readMode = 2;
                            break;
                        }
                        switch (commandArray[0])
                        {
                            case "TITLE":
                                Console.WriteLine("Title: " + commandArray[1]);
                                commandArray = commandArray.Skip(1).ToArray();
                                String toTitle = "";
                                for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                                {
                                    if (i == commandArray.Length - 1)
                                    {
                                        toTitle += commandArray[i];
                                    }
                                    else
                                    {
                                        toTitle += commandArray[i] + " ";
                                    }
                                }
                                // Console Title
                                Console.Title = toTitle;
                                break;
                            case "DELAY":
                                Console.WriteLine("Delay: " + commandArray[1]);
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
                                IntPtr h = p2.MainWindowHandle;
                                SetForegroundWindow(h);
                                InputSimulator inp = new InputSimulator();
                                String tempString = "";
                                int keyCode = Convert.ToInt32(commandArray[1], 16);
                                if (specialKeys.ContainsKey(keyCode))
                                {
                                    tempString = specialKeys[keyCode];
                                }
                                else
                                {
                                    tempString = "" + (char)keyCode;
                                }
                                for (int i = 0; i < 200; i++)
                                {
                                    //inp.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                                    SendKeys.SendWait(tempString);
                                }
                                break;

                            case "KEYPRESS": // Presses a single key
                                Console.WriteLine("Keypress: " + Convert.ToInt32(commandArray[1], 16));
                                IntPtr h2 = p2.MainWindowHandle;
                                SetForegroundWindow(h2);
                                InputSimulator inp1 = new InputSimulator();
                                String tempString1 = "";
                                int keyCode1 = Convert.ToInt32(commandArray[1], 16);
                                if (specialKeys.ContainsKey(keyCode1))
                                {
                                    tempString1 = specialKeys[keyCode1];
                                }
                                else
                                {
                                    tempString1 = "" + (char)keyCode1;
                                }
                                SendKeys.SendWait(tempString1);
                                //inp1.Keyboard.KeyPress((VirtualKeyCode)Convert.ToInt32(commandArray[1], 16));
                                break;
                            case "IF": // Branch conditions

                                // TO-DO --- Remove hardcoded shit
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
                                                readMode = 3;
                                                break;
                                            case "DORESET":
                                                readMode = 2;
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
                                                readMode = 3;
                                                break;
                                            case "DORESET":
                                                readMode = 2;
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
                            case "PRINTRESET": // Prints the current reset count
                                Console.Write(resetCounter);
                                break;
                            default:
                                break;
                        }
                    }

                    // Increment Loop Counter
                    loopNumber++;

                    //

                }
                else if (readMode == 2)
                {
                    // Reset Mode

                    for (int j = resetIndex[0]; j < fileContents.Count(); j++)
                    {
                        String command = fileContents.ElementAt(j);
                        String[] commandArray = command.Split(' ');
                        if (commandArray.Length == 0) // Quit if no command is found
                        {
                            runningLoop = false;
                            break;
                        }
                        if (commandArray[0].Equals("ENDRESET"))
                            break;

                        switch (commandArray[0])
                        {
                            case "DELAY":
                                Console.WriteLine("Delay: " + commandArray[1]);
                                sleep(int.Parse(commandArray[1]));
                                break;
                            case "TITLE":
                                Console.WriteLine("Title: " + commandArray[1]);
                                commandArray = commandArray.Skip(1).ToArray();
                                String toTitle = "";
                                for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                                {
                                    if (i == commandArray.Length - 1)
                                    {
                                        toTitle += commandArray[i];
                                    }
                                    else
                                    {
                                        toTitle += commandArray[i] + " ";
                                    }
                                }
                                // Console Title
                                Console.Title = toTitle;
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
                            case "PRINTLOOP": // Prints the current loop
                                Console.Write(loopNumber);
                                break;
                            case "PRINTRESET": // Prints the current reset count
                                Console.Write(resetCounter);
                                break;
                            default:
                                break;
                        }
                    }

                    // Change Back to Mode 1 after hitting the reset key
                    IntPtr h = p2.MainWindowHandle;
                    SetForegroundWindow(h);
                    Console.WriteLine("RESET Keypress: " + resetKey);
                    String resetString = "" + (char)resetKey;
                    if (specialKeys.ContainsKey(resetKey))
                    {
                        resetString = specialKeys[resetKey];
                        //Console.WriteLine(resetString);
                    }
                    InputSimulator inp2 = new InputSimulator();
                    for (int k = 0; k < 200; k++)
                    {
                        //inp2.Keyboard.KeyPress((VirtualKeyCode)resetKey);
                        SendKeys.SendWait(resetString);
                    }
                    resetCounter++;
                    readMode = 1;
                }
                else if (readMode == 3)
                {
                    // End Mode
                    for (int j = endIndex; j < fileContents.Count(); j++)
                    {
                        String command = fileContents.ElementAt(j);
                        String[] commandArray = command.Split(' ');
                        if (commandArray.Length == 0) // Quit if no command is found
                        {
                            runningLoop = false;
                            break;
                        }
                        switch (commandArray[0])
                        {
                            case "DELAY":
                                Console.WriteLine("Delay: " + commandArray[1]);
                                sleep(int.Parse(commandArray[1]));
                                break;
                            case "TITLE":
                                Console.WriteLine("Title: " + commandArray[1]);
                                commandArray = commandArray.Skip(1).ToArray();
                                String toTitle = "";
                                for (int i = 0; i < commandArray.Length; i++) // Concatenar o array (sem PRINT)
                                {
                                    if (i == commandArray.Length - 1)
                                    {
                                        toTitle += commandArray[i];
                                    }
                                    else
                                    {
                                        toTitle += commandArray[i] + " ";
                                    }
                                }
                                // Console Title
                                Console.Title = toTitle;
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
                            case "PRINTLOOP": // Prints the current loop
                                Console.Write(loopNumber);
                                break;
                            case "PRINTRESET": // Prints the current reset count
                                Console.Write(resetCounter);
                                break;
                            default:
                                break;
                        }
                    }
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
                for (int i = 0; i < line.Length; i++) // Remove indentation from script
                {
                    if (char.IsWhiteSpace(line[i]))
                    {
                        line = line.Substring(1);
                    }
                    else
                    {
                        break;
                    }
                }

                if (line.Contains("#")) // Remove comments
                {
                    line = line.Substring(0, line.IndexOf("#"));
                }

                if (!line.StartsWith("#") && !String.IsNullOrWhiteSpace(line)) // Removes comment lines / Adds
                    retList.Add(line);
            }
            return retList;
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
