using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace SiopaRetardBot
{
    class Program
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
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(out int x, out int y);
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        const String versionString = "RETARD1.1";
        const int RESET_KEY = 0x07B;

        static void Main(string[] args)
        {
            int count = 0; 
            
            Console.WriteLine("Siopa Retard Bot v." + versionString);

            bool shinyFound = false;

            Console.ReadLine();
            Console.WriteLine("Registering colour of original Pokémon in 5...");
            System.Threading.Thread.Sleep(50);
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            Color ogColor = GetColorAt(lpPoint.X, lpPoint.Y);
            Console.WriteLine("Done. " + ogColor);

            Console.ReadLine();
            Console.WriteLine("Registering colour of shiny Pokémon in 5...");
            System.Threading.Thread.Sleep(50);
            GetCursorPos(out lpPoint);
            Color shinyColor = GetColorAt(lpPoint.X, lpPoint.Y);
            Console.WriteLine("Done. " + shinyColor);


            Console.ReadLine();
            Console.WriteLine("Registering target X/Y coordinates in 5...");
            System.Threading.Thread.Sleep(50);
            POINT targetPoint;
            GetCursorPos(out targetPoint);
            Console.WriteLine("Done . [X=" + targetPoint.X + ", Y=" + targetPoint.Y + "]");


            /*Console.WriteLine("Input X offset:");
            Console.Write(">");
            targetPoint.X += int.Parse (Console.ReadLine());

            Console.WriteLine("Input Y offset:");
            Console.Write(">");
            targetPoint.Y += int.Parse(Console.ReadLine());*/

            //Console.WriteLine("Done . [X=" + targetPoint.X + ", Y=" + targetPoint.Y + "]");


            Console.WriteLine("Input config filename:");
            Console.Write(">");
            String fileName = Console.ReadLine();
            List<String> fileContents = new List<string>();
            List<int> inputList = new List<int>();

            Console.WriteLine("Input target process:");
            Console.Write(">");
            String processName = Console.ReadLine();

            try
            {
                // Ler ficheiro para a lista
                String line;
                System.IO.StreamReader file = new System.IO.StreamReader(fileName);
                while ((line = file.ReadLine()) != null)
                {
                    fileContents.Add(line);
                }



                // Parse the input config
                inputList = parseInputConfig(fileContents);
            } catch (Exception e)
            {
                Console.WriteLine("Error.");
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }
            /*
            while (!shinyFound)
            {
                Console.WriteLine("Loop number:" + count);
                GetCursorPos(out lpPoint);

                Color curPixelColor = GetColorAt(lpPoint.X, lpPoint.Y);

                Console.WriteLine(curPixelColor.ToString());


                // Se shiny, parar
                if (curPixelColor.Equals(shinyColor))
                {
                    ConsoleColor curColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Found target!\nLoops:" + count);
                    Console.ForegroundColor = curColor;
                    shinyFound = true;
                    break;
                }
                System.Threading.Thread.Sleep(200);
                count++;
            }
            */
            //String processName = System.Text.Encoding.GetEncoding("utf-32").GetString(BitConverter.GetBytes(inputList.ElementAt(0)));
            Process p = Process.GetProcessesByName(processName).FirstOrDefault(); // Adicionar escolha de processo?
            if (p == null)
            {
                Console.WriteLine("Null process error (" + processName + ").");
                Console.ReadLine();
                return;
            }
            inputList.RemoveAt(0);
            while (!shinyFound)
            {
                Console.WriteLine("Loop number:" + count);

                // Check if process exists
                Process p2 = Process.GetProcessesByName(processName).FirstOrDefault(); // Adicionar escolha de processo?
                if (p2 == null)
                {
                    Console.WriteLine("Null process error (" + processName + ").");
                    Console.ReadLine();
                    return;
                }
                else
                {
                    InputSimulator a = new InputSimulator();
                    if (a.InputDeviceState.IsTogglingKeyInEffect(VirtualKeyCode.CAPITAL))
                    {
                        Console.WriteLine("Terminated bot!");
                        Console.ReadLine();
                    }
                }

                // Run commands

                foreach (int command in inputList)
                {
                    switch(command)
                    {
                        case 0x7B:
                            IntPtr h = p.MainWindowHandle;
                            SetForegroundWindow(h);
                            //keybd_event(0x7b, 0x58, 0, 0);
                            Console.WriteLine("Keypress: " + 0x07B);
                            //keybd_event(0x7b, 0xd8, 2, 0);
                            InputSimulator inp2 = new InputSimulator();
                            inp2.Keyboard.KeyPress((VirtualKeyCode)0x07B);
                            break;
                        case 0:
                            IntPtr h3 = p.MainWindowHandle;
                            SetForegroundWindow(h3);

                            Console.WriteLine("Keypress: " + command);
                            SendKeys.SendWait(" ");
                            break;
                        case -2:
                           //etForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
                            Color curPixelColor = GetColorAt(targetPoint.X, targetPoint.Y);
                            Console.Write("Current Colour:" + curPixelColor + " | Target:" + shinyColor);
                            if (curPixelColor.Equals(shinyColor))
                            {
                                ConsoleColor curColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(" MATCH!");
                                Console.ForegroundColor = curColor;
                                shinyFound = true;
                            }
                            else
                            {
                                ConsoleColor curColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(" MISMATCH!");
                                Console.ForegroundColor = curColor;
                            }

                            break;
                        case -3:
                            //etForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
                            Color curPixelColor2 = GetColorAt(targetPoint.X, targetPoint.Y);
                            Console.Write("Current Colour:" + curPixelColor2 + " | Target:" + shinyColor);
                            if (curPixelColor2.Equals(shinyColor) )
                            {
                                ConsoleColor curColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(" MATCH!");
                                Console.ForegroundColor = curColor;
                                shinyFound = true;
                            }
                            else
                            {
                                ConsoleColor curColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(" MISMATCH!");
                                Console.ForegroundColor = curColor;

                                // Reset spam
                                if (curPixelColor2.Equals(ogColor))
                                {
                                    for (int i = 0; i < 200; i++)
                                    {
                                        IntPtr h4 = p.MainWindowHandle;
                                        SetForegroundWindow(h4);
                                        //keybd_event(0x7b, 0x58, 0, 0);
                                        Console.WriteLine("Keypress (SPAM): " + RESET_KEY);
                                        //keybd_event(0x7b, 0xd8, 2, 0);
                                        InputSimulator inp24 = new InputSimulator();
                                        inp24.Keyboard.KeyPress((VirtualKeyCode)RESET_KEY);
                                    }
                                }

                            }

                            break;
                        default:
                            if (command < -4) // É um delay
                            {

                                Console.WriteLine("Delay: " + -1*(command+4));
                                System.Threading.Thread.Sleep(-1 * (command + 4));
                            }

                            // Keys
                            if (command > 0 && command < 1000)
                            {
                                IntPtr h2 = p.MainWindowHandle;
                                SetForegroundWindow(h2);

                                Console.WriteLine("Keypress: " + command );
                                String commandCharStr = "" + (char) command;
                                SendKeys.SendWait(commandCharStr);
                                
                                //PostMessage(h, 0x0100, command, 0);
                            }
                            else if (command < 2000 && command > 1000)
                            {
                                SetForegroundWindow(p.MainWindowHandle);

                                Console.WriteLine("Keypress: " + (command-1000));
                                InputSimulator inp = new InputSimulator();
                                inp.Keyboard.KeyPress((VirtualKeyCode)command - 1000);
                                
                            }
                            else if (command < 3000 && command > 2000)
                            {
                                SetForegroundWindow(p.MainWindowHandle);

                                Console.WriteLine("Keypress (SPAM EDITION): " + (command - 1000));
                                InputSimulator inp = new InputSimulator();
                                for (int i = 0; i < 200; i++)
                                {
                                    inp.Keyboard.KeyPress((VirtualKeyCode)command - 2000);
                                }
                            }
                            break;
                    }
                    if (shinyFound) break;
                    
                }
                count++;
            }
            Console.ReadLine();

        }


        public static Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }

        public static List<int> parseInputConfig(List<String> fileInput)
        {
            List<int> outputList = new List<int>();
            String[] validKeywords = { "$process"};

            foreach (String input in fileInput)
            {
                Console.WriteLine("Command: " + input);
                int characterVal = -1;
                if (input.StartsWith("#") || input.StartsWith("//"))
                {
                    // Comment
                }
                else
                {
                    String[] words = input.Split(' ');
                    switch (words[0])
                    {
                        case "$process": // Doesn't work
                            characterVal = BitConverter.ToInt32(System.Text.Encoding.GetEncoding("utf-32").GetBytes(words[1]),0);
                            Console.WriteLine(characterVal);
                            break;
                        case "check":
                            characterVal = -2;
                            break;
                        case "checkreset":
                            characterVal = -3;
                            break;
                        case "delay":
                            if (Int32.Parse(words[1]) <= 0) words[1] = "1000";
                            characterVal = -4 - Int32.Parse(words[1]);
                            break;
                        /*case "ENTER":
                            characterVal = 0x0D;
                            break;
                        case "SPACE":
                            characterVal = 0x20;
                            break;
                        case "M":
                            characterVal = 0x4D;
                            break;
                        case "C":
                            characterVal = 0x43;
                            break;
                        case "X":
                            characterVal = 0x58;
                            break;*/
                        case "ENTER":
                            characterVal = (char)'\n';
                            break;
                        case "SPACE":
                            characterVal = 0;
                            break;
                        case "F12":
                            characterVal = 0x7B;
                            break;
                        case "DKEYCODE":
                            characterVal = Int32.Parse(words[1]);
                            break;
                        case "KEYCODE":
                            characterVal = Convert.ToInt32(words[1], 16);
                            break;
                        case "KEYSIM":
                            characterVal = 1000 + Convert.ToInt32(words[1], 16);
                            break;
                        case "KEYSPAM":
                            characterVal = 2000 + Convert.ToInt32(words[1], 16);
                            break;
                        default:
                            // Get character
                            characterVal = (char) words[0][0];
                            break;
                    }
                    if (characterVal != -1)
                        outputList.Add(characterVal);

                }

            }


            return outputList;
        }
    }
}
