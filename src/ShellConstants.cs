namespace CodeCraftersShell
{
    static class ShellConstants
    {
        public const char SYMB_PROMPT = '$';
        public const string CMD_ECHO = "echo";
        public const string CMD_EXIT = "exit";
        public const string CMD_TYPE = "type";
        public const string CMD_PWD = "pwd";
        public const string RESP_INVALID_CMD = "command not found";
        public const string RESP_VALID_TYPE = "is a shell builtin";
        public const string RESP_INVALID_TYPE = "not found";
        public const string RESP_VALID_PATH = "is";
        public const string ENV_VAR_PATH = "PATH";
        public const string ENV_VAR_PATH_SEPARATOR = ":";
        public const string ENV_DIR_SEPARATOR = "/";
        public static readonly HashSet<string> BUILTINS = new([CMD_ECHO, CMD_EXIT, CMD_TYPE, CMD_PWD]);
    }
}
