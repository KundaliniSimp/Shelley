using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCraftersShell
{
    enum RedirectionType {
        NO_REDIRECT,
        STD_OUTPUT,
        STD_ERROR,
    }

    enum RedirectionPrintMode {
        NULL,
        NEW,
        APPEND,
    }

    static class Redirection {

        public static void GetRedirectionData(ref string[] arguments, CommandResponse response) {

            string redirectFlag = "";
            string redirectDirectory = "";
            int redirectFlagIndex = -1;
            int end = arguments.Length - 1;

            for (int i = 0; i < arguments.Length; ++i) {
                if (arguments[i][arguments[i].Length - 1] == ShellConstants.FLAG_REDIRECT_OUTPUT_DEFAULT[0]) {
                    redirectFlag = arguments[i];
                    redirectFlagIndex = i;

                    if (i < end) {
                        redirectDirectory = arguments[i + 1];
                    }

                    break;
                }
            }

            if (String.IsNullOrEmpty(redirectFlag) || String.IsNullOrEmpty(redirectDirectory)) {      // complete redirection data not found
                goto exit;
            }

            response.RedirectDirectory = redirectDirectory;

            switch (redirectFlag) {
                case ShellConstants.FLAG_REDIRECT_OUTPUT_DEFAULT:
                case ShellConstants.FLAG_REDIRECT_OUTPUT_NEW:
                    response.RedirectionType = RedirectionType.STD_OUTPUT;
                    response.RedirectionPrintMode = RedirectionPrintMode.NEW;
                    break;
                case ShellConstants.FLAG_REDIRECT_OUTPUT_APPEND_DEFAULT:
                case ShellConstants.FLAG_REDIRECT_OUTPUT_APPEND:
                    response.RedirectionType = RedirectionType.STD_OUTPUT;
                    response.RedirectionPrintMode = RedirectionPrintMode.APPEND;
                    break;
                case ShellConstants.FLAG_REDIRECT_ERROR_NEW:
                    response.RedirectionType = RedirectionType.STD_ERROR;
                    response.RedirectionPrintMode = RedirectionPrintMode.NEW;
                    break;
                case ShellConstants.FLAG_REDIRECT_ERROR_APPEND:
                    response.RedirectionType = RedirectionType.STD_ERROR;
                    response.RedirectionPrintMode = RedirectionPrintMode.APPEND;
                    break;
            }

        exit:
            if (redirectFlagIndex > -1) {
                Array.Resize(ref arguments, redirectFlagIndex);             // truncate redirection data from arguments
            }
        }
    }
}