/********************************************************************++
Copyright (c) Microsoft Corporation.  All rights reserved.
--********************************************************************/


using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Internal;
using System.Diagnostics;
using Dbg = System.Management.Automation.Diagnostics;
using System.Management.Automation.Host;
using System.Collections.Generic;
using System.Security;

namespace Microsoft.PowerShell
{
    /// <summary>
    /// Null class implementation of PSHostUserInterface used when no console is available and when PowerShell
    /// is run in servmode where no console is needed.
    /// </summary>
    internal class NullHostUserInterface : PSHostUserInterface
    {
        /// <summary>
        /// RawUI
        /// </summary>
        public override PSHostRawUserInterface RawUI
        {
            get { return null; }
        }

        /// <summary>
        /// Prompt
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        /// <param name="descriptions"></param>
        /// <returns></returns>
        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new PSNotImplementedException();
        }

        /// <summary>
        /// PromptForChoice
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        /// <param name="choices"></param>
        /// <param name="defaultChoice"></param>
        /// <returns></returns>
        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new PSNotImplementedException();
        }

        /// <summary>
        /// PromptForCredential
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        /// <param name="userName"></param>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new PSNotImplementedException();
        }

        /// <summary>
        /// PromptForCredential
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        /// <param name="userName"></param>
        /// <param name="targetName"></param>
        /// <param name="allowedCredentialTypes"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new PSNotImplementedException();
        }

        /// <summary>
        /// ReadLine
        /// </summary>
        /// <returns></returns>
        public override string ReadLine()
        {
            throw new PSNotImplementedException();
        }

        /// <summary>
        /// ReadLineAsSecureString
        /// </summary>
        /// <returns></returns>
        public override SecureString ReadLineAsSecureString()
        {
            throw new PSNotImplementedException();
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="value"></param>
        public override void Write(string value)
        { }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="foregroundColor"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="value"></param>
        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        { }

        /// <summary>
        /// WriteDebugLine
        /// </summary>
        /// <param name="message"></param>
        public override void WriteDebugLine(string message)
        { }

        /// <summary>
        /// WriteErrorLine
        /// </summary>
        /// <param name="value"></param>
        public override void WriteErrorLine(string value)
        {
            Console.Out.WriteLine(value);
        }

        /// <summary>
        /// WriteLine
        /// </summary>
        /// <param name="value"></param>
        public override void WriteLine(string value)
        { }

        /// <summary>
        /// WriteProgress
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="record"></param>
        public override void WriteProgress(long sourceId, ProgressRecord record)
        { }

        /// <summary>
        /// WriteVerboseLine
        /// </summary>
        /// <param name="message"></param>
        public override void WriteVerboseLine(string message)
        { }

        /// <summary>
        /// WriteWarningLine
        /// </summary>
        /// <param name="message"></param>
        public override void WriteWarningLine(string message)
        { }
    }

    internal class CommandLineParameterParser
    {
        internal CommandLineParameterParser(PSHostUserInterface hostUI, string bannerText, string helpText)
        {
            if (hostUI == null) { throw new PSArgumentNullException("hostUI"); }
            _hostUI = hostUI;

            _bannerText = bannerText;
            _helpText = helpText;
        }

        internal bool AbortStartup
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _abortStartup;
            }
        }

        internal string InitialCommand
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _commandLineCommand;
            }
        }

        internal bool WasInitialCommandEncoded
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _wasCommandEncoded;
            }
        }

        internal bool ShowBanner
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");
                return _showBanner;
            }
        }

        internal bool NoExit
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _noExit;
            }
        }

        internal bool ImportSystemModules
        {
            get
            {
                return _importSystemModules;
            }
        }

        internal bool SkipProfiles
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _skipUserInit;
            }
        }

        internal uint ExitCode
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _exitCode;
            }
        }

        internal bool ExplicitReadCommandsFromStdin
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _explicitReadCommandsFromStdin;
            }
        }

        internal bool NoPrompt
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _noPrompt;
            }
        }

        internal Collection<CommandParameter> Args
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _collectedArgs;
            }
        }

        internal string ConfigurationName
        {
            get { return _configurationName; }
        }

        internal bool SocketServerMode
        {
            get
            {
                return _socketServerMode;
            }
        }

        internal bool NamedPipeServerMode
        {
            get { return _namedPipeServerMode; }
        }

        internal bool SSHServerMode
        {
            get { return _sshServerMode; }
        }

        internal bool ServerMode
        {
            get
            {
                return _serverMode;
            }
        }

        internal Serialization.DataFormat OutputFormat
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");

                return _outFormat;
            }
        }

        internal Serialization.DataFormat InputFormat
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");
                return _inFormat;
            }
        }

        internal string File
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");
                return _file;
            }
        }

        internal string ExecutionPolicy
        {
            get
            {
                Dbg.Assert(_dirty, "Parse has not been called yet");
                return _executionPolicy;
            }
        }

        internal bool ThrowOnReadAndPrompt
        {
            get
            {
                return _noInteractive;
            }
        }

        internal bool NonInteractive
        {
            get { return _noInteractive; }
        }

        private void ShowHelp()
        {
            _hostUI.WriteLine("");
            if (_helpText == null)
            {
                _hostUI.WriteLine(CommandLineParameterParserStrings.DefaultHelp);
            }
            else
            {
                _hostUI.Write(_helpText);
            }
            _hostUI.WriteLine("");
        }

        private void DisplayBanner()
        {
            // If banner text is not supplied do nothing.
            if (!String.IsNullOrEmpty(_bannerText))
            {
                _hostUI.WriteLine(_bannerText);
                _hostUI.WriteLine();
            }
        }

        internal bool StaMode
        {
            get
            {
                if (_staMode.HasValue)
                {
                    return _staMode.Value;
                }
                else
                {
#if CORECLR
                    // Nano doesn't support STA COM apartment, so on Nano powershell has to use MTA as the default.
                    return false;
#else
                    // Win8: 182409 PowerShell 3.0 should run in STA mode by default
                    return true;
#endif
                }
            }
        }


        /// <summary>
        ///
        /// Processes all the command line parameters to ConsoleHost.  Returns the exit code to be used to terminate the process, or
        /// Success to indicate that the program should continue running.
        ///
        /// </summary>
        /// <param name="args">
        ///
        /// The command line parameters to be processed.
        ///
        /// </param>

        internal void Parse(string[] args)
        {
            Dbg.Assert(!_dirty, "This instance has already been used. Create a new instance.");

            // indicates that we've called this method on this instance, and that when it's done, the state variables
            // will reflect the parse.

            _dirty = true;

            ParseHelper(args);

            // Check registry setting for a Group Policy ConfigurationName entry and
            // use it to override anything set by the user.
            var configurationName = GetConfigurationNameFromGroupPolicy();
            if (!string.IsNullOrEmpty(configurationName))
            {
                _configurationName = configurationName;
            }
        }

        private static string s_groupPolicyBase = @"Software\Policies\Microsoft\Windows\PowerShell";
        private static string s_consoleSessionConfigurationKey = "ConsoleSessionConfiguration";
        private static string s_enableConsoleSessionConfiguration = "EnableConsoleSessionConfiguration";
        private static string s_consoleSessionConfigurationName = "ConsoleSessionConfigurationName";
        private static string GetConfigurationNameFromGroupPolicy()
        {
            // Current user policy takes precedence.
            var groupPolicySettings = Utils.GetGroupPolicySetting(s_groupPolicyBase, s_consoleSessionConfigurationKey, Utils.RegCurrentUserThenLocalMachine);
            if (groupPolicySettings != null)
            {
                object keyValue;
                if (groupPolicySettings.TryGetValue(s_enableConsoleSessionConfiguration, out keyValue))
                {
                    if (String.Equals(keyValue.ToString(), "1", StringComparison.OrdinalIgnoreCase))
                    {
                        if (groupPolicySettings.TryGetValue(s_consoleSessionConfigurationName, out keyValue))
                        {
                            string consoleSessionConfigurationName = keyValue.ToString();
                            if (!string.IsNullOrEmpty(consoleSessionConfigurationName))
                            {
                                return consoleSessionConfigurationName;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        private void ParseHelper(string[] args)
        {
            Dbg.Assert(args != null, "Argument 'args' to ParseHelper should never be null");
            bool noexitSeen = false;

            for (int i = 0; i < args.Length; ++i)
            {
                // Invariant culture used because command-line parameters are not localized.

                string switchKey = args[i].Trim().ToLowerInvariant();
                if (String.IsNullOrEmpty(switchKey))
                {
                    continue;
                }

                if (!SpecialCharacters.IsDash(switchKey[0]) && switchKey[0] != '/')
                {
                    // then its a command

                    --i;
                    ParseCommand(args, ref i, noexitSeen, false);
                    break;
                }

                // chop off the first character so that we're agnostic wrt specifying / or -
                // in front of the switch name.

                switchKey = switchKey.Substring(1);

                // chop off the second dash so we're agnostic wrt specifying - or --
                if (!String.IsNullOrEmpty(switchKey) && SpecialCharacters.IsDash(switchKey[0]))
                {
                    switchKey = switchKey.Substring(1);
                }

                if (MatchSwitch(switchKey, "help", "h") || MatchSwitch(switchKey, "?", "?"))
                {
                    _showHelp = true;
                    _abortStartup = true;
                }
                else if (MatchSwitch(switchKey, "noexit", "noe"))
                {
                    _noExit = true;
                    noexitSeen = true;
                }
                else if (MatchSwitch(switchKey, "importsystemmodules", "imp"))
                {
                    _importSystemModules = true;
                }
                else if (MatchSwitch(switchKey, "noprofile", "nop"))
                {
                    _skipUserInit = true;
                }
                else if (MatchSwitch(switchKey, "nologo", "nol"))
                {
                    _showBanner = false;
                }
                else if (MatchSwitch(switchKey, "noninteractive", "noni"))
                {
                    _noInteractive = true;
                }
                else if (MatchSwitch(switchKey, "socketservermode", "so"))
                {
                    _socketServerMode = true;
                }
                else if (MatchSwitch(switchKey, "servermode", "s"))
                {
                    _serverMode = true;
                }
                else if (MatchSwitch(switchKey, "namedpipeservermode", "nam"))
                {
                    _namedPipeServerMode = true;
                }
                else if (MatchSwitch(switchKey, "sshservermode", "sshs"))
                {
                    _sshServerMode = true;
                }
                else if (MatchSwitch(switchKey, "configurationname", "config"))
                {
                    ++i;
                    if (i >= args.Length)
                    {
                        WriteCommandLineError(
                            CommandLineParameterParserStrings.MissingConfigurationNameArgument);
                        break;
                    }

                    _configurationName = args[i];
                }
                else if (MatchSwitch(switchKey, "command", "c"))
                {
                    if (!ParseCommand(args, ref i, noexitSeen, false))
                    {
                        break;
                    }
                }
#if !CORECLR  // windowstyle parameter not supported on NanoServer because ProcessWindowStyle does Not exist on CoreCLR
                else if (MatchSwitch(switchKey, "windowstyle", "w"))
                {
                    ++i;
                    if (i >= args.Length)
                    {
                        WriteCommandLineError(
                            CommandLineParameterParserStrings.MissingWindowStyleArgument);
                        break;
                    }

                    try
                    {
                        ProcessWindowStyle style = (ProcessWindowStyle)LanguagePrimitives.ConvertTo(
                            args[i], typeof(ProcessWindowStyle), CultureInfo.InvariantCulture);
                        ConsoleControl.SetConsoleMode(style);
                    }
                    catch (PSInvalidCastException e)
                    {
                        WriteCommandLineError(
                            string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.InvalidWindowStyleArgument, args[i], e.Message));
                        break;
                    }
                }
#endif
                else if (MatchSwitch(switchKey, "file", "f"))
                {
                    // Process file execution. We don't need to worry about checking -command
                    // since if -command comes before -file, -file will be treated as part
                    // of the script to evaluate. If -file comes before -command, it will
                    // treat -command as an argument to the script...

                    ++i;
                    if (i >= args.Length)
                    {
                        WriteCommandLineError(
                            CommandLineParameterParserStrings.MissingFileArgument,
                            showHelp: true,
                            showBanner: true);
                        break;
                    }

                    // Don't show the startup banner unless -noexit has been specified.
                    if (!noexitSeen)
                        _showBanner = false;

                    // Process interactive input...
                    if (args[i] == "-")
                    {
                        // the arg to -file is -, which is secret code for "read the commands from stdin with prompts"

                        _explicitReadCommandsFromStdin = true;
                        _noPrompt = false;
                    }
                    else
                    {
                        // Exit on script completion unless -noexit was specified...
                        if (!noexitSeen)
                            _noExit = false;

                        // We need to get the full path to the script because it will be
                        // executed after the profiles are run and they may change the current
                        // directory.
                        string exceptionMessage = null;
                        try
                        {
                            // Normalize slashes
                            _file = args[i].Replace(StringLiterals.AlternatePathSeparator,
                                                   StringLiterals.DefaultPathSeparator);
                            _file = Path.GetFullPath(_file);
                        }
                        catch (Exception e)
                        {
                            // Catch all exceptions - we're just going to exit anyway so there's
                            // no issue of the system being destabilized.
                            exceptionMessage = e.Message;
                        }

                        if (exceptionMessage != null)
                        {
                            WriteCommandLineError(
                                string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.InvalidFileArgument, args[i], exceptionMessage),
                                showBanner: true);
                            break;
                        }

                        if (!Path.GetExtension(_file).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                        {
                            WriteCommandLineError(
                                string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.InvalidFileArgumentExtension, args[i]),
                                showBanner: true);
                            break;
                        }

                        if (!System.IO.File.Exists(_file))
                        {
                            WriteCommandLineError(
                                string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.ArgumentFileDoesNotExist, args[i]),
                                showBanner: true);
                            break;
                        }

                        i++;

                        Regex argPattern = new Regex(@"^.\w+\:", RegexOptions.CultureInvariant);
                        string pendingParameter = null;

                        // Accumulate the arguments to this script...
                        while (i < args.Length)
                        {
                            string arg = args[i];

                            // If there was a pending parameter, add a named parameter
                            // using the pending parameter and current argument
                            if (pendingParameter != null)
                            {
                                _collectedArgs.Add(new CommandParameter(pendingParameter, arg));
                                pendingParameter = null;
                            }
                            else if (!string.IsNullOrEmpty(arg) && SpecialCharacters.IsDash(arg[0]))
                            {
                                Match m = argPattern.Match(arg);
                                if (m.Success)
                                {
                                    int offset = arg.IndexOf(':');
                                    if (offset == arg.Length - 1)
                                    {
                                        pendingParameter = arg.TrimEnd(':');
                                    }
                                    else
                                    {
                                        _collectedArgs.Add(new CommandParameter(arg.Substring(0, offset), arg.Substring(offset + 1)));
                                    }
                                }
                                else
                                {
                                    _collectedArgs.Add(new CommandParameter(arg));
                                }
                            }
                            else
                            {
                                _collectedArgs.Add(new CommandParameter(null, arg));
                            }
                            ++i;
                        }
                    }
                    break;
                }
#if DEBUG
                // this option is useful when debugging ConsoleHost remotely using VS remote debugging, as you can only
                // attach to an already running process with that debugger.
                else if (MatchSwitch(switchKey, "wait", "w"))
                {
                    // This does not need to be localized: its chk only

                    ((ConsoleHostUserInterface)_hostUI).WriteToConsole("Waiting - type enter to continue:", false);
                    _hostUI.ReadLine();
                }

                // this option is useful for testing the initial InitialSessionState experience
                else if (MatchSwitch(switchKey, "iss", "iss"))
                {
                    // Just toss this option, it was processed earlier...
                }

                // this option is useful for testing the initial InitialSessionState experience
                // this is independent of the normal wait switch because configuration processing
                // happens so early in the cycle...
                else if (MatchSwitch(switchKey, "isswait", "isswait"))
                {
                    // Just toss this option, it was processed earlier...
                }

                else if (MatchSwitch(switchKey, "modules", "mod"))
                {
                    if (ConsoleHost.DefaultInitialSessionState == null)
                    {
                        WriteCommandLineError(
                            "The -module option can only be specified with the -iss option.",
                            showHelp: true,
                            showBanner: true);
                        break;
                    }

                    ++i;
                    int moduleCount = 0;
                    // Accumulate the arguments to this script...
                    while (i < args.Length)
                    {
                        string arg = args[i];

                        if (!string.IsNullOrEmpty(arg) && SpecialCharacters.IsDash(arg[0]))
                        {
                            break;
                        }
                        else
                        {
                            ConsoleHost.DefaultInitialSessionState.ImportPSModule(new string[] { arg });
                            moduleCount++;
                        }
                        ++i;
                    }
                    if (moduleCount < 1)
                    {
                        _hostUI.WriteErrorLine("No modules specified for -module option");
                    }
                }
#endif
                else if (MatchSwitch(switchKey, "outputformat", "o") || MatchSwitch(switchKey, "of", "o"))
                {
                    ParseFormat(args, ref i, ref _outFormat, CommandLineParameterParserStrings.MissingOutputFormatParameter);
                }
                else if (MatchSwitch(switchKey, "inputformat", "i") || MatchSwitch(switchKey, "if", "i"))
                {
                    ParseFormat(args, ref i, ref _inFormat, CommandLineParameterParserStrings.MissingInputFormatParameter);
                }
                else if (MatchSwitch(switchKey, "executionpolicy", "ex") || MatchSwitch(switchKey, "ep", "ep"))
                {
                    ParseExecutionPolicy(args, ref i, ref _executionPolicy, CommandLineParameterParserStrings.MissingExecutionPolicyParameter);
                }
                else if (MatchSwitch(switchKey, "encodedcommand", "e") || MatchSwitch(switchKey, "ec", "e"))
                {
                    _wasCommandEncoded = true;
                    if (!ParseCommand(args, ref i, noexitSeen, true))
                    {
                        break;
                    }
                }
                else if (MatchSwitch(switchKey, "encodedarguments", "encodeda") || MatchSwitch(switchKey, "ea", "ea"))
                {
                    if (!CollectArgs(args, ref i))
                    {
                        break;
                    }
                }
#if !CORECLR  // explicit setting of the ApartmentState Not supported on NanoServer
                else if (MatchSwitch(switchKey, "sta", "s"))
                {
                    if (_staMode.HasValue)
                    {
                        // -sta and -mta are mutually exclusive.
                        WriteCommandLineError(
                            CommandLineParameterParserStrings.MtaStaMutuallyExclusive);
                        break;
                    }

                    _staMode = true;
                }
                // Win8: 182409 PowerShell 3.0 should run in STA mode by default..so, consequently adding the switch -mta.
                // Not deleting -sta for backward compatability reasons
                else if (MatchSwitch(switchKey, "mta", "mta"))
                {
                    if (_staMode.HasValue)
                    {
                        // -sta and -mta are mutually exclusive.
                        WriteCommandLineError(
                            CommandLineParameterParserStrings.MtaStaMutuallyExclusive);
                        break;
                    }

                    _staMode = false;
                }
#endif
                else
                {
                    // The first parameter we fail to recognize marks the beginning of the command string.

                    --i;
                    if (!ParseCommand(args, ref i, noexitSeen, false))
                    {
                        break;
                    }
                }
            }

            if (_showHelp)
            {
                ShowHelp();
            }

            if (_showBanner && !_showHelp)
            {
                DisplayBanner();
            }

            Dbg.Assert(
                    ((_exitCode == ConsoleHost.ExitCodeBadCommandLineParameter) && _abortStartup)
                || (_exitCode == ConsoleHost.ExitCodeSuccess),
                "if exit code is failure, then abortstartup should be true");
        }

        private void WriteCommandLineError(string msg, bool showHelp = false, bool showBanner = false)
        {
            _hostUI.WriteErrorLine(msg);
            _showHelp = showHelp;
            _showBanner = showBanner;
            _abortStartup = true;
            _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
        }

        private bool MatchSwitch(string switchKey, string match, string smallestUnambiguousMatch)
        {
            Dbg.Assert(switchKey != null, "need a value");
            Dbg.Assert(!String.IsNullOrEmpty(match), "need a value");
            Dbg.Assert(match.Trim().ToLowerInvariant() == match, "match should be normalized to lowercase w/ no outside whitespace");
            Dbg.Assert(smallestUnambiguousMatch.Trim().ToLowerInvariant() == smallestUnambiguousMatch, "match should be normalized to lowercase w/ no outside whitespace");
            Dbg.Assert(match.Contains(smallestUnambiguousMatch), "sUM should be a substring of match");

            if (match.Trim().ToLowerInvariant().IndexOf(switchKey, StringComparison.Ordinal) == 0)
            {
                if (switchKey.Length >= smallestUnambiguousMatch.Length)
                {
                    return true;
                }
            }

            return false;
        }

        private void ParseFormat(string[] args, ref int i, ref Serialization.DataFormat format, string resourceStr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in Enum.GetNames(typeof(Serialization.DataFormat)))
            {
                sb.Append(s);
                sb.Append(ConsoleHostUserInterface.Crlf);
            }

            ++i;
            if (i >= args.Length)
            {
                _hostUI.WriteErrorLine(
                    StringUtil.Format(
                        resourceStr,
                        sb.ToString()));
                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                return;
            }

            try
            {
                format = (Serialization.DataFormat)Enum.Parse(typeof(Serialization.DataFormat), args[i], true);
            }
            catch (ArgumentException)
            {
                _hostUI.WriteErrorLine(
                    StringUtil.Format(
                        CommandLineParameterParserStrings.BadFormatParameterValue,
                        args[i],
                        sb.ToString()));
                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
            }
        }

        private void ParseExecutionPolicy(string[] args, ref int i, ref string executionPolicy, string resourceStr)
        {
            ++i;
            if (i >= args.Length)
            {
                _hostUI.WriteErrorLine(resourceStr);

                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                return;
            }

            executionPolicy = args[i];
        }

        private bool ParseCommand(string[] args, ref int i, bool noexitSeen, bool isEncoded)
        {
            if (_commandLineCommand != null)
            {
                // we've already set the command, so squawk

                _hostUI.WriteErrorLine(CommandLineParameterParserStrings.CommandAlreadySpecified);
                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                return false;
            }

            ++i;
            if (i >= args.Length)
            {
                _hostUI.WriteErrorLine(CommandLineParameterParserStrings.MissingCommandParameter);
                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                return false;
            }

            if (isEncoded)
            {
                try
                {
                    _commandLineCommand = StringToBase64Converter.Base64ToString(args[i]);
                }
                // decoding failed
                catch
                {
                    _hostUI.WriteErrorLine(CommandLineParameterParserStrings.BadCommandValue);
                    _showHelp = true;
                    _abortStartup = true;
                    _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                    return false;
                }
            }
            else if (args[i] == "-")
            {
                // the arg to -command is -, which is secret code for "read the commands from stdin with no prompts"

                _explicitReadCommandsFromStdin = true;
                _noPrompt = true;

                ++i;
                if (i != args.Length)
                {
                    // there are more parameters to -command than -, which is an error.

                    _hostUI.WriteErrorLine(CommandLineParameterParserStrings.TooManyParametersToCommand);
                    _showHelp = true;
                    _abortStartup = true;
                    _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                    return false;
                }

                if (!Console.IsInputRedirected)
                {
                    _hostUI.WriteErrorLine(CommandLineParameterParserStrings.StdinNotRedirected);
                    _showHelp = true;
                    _abortStartup = true;
                    _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                    return false;
                }
            }
            else
            {
                // Collect the remaining parameters and combine them into a single command to be run.

                StringBuilder cmdLineCmdSB = new StringBuilder();

                while (i < args.Length)
                {
                    cmdLineCmdSB.Append(args[i] + " ");
                    ++i;
                }
                if (cmdLineCmdSB.Length > 0)
                {
                    // remove the last blank
                    cmdLineCmdSB.Remove(cmdLineCmdSB.Length - 1, 1);
                }
                _commandLineCommand = cmdLineCmdSB.ToString();
            }

            if (!noexitSeen && !_explicitReadCommandsFromStdin)
            {
                // don't reset this if they've already specified -noexit
                _noExit = false;
            }

            _showBanner = false;

            return true;
        }

        private bool CollectArgs(string[] args, ref int i)
        {
            if (_collectedArgs.Count != 0)
            {
                _hostUI.WriteErrorLine(CommandLineParameterParserStrings.ArgsAlreadySpecified);
                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                return false;
            }

            ++i;
            if (i >= args.Length)
            {
                _hostUI.WriteErrorLine(CommandLineParameterParserStrings.MissingArgsValue);
                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                return false;
            }

            try
            {
                object[] a = StringToBase64Converter.Base64ToArgsConverter(args[i]);
                if (a != null)
                {
                    foreach (object obj in a)
                    {
                        _collectedArgs.Add(new CommandParameter(null, obj));
                    }
                }
            }
            catch
            {
                // decoding failed

                _hostUI.WriteErrorLine(CommandLineParameterParserStrings.BadArgsValue);
                _showHelp = true;
                _abortStartup = true;
                _exitCode = ConsoleHost.ExitCodeBadCommandLineParameter;
                return false;
            }

            return true;
        }

        private bool _socketServerMode;
        private bool _serverMode;
        private bool _namedPipeServerMode;
        private bool _sshServerMode;
        private string _configurationName;
        private PSHostUserInterface _hostUI;
        private bool _showHelp;
        private bool _showBanner = true;
        private bool _noInteractive;
        private string _bannerText;
        private string _helpText;
        private bool _abortStartup;
        private bool _skipUserInit;
        // Win8: 182409 PowerShell 3.0 should run in STA mode by default
        // -sta and -mta are mutually exclusive..so tracking them using nullable boolean
        // if true, then sta is specified on the command line.
        // if false, then mta is specified on the command line.
        // if null, then none is specified on the command line..use default in this case
        // default is sta.
        private bool? _staMode = null;
        private bool _noExit = true;
        private bool _explicitReadCommandsFromStdin;
        private bool _noPrompt;
        private string _commandLineCommand;
        private bool _wasCommandEncoded;
        private uint _exitCode = ConsoleHost.ExitCodeSuccess;
        private bool _dirty;
        private Serialization.DataFormat _outFormat = Serialization.DataFormat.Text;
        private Serialization.DataFormat _inFormat = Serialization.DataFormat.Text;
        private Collection<CommandParameter> _collectedArgs = new Collection<CommandParameter>();
        private string _file;
        private string _executionPolicy;
        private bool _importSystemModules = false;
    }
}   // namespace

