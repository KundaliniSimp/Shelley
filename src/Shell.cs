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
            string? response = Eval(userInput);

            if (response == null) {
                return;
            }

            Print(response);
        }

        string Read() {

            Console.Write($"{ShellConstants.SYMB_PROMPT} ");
            string? userInput = Console.ReadLine();

            if (userInput != null) {
                return userInput;
            }
            return "";
        }

        string? Eval(string userInput) {

            string[] parsedInput = userInput.Split(" ");
            string command = parsedInput[0];
            string[] arguments = GetArguments(parsedInput);

            switch (command) {
                case ShellConstants.CMD_ECHO: return Echo(userInput);
                case ShellConstants.CMD_EXIT: isRunning = false; return null;
                default: return $"{command}: {ShellConstants.RESP_INVALID_CMD}";
            }
        }

        void Print(string response) {
            
            Console.WriteLine(response);
        }

        string[] GetArguments(string[] parsedInput) {

            string[] arguments = new string[parsedInput.Length - 1];

            for (int i = 1; i < parsedInput.Length; ++i) {
                arguments[i - 1] = parsedInput[i];
            }

            return arguments;
        }

        string Echo(string userInput) {

            return userInput.Substring(ShellConstants.CMD_ECHO.Length + 1);
        }
    }
}
