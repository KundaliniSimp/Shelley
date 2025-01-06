namespace CodeCraftersShell
{
    static class ShellConstants
    {
        public const char SYMB_PROMPT = '$';
        public const string CMD_ECHO = "echo";
        public const string CMD_EXIT = "exit";
        public const string CMD_TYPE = "type";
        public const string RESP_INVALID_CMD = "command not found";
        public const string RESP_VALID_TYPE = "is a shell builtin";
        public const string RESP_INVALID_TYPE = "not found";
        public static readonly string[] BUILTINS = { CMD_ECHO, CMD_EXIT, CMD_TYPE };
    }
}
