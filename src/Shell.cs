using System;

namespace CodeCraftersShell
{
    class Shell
    {

        bool isRunning;

        public Shell() {

            isRunning = false;
        }

        public void Run() {

            isRunning = true;

            while (isRunning) {
                REPLoop();
            }
        }

        void REPLoop() {

            string userInput = Read();
            string response = Eval(userInput);
            Print(response);
        }

        string Read() {

            Console.Write($"{ShellConstants.SYMB_PROMPT} ");
            return Console.ReadLine();
        }

        string Eval(string userInput) {

            string[] parsedInput = userInput.Split(" ");
            string command = parsedInput[0];

            switch (command) {
                default: return $"{command}: {ShellConstants.RESP_INVALID_CMD}";
            }
        }

        void Print(string response) {
            Console.WriteLine(response);
        }
    }
}
