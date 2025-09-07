using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeebookConnector
{
    internal class SplashScreen
    {
        public static int Show()
        {
            string[] options = { "Kør nu", "Nulstil mail liste", "Nulstil Unilogin", "Nulstil afsender mail" };
            int selectedIndex = 0;
            int timeoutSeconds = 30;

            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Velkommen til \"AulaGPT\" - du har følgende valgmuligheder:");
            Console.WriteLine("(Brug piletasterne til at vælge, Enter for at bekræfte)");
            Console.ResetColor();

            DateTime endTime = DateTime.Now.AddSeconds(timeoutSeconds);

            while (true)
            {
                // Beregn resterende tid
                int remaining = (int)(endTime - DateTime.Now).TotalSeconds;
                if (remaining < 0) remaining = 0;

                // Tegn menu
                Console.SetCursorPosition(0, 3);
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine($"{i + 1}. {options[i]}".PadRight(Console.WindowWidth));
                    Console.ResetColor();
                }

                // Tegn nedtælling
                Console.WriteLine();
                Console.WriteLine($"Valget bekræftes automatisk om {remaining} sekunder...".PadRight(Console.WindowWidth));

                // Hvis tiden er gået -> vælg automatisk
                if (remaining == 0)
                    break;

                // Vent på input i 1 sek. (ellers fortsæt for at opdatere nedtælling)
                for (int ms = 0; ms < 1000; ms += 50)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.UpArrow && selectedIndex > 0)
                            selectedIndex--;
                        else if (key.Key == ConsoleKey.DownArrow && selectedIndex < options.Length - 1)
                            selectedIndex++;
                        else if (key.Key == ConsoleKey.Enter)
                            remaining = 0; // Force afslutning
                        endTime = DateTime.Now.AddSeconds(remaining); // opdatér hvis Enter
                        break;
                    }
                    Thread.Sleep(50);
                }
            }

            Console.Clear();
            //Console.WriteLine($"Du valgte: {options[selectedIndex]}");
            Console.CursorVisible = true;
            return selectedIndex;
        }
    }
}
