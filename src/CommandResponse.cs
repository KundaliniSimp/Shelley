using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCraftersShell
{
    class CommandResponse {

        public string Message { get; set; }
        public string RedirectDirectory { get; set; }
        public RedirectionType RedirectionType { get; set; }

        public CommandResponse() {
            Message = "";
            RedirectDirectory = "";
            RedirectionType = RedirectionType.NO_REDIRECT;
        }
    } 
}
