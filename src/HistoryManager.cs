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

        public string ListHistory() {

            string history = "";

            for (int i = 0; i < historyBuffer.Count; ++i) {
                history += $"{ShellConstants.HIST_LEFT_TAB}{i}{ShellConstants.HIST_MIDDLE_TAB}{historyBuffer[i]}{ShellConstants.SYMB_NEWLINE}";
            }

            return history;
        }
    }
}
