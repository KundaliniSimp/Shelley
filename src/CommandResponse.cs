using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCraftersShell
{
    class CommandResponse {

        public string OutputMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string RedirectDirectory { get; set; }
        public RedirectionType RedirectionType { get; set; }
        public RedirectionPrintMode RedirectionPrintMode { get; set; }

        public CommandResponse() {
            OutputMessage = "";
            ErrorMessage = "";
            RedirectDirectory = "";
            RedirectionType = RedirectionType.NO_REDIRECT;
            RedirectionPrintMode = RedirectionPrintMode.NULL;
        }
    } 
}
