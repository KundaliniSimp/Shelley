using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCraftersShell
{
    class HistoryManager
    {
        List<string> historyBuffer; 

        public HistoryManager() {
            
            historyBuffer = new();
        }

        public void AddToHistory(string[] arguments) {

            historyBuffer.Add(String.Join(ShellConstants.SYMB_WHITESPACE, arguments));
        }

        public string[] ListHistory() {

            string[] history = new string[historyBuffer.Count];

            for (int i = 0; i < historyBuffer.Count; ++i) {
                history[i] = $"{ShellConstants.HIST_LEFT_TAB}{i}{ShellConstants.HIST_MIDDLE_TAB}{historyBuffer[i]}{(i < historyBuffer.Count - 1 ? ShellConstants.SYMB_NEWLINE : '\0')}";

            }

            return history;
        }
    }
}
