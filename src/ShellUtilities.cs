using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeCraftersShell
{
   static class ShellUtilities {

        public static string[] ParseInput(string userInput) {

            List<string> arguments = new();
            string currentArg = "";
            int end = userInput.Length - 1;

            for (int i = 0; i < userInput.Length; ++i) {

                if (userInput[i] == ShellConstants.SYMB_ESCAPE) {
                    if (i == end || !ShellConstants.ESCAPABLES.Contains(userInput[i + 1])) {  // a non-escaping backslash
                        goto pushChar;
                    }

                    ++i;
                    goto pushChar;                                               // backslash precedes an escapable char
                }

                if (Char.IsWhiteSpace(userInput[i])) {
                    if (currentArg.Length > 0) {                                // argument break
                        arguments.Add(currentArg);
                        currentArg = "";
                    }
                    continue;                                                 // skip whitespace
                }

                if (ShellConstants.SYMB_QUOTES.Contains(userInput[i])) {
                    string literal = "";
                    int jumpPosition = 0;

                    switch (userInput[i]) {
                        case ShellConstants.SYMB_QUOTE_SINGLE:
                            jumpPosition = TryExtractSingleQuote(userInput, i, out literal); break;
                        case ShellConstants.SYMB_QUOTE_DOUBLE:
                            jumpPosition = TryExtractDoubleQuote(userInput, i, out literal); break;
                    }

                    currentArg += literal;

                    if (jumpPosition >= 0) {
                        i = jumpPosition;
                        continue;
                    }
                }

            pushChar:
                currentArg += userInput[i];
            }

            if (currentArg.Length > 0) {         // clear argument buffer
                arguments.Add(currentArg);
            }

            return arguments.ToArray();
        }

        static int TryExtractSingleQuote(string userInput, int startPosition, out string literal) {

            int quoteStart = startPosition;
            int quoteEnd = userInput.IndexOf(ShellConstants.SYMB_QUOTE_SINGLE, quoteStart + 1);
            literal = "";

            if (quoteEnd == -1) {              // no matching quote found
                return -1;
            }

            if (quoteEnd - quoteStart < 2) {  // quotes match but literal is empty
                return quoteEnd;
            }

            literal = userInput.Substring(quoteStart + 1, quoteEnd - (quoteStart + 1));

            return quoteEnd;
        }

        static int TryExtractDoubleQuote(string userInput, int startPosition, out string literal) {

            int quoteStart = startPosition;
            int quoteEnd = quoteStart;
            literal = "";

            while (true) {
                quoteEnd = userInput.IndexOf(ShellConstants.SYMB_QUOTE_DOUBLE, quoteEnd + 1);

                if (quoteEnd == -1) {
                    return quoteEnd;
                }

                if (userInput[quoteEnd - 1] != ShellConstants.SYMB_ESCAPE) {
                    break;
                }

                int i = quoteEnd - 1;
                int escapeCount = 0;

                while (userInput[i] == ShellConstants.SYMB_ESCAPE) {
                    ++escapeCount;
                    --i;
                }

                if (escapeCount % 2 == 0) {
                    break;
                }
            }

            if (quoteEnd - quoteStart < 2) {
                return quoteEnd;
            }

            literal = ParseDoubleQuoteLiteral(userInput.Substring(quoteStart + 1, quoteEnd - (quoteStart + 1)));

            return quoteEnd;
        }

        static string ParseDoubleQuoteLiteral(string literal) {

            string parsedLiteral = "";
            int end = literal.Length - 1;

            for (int i = 0; i < literal.Length; ++i) {
                if (literal[i] == ShellConstants.SYMB_ESCAPE && i < end) {
                    if (ShellConstants.DOUBLE_QUOTE_ESCAPABLES.Contains(literal[i + 1])) {
                        ++i;
                    }
                }
                parsedLiteral += literal[i];
            }

            return parsedLiteral;
        }

        public static bool IsEnvironmentWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
