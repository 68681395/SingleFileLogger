//===============================================================================

#region Using

// Simple File Logger Not Simple Function,Entirely Independent
//===============================================================================
//This is an open source from Microsoft patterns & practices Enterprise Library Logging Application Block
//We have done lots of enhancement,
//===============================================================================

#endregion

#region USING BLOCK

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

#endregion

namespace Tsharp
{
    public static class SimpleLogger
    {
        public delegate string FormatMessageHandler(string format, params object[] args);

        /// <summary>
        ///     Key mode.
        /// </summary>
        public enum KeyMode
        {
            /// <summary>
            ///     Overwrite value of the existing key.
            /// </summary>
            Overwrite,

            /// <summary>
            ///     Append value to existing key.
            /// </summary>
            Append,

            /// <summary>
            ///     Ignore new value for existing key.
            /// </summary>
            Ignore
        }

        public enum LogLevel
        {
            /// <summary>
            ///     All logging levels
            /// </summary>
            All = 0,

            /// <summary>
            ///     A trace logging level
            /// </summary>
            Trace = 1,

            /// <summary>
            ///     A debug logging level
            /// </summary>
            Debug = 2,

            /// <summary>
            ///     A info logging level
            /// </summary>
            Info = 3,

            /// <summary>
            ///     A warn logging level
            /// </summary>
            Warn = 4,

            /// <summary>
            ///     An error logging level
            /// </summary>
            Error = 5,

            /// <summary>
            ///     A fatal logging level
            /// </summary>
            Fatal = 6,

            /// <summary>
            ///     Do not log anything.
            /// </summary>
            Off = 7
        }

        /// <summary>
        ///     Processing modes of the parser
        /// </summary>
        public enum ParserMode
        {
            /// <summary>
            ///     Normal parsing mode.
            /// </summary>
            Normal,

            /// <summary>
            ///     Continuation mode.
            /// </summary>
            Continuation,

            /// <summary>
            ///     Here document mode.
            /// </summary>
            Here
        }

        /// <summary>
        ///     Defines the behavior when the roll file is created.
        /// </summary>
        public enum RollFileExistsBehavior
        {
            /// <summary>
            ///     Overwrites the file if it already exists.
            /// </summary>
            Overwrite,

            /// <summary>
            ///     Use a secuence number at the end of the generated file if it already exists. If it fails again then increment the
            ///     secuence until a non existent filename is found.
            /// </summary>
            Increment
        }

        /// <summary>
        ///     Defines the frequency when the file need to be rolled.
        /// </summary>
        public enum RollInterval
        {
            /// <summary>
            ///     None Interval
            /// </summary>
            None,

            /// <summary>
            ///     Minute Interval
            /// </summary>
            Minute,

            /// <summary>
            ///     Hour interval
            /// </summary>
            Hour,

            /// <summary>
            ///     Day Interval
            /// </summary>
            Day,

            /// <summary>
            ///     Week Interval
            /// </summary>
            Week,

            /// <summary>
            ///     Month Interval
            /// </summary>
            Month,

            /// <summary>
            ///     Year Interval
            /// </summary>
            Year,

            /// <summary>
            ///     At Midnight
            /// </summary>
            Midnight
        }



        public interface ILog
        {
            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            bool IsTraceEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            bool IsDebugEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Error" /> level.
            /// </summary>
            bool IsErrorEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            bool IsFatalEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Info" /> level.
            /// </summary>
            bool IsInfoEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            bool IsWarnEnabled { get; }

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            void Trace(object message);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Trace" /> level including
            ///     the stack trace of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Trace(object message, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            void TraceFormat(string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            void TraceFormat(string format, Exception exception, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            void TraceFormat(IFormatProvider formatProvider, string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            void TraceFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Trace(Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Trace(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Trace(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            void Debug(object message);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Debug" /> level including
            ///     the stack trace of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Debug(object message, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            void DebugFormat(string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            void DebugFormat(string format, Exception exception, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            void DebugFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Debug(Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Debug(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Debug.</param>
            void Debug(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            void Info(object message);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Info" /> level including
            ///     the stack trace of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Info(object message, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            void InfoFormat(string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            void InfoFormat(string format, Exception exception, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            void InfoFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Info(Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Info(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Info.</param>
            void Info(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            void Warn(object message);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Warn" /> level including
            ///     the stack trace of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Warn(object message, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            void WarnFormat(string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            void WarnFormat(string format, Exception exception, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            void WarnFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Warn(Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Warn(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Warn.</param>
            void Warn(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            void Error(object message);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Error" /> level including
            ///     the stack trace of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Error(object message, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            void ErrorFormat(string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            void ErrorFormat(string format, Exception exception, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            void ErrorFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Error(Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Error(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Error.</param>
            void Error(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            void Fatal(object message);

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Fatal" /> level including
            ///     the stack trace of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Fatal(object message, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            void FatalFormat(string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            void FatalFormat(string format, Exception exception, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            void FatalFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Fatal(Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            void Fatal(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Fatal.</param>
            void Fatal(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception);
        }

        public abstract class AbstractLogger : ILog
        {
            /// <summary>
            ///     Holds the method for writing a message to the log system.
            /// </summary>
            private readonly WriteHandler Write;

            /// <summary>
            ///     Creates a new logger instance using <see cref="WriteInternal" /> for
            ///     writing log events to the underlying log system.
            /// </summary>
            /// <seealso cref="GetWriteHandler" />
            protected AbstractLogger()
            {
                Write = GetWriteHandler();
                if (Write == null)
                    Write = WriteInternal;
            }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <remarks>
            ///     Override this in your derived class to comply with the underlying logging system
            /// </remarks>
            public abstract bool IsTraceEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <remarks>
            ///     Override this in your derived class to comply with the underlying logging system
            /// </remarks>
            public abstract bool IsDebugEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <remarks>
            ///     Override this in your derived class to comply with the underlying logging system
            /// </remarks>
            public abstract bool IsInfoEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <remarks>
            ///     Override this in your derived class to comply with the underlying logging system
            /// </remarks>
            public abstract bool IsWarnEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <remarks>
            ///     Override this in your derived class to comply with the underlying logging system
            /// </remarks>
            public abstract bool IsErrorEnabled { get; }

            /// <summary>
            ///     Checks if this logger is enabled for the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <remarks>
            ///     Override this in your derived class to comply with the underlying logging system
            /// </remarks>
            public abstract bool IsFatalEnabled { get; }

            /// <summary>
            ///     Override this method to use a different method than <see cref="WriteInternal" />
            ///     for writing log events to the underlying log system.
            /// </summary>
            /// <remarks>
            ///     Usually you don't need to override thise method. The default implementation returns
            ///     <c>null</c> to indicate that the default handler <see cref="WriteInternal" /> should be
            ///     used.
            /// </remarks>
            protected virtual WriteHandler GetWriteHandler()
            {
                return null;
            }

            /// <summary>
            ///     Actually sends the message to the underlying log system.
            /// </summary>
            /// <param name="level">the level of this log event.</param>
            /// <param name="message">the message to log</param>
            /// <param name="exception">the exception to log (may be null)</param>
            protected abstract void WriteInternal(
                LogLevel level, object message, Exception exception);

            #region FormatMessageCallbackFormattedMessage

            private class FormatMessageCallbackFormattedMessage
            {
                private readonly Action<FormatMessageHandler> formatMessageCallback;

                private readonly IFormatProvider formatProvider;
                private volatile string cachedMessage;

                public FormatMessageCallbackFormattedMessage(
                    Action<FormatMessageHandler> formatMessageCallback)
                {
                    this.formatMessageCallback = formatMessageCallback;
                }

                public FormatMessageCallbackFormattedMessage(
                    IFormatProvider formatProvider,
                    Action<FormatMessageHandler> formatMessageCallback)
                {
                    this.formatProvider = formatProvider;
                    this.formatMessageCallback = formatMessageCallback;
                }

                public override string ToString()
                {
                    if (cachedMessage == null && formatMessageCallback != null)
                        formatMessageCallback(FormatMessage);
                    return cachedMessage;
                }

                private string FormatMessage(string format, params object[] args)
                {
                    cachedMessage = string.Format(formatProvider, format, args);
                    return cachedMessage;
                }
            }

            #endregion

            #region StringFormatFormattedMessage

            private class StringFormatFormattedMessage
            {
                private readonly object[] Args;

                private readonly IFormatProvider FormatProvider;
                private readonly string Message;
                private volatile string cachedMessage;

                public StringFormatFormattedMessage(
                    IFormatProvider formatProvider, string message, params object[] args)
                {
                    FormatProvider = formatProvider;
                    Message = message;
                    Args = args;
                }

                public override string ToString()
                {
                    if (cachedMessage == null && Message != null)
                        cachedMessage = string.Format(FormatProvider, Message, Args);
                    return cachedMessage;
                }
            }

            #endregion

            /// <summary>
            ///     Represents a method responsible for writing a message to the log system.
            /// </summary>
            protected delegate void WriteHandler(LogLevel level, object message, Exception exception
            );

            #region Trace

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            public virtual void Trace(object message)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace, message, null);
            }

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Trace" /> level including
            ///     the stack trace of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            public virtual void Trace(object message, Exception exception)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace, message, exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            public virtual void TraceFormat(
                IFormatProvider formatProvider, string format, params object[] args)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace,
                        new StringFormatFormattedMessage(formatProvider, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            public virtual void TraceFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace,
                        new StringFormatFormattedMessage(formatProvider, format, args), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            public virtual void TraceFormat(string format, params object[] args)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace, new StringFormatFormattedMessage(null, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            public virtual void TraceFormat(
                string format, Exception exception, params object[] args)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace, new StringFormatFormattedMessage(null, format, args),
                        exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Trace(Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            public virtual void Trace(
                Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Trace(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Trace" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack trace.</param>
            public virtual void Trace(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                if (IsTraceEnabled)
                    Write(LogLevel.Trace,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), exception);
            }

            #endregion

            #region Debug

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            public virtual void Debug(object message)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug, message, null);
            }

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Debug" /> level including
            ///     the stack Debug of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack Debug.</param>
            public virtual void Debug(object message, Exception exception)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug, message, exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            public virtual void DebugFormat(
                IFormatProvider formatProvider, string format, params object[] args)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug,
                        new StringFormatFormattedMessage(formatProvider, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            public virtual void DebugFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug,
                        new StringFormatFormattedMessage(formatProvider, format, args), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            public virtual void DebugFormat(string format, params object[] args)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug, new StringFormatFormattedMessage(null, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            public virtual void DebugFormat(
                string format, Exception exception, params object[] args)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug, new StringFormatFormattedMessage(null, format, args),
                        exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Debug(Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Debug.</param>
            public virtual void Debug(
                Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Debug(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Debug" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Debug.</param>
            public virtual void Debug(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                if (IsDebugEnabled)
                    Write(LogLevel.Debug,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), exception);
            }

            #endregion

            #region Info

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            public virtual void Info(object message)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info, message, null);
            }

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Info" /> level including
            ///     the stack Info of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack Info.</param>
            public virtual void Info(object message, Exception exception)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info, message, exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            public virtual void InfoFormat(
                IFormatProvider formatProvider, string format, params object[] args)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info,
                        new StringFormatFormattedMessage(formatProvider, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            public virtual void InfoFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info,
                        new StringFormatFormattedMessage(formatProvider, format, args), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            public virtual void InfoFormat(string format, params object[] args)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info, new StringFormatFormattedMessage(null, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            public virtual void InfoFormat(string format, Exception exception, params object[] args)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info, new StringFormatFormattedMessage(null, format, args),
                        exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Info(Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Info.</param>
            public virtual void Info(
                Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Info(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Info" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Info.</param>
            public virtual void Info(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                if (IsInfoEnabled)
                    Write(LogLevel.Info,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), exception);
            }

            #endregion

            #region Warn

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            public virtual void Warn(object message)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn, message, null);
            }

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Warn" /> level including
            ///     the stack Warn of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack Warn.</param>
            public virtual void Warn(object message, Exception exception)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn, message, exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting Warnrmation.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            public virtual void WarnFormat(
                IFormatProvider formatProvider, string format, params object[] args)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn,
                        new StringFormatFormattedMessage(formatProvider, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting Warnrmation.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            public virtual void WarnFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn,
                        new StringFormatFormattedMessage(formatProvider, format, args), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            public virtual void WarnFormat(string format, params object[] args)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn, new StringFormatFormattedMessage(null, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            public virtual void WarnFormat(string format, Exception exception, params object[] args)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn, new StringFormatFormattedMessage(null, format, args),
                        exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Warn(Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Warn.</param>
            public virtual void Warn(
                Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Warn(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Warn" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Warn.</param>
            public virtual void Warn(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                if (IsWarnEnabled)
                    Write(LogLevel.Warn,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), exception);
            }

            #endregion

            #region Error

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            public virtual void Error(object message)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error, message, null);
            }

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Error" /> level including
            ///     the stack Error of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack Error.</param>
            public virtual void Error(object message, Exception exception)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error, message, exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting Errorrmation.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            public virtual void ErrorFormat(
                IFormatProvider formatProvider, string format, params object[] args)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error,
                        new StringFormatFormattedMessage(formatProvider, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting Errorrmation.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            public virtual void ErrorFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error,
                        new StringFormatFormattedMessage(formatProvider, format, args), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            public virtual void ErrorFormat(string format, params object[] args)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error, new StringFormatFormattedMessage(null, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            public virtual void ErrorFormat(
                string format, Exception exception, params object[] args)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error, new StringFormatFormattedMessage(null, format, args),
                        exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Error(Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Error.</param>
            public virtual void Error(
                Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Error(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Error" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Error.</param>
            public virtual void Error(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                if (IsErrorEnabled)
                    Write(LogLevel.Error,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), exception);
            }

            #endregion

            #region Fatal

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            public virtual void Fatal(object message)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal, message, null);
            }

            /// <summary>
            ///     Log a message object with the <see cref="LogLevel.Fatal" /> level including
            ///     the stack Fatal of the <see cref="Exception" /> passed
            ///     as a parameter.
            /// </summary>
            /// <param name="message">The message object to log.</param>
            /// <param name="exception">The exception to log, including its stack Fatal.</param>
            public virtual void Fatal(object message, Exception exception)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal, message, exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting Fatalrmation.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args"></param>
            public virtual void FatalFormat(
                IFormatProvider formatProvider, string format, params object[] args)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal,
                        new StringFormatFormattedMessage(formatProvider, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting Fatalrmation.</param>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args"></param>
            public virtual void FatalFormat(
                IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal,
                        new StringFormatFormattedMessage(formatProvider, format, args), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="args">the list of format arguments</param>
            public virtual void FatalFormat(string format, params object[] args)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal, new StringFormatFormattedMessage(null, format, args), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level.
            /// </summary>
            /// <param name="format">The format of the message object to log.<see cref="string.Format(string,object[])" /> </param>
            /// <param name="exception">The exception to log.</param>
            /// <param name="args">the list of format arguments</param>
            public virtual void FatalFormat(
                string format, Exception exception, params object[] args)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal, new StringFormatFormattedMessage(null, format, args),
                        exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Fatal(Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Fatal.</param>
            public virtual void Fatal(
                Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal,
                        new FormatMessageCallbackFormattedMessage(formatMessageCallback), exception);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            public virtual void Fatal(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), null);
            }

            /// <summary>
            ///     Log a message with the <see cref="LogLevel.Fatal" /> level using a callback to obtain the message
            /// </summary>
            /// <remarks>
            ///     Using this method avoids the cost of creating a message and evaluating message arguments
            ///     that probably won't be logged due to loglevel settings.
            /// </remarks>
            /// <param name="formatProvider">An <see cref="IFormatProvider" /> that supplies culture-specific formatting information.</param>
            /// <param name="formatMessageCallback">A callback used by the logger to obtain the message if log level is matched</param>
            /// <param name="exception">The exception to log, including its stack Fatal.</param>
            public virtual void Fatal(
                IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                if (IsFatalEnabled)
                    Write(LogLevel.Fatal,
                        new FormatMessageCallbackFormattedMessage(formatProvider,
                            formatMessageCallback), exception);
            }

            #endregion
        }

        public abstract class AbstractSimpleLogger : AbstractLogger
        {
            /// <summary>
            ///     Creates and initializes a the simple logger.
            /// </summary>
            /// <param name="logName">The name, usually type name of the calling class, of the logger.</param>
            /// <param name="logLevel">
            ///     The current logging threshold. Messages recieved that are beneath this threshold will not be
            ///     logged.
            /// </param>
            /// <param name="showlevel">Include level in the log message.</param>
            /// <param name="showDateTime">Include the current time in the log message.</param>
            /// <param name="showLogName">Include the instance name in the log message.</param>
            /// <param name="dateTimeFormat">The date and time format to use in the log message.</param>
            public AbstractSimpleLogger(
                string logName, LogLevel logLevel, bool showlevel, bool showDateTime,
                bool showLogName, string dateTimeFormat)
            {
                Name = logName;
                CurrentLogLevel = logLevel;
                ShowLevel = showlevel;
                ShowDateTime = showDateTime;
                ShowLogName = showLogName;
                DateTimeFormat = dateTimeFormat;
                HasDateTimeFormat = !string.IsNullOrEmpty(DateTimeFormat);
            }

            /// <summary>
            ///     Appends the formatted message to the specified <see cref="StringBuilder" />.
            /// </summary>
            /// <param name="stringBuilder">the <see cref="StringBuilder" /> that receíves the formatted message.</param>
            /// <param name="level"></param>
            /// <param name="message"></param>
            /// <param name="e"></param>
            protected virtual void FormatOutput(
                StringBuilder stringBuilder, LogLevel level, object message, Exception e)
            {
                if (stringBuilder == null)
                    throw new ArgumentNullException("stringBuilder");

                // Append date-time if so configured
                if (ShowDateTime)
                {
                    if (HasDateTimeFormat)
                        stringBuilder.Append(DateTimeOffset.Now.ToString(DateTimeFormat,
                            CultureInfo.InvariantCulture));
                    else
                        stringBuilder.Append(DateTimeOffset.Now);

                    stringBuilder.Append(" ");
                }

                if (ShowLevel)
                    stringBuilder.Append(("[" + level.ToString().ToUpper() + "]").PadRight(8));

                // Append the name of the log instance if so configured
                if (ShowLogName)
                    stringBuilder.Append(Name).Append(" - ");

                // Append the message
                stringBuilder.Append(message);

                // Append stack trace if not null
                if (e != null)
                    stringBuilder.Append(Environment.NewLine).Append(e);
            }

            /// <summary>
            ///     Determines if the given log level is currently enabled.
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            protected virtual bool IsLevelEnabled(LogLevel level)
            {
                var iLevel = (int)level;
                var iCurrentLogLevel = (int)CurrentLogLevel;

                // return iLevel.CompareTo(iCurrentLogLevel); better ???
                return iLevel >= iCurrentLogLevel;
            }

            #region Properties

            /// <summary>
            ///     The name of the logger.
            /// </summary>
            public string Name { get; }

            /// <summary>
            ///     Include the current log level in the log message.
            /// </summary>
            public bool ShowLevel { get; }

            /// <summary>
            ///     Include the current time in the log message.
            /// </summary>
            public bool ShowDateTime { get; }

            /// <summary>
            ///     Include the instance name in the log message.
            /// </summary>
            public bool ShowLogName { get; }

            /// <summary>
            ///     The current logging threshold. Messages recieved that are beneath this threshold will not be logged.
            /// </summary>
            public LogLevel CurrentLogLevel { get; set; }

            /// <summary>
            ///     The date and time format to use in the log message.
            /// </summary>
            public string DateTimeFormat { get; }

            /// <summary>
            ///     Determines Whether <see cref="DateTimeFormat" /> is set.
            /// </summary>
            public bool HasDateTimeFormat { get; }

            #endregion

            #region ILog Members

            /// <summary>
            ///     Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
            ///     equal to <see cref="LogLevel.Trace" />. If it is, all messages will be sent to <see cref="Console.Out" />.
            /// </summary>
            public override bool IsTraceEnabled
            {
                get { return IsLevelEnabled(LogLevel.Trace); }
            }

            /// <summary>
            ///     Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
            ///     equal to <see cref="LogLevel.Debug" />. If it is, all messages will be sent to <see cref="Console.Out" />.
            /// </summary>
            public override bool IsDebugEnabled
            {
                get { return IsLevelEnabled(LogLevel.Debug); }
            }

            /// <summary>
            ///     Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
            ///     equal to <see cref="LogLevel.Info" />. If it is, only messages with a <see cref="LogLevel" /> of
            ///     <see cref="LogLevel.Info" />, <see cref="LogLevel.Warn" />, <see cref="LogLevel.Error" />, and
            ///     <see cref="LogLevel.Fatal" /> will be sent to <see cref="Console.Out" />.
            /// </summary>
            public override bool IsInfoEnabled
            {
                get { return IsLevelEnabled(LogLevel.Info); }
            }


            /// <summary>
            ///     Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
            ///     equal to <see cref="LogLevel.Warn" />. If it is, only messages with a <see cref="LogLevel" /> of
            ///     <see cref="LogLevel.Warn" />, <see cref="LogLevel.Error" />, and <see cref="LogLevel.Fatal" />
            ///     will be sent to <see cref="Console.Out" />.
            /// </summary>
            public override bool IsWarnEnabled
            {
                get { return IsLevelEnabled(LogLevel.Warn); }
            }

            /// <summary>
            ///     Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
            ///     equal to <see cref="LogLevel.Error" />. If it is, only messages with a <see cref="LogLevel" /> of
            ///     <see cref="LogLevel.Error" /> and <see cref="LogLevel.Fatal" /> will be sent to <see cref="Console.Out" />.
            /// </summary>
            public override bool IsErrorEnabled
            {
                get { return IsLevelEnabled(LogLevel.Error); }
            }

            /// <summary>
            ///     Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
            ///     equal to <see cref="LogLevel.Fatal" />. If it is, only messages with a <see cref="LogLevel" /> of
            ///     <see cref="LogLevel.Fatal" /> will be sent to <see cref="Console.Out" />.
            /// </summary>
            public override bool IsFatalEnabled
            {
                get { return IsLevelEnabled(LogLevel.Fatal); }
            }

            #endregion
        }

        public class RollingLogger : AbstractSimpleLogger //, IDeserializationCallback
        {
            private readonly LogSource _traceSource;

            /// <summary>
            ///     Creates a new RollingLogger instance.
            /// </summary>
            /// <param name="traceSource">The trace source.</param>
            /// <param name="logName">the name of this logger</param>
            /// <param name="logLevel">the default log level to use</param>
            /// <param name="showLevel">Include the current log level in the log message.</param>
            /// <param name="showDateTime">Include the current time in the log message.</param>
            /// <param name="showLogName">Include the instance name in the log message.</param>
            /// <param name="dateTimeFormat">The date and time format to use in the log message.</param>
            public RollingLogger(
                LogSource traceSource, string logName, LogLevel logLevel, bool showLevel,
                bool showDateTime, bool showLogName, string dateTimeFormat)
                : base(logName, logLevel, showLevel, showDateTime, showLogName, dateTimeFormat)
            {
                _traceSource = traceSource;
            }

            /// <summary>
            ///     Do the actual logging.
            /// </summary>
            /// <param name="level"></param>
            /// <param name="message"></param>
            /// <param name="e"></param>
            protected override void WriteInternal(LogLevel level, object message, Exception e)
            {
                var outmsg = new FormatOutputMessage(this, level, message, e);
                _traceSource.WriteLine(outmsg);
            }

            /// <summary>
            ///     Used to defer message formatting until it is really needed.
            /// </summary>
            /// <remarks>
            ///     This class also improves performance when multiple
            ///     <see cref="TraceListener" />s are configured.
            /// </remarks>
            private class FormatOutputMessage
            {
                private readonly Exception ex;
                private readonly LogLevel level;
                private readonly object message;
                private readonly RollingLogger outer;

                public FormatOutputMessage(
                    RollingLogger outer, LogLevel level, object message, Exception ex)
                {
                    this.outer = outer;
                    this.level = level;
                    this.message = message;
                    this.ex = ex;
                }

                public override string ToString()
                {
                    var sb = new StringBuilder();
                    outer.FormatOutput(sb, level, message, ex);
                    return sb.ToString();
                }
            }
        }

        public static class LogManager
        {
            private static readonly object _loadLock = new object();
            private static RollingFlatFileTraceListener listener = new RollingFlatFileTraceListener("App_Data/log/trace.log",
                  null,
                  null, 1024, "yyyyMMddHHmm", "'archived'yyyyMMdd",
                  RollFileExistsBehavior.Increment, RollInterval.Day);

            private static LogSource source;
            private static readonly ConcurrentDictionary<string, ILog> _cachedLoggers;

            private static ConfigReader reader;

            private static string _dateTimeFormat;
            private static bool _showDateTime;
            private static bool _showLevel;
            private static bool _showLogName;

            /// <summary>
            ///     The default <see cref="LogLevel" /> to use when creating new <see cref="ILog" /> instances.
            /// </summary>
            public static LogLevel Level { get; set; }
            static LogManager()
            {
                _cachedLoggers =
                     new ConcurrentDictionary<string, ILog>(StringComparer.OrdinalIgnoreCase);
                reader = new ConfigReader("App_Data/simplelogger.conf");
                reader.ConfigChange += Reader_ConfigChange;
                _dateTimeFormat = reader.GetValue("dateTimeFormat", "yyyy-MM-ddTHH:mm:ssK");
                _showDateTime = reader.GetValue("showDateTime", true);
                _showLevel = reader.GetValue("showLevel", true);
                _showLogName = reader.GetValue("showLogName", false);
                Level = reader.GetValue("Level", LogLevel.All);
                source = new LogSource(new[] { listener }, true);
            }

            private static void Reader_ConfigChange(object sender, ConfigReader e)
            {

            }


            /// <summary>
            ///     Gets the logger by calling <see cref="ILoggerFactoryAdapter.GetLogger(Type)" />
            ///     on the currently configured <see cref="Adapter" /> using the type of the calling class.
            /// </summary>
            /// <remarks>
            ///     This method needs to inspect the <see cref="StackTrace" /> in order to determine the calling
            ///     class. This of course comes with a performance penalty, thus you shouldn't call it too
            ///     often in your application.
            /// </remarks>
            /// <seealso cref="GetLogger(Type)" />
            /// <returns>the logger instance obtained from the current <see cref="Adapter" /></returns>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static ILog GetCurrentClassLogger()
            {
                var frame = new StackFrame(1, false);
                return GetLogger(frame.GetMethod().ReflectedType);
            }

            /// <summary>
            ///     Gets the logger by calling <see cref="ILoggerFactoryAdapter.GetLogger(Type)" />
            ///     on the currently configured <see cref="Adapter" /> using the specified type.
            /// </summary>
            /// <returns>the logger instance obtained from the current <see cref="Adapter" /></returns>
            public static ILog GetLogger<T>()
            {
                return GetLogger(typeof(T));
            }

            /// <summary>
            ///     Gets the logger by calling <see cref="ILoggerFactoryAdapter.GetLogger(Type)" />
            ///     on the currently configured <see cref="Adapter" /> using the specified type.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <returns>the logger instance obtained from the current <see cref="Adapter" /></returns>
            public static ILog GetLogger(Type type)
            {
                return GetLogger(type.FullName);
            }


            /// <summary>
            ///     Gets the logger by calling <see cref="ILoggerFactoryAdapter.GetLogger(string)" />
            ///     on the currently configured <see cref="Adapter" /> using the specified name.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <returns>the logger instance obtained from the current <see cref="Adapter" /></returns>
            public static ILog GetLogger(string name)
            {
                return new RollingLogger(source, name, Level, _showLevel, _showDateTime,
                    _showLogName, _dateTimeFormat);
            }
        }

        public static class ArgUtils
        {
            /// <summary>
            ///     An anonymous action delegate with no arguments and no return value.
            /// </summary>
            /// <seealso cref="Guard" />
            public delegate void Action();

            /// <summary>
            ///     An anonymous action delegate with no arguments and no return value.
            /// </summary>
            /// <seealso cref="Guard{T}" />
            public delegate T Function<T>();

            /// <summary>
            ///     A delegate converting a string representation into the target type
            /// </summary>
            public delegate T ParseHandler<T>(string strValue);

            private static readonly Hashtable s_parsers;

            /// <summary>
            ///     Initialize all members before any of this class' methods can be accessed (avoids beforeFieldInit)
            /// </summary>
            static ArgUtils()
            {
                s_parsers = new Hashtable();
                RegisterTypeParser(delegate (string s) { return Convert.ToBoolean(s); });
                RegisterTypeParser(delegate (string s) { return Convert.ToInt16(s); });
                RegisterTypeParser(delegate (string s) { return Convert.ToInt32(s); });
                RegisterTypeParser(delegate (string s) { return Convert.ToInt64(s); });
                RegisterTypeParser(delegate (string s) { return Convert.ToSingle(s); });
                RegisterTypeParser(delegate (string s) { return Convert.ToDouble(s); });
                RegisterTypeParser(delegate (string s) { return Convert.ToDecimal(s); });
            }

            /// <summary>
            ///     Adds the parser to the list of known type parsers.
            /// </summary>
            /// <remarks>
            ///     .NET intrinsic types are pre-registerd: short, int, long, float, double, decimal, bool
            /// </remarks>
            public static void RegisterTypeParser<T>(ParseHandler<T> parser)
            {
                s_parsers[typeof(T)] = parser;
            }

            /// <summary>
            ///     Retrieves the named value from the specified <see cref="NameValueCollection" />.
            /// </summary>
            /// <param name="values">may be null</param>
            /// <param name="name">the value's key</param>
            /// <returns>if <paramref name="values" /> is not null, the value returned by values[name]. <c>null</c> otherwise.</returns>
            public static string GetValue(NameValueCollection values, string name)
            {
                return GetValue(values, name, null);
            }

            /// <summary>
            ///     Retrieves the named value from the specified <see cref="NameValueCollection" />.
            /// </summary>
            /// <param name="values">may be null</param>
            /// <param name="name">the value's key</param>
            /// <param name="defaultValue">the default value, if not found</param>
            /// <returns>if <paramref name="values" /> is not null, the value returned by values[name]. <c>null</c> otherwise.</returns>
            public static string GetValue(
                NameValueCollection values, string name, string defaultValue)
            {
                if (values != null)
                    foreach (var key in values.AllKeys)
                        if (string.Compare(name, key, StringComparison.OrdinalIgnoreCase) == 0)
                            return values[name];
                return defaultValue;
            }

            /// <summary>
            ///     Returns the first nonnull, nonempty value among its arguments.
            /// </summary>
            /// <remarks>
            ///     Returns <c>null</c>, if the initial list was null or empty.
            /// </remarks>
            /// <seealso cref="Coalesce{T}" />
            public static string Coalesce(params string[] values)
            {
                return Coalesce(delegate (string v) { return !string.IsNullOrEmpty(v); }, values);
            }

            /// <summary>
            ///     Returns the first nonnull, nonempty value among its arguments.
            /// </summary>
            /// <remarks>
            ///     Also
            /// </remarks>
            public static T Coalesce<T>(Predicate<T> predicate, params T[] values) where T : class
            {
                if (values == null || values.Length == 0)
                    return null;

                if (predicate == null)
                    predicate = delegate (T v) { return v != null; };

                for (var i = 0; i < values.Length; i++)
                {
                    var val = values[i];
                    if (predicate(val))
                        return val;
                }
                return null;
            }

            /// <summary>
            ///     Tries parsing <paramref name="stringValue" /> into an enum of the type of <paramref name="defaultValue" />.
            /// </summary>
            /// <param name="defaultValue">the default value to return if parsing fails</param>
            /// <param name="stringValue">the string value to parse</param>
            /// <returns>the successfully parsed value, <paramref name="defaultValue" /> otherwise.</returns>
            public static T TryParseEnum<T>(T defaultValue, string stringValue) where T : struct
            {
                if (!typeof(T).IsEnum)
                    throw new ArgumentException(string.Format("Type '{0}' is not an enum type",
                        typeof(T).FullName));

                var result = defaultValue;
                if (string.IsNullOrEmpty(stringValue))
                    return defaultValue;
                try
                {
                    result = (T)Enum.Parse(typeof(T), stringValue, true);
                }
                catch
                {
                    Trace.WriteLine(
                        string.Format("WARN: failed converting value '{0}' to enum type '{1}'",
                            stringValue, defaultValue.GetType().FullName));
                }
                return result;
            }

            /// <summary>
            ///     Tries parsing <paramref name="stringValue" /> into the specified return type.
            /// </summary>
            /// <param name="defaultValue">the default value to return if parsing fails</param>
            /// <param name="stringValue">the string value to parse</param>
            /// <returns>the successfully parsed value, <paramref name="defaultValue" /> otherwise.</returns>
            public static T TryParse<T>(T defaultValue, string stringValue)
            {
                var result = defaultValue;
                if (string.IsNullOrEmpty(stringValue))
                    return defaultValue;

                var parser = s_parsers[typeof(T)] as ParseHandler<T>;
                if (parser == null)
                    throw new ArgumentException(
                        string.Format("There is no parser registered for type {0}",
                            typeof(T).FullName));

                try
                {
                    result = parser(stringValue);
                }
                catch
                {
                    Trace.WriteLine(
                        string.Format(
                            "WARN: failed converting value '{0}' to type '{1}' - returning default '{2}'",
                            stringValue, typeof(T).FullName, result));
                }
                return result;
            }

            /// <summary>
            ///     Throws a <see cref="ArgumentNullException" /> if <paramref name="val" /> is <c>null</c>.
            /// </summary>
            public static T AssertNotNull<T>(string paramName, T val) where T : class
            {
                if (ReferenceEquals(val, null))
                    throw new ArgumentNullException(paramName);
                return val;
            }

            /// <summary>
            ///     Throws a <see cref="ArgumentNullException" /> if <paramref name="val" /> is <c>null</c>.
            /// </summary>
            public static T AssertNotNull<T>(
                string paramName, T val, string messageFormat, params object[] args) where T : class
            {
                if (ReferenceEquals(val, null))
                    throw new ArgumentNullException(paramName, string.Format(messageFormat, args));
                return val;
            }

            /// <summary>
            ///     Throws a <see cref="ArgumentOutOfRangeException" /> if an object of type <paramref name="valType" /> is not
            ///     assignable to type
            ///     <typeparam name="T"></typeparam>
            ///     .
            /// </summary>
            public static Type AssertIsAssignable<T>(string paramName, Type valType)
            {
                return AssertIsAssignable<T>(paramName, valType,
                    string.Format(
                        "Type '{0}' of parameter '{1}' is not assignable to target type '{2}'",
                        valType == null ? "<undefined>" : valType.AssemblyQualifiedName, paramName,
                        typeof(T).AssemblyQualifiedName));
            }

            /// <summary>
            ///     Throws a <see cref="ArgumentOutOfRangeException" /> if an object of type <paramref name="valType" /> is not
            ///     assignable to type
            ///     <typeparam name="T"></typeparam>
            ///     .
            /// </summary>
            public static Type AssertIsAssignable<T>(
                string paramName, Type valType, string messageFormat, params object[] args)
            {
                if (valType == null)
                    throw new ArgumentNullException("valType");

                if (!typeof(T).IsAssignableFrom(valType))
                    throw new ArgumentOutOfRangeException(paramName, valType,
                        string.Format(messageFormat, args));
                return valType;
            }

            /// <summary>
            ///     Ensures any exception thrown by the given <paramref name="action" /> is wrapped with an
            ///     <see cref="ConfigurationException" />.
            /// </summary>
            /// <remarks>
            ///     If <paramref name="action" /> already throws a ConfigurationException, it will not be wrapped.
            /// </remarks>
            /// <param name="action">the action to execute</param>
            /// <param name="messageFormat">the message to be set on the thrown <see cref="ConfigurationException" /></param>
            /// <param name="args">args to be passed to <see cref="string.Format(string,object[])" /> to format the message</param>
            public static void Guard(Action action, string messageFormat, params object[] args)
            {
                Guard(delegate
                {
                    action();
                    return 0;
                }, messageFormat, args);
            }

            /// <summary>
            ///     Ensures any exception thrown by the given <paramref name="function" /> is wrapped with an
            ///     <see cref="ConfigurationException" />.
            /// </summary>
            /// <remarks>
            ///     If <paramref name="function" /> already throws a ConfigurationException, it will not be wrapped.
            /// </remarks>
            /// <param name="function">the action to execute</param>
            /// <param name="messageFormat">the message to be set on the thrown <see cref="ConfigurationException" /></param>
            /// <param name="args">args to be passed to <see cref="string.Format(string,object[])" /> to format the message</param>
            public static T Guard<T>(
                Function<T> function, string messageFormat, params object[] args)
            {
                try
                {
                    return function();
                }
                catch (ConfigurationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException(string.Format(messageFormat, args), ex);
                }
            }
        }

        /// <summary>
        ///     Purges archive files generated by the <see cref="RollingFlatFileTraceListener" />.
        /// </summary>
        public class RollingFlatFilePurger
        {
            private readonly string baseFileName;

            private readonly int cap;

            private readonly string directory;

            /// <summary>
            ///     Initializes a new instance of the <see cref="RollingFlatFilePurger" /> class.
            /// </summary>
            /// <param name="directory">The folder where archive files are kept.</param>
            /// <param name="baseFileName">The base name for archive files.</param>
            /// <param name="cap">The number of archive files to keep.</param>
            public RollingFlatFilePurger(string directory, string baseFileName, int cap)
            {
                if (directory == null) throw new ArgumentNullException("directory");
                if (baseFileName == null) throw new ArgumentNullException("baseFileName");
                if (cap < 1) throw new ArgumentOutOfRangeException("cap");

                this.directory = directory;
                this.baseFileName = baseFileName;
                this.cap = cap;
            }

            /// <summary>
            ///     Purges archive files.
            /// </summary>
            public void Purge()
            {
                var extension = Path.GetExtension(baseFileName);
                var searchPattern = Path.GetFileNameWithoutExtension(baseFileName) + ".*" +
                                    extension;

                var matchingFiles = TryGetMatchingFiles(searchPattern);

                if (matchingFiles.Length <= cap) return;

                // sort the archive files in descending order by creation date and sequence number
                var sortedArchiveFiles =
                    matchingFiles.Select(matchingFile => new ArchiveFile(matchingFile))
                        .OrderByDescending(archiveFile => archiveFile);

                using (var enumerator = sortedArchiveFiles.GetEnumerator())
                {
                    // skip the most recent files
                    for (var i = 0; i < cap; i++) if (!enumerator.MoveNext()) return;

                    // delete the older files
                    while (enumerator.MoveNext()) TryDelete(enumerator.Current.Path);
                }
            }

            private string[] TryGetMatchingFiles(string searchPattern)
            {
                try
                {
                    return Directory.GetFiles(directory, searchPattern,
                        SearchOption.TopDirectoryOnly);
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }

                return new string[0];
            }

            private static void TryDelete(string path)
            {
                try
                {
                    File.Delete(path);
                }
                catch (UnauthorizedAccessException)
                {
                    // cannot delete the file because of a permissions issue - just skip it
                }
                catch (IOException)
                {
                    // cannot delete the file, most likely because it is already opened - just skip it
                }
            }

            private static DateTimeOffset GetCreationTime(string path)
            {
                try
                {
                    return File.GetCreationTimeUtc(path);
                }
                catch (UnauthorizedAccessException)
                {
                    // will cause file be among the first files when sorting, 
                    // and its deletion will likely fail causing it to be skipped
                    return DateTimeOffset.MinValue;
                }
            }

            /// <summary>
            ///     Extracts the sequence number from an archive file name.
            /// </summary>
            /// <param name="fileName">The archive file name.</param>
            /// <returns>The sequence part of the file name.</returns>
            public static string GetSequence(string fileName)
            {
                if (fileName == null) throw new ArgumentNullException(fileName, "fileName");

                var extensionDotIndex = fileName.LastIndexOf('.');
                if (extensionDotIndex <= 0) return string.Empty;
                var sequenceDotIndex = fileName.LastIndexOf('.', extensionDotIndex - 1);
                if (sequenceDotIndex < 0) return string.Empty;

                return fileName.Substring(sequenceDotIndex + 1,
                    extensionDotIndex - sequenceDotIndex - 1);
            }

            internal class ArchiveFile : IComparable<ArchiveFile>
            {
                private readonly string fileName;

                private int? sequence;

                private string sequenceString;

                public ArchiveFile(string path)
                {
                    Path = path;
                    fileName = System.IO.Path.GetFileName(path);
                    CreationTime = GetCreationTime(path);
                }

                public string Path { get; }

                public DateTimeOffset CreationTime { get; }

                public string SequenceString
                {
                    get
                    {
                        if (sequenceString == null) sequenceString = GetSequence(fileName);

                        return sequenceString;
                    }
                }

                public int Sequence
                {
                    get
                    {
                        if (!sequence.HasValue)
                        {
                            int theSequence;
                            if (int.TryParse(SequenceString, NumberStyles.None,
                                CultureInfo.InvariantCulture, out theSequence))
                                sequence = theSequence;
                            else sequence = 0;
                        }

                        return sequence.Value;
                    }
                }

                public int CompareTo(ArchiveFile other)
                {
                    var creationDateComparison = CreationTime.CompareTo(other.CreationTime);
                    if (creationDateComparison != 0) return creationDateComparison;

                    if (Sequence != 0 && other.Sequence != 0)
                        return Sequence.CompareTo(other.Sequence);
                    // compare the sequence part of the file name as plain strings
                    return SequenceString.CompareTo(other.SequenceString);
                }
            }
        }

        /// <summary>
        ///     Helper class for working with environment variables.
        /// </summary>
        public static class EnvironmentHelper
        {
            /// <summary>
            ///     Sustitute the Environment Variables
            /// </summary>
            /// <param name="fileName">The filename.</param>
            /// <returns></returns>
            public static string ReplaceEnvironmentVariables(string fileName)
            {
                // Check EnvironmentPermission for the ability to access the environment variables.
                try
                {
                    var variables = Environment.ExpandEnvironmentVariables(fileName);

                    // If an Environment Variable is not found then remove any invalid tokens
                    var filter = new Regex("%(.*?)%",
                        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                    var filePath = filter.Replace(variables, "");

                    if (Path.GetDirectoryName(filePath) == null)
                        filePath = Path.GetFileName(filePath);

                    return RootFileNameAndEnsureTargetFolderExists(filePath);
                }
                catch (SecurityException)
                {
                    throw new InvalidOperationException("Environment Variables access denied.");
                }
            }

            private static string RootFileNameAndEnsureTargetFolderExists(string fileName)
            {

                var rootedFileName = fileName;
                if (!Path.IsPathRooted(rootedFileName))
                    rootedFileName =
                        Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory") as string ??
                        AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                            rootedFileName);

                var directory = Path.GetDirectoryName(rootedFileName);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return rootedFileName;
            }
        }

        /// <summary>
        ///     <para>
        ///         Provides the <see langword='abstract ' />base class for the listeners who
        ///         monitor trace and debug output.
        ///     </para>
        /// </summary>
        [HostProtection(Synchronization = true)]
        public abstract class TraceListener : IDisposable
        {
            private int indentLevel;

            private int indentSize = 4;

            private string listenerName;

            /// <summary>
            ///     <para>Initializes a new instance of the <see cref='System.Diagnostics.TraceListener' /> class.</para>
            /// </summary>
            protected TraceListener()
            {
            }

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref='System.Diagnostics.TraceListener' /> class using the specified
            ///         name as the
            ///         listener.
            ///     </para>
            /// </summary>
            protected TraceListener(string name)
            {
                listenerName = name;
            }

            /// <summary>
            ///     <para> Gets or sets a name for this <see cref='System.Diagnostics.TraceListener' />.</para>
            /// </summary>
            public virtual string Name
            {
                get { return listenerName == null ? "" : listenerName; }

                set { listenerName = value; }
            }

            public virtual bool IsThreadSafe
            {
                get { return false; }
            }

            /// <summary>
            ///     <para>Gets or sets the indent level.</para>
            /// </summary>
            public int IndentLevel
            {
                get { return indentLevel; }

                set { indentLevel = value < 0 ? 0 : value; }
            }

            /// <summary>
            ///     <para>Gets or sets the number of spaces in an indent.</para>
            /// </summary>
            public int IndentSize
            {
                get { return indentSize; }

                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("IndentSize", value,
                            "IndentSize must be greater then zero");
                    indentSize = value;
                }
            }

            /// <summary>
            ///     <para>Gets or sets a value indicating whether an indent is needed.</para>
            /// </summary>
            protected bool NeedIndent { get; set; } = true;

            /// <summary>
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
            }

            /// <summary>
            ///     <para>
            ///         When overridden in a derived class, closes the output stream
            ///         so that it no longer receives tracing or debugging output.
            ///     </para>
            /// </summary>
            public virtual void Close()
            {
            }

            /// <summary>
            ///     <para>When overridden in a derived class, flushes the output buffer.</para>
            /// </summary>
            public virtual void Flush()
            {
            }

            /// <summary>
            ///     <para>
            ///         When overridden in a derived class, writes the specified
            ///         message to the listener you specify in the derived class.
            ///     </para>
            /// </summary>
            public abstract void Write(string message);

            /// <summary>
            ///     <para>
            ///         Writes the name of the <paramref name="o" /> parameter to the listener you specify when you inherit from the
            ///         <see cref='System.Diagnostics.TraceListener' />
            ///         class.
            ///     </para>
            /// </summary>
            public virtual void Write(object o)
            {
                if (o == null) return;
                Write(o.ToString());
            }

            ///// <summary>
            /////     <para>
            /////         Writes a category name and a message to the listener you specify when you
            /////         inherit from the <see cref='System.Diagnostics.TraceListener' />
            /////         class.
            /////     </para>
            ///// </summary>
            //public virtual void Write(string message, string category)
            //{
            //    if (category == null) Write(message);
            //    else Write(category + ": " + (message == null ? string.Empty : message));
            //}

            ///// <summary>
            /////     <para>
            /////         Writes a category name and the name of the <paramref name="o" /> parameter to the listener you
            /////         specify when you inherit from the <see cref='System.Diagnostics.TraceListener' />
            /////         class.
            /////     </para>
            ///// </summary>
            //public virtual void Write(object o, string category)
            //{
            //    if (category == null) Write(o);
            //    else Write(o == null ? "" : o.ToString(), category);
            //}

            /// <summary>
            ///     <para>
            ///         Writes the indent to the listener you specify when you
            ///         inherit from the <see cref='System.Diagnostics.TraceListener' />
            ///         class, and resets the <see cref='TraceListener.NeedIndent' /> property to <see langword='false' />.
            ///     </para>
            /// </summary>
            protected virtual void WriteIndent()
            {
                NeedIndent = false;
                for (var i = 0; i < indentLevel; i++)
                    if (indentSize == 4) Write("    ");
                    else for (var j = 0; j < indentSize; j++) Write(" ");
            }

            /// <summary>
            ///     <para>
            ///         When overridden in a derived class, writes a message to the listener you specify in
            ///         the derived class, followed by a line terminator. The default line terminator is a carriage return followed
            ///         by a line feed (\r\n).
            ///     </para>
            /// </summary>
            public abstract void WriteLine(string message);

            /// <summary>
            ///     <para>
            ///         Writes the name of the <paramref name="o" /> parameter to the listener you specify when you inherit from the
            ///         <see cref='System.Diagnostics.TraceListener' /> class, followed by a line terminator. The default line
            ///         terminator is a
            ///         carriage return followed by a line feed
            ///         (\r\n).
            ///     </para>
            /// </summary>
            public virtual void WriteLine(object o)
            {
                WriteLine(o == null ? "" : o.ToString());
            }

            ///// <summary>
            /////     <para>
            /////         Writes a category name and a message to the listener you specify when you
            /////         inherit from the <see cref='System.Diagnostics.TraceListener' /> class,
            /////         followed by a line terminator. The default line terminator is a carriage return followed by a line feed (\r\n).
            /////     </para>
            ///// </summary>
            //public virtual void WriteLine(string message, string category)
            //{
            //    if (category == null) WriteLine(message);
            //    else WriteLine(category + ": " + (message == null ? string.Empty : message));
            //}

            ///// <summary>
            /////     <para>
            /////         Writes a category
            /////         name and the name of the <paramref name="o" />parameter to the listener you
            /////         specify when you inherit from the <see cref='System.Diagnostics.TraceListener' />
            /////         class, followed by a line terminator. The default line terminator is a carriage
            /////         return followed by a line feed (\r\n).
            /////     </para>
            ///// </summary>
            //public virtual void WriteLine(object o, string category)
            //{
            //    WriteLine(o == null ? "" : o.ToString(), category);
            //}
        }

        /// <summary>
        ///     <para>
        ///         Directs tracing or debugging output to
        ///         a <see cref='T:System.IO.TextWriter' /> or to a <see cref='T:System.IO.Stream' />,
        ///         such as <see cref='F:System.Console.Out' /> or <see cref='T:System.IO.FileStream' />.
        ///     </para>
        /// </summary>
        [HostProtection(Synchronization = true)]
        public class TextWriterTraceListener : TraceListener
        {
            private readonly string _carriageReturnAndLineFeedReplacement = ",";

            private readonly bool _replaceCarriageReturnsAndLineFeedsFromFieldValues = true;

            private string fileName;

            internal TextWriter writer;

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener' /> class with
            ///         <see cref='System.IO.TextWriter' />
            ///         as the output recipient.
            ///     </para>
            /// </summary>
            public TextWriterTraceListener()
            {
            }

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener' /> class, using the
            ///         stream as the recipient of the debugging and tracing output.
            ///     </para>
            /// </summary>
            public TextWriterTraceListener(Stream stream) : this(stream, string.Empty)
            {
            }

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener' /> class with the
            ///         specified name and using the stream as the recipient of the debugging and tracing output.
            ///     </para>
            /// </summary>
            public TextWriterTraceListener(Stream stream, string name) : base(name)
            {
                if (stream == null) throw new ArgumentNullException("stream");
                writer = new StreamWriter(stream);
            }

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener' /> class using the
            ///         specified writer as recipient of the tracing or debugging output.
            ///     </para>
            /// </summary>
            public TextWriterTraceListener(TextWriter writer) : this(writer, string.Empty)
            {
            }

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener' /> class with the
            ///         specified name and using the specified writer as recipient of the tracing or
            ///         debugging
            ///         output.
            ///     </para>
            /// </summary>
            public TextWriterTraceListener(TextWriter writer, string name) : base(name)
            {
                if (writer == null) throw new ArgumentNullException("writer");
                this.writer = writer;
            }

            /// <summary>
            ///     <para>[To be supplied.]</para>
            /// </summary>
            [ResourceExposure(ResourceScope.Machine)]
            public TextWriterTraceListener(string fileName)
            {
                this.fileName = fileName;
            }

            /// <summary>
            ///     <para>[To be supplied.]</para>
            /// </summary>
            [ResourceExposure(ResourceScope.Machine)]
            public TextWriterTraceListener(string fileName, string name) : base(name)
            {
                this.fileName = fileName;
            }

            /// <summary>
            ///     <para>
            ///         Indicates the text writer that receives the tracing
            ///         or debugging output.
            ///     </para>
            /// </summary>
            public TextWriter Writer
            {
                get
                {
                    EnsureWriter();
                    return writer;
                }

                set { writer = value; }
            }

            /// <summary>
            ///     <para>
            ///         Closes the <see cref='System.Diagnostics.TextWriterTraceListener.Writer' /> so that it no longer
            ///         receives tracing or debugging output.
            ///     </para>
            /// </summary>
            public override void Close()
            {
                if (writer != null)
                    try
                    {
                        writer.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }

                writer = null;
            }

            /// <internalonly />
            /// <summary>
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing)
                    {
                        Close();
                    }
                    else
                    {
                        // clean up resources
                        if (writer != null)
                            try
                            {
                                writer.Close();
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                        writer = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            /// <summary>
            ///     <para>Flushes the output buffer for the <see cref='System.Diagnostics.TextWriterTraceListener.Writer' />.</para>
            /// </summary>
            public override void Flush()
            {
                if (!EnsureWriter()) return;
                try
                {
                    writer.Flush();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            /// <summary>
            ///     <para>
            ///         Writes a message
            ///         to this instance's <see cref='System.Diagnostics.TextWriterTraceListener.Writer' />.
            ///     </para>
            /// </summary>
            public override void Write(string message)
            {
                if (!EnsureWriter()) return;
                if (NeedIndent) WriteIndent();
                try
                {
                    writer.Write(message);
                }
                catch (ObjectDisposedException)
                {
                }
            }

            /// <summary>
            ///     <para>
            ///         Writes a message
            ///         to this instance's <see cref='System.Diagnostics.TextWriterTraceListener.Writer' /> followed by a line
            ///         terminator. The
            ///         default line terminator is a carriage return followed by a line feed (\r\n).
            ///     </para>
            /// </summary>
            public override void WriteLine(string message)
            {
                if (!EnsureWriter()) return;
                if (NeedIndent) WriteIndent();
                try
                {
                    writer.WriteLine(message);
                    NeedIndent = true;
                }
                catch (ObjectDisposedException)
                {
                }
            }

            public virtual void WriteCsvLine(params string[] fields)
            {
                if (!EnsureWriter()) return;
                if (NeedIndent) WriteIndent();
                try
                {
                    WriteRecord(writer, fields);
                }
                catch (ObjectDisposedException)
                {
                }
            }

            private void WriteRecord(TextWriter writer, params string[] fields)
            {
                if (null == fields) return;
                for (var i = 0; i < fields.Length; i++)
                {
                    var quotesRequired = fields[i].Contains(",");
                    var escapeQuotes = fields[i].Contains("\"");
                    var fieldValue = escapeQuotes ? fields[i].Replace("\"", "\"\"") : fields[i];

                    if (_replaceCarriageReturnsAndLineFeedsFromFieldValues &&
                        (fieldValue.Contains("\r") || fieldValue.Contains("\n")))
                    {
                        quotesRequired = true;
                        fieldValue = fieldValue.Replace("\r\n",
                            _carriageReturnAndLineFeedReplacement);
                        fieldValue = fieldValue.Replace("\r", _carriageReturnAndLineFeedReplacement);
                        fieldValue = fieldValue.Replace("\n", _carriageReturnAndLineFeedReplacement);
                    }

                    writer.Write("{0}{1}{0}{2}",
                        quotesRequired || escapeQuotes ? "\"" : string.Empty, fieldValue,
                        i < fields.Length - 1 ? "," : string.Empty);
                }
                writer.WriteLine();
            }

            private static Encoding GetEncodingWithFallback(Encoding encoding)
            {
                // Clone it and set the "?" replacement fallback
                var fallbackEncoding = (Encoding)encoding.Clone();
                fallbackEncoding.EncoderFallback = EncoderFallback.ReplacementFallback;
                fallbackEncoding.DecoderFallback = DecoderFallback.ReplacementFallback;

                return fallbackEncoding;
            }

            // This uses a machine resource, scoped by the fileName variable. 
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            internal bool EnsureWriter()
            {
                var ret = true;

                if (writer == null)
                {
                    ret = false;

                    if (fileName == null) return ret;

                    // StreamWriter by default uses UTF8Encoding which will throw on invalid encoding errors.
                    // This can cause the internal StreamWriter's state to be irrecoverable. It is bad for tracing
                    // APIs to throw on encoding errors. Instead, we should provide a "?" replacement fallback
                    // encoding to substitute illegal chars. For ex, In case of high surrogate character 
                    // D800-DBFF without a following low surrogate character DC00-DFFF
                    // NOTE: We also need to use an encoding that does't emit BOM whic is StreamWriter's default 
                    var noBOMwithFallback = GetEncodingWithFallback(new UTF8Encoding(false));

                    // To support multiple appdomains/instances tracing to the same file,
                    // we will try to open the given file for append but if we encounter
                    // IO errors, we will prefix the file name with a unique GUID value
                    // and try one more time 
                    var fullPath = Path.GetFullPath(fileName);
                    var dirPath = Path.GetDirectoryName(fullPath);
                    var fileNameOnly = Path.GetFileName(fullPath);

                    for (var i = 0; i < 2; i++)
                        try
                        {
                            writer = new StreamWriter(fullPath, true, noBOMwithFallback, 4096);
                            ret = true;
                            break;
                        }
                        catch (IOException)
                        {
                            // Should we do this only for ERROR_SHARING_VIOLATION?
                            //if (InternalResources.MakeErrorCodeFromHR(Marshal.GetHRForException(ioexc)) == InternalResources.ERROR_SHARING_VIOLATION) { 

                            fileNameOnly = Guid.NewGuid() + fileNameOnly;
                            fullPath = Path.Combine(dirPath, fileNameOnly);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            //ERROR_ACCESS_DENIED, mostly ACL issues 
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }

                    if (!ret) fileName = null;
                }
                return ret;
            }
        }

        /// <summary>
        ///     Extends <see cref="TextWriterTraceListener" /> to add formatting capabilities.
        /// </summary>
        public class FormattedTextWriterTraceListener : TextWriterTraceListener
        {
            /// <summary>
            ///     Initializes a new instance of <see cref="FormattedTextWriterTraceListener" />.
            /// </summary>
            public FormattedTextWriterTraceListener()
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="FormattedTextWriterTraceListener" /> with a <see cref="Stream" />.
            /// </summary>
            /// <param name="stream">The stream to write to.</param>
            public FormattedTextWriterTraceListener(Stream stream) : base(stream)
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="FormattedTextWriterTraceListener" /> with a <see cref="TextWriter" />.
            /// </summary>
            /// <param name="writer">The writer to write to.</param>
            public FormattedTextWriterTraceListener(TextWriter writer) : base(writer)
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="FormattedTextWriterTraceListener" /> with a file name.
            /// </summary>
            /// <param name="fileName">The file name to write to.</param>
            public FormattedTextWriterTraceListener(string fileName)
                : base(EnvironmentHelper.ReplaceEnvironmentVariables(fileName))
            {
            }

            /// <summary>
            ///     Initializes a new named instance of <see cref="FormattedTextWriterTraceListener" /> with a <see cref="Stream" />.
            /// </summary>
            /// <param name="stream">The stream to write to.</param>
            /// <param name="name">The name.</param>
            public FormattedTextWriterTraceListener(Stream stream, string name) : base(stream, name)
            {
            }

            /// <summary>
            ///     Initializes a new named instance of <see cref="FormattedTextWriterTraceListener" /> with a
            ///     <see cref="TextWriter" />.
            /// </summary>
            /// <param name="writer">The writer to write to.</param>
            /// <param name="name">The name.</param>
            public FormattedTextWriterTraceListener(TextWriter writer, string name)
                : base(writer, name)
            {
            }

            /// <summary>
            ///     Initializes a new named instance of <see cref="FormattedTextWriterTraceListener" /> with a
            ///     <see cref="ILogFormatter" /> and a file name.
            /// </summary>
            /// <param name="fileName">The file name to write to.</param>
            /// <param name="name">The name.</param>
            public FormattedTextWriterTraceListener(string fileName, string name)
                : base(EnvironmentHelper.ReplaceEnvironmentVariables(fileName), name)
            {
            }

            public sealed override void Write(object o)
            {
                base.Write(o);
            }
        }

        /// <summary>
        ///     A <see cref="TraceListener" /> that writes to a flat file, formatting the output with an
        ///     <see cref="ILogFormatter" />.
        /// </summary>
        public class FlatFileTraceListener : FormattedTextWriterTraceListener
        {
            private readonly Func<string> footer;

            private readonly Func<string> header;

            /// <summary>
            ///     Initializes a new instance of <see cref="FlatFileTraceListener" />.
            /// </summary>
            public FlatFileTraceListener()
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="FlatFileTraceListener" /> with a <see cref="FileStream" />.
            /// </summary>
            /// <param name="stream">The file stream.</param>
            public FlatFileTraceListener(FileStream stream) : base(stream)
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="FlatFileTraceListener" /> with a <see cref="StreamWriter" />.
            /// </summary>
            /// <param name="writer">The stream writer.</param>
            public FlatFileTraceListener(StreamWriter writer) : base(writer)
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="FlatFileTraceListener" /> with a file name.
            /// </summary>
            /// <param name="fileName">The file name.</param>
            public FlatFileTraceListener(string fileName) : base(fileName)
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="FlatFileTraceListener" /> with a file name, a header, and a footer.
            /// </summary>
            /// <param name="fileName">The file stream.</param>
            /// <param name="header">The header.</param>
            /// <param name="footer">The footer.</param>
            public FlatFileTraceListener(string fileName, Func<string> header, Func<string> footer)
                : base(fileName)
            {
                this.header = header;
                this.footer = footer;
            }

            /// <summary>
            ///     Initializes a new name instance of <see cref="FlatFileTraceListener" /> with a <see cref="FileStream" />.
            /// </summary>
            /// <param name="stream">The file stream.</param>
            /// <param name="name">The name.</param>
            public FlatFileTraceListener(FileStream stream, string name) : base(stream, name)
            {
            }

            /// <summary>
            ///     Initializes a new named instance of <see cref="FlatFileTraceListener" /> with a <see cref="StreamWriter" />.
            /// </summary>
            /// <param name="writer">The stream writer.</param>
            /// <param name="name">The name.</param>
            public FlatFileTraceListener(StreamWriter writer, string name) : base(writer, name)
            {
            }

            /// <summary>
            ///     Initializes a new named instance of <see cref="FlatFileTraceListener" /> with a file name.
            /// </summary>
            /// <param name="fileName">The file name.</param>
            /// <param name="name">The name.</param>
            public FlatFileTraceListener(string fileName, string name) : base(fileName, name)
            {
            }

            public override void WriteLine(string message)
            {
                if (header != null) base.WriteLine(header());
                base.WriteLine(message);
                if (footer != null) base.WriteLine(footer());
            }
        }

        /// <summary>
        ///     Performs logging to a file and rolls the output file when either time or size thresholds are
        ///     exceeded.
        /// </summary>
        /// <remarks>
        ///     Logging always occurs to the configured file name, and when roll occurs a new rolled file name is calculated
        ///     by adding the timestamp pattern to the configured file name.
        ///     <para />
        ///     The need of rolling is calculated before performing a logging operation, so even if the thresholds are exceeded
        ///     roll will not occur until a new entry is logged.
        ///     <para />
        ///     Both time and size thresholds can be configured, and when the first of them occurs both will be reset.
        ///     <para />
        ///     The elapsed time is calculated from the creation date of the logging file.
        /// </remarks>
        public class RollingFlatFileTraceListener : FlatFileTraceListener
        {
            private readonly string archivedFolderPattern;

            private readonly int maxArchivedFiles;

            private readonly RollFileExistsBehavior rollFileExistsBehavior;

            private readonly RollInterval rollInterval;

            private readonly int rollSizeInBytes;

            private readonly string timeStampPattern;

            /// <summary>
            ///     Initializes a new instance of <see cref="RollingFlatFileTraceListener" />
            /// </summary>
            /// <param name="fileName">The filename where the entries will be logged.</param>
            /// <param name="header">The header to add before logging an entry.</param>
            /// <param name="footer">The footer to add after logging an entry.</param>
            /// <param name="formatter">The formatter.</param>
            /// <param name="rollSizeKB">The maxium file size (KB) before rolling.</param>
            /// <param name="timeStampPattern">The date format that will be appended to the new roll file.</param>
            /// <param name="archivedFolderPattern">The archived folder pattern.</param>
            /// <param name="rollFileExistsBehavior">Expected behavior that will be used when the roll file has to be created.</param>
            /// <param name="rollInterval">The time interval that makes the file rolles.</param>
            public RollingFlatFileTraceListener(
                string fileName, Func<string> header, Func<string> footer, int rollSizeKB,
                string timeStampPattern, string archivedFolderPattern,
                RollFileExistsBehavior rollFileExistsBehavior, RollInterval rollInterval)
                : this(
                    fileName, header, footer, rollSizeKB, timeStampPattern, archivedFolderPattern,
                    rollFileExistsBehavior, rollInterval, 0)
            {
            }

            /// <summary>
            ///     Initializes a new instance of <see cref="RollingFlatFileTraceListener" />
            /// </summary>
            /// <param name="fileName">The filename where the entries will be logged.</param>
            /// <param name="header">The header to add before logging an entry.</param>
            /// <param name="footer">The footer to add after logging an entry.</param>
            /// <param name="formatter">The formatter.</param>
            /// <param name="rollSizeKB">The maxium file size (KB) before rolling.</param>
            /// <param name="timeStampPattern">The date format that will be appended to the new roll file.</param>
            /// <param name="archivedFolderPattern">The archived folder pattern.</param>
            /// <param name="rollFileExistsBehavior">Expected behavior that will be used when the roll file has to be created.</param>
            /// <param name="rollInterval">The time interval that makes the file rolles.</param>
            /// <param name="maxArchivedFiles">The maximum number of archived files to keep.</param>
            public RollingFlatFileTraceListener(
                string fileName, Func<string> header, Func<string> footer, int rollSizeKB,
                string timeStampPattern, string archivedFolderPattern,
                RollFileExistsBehavior rollFileExistsBehavior, RollInterval rollInterval,
                int maxArchivedFiles) : base(fileName, header, footer)
            {
                this.archivedFolderPattern = archivedFolderPattern;
                rollSizeInBytes = rollSizeKB * 1024;
                this.timeStampPattern = timeStampPattern;
                this.rollFileExistsBehavior = rollFileExistsBehavior;
                this.rollInterval = rollInterval;
                this.maxArchivedFiles = maxArchivedFiles;

                RollingHelper = new StreamWriterRollingHelper(this);
            }

            /// <summary>
            ///     Gets the <see cref="StreamWriterRollingHelper" /> for the flat file.
            /// </summary>
            /// <value>
            ///     The <see cref="StreamWriterRollingHelper" /> for the flat file.
            /// </value>
            public StreamWriterRollingHelper RollingHelper { get; }

            public override void WriteLine(object o)
            {
                RollingHelper.RollIfNecessary();
                base.WriteLine(o);
            }

            public override void Write(string message)
            {
                RollingHelper.RollIfNecessary();
                base.Write(message);
            }

            public override void WriteCsvLine(params string[] fields)
            {
                RollingHelper.RollIfNecessary();
                base.WriteCsvLine(fields);
            }

            /// <summary>
            ///     A data time provider.
            /// </summary>
            public class DateTimeProvider
            {
                /// <summary>
                ///     Gets the current data time.
                /// </summary>
                /// <value>
                ///     The current data time.
                /// </value>
                public virtual DateTimeOffset CurrentDateTime
                {
                    get { return DateTimeOffset.UtcNow; }
                }
            }

            /// <summary>
            ///     Encapsulates the logic to perform rolls.
            /// </summary>
            /// <remarks>
            ///     If no rolling behavior has been configured no further processing will be performed.
            /// </remarks>
            public sealed class StreamWriterRollingHelper
            {
                /// <summary>
                ///     The trace listener for which rolling is being managed.
                /// </summary>
                private readonly RollingFlatFileTraceListener owner;

                /// <summary>
                ///     A flag indicating whether at least one rolling criteria has been configured.
                /// </summary>
                private readonly bool performsRolling;

                private DateTimeProvider dateTimeProvider;

                /// <summary>
                ///     A tally keeping writer used when file size rolling is configured.
                ///     <para />
                ///     The original stream writer from the base trace listener will be replaced with
                ///     this listener.
                /// </summary>
                private TallyKeepingFileStreamWriter managedWriter;

                /// <summary>
                ///     Initialize a new instance of the <see cref="StreamWriterRollingHelper" /> class with a
                ///     <see cref="RollingFlatFileTraceListener" />.
                /// </summary>
                /// <param name="owner">The <see cref="RollingFlatFileTraceListener" /> to use.</param>
                public StreamWriterRollingHelper(RollingFlatFileTraceListener owner)
                {
                    this.owner = owner;
                    dateTimeProvider = new DateTimeProvider();

                    performsRolling = this.owner.rollInterval != RollInterval.None ||
                                      this.owner.rollSizeInBytes > 0;
                }

                /// <summary>
                ///     Gets the provider for the current date. Necessary for unit testing.
                /// </summary>
                /// <value>
                ///     The provider for the current date. Necessary for unit testing.
                /// </value>
                public DateTimeProvider DateTimeProvider
                {
                    set { dateTimeProvider = value; }
                }

                /// <summary>
                ///     Gets the next date when date based rolling should occur if configured.
                /// </summary>
                /// <value>
                ///     The next date when date based rolling should occur if configured.
                /// </value>
                public DateTimeOffset? NextRollDateTime { get; private set; }

                /// <summary>
                ///     Calculates the next roll date for the file.
                /// </summary>
                /// <param name="dateTime">The new date.</param>
                /// <returns>The new date time to use.</returns>
                public DateTimeOffset CalculateNextRollDate(DateTimeOffset dateTime)
                {
                    switch (owner.rollInterval)
                    {
                        case RollInterval.Minute:
                            return dateTime.AddMinutes(1);
                        case RollInterval.Hour:
                            return dateTime.AddHours(1);
                        case RollInterval.Day:
                            return dateTime.AddDays(1);
                        case RollInterval.Week:
                            return dateTime.AddDays(7);
                        case RollInterval.Month:
                            return dateTime.AddMonths(1);
                        case RollInterval.Year:
                            return dateTime.AddYears(1);
                        case RollInterval.Midnight:
                            return dateTime.AddDays(1).Date;
                        default:
                            return DateTimeOffset.MaxValue;
                    }
                }

                /// <summary>
                ///     Checks whether rolling should be performed, and returns the date to use when performing the roll.
                /// </summary>
                /// <returns>The date roll to use if performing a roll, or <see langword="null" /> if no rolling should occur.</returns>
                /// <remarks>
                ///     Defer request for the roll date until it is necessary to avoid overhead.
                ///     <para />
                ///     Information used for rolling checks should be set by now.
                /// </remarks>
                public DateTimeOffset? CheckIsRollNecessary()
                {
                    // check for size roll, if enabled.
                    if (owner.rollSizeInBytes > 0 && managedWriter != null &&
                        managedWriter.Tally > owner.rollSizeInBytes)
                        return dateTimeProvider.CurrentDateTime;

                    // check for date roll, if enabled.
                    var currentDateTime = dateTimeProvider.CurrentDateTime;
                    if (owner.rollInterval != RollInterval.None && NextRollDateTime != null &&
                        currentDateTime.CompareTo(NextRollDateTime.Value) >= 0)
                        return currentDateTime;

                    // no roll is necessary, return a null roll date
                    return null;
                }

                /// <summary>
                ///     Gets the file name to use for archiving the file.
                /// </summary>
                /// <param name="actualFileName">The actual file name.</param>
                /// <param name="currentDateTime">The current date and time.</param>
                /// <returns>The new file name.</returns>
                public string ComputeArchiveFileName(
                    string actualFileName, DateTimeOffset currentDateTime)
                {
                    var directory = Path.GetDirectoryName(actualFileName);
                    if (!string.IsNullOrWhiteSpace(owner.archivedFolderPattern))
                    {
                        var rollingDirectory = Path.Combine(directory,
                            DateTimeOffset.UtcNow.ToString(owner.archivedFolderPattern));
                        try
                        {
                            if (!Directory.Exists(rollingDirectory))
                                Directory.CreateDirectory(rollingDirectory);
                            directory = rollingDirectory;
                        }
                        catch (Exception)
                        {
                            //Debug.WriteLine(ex);
                        }
                    }
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(actualFileName);
                    var extension = Path.GetExtension(actualFileName);

                    var fileNameBuilder = new StringBuilder(fileNameWithoutExtension);
                    if (!string.IsNullOrEmpty(owner.timeStampPattern))
                    {
                        fileNameBuilder.Append('.');
                        fileNameBuilder.Append(currentDateTime.ToString(owner.timeStampPattern,
                            CultureInfo.InvariantCulture));
                    }

                    if (owner.rollFileExistsBehavior == RollFileExistsBehavior.Increment)
                    {
                        // look for max sequence for date
                        var newSequence =
                            FindMaxSequenceNumber(directory, fileNameBuilder.ToString(), extension) +
                            1;
                        fileNameBuilder.Append('.');
                        fileNameBuilder.Append(newSequence.ToString(CultureInfo.InvariantCulture));
                    }

                    fileNameBuilder.Append(extension);

                    return Path.Combine(directory, fileNameBuilder.ToString());
                }

                /// <summary>
                ///     Finds the max sequence number for a log file.
                /// </summary>
                /// <param name="directoryName">The directory to scan.</param>
                /// <param name="fileName">The file name.</param>
                /// <param name="extension">The extension to use.</param>
                /// <returns>The next sequence number.</returns>
                public static int FindMaxSequenceNumber(
                    string directoryName, string fileName, string extension)
                {
                    var existingFiles = Directory.GetFiles(directoryName,
                        string.Format("{0}*{1}", fileName, extension));

                    var maxSequence = 0;
                    var regex =
                        new Regex(string.Format(@"{0}\.(?<sequence>\d+){1}$", fileName, extension));
                    for (var i = 0; i < existingFiles.Length; i++)
                    {
                        var sequenceMatch = regex.Match(existingFiles[i]);
                        if (sequenceMatch.Success)
                        {
                            var currentSequence = 0;

                            var sequenceInFile = sequenceMatch.Groups["sequence"].Value;
                            if (!int.TryParse(sequenceInFile, out currentSequence))
                                continue; // very unlikely

                            if (currentSequence > maxSequence) maxSequence = currentSequence;
                        }
                    }

                    return maxSequence;
                }

                private static Encoding GetEncodingWithFallback()
                {
                    var encoding = (Encoding)new UTF8Encoding(false).Clone();
                    encoding.EncoderFallback = EncoderFallback.ReplacementFallback;
                    encoding.DecoderFallback = DecoderFallback.ReplacementFallback;
                    return encoding;
                }

                /// <summary>
                ///     Perform the roll for the next date.
                /// </summary>
                /// <param name="rollDateTime">The roll date.</param>
                public void PerformRoll(DateTimeOffset rollDateTime)
                {
                    var actualFileName =
                        ((FileStream)((StreamWriter)owner.Writer).BaseStream).Name;

                    if (owner.rollFileExistsBehavior == RollFileExistsBehavior.Overwrite &&
                        string.IsNullOrEmpty(owner.timeStampPattern))
                    {
                        // no roll will be actually performed: no timestamp pattern is available, and 
                        // the roll behavior is overwrite, so the original file will be truncated
                        owner.Writer.Close();
                        File.WriteAllText(actualFileName, string.Empty);
                    }
                    else
                    {
                        // calculate archive name
                        var archiveFileName = ComputeArchiveFileName(actualFileName, rollDateTime);
                        // close file
                        owner.Writer.Close();
                        // move file
                        SafeMove(actualFileName, archiveFileName, rollDateTime);
                        // purge if necessary
                        PurgeArchivedFiles(archiveFileName);
                    }

                    // update writer - let TWTL open the file as needed to keep consistency
                    owner.Writer = null;
                    managedWriter = null;
                    NextRollDateTime = null;
                    UpdateRollingInformationIfNecessary();
                }

                /// <summary>
                ///     Rolls the file if necessary.
                /// </summary>
                public void RollIfNecessary()
                {
                    if (!performsRolling) return;

                    if (!UpdateRollingInformationIfNecessary()) return;

                    DateTimeOffset? rollDateTime;
                    if ((rollDateTime = CheckIsRollNecessary()) != null)
                        PerformRoll(rollDateTime.Value);
                }

                private void SafeMove(
                    string actualFileName, string archiveFileName, DateTimeOffset currentDateTime)
                {
                    try
                    {
                        if (File.Exists(archiveFileName)) File.Delete(archiveFileName);
                        // take care of tunneling issues http://support.microsoft.com/kb/172190
                        File.SetCreationTime(actualFileName, currentDateTime.UtcDateTime);
                        File.Move(actualFileName, archiveFileName);
                    }
                    catch (IOException)
                    {
                        // catch errors and attempt move to a new file with a GUID
                        archiveFileName = archiveFileName + Guid.NewGuid();

                        try
                        {
                            File.Move(actualFileName, archiveFileName);
                        }
                        catch (IOException)
                        {
                        }
                    }
                }

                private void PurgeArchivedFiles(string archiveFileName)
                {
                    if (owner.maxArchivedFiles > 0)
                    {
                        var directoryName = Path.GetDirectoryName(archiveFileName);
                        var fileName = Path.GetFileName(archiveFileName);

                        new RollingFlatFilePurger(directoryName, fileName, owner.maxArchivedFiles)
                            .Purge();
                    }
                }

                /// <summary>
                ///     Updates bookeeping information necessary for rolling, as required by the specified
                ///     rolling configuration.
                /// </summary>
                /// <returns>true if update was successful, false if an error occurred.</returns>
                public bool UpdateRollingInformationIfNecessary()
                {
                    StreamWriter currentWriter = null;

                    // replace writer with the tally keeping version if necessary for size rolling
                    if (owner.rollSizeInBytes > 0 && managedWriter == null)
                    {
                        currentWriter = owner.Writer as StreamWriter;
                        if (currentWriter == null) return false;
                        var actualFileName = ((FileStream)currentWriter.BaseStream).Name;

                        currentWriter.Close();

                        FileStream fileStream = null;
                        try
                        {
                            fileStream = File.Open(actualFileName, FileMode.Append, FileAccess.Write,
                                FileShare.Read);
                            managedWriter = new TallyKeepingFileStreamWriter(fileStream,
                                GetEncodingWithFallback());
                        }
                        catch (Exception)
                        {
                            // there's a slight chance of error here - abort if this occurs and just let TWTL handle it without attempting to roll
                            return false;
                        }

                        owner.Writer = managedWriter;
                    }

                    // compute the next roll date if necessary
                    if (owner.rollInterval != RollInterval.None && NextRollDateTime == null)
                        try
                        {
                            // casting should be safe at this point - only file stream writers can be the writers for the owner trace listener.
                            // it should also happen rarely
                            NextRollDateTime =
                                CalculateNextRollDate(
                                    File.GetCreationTime(
                                        ((FileStream)((StreamWriter)owner.Writer).BaseStream).Name));
                        }
                        catch (Exception)
                        {
                            NextRollDateTime = DateTimeOffset.MaxValue;
                            // disable rolling if not date could be retrieved.

                            // there's a slight chance of error here - abort if this occurs and just let TWTL handle it without attempting to roll
                            return false;
                        }

                    return true;
                }
            }

            /// <summary>
            ///     Represents a file stream writer that keeps a tally of the length of the file.
            /// </summary>
            public sealed class TallyKeepingFileStreamWriter : StreamWriter
            {
                /// <summary>
                ///     Initialize a new instance of the <see cref="TallyKeepingFileStreamWriter" /> class with a <see cref="FileStream" />
                ///     .
                /// </summary>
                /// <param name="stream">The <see cref="FileStream" /> to write to.</param>
                public TallyKeepingFileStreamWriter(FileStream stream) : base(stream)
                {
                    Tally = stream.Length;
                }

                /// <summary>
                ///     Initialize a new instance of the <see cref="TallyKeepingFileStreamWriter" /> class with a <see cref="FileStream" />
                ///     .
                /// </summary>
                /// <param name="stream">The <see cref="FileStream" /> to write to.</param>
                /// <param name="encoding">The <see cref="Encoding" /> to use.</param>
                public TallyKeepingFileStreamWriter(FileStream stream, Encoding encoding)
                    : base(stream, encoding)
                {
                    Tally = stream.Length;
                }

                /// <summary>
                ///     Gets the tally of the length of the string.
                /// </summary>
                /// <value>
                ///     The tally of the length of the string.
                /// </value>
                public long Tally { get; private set; }

                /// <summary>
                ///     Writes a character to the stream.
                /// </summary>
                /// <param name="value">The character to write to the text stream. </param>
                /// <exception cref="T:System.ObjectDisposedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed.
                /// </exception>
                /// <exception cref="T:System.NotSupportedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the
                ///     underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream.
                /// </exception>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <filterpriority>1</filterpriority>
                public override void Write(char value)
                {
                    base.Write(value);
                    Tally += Encoding.GetByteCount(new[] { value });
                }

                /// <summary>
                ///     Writes a character array to the stream.
                /// </summary>
                /// <param name="buffer">A character array containing the data to write. If buffer is null, nothing is written. </param>
                /// <exception cref="T:System.ObjectDisposedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed.
                /// </exception>
                /// <exception cref="T:System.NotSupportedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the
                ///     underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream.
                /// </exception>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <filterpriority>1</filterpriority>
                public override void Write(char[] buffer)
                {
                    base.Write(buffer);
                    Tally += Encoding.GetByteCount(buffer);
                }

                /// <summary>
                ///     Writes a subarray of characters to the stream.
                /// </summary>
                /// <param name="count">The number of characters to read from buffer. </param>
                /// <param name="buffer">A character array containing the data to write. </param>
                /// <param name="index">The index into buffer at which to begin writing. </param>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <exception cref="T:System.ObjectDisposedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed.
                /// </exception>
                /// <exception cref="T:System.NotSupportedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the
                ///     underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream.
                /// </exception>
                /// <exception cref="T:System.ArgumentOutOfRangeException">index or count is negative. </exception>
                /// <exception cref="T:System.ArgumentException">The buffer length minus index is less than count. </exception>
                /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
                /// <filterpriority>1</filterpriority>
                public override void Write(char[] buffer, int index, int count)
                {
                    base.Write(buffer, index, count);
                    Tally += Encoding.GetByteCount(buffer, index, count);
                }

                /// <summary>
                ///     Writes a string to the stream.
                /// </summary>
                /// <param name="value">The string to write to the stream. If value is null, nothing is written. </param>
                /// <exception cref="T:System.ObjectDisposedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed.
                /// </exception>
                /// <exception cref="T:System.NotSupportedException">
                ///     <see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the
                ///     <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the
                ///     underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream.
                /// </exception>
                /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
                /// <filterpriority>1</filterpriority>
                public override void Write(string value)
                {
                    base.Write(value);
                    Tally += Encoding.GetByteCount(value);
                }
            }
        }

        /// <summary>
        ///     Provides tracing services through a set of <see cref="TraceListener" />s.
        /// </summary>
        public class LogSource : IDisposable
        {
            /// <summary>
            ///     Default Auto Flush property for the LogSource instance.
            /// </summary>
            public const bool DefaultAutoFlushProperty = true;

            /// <summary>
            ///     Initializes a new instance of the <see cref="LogSource" /> class with a name, a collection of
            ///     <see cref="TraceListener" />s, a level and the auto flush.
            /// </summary>
            /// <param name="name">The name for the instance.</param>
            /// <param name="traceListeners">The collection of <see cref="TraceListener" />s.</param>
            /// <param name="level">The <see cref="SourceLevels" /> value.</param>
            /// <param name="autoFlush">If Flush should be called on the Listeners after every write.</param>
            public LogSource(TraceListener[] traceListeners, bool autoFlush)
            {
                Listeners = traceListeners;

                AutoFlush = autoFlush;
            }

            /// <summary>
            ///     Gets the collection of trace listeners for the <see cref="LogSource" /> instance.
            /// </summary>
            /// <value>The listeners.</value>
            public TraceListener[] Listeners { get; }

            /// <summary>
            ///     Gets or sets the <see cref="AutoFlush" /> values for the <see cref="LogSource" /> instance.
            /// </summary>
            public bool AutoFlush { get; set; } = DefaultAutoFlushProperty;

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

           

            public void WriteLine(object message)
            {
                foreach (var item in Listeners)
                {
                    var listener = item;
                    try
                    {
                        if (!listener.IsThreadSafe) Monitor.Enter(listener);
                        listener.WriteLine(message);
                        if (AutoFlush) listener.Flush();
                    }
                    finally
                    {
                        if (!listener.IsThreadSafe) Monitor.Exit(listener);
                    }
                }
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <param name="disposing">
            ///     <see langword="true" /> if the method is being called from the <see cref="Dispose()" /> method.
            ///     <see langword="false" /> if it is being called from within the object finalizer.
            /// </param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing) foreach (var listener in Listeners) listener.Dispose();
            }

            /// <summary>
            ///     Releases resources for the <see cref="LogSource" /> instance before garbage collection.
            /// </summary>
            ~LogSource()
            {
                Dispose(false);
            }
        }

        /// <summary>
        ///     Class to write data to a csv file
        /// </summary>
        public sealed class CsvWriter : IDisposable
        {
            #region Members

            private StreamWriter _streamWriter;

            #endregion Members

            #region Properties

            /// <summary>
            ///     Gets or sets whether carriage returns and line feeds should be removed from
            ///     field values, the default is true
            /// </summary>
            public bool ReplaceCarriageReturnsAndLineFeedsFromFieldValues { get; set; } = true;

            /// <summary>
            ///     Gets or sets what the carriage return and line feed replacement characters should be
            /// </summary>
            public string CarriageReturnAndLineFeedReplacement { get; set; } = ",";

            #endregion Properties

            #region Methods

            #region CsvFile write methods

            /// <summary>
            ///     Writes csv content to a file
            /// </summary>
            /// <param name="csvFile">CsvFile</param>
            /// <param name="filePath">File path</param>
            public void WriteCsv(CsvFile csvFile, string filePath)
            {
                WriteCsv(csvFile, filePath, null);
            }

            /// <summary>
            ///     Writes csv content to a file
            /// </summary>
            /// <param name="csvFile">CsvFile</param>
            /// <param name="filePath">File path</param>
            /// <param name="encoding">Encoding</param>
            public void WriteCsv(CsvFile csvFile, string filePath, Encoding encoding)
            {
                if (File.Exists(filePath)) File.Delete(filePath);

                using (var writer = new StreamWriter(filePath, false, encoding ?? Encoding.Default))
                {
                    WriteToStream(csvFile, writer);
                    writer.Flush();
                    writer.Close();
                }
            }

            /// <summary>
            ///     Writes csv content to a stream
            /// </summary>
            /// <param name="csvFile">CsvFile</param>
            /// <param name="stream">Stream</param>
            public void WriteCsv(CsvFile csvFile, Stream stream)
            {
                WriteCsv(csvFile, stream, null);
            }

            /// <summary>
            ///     Writes csv content to a stream
            /// </summary>
            /// <param name="csvFile">CsvFile</param>
            /// <param name="stream">Stream</param>
            /// <param name="encoding">Encoding</param>
            public void WriteCsv(CsvFile csvFile, Stream stream, Encoding encoding)
            {
                stream.Position = 0;
                _streamWriter = new StreamWriter(stream, encoding ?? Encoding.Default);
                WriteToStream(csvFile, _streamWriter);
                _streamWriter.Flush();
                stream.Position = 0;
            }

            /// <summary>
            ///     Writes csv content to a string
            /// </summary>
            /// <param name="csvFile">CsvFile</param>
            /// <param name="encoding">Encoding</param>
            /// <returns>Csv content in a string</returns>
            public string WriteCsv(CsvFile csvFile, Encoding encoding)
            {
                var content = string.Empty;

                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(memoryStream, encoding ?? Encoding.Default)
                    )
                    {
                        WriteToStream(csvFile, writer);
                        writer.Flush();
                        memoryStream.Position = 0;

                        using (
                            var reader = new StreamReader(memoryStream, encoding ?? Encoding.Default)
                        )
                        {
                            content = reader.ReadToEnd();
                            writer.Close();
                            reader.Close();
                            memoryStream.Close();
                        }
                    }
                }

                return content;
            }

            #endregion CsvFile write methods

            #region DataTable write methods

            /// <summary>
            ///     Writes a DataTable to a file
            /// </summary>
            /// <param name="dataTable">DataTable</param>
            /// <param name="filePath">File path</param>
            public void WriteCsv(DataTable dataTable, string filePath)
            {
                WriteCsv(dataTable, filePath, null);
            }

            /// <summary>
            ///     Writes a DataTable to a file
            /// </summary>
            /// <param name="dataTable">DataTable</param>
            /// <param name="filePath">File path</param>
            /// <param name="encoding">Encoding</param>
            public void WriteCsv(DataTable dataTable, string filePath, Encoding encoding)
            {
                if (File.Exists(filePath)) File.Delete(filePath);

                using (var writer = new StreamWriter(filePath, false, encoding ?? Encoding.Default))
                {
                    WriteToStream(dataTable, writer);
                    writer.Flush();
                    writer.Close();
                }
            }

            /// <summary>
            ///     Writes a DataTable to a stream
            /// </summary>
            /// <param name="dataTable">DataTable</param>
            /// <param name="stream">Stream</param>
            public void WriteCsv(DataTable dataTable, Stream stream)
            {
                WriteCsv(dataTable, stream, null);
            }

            /// <summary>
            ///     Writes a DataTable to a stream
            /// </summary>
            /// <param name="dataTable">DataTable</param>
            /// <param name="stream">Stream</param>
            /// <param name="encoding">Encoding</param>
            public void WriteCsv(DataTable dataTable, Stream stream, Encoding encoding)
            {
                stream.Position = 0;
                _streamWriter = new StreamWriter(stream, encoding ?? Encoding.Default);
                WriteToStream(dataTable, _streamWriter);
                _streamWriter.Flush();
                stream.Position = 0;
            }

            /// <summary>
            ///     Writes the DataTable to a string
            /// </summary>
            /// <param name="dataTable">DataTable</param>
            /// <param name="encoding">Encoding</param>
            /// <returns>Csv content in a string</returns>
            public string WriteCsv(DataTable dataTable, Encoding encoding)
            {
                var content = string.Empty;

                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(memoryStream, encoding ?? Encoding.Default)
                    )
                    {
                        WriteToStream(dataTable, writer);
                        writer.Flush();
                        memoryStream.Position = 0;

                        using (
                            var reader = new StreamReader(memoryStream, encoding ?? Encoding.Default)
                        )
                        {
                            content = reader.ReadToEnd();
                            writer.Close();
                            reader.Close();
                            memoryStream.Close();
                        }
                    }
                }

                return content;
            }

            #endregion DataTable write methods

            /// <summary>
            ///     Writes the Csv File
            /// </summary>
            /// <param name="csvFile">CsvFile</param>
            /// <param name="writer">TextWriter</param>
            private void WriteToStream(CsvFile csvFile, TextWriter writer)
            {
                if (csvFile.Headers.Count > 0) WriteRecord(csvFile.Headers, writer);

                csvFile.Records.ForEach(record => WriteRecord(record.Fields, writer));
            }

            /// <summary>
            ///     Writes the Csv File
            /// </summary>
            /// <param name="dataTable">DataTable</param>
            /// <param name="writer">TextWriter</param>
            private void WriteToStream(DataTable dataTable, TextWriter writer)
            {
                var fields =
                    (from DataColumn column in dataTable.Columns select column.ColumnName).ToList();
                WriteRecord(fields, writer);

                foreach (DataRow row in dataTable.Rows)
                {
                    fields.Clear();
                    fields.AddRange(row.ItemArray.Select(o => o.ToString()));
                    WriteRecord(fields, writer);
                }
            }

            /// <summary>
            ///     Writes the record to the underlying stream
            /// </summary>
            /// <param name="fields">Fields</param>
            /// <param name="writer">TextWriter</param>
            private void WriteRecord(IList<string> fields, TextWriter writer)
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    var quotesRequired = fields[i].Contains(",");
                    var escapeQuotes = fields[i].Contains("\"");
                    var fieldValue = escapeQuotes ? fields[i].Replace("\"", "\"\"") : fields[i];

                    if (ReplaceCarriageReturnsAndLineFeedsFromFieldValues &&
                        (fieldValue.Contains("\r") || fieldValue.Contains("\n")))
                    {
                        quotesRequired = true;
                        fieldValue = fieldValue.Replace("\r\n", CarriageReturnAndLineFeedReplacement);
                        fieldValue = fieldValue.Replace("\r", CarriageReturnAndLineFeedReplacement);
                        fieldValue = fieldValue.Replace("\n", CarriageReturnAndLineFeedReplacement);
                    }

                    writer.Write("{0}{1}{0}{2}",
                        quotesRequired || escapeQuotes ? "\"" : string.Empty, fieldValue,
                        i < fields.Count - 1 ? "," : string.Empty);
                }

                writer.WriteLine();
            }

            /// <summary>
            ///     Disposes of all unmanaged resources
            /// </summary>
            public void Dispose()
            {
                if (_streamWriter == null) return;

                _streamWriter.Close();
                _streamWriter.Dispose();
            }

            #endregion Methods
        }

        /// <summary>
        ///     Class to read csv content from various sources
        /// </summary>
        public sealed class CsvReader : IDisposable
        {
            #region Enums

            /// <summary>
            ///     Type enum
            /// </summary>
            private enum Type
            {
                File,

                Stream,

                String
            }

            #endregion Enums

            #region Members

            private FileStream _fileStream;

            private Stream _stream;

            private StreamReader _streamReader;

            private StreamWriter _streamWriter;

            private Stream _memoryStream;

            private Encoding _encoding;

            private readonly StringBuilder _columnBuilder = new StringBuilder(100);

            private readonly Type _type = Type.File;

            #endregion Members

            #region Properties

            /// <summary>
            ///     Gets or sets whether column values should be trimmed
            /// </summary>
            public bool TrimColumns { get; set; }

            /// <summary>
            ///     Gets or sets whether the csv file has a header row
            /// </summary>
            public bool HasHeaderRow { get; set; }

            /// <summary>
            ///     Returns a collection of fields or null if no record has been read
            /// </summary>
            public List<string> Fields { get; private set; }

            /// <summary>
            ///     Gets the field count or returns null if no fields have been read
            /// </summary>
            public int? FieldCount
            {
                get { return Fields != null ? Fields.Count : (int?)null; }
            }

            #endregion Properties

            #region Constructors

            /// <summary>
            ///     Initialises the reader to work from a file
            /// </summary>
            /// <param name="filePath">File path</param>
            public CsvReader(string filePath)
            {
                _type = Type.File;
                Initialise(filePath, Encoding.Default);
            }

            /// <summary>
            ///     Initialises the reader to work from a file
            /// </summary>
            /// <param name="filePath">File path</param>
            /// <param name="encoding">Encoding</param>
            public CsvReader(string filePath, Encoding encoding)
            {
                _type = Type.File;
                Initialise(filePath, encoding);
            }

            /// <summary>
            ///     Initialises the reader to work from an existing stream
            /// </summary>
            /// <param name="stream">Stream</param>
            public CsvReader(Stream stream)
            {
                _type = Type.Stream;
                Initialise(stream, Encoding.Default);
            }

            /// <summary>
            ///     Initialises the reader to work from an existing stream
            /// </summary>
            /// <param name="stream">Stream</param>
            /// <param name="encoding">Encoding</param>
            public CsvReader(Stream stream, Encoding encoding)
            {
                _type = Type.Stream;
                Initialise(stream, encoding);
            }

            /// <summary>
            ///     Initialises the reader to work from a csv string
            /// </summary>
            /// <param name="encoding"></param>
            /// <param name="csvContent"></param>
            public CsvReader(Encoding encoding, string csvContent)
            {
                _type = Type.String;
                Initialise(encoding, csvContent);
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            ///     Initialises the class to use a file
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="encoding"></param>
            private void Initialise(string filePath, Encoding encoding)
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException(string.Format("The file '{0}' does not exist.",
                        filePath));

                _fileStream = File.OpenRead(filePath);
                Initialise(_fileStream, encoding);
            }

            /// <summary>
            ///     Initialises the class to use a stream
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="encoding"></param>
            private void Initialise(Stream stream, Encoding encoding)
            {
                if (stream == null) throw new ArgumentNullException("The supplied stream is null.");

                _stream = stream;
                _stream.Position = 0;
                _encoding = encoding ?? Encoding.Default;
                _streamReader = new StreamReader(_stream, _encoding);
            }

            /// <summary>
            ///     Initialies the class to use a string
            /// </summary>
            /// <param name="encoding"></param>
            /// <param name="csvContent"></param>
            private void Initialise(Encoding encoding, string csvContent)
            {
                if (csvContent == null)
                    throw new ArgumentNullException("The supplied csvContent is null.");

                _encoding = encoding ?? Encoding.Default;

                _memoryStream = new MemoryStream(csvContent.Length);
                _streamWriter = new StreamWriter(_memoryStream);
                _streamWriter.Write(csvContent);
                _streamWriter.Flush();
                Initialise(_memoryStream, encoding);
            }

            /// <summary>
            ///     Reads the next record
            /// </summary>
            /// <returns>True if a record was successfuly read, otherwise false</returns>
            public bool ReadNextRecord()
            {
                Fields = null;
                var line = _streamReader.ReadLine();

                if (line == null) return false;

                ParseLine(line);
                return true;
            }

            /// <summary>
            ///     Reads a csv file format into a data table.  This method
            ///     will always assume that the table has a header row as this will be used
            ///     to determine the columns.
            /// </summary>
            /// <returns></returns>
            public DataTable ReadIntoDataTable()
            {
                return ReadIntoDataTable(new System.Type[] { });
            }

            /// <summary>
            ///     Reads a csv file format into a data table.  This method
            ///     will always assume that the table has a header row as this will be used
            ///     to determine the columns.
            /// </summary>
            /// <param name="columnTypes">Array of column types</param>
            /// <returns></returns>
            public DataTable ReadIntoDataTable(System.Type[] columnTypes)
            {
                var dataTable = new DataTable();
                var addedHeader = false;
                _stream.Position = 0;

                while (ReadNextRecord())
                {
                    if (!addedHeader)
                    {
                        for (var i = 0; i < Fields.Count; i++)
                            dataTable.Columns.Add(Fields[i],
                                columnTypes.Length > 0 ? columnTypes[i] : typeof(string));

                        addedHeader = true;
                        continue;
                    }

                    var row = dataTable.NewRow();

                    for (var i = 0; i < Fields.Count; i++) row[i] = Fields[i];

                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }

            /// <summary>
            ///     Parses a csv line
            /// </summary>
            /// <param name="line">Line</param>
            private void ParseLine(string line)
            {
                Fields = new List<string>();
                var inColumn = false;
                var inQuotes = false;
                //_columnBuilder.Remove(0, _columnBuilder.Length);
                _columnBuilder.Length = 0;
                // Iterate through every character in the line
                for (var i = 0; i < line.Length; i++)
                {
                    var character = line[i];

                    // If we are not currently inside a column
                    if (!inColumn)
                    {
                        // If the current character is a double quote then the column value is contained within
                        // double quotes, otherwise append the next character
                        if (character == '"') inQuotes = true;
                        else _columnBuilder.Append(character);

                        inColumn = true;
                        continue;
                    }

                    // If we are in between double quotes
                    if (inQuotes)
                    {
                        // If the current character is a double quote and the next character is a comma or we are at the end of the line
                        // we are now no longer within the column.
                        // Otherwise increment the loop counter as we are looking at an escaped double quote e.g. "" within a column
                        if (character == '"' &&
                            (line.Length > i + 1 && line[i + 1] == ',' || i + 1 == line.Length))
                        {
                            inQuotes = false;
                            inColumn = false;
                            i++;
                        }
                        else if (character == '"' && line.Length > i + 1 && line[i + 1] == '"')
                        {
                            i++;
                        }
                    }
                    else if (character == ',')
                    {
                        inColumn = false;
                    }

                    // If we are no longer in the column clear the builder and add the columns to the list
                    if (!inColumn)
                    {
                        Fields.Add(TrimColumns
                            ? _columnBuilder.ToString().Trim()
                            : _columnBuilder.ToString());
                        //_columnBuilder.Remove(0, _columnBuilder.Length);
                        _columnBuilder.Length = 0;
                    }
                    else // append the current column
                    {
                        _columnBuilder.Append(character);
                    }
                }

                // If we are still inside a column add a new one
                if (inColumn)
                    Fields.Add(TrimColumns
                        ? _columnBuilder.ToString().Trim()
                        : _columnBuilder.ToString());
            }

            /// <summary>
            ///     Disposes of all unmanaged resources
            /// </summary>
            public void Dispose()
            {
                if (_streamReader != null)
                {
                    _streamReader.Close();
                    _streamReader.Dispose();
                }

                if (_streamWriter != null)
                {
                    _streamWriter.Close();
                    _streamWriter.Dispose();
                }

                if (_memoryStream != null)
                {
                    _memoryStream.Close();
                    _memoryStream.Dispose();
                }

                if (_fileStream != null)
                {
                    _fileStream.Close();
                    _fileStream.Dispose();
                }

                if ((_type == Type.String || _type == Type.File) && _stream != null)
                {
                    _stream.Close();
                    _stream.Dispose();
                }
            }

            #endregion Methods
        }

        /// <summary>
        ///     Class to hold csv data
        /// </summary>
        [Serializable]
        public sealed class CsvFile
        {
            #region Properties

            /// <summary>
            ///     Gets the file headers
            /// </summary>
            public readonly List<string> Headers = new List<string>();

            /// <summary>
            ///     Gets the records in the file
            /// </summary>
            public readonly CsvRecords Records = new CsvRecords();

            /// <summary>
            ///     Gets the header count
            /// </summary>
            public int HeaderCount
            {
                get { return Headers.Count; }
            }

            /// <summary>
            ///     Gets the record count
            /// </summary>
            public int RecordCount
            {
                get { return Records.Count; }
            }

            #endregion Properties

            #region Indexers

            /// <summary>
            ///     Gets a record at the specified index
            /// </summary>
            /// <param name="recordIndex">Record index</param>
            /// <returns>CsvRecord</returns>
            public CsvRecord this[int recordIndex]
            {
                get
                {
                    if (recordIndex > Records.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no record at index {0}.", recordIndex));

                    return Records[recordIndex];
                }
            }

            /// <summary>
            ///     Gets the field value at the specified record and field index
            /// </summary>
            /// <param name="recordIndex">Record index</param>
            /// <param name="fieldIndex">Field index</param>
            /// <returns></returns>
            public string this[int recordIndex, int fieldIndex]
            {
                get
                {
                    if (recordIndex > Records.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no record at index {0}.", recordIndex));

                    var record = Records[recordIndex];
                    if (fieldIndex > record.Fields.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no field at index {0} in record {1}.",
                                fieldIndex, recordIndex));

                    return record.Fields[fieldIndex];
                }
                set
                {
                    if (recordIndex > Records.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no record at index {0}.", recordIndex));

                    var record = Records[recordIndex];

                    if (fieldIndex > record.Fields.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no field at index {0}.", fieldIndex));

                    record.Fields[fieldIndex] = value;
                }
            }

            /// <summary>
            ///     Gets the field value at the specified record index for the supplied field name
            /// </summary>
            /// <param name="recordIndex">Record index</param>
            /// <param name="fieldName">Field name</param>
            /// <returns></returns>
            public string this[int recordIndex, string fieldName]
            {
                get
                {
                    if (recordIndex > Records.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no record at index {0}.", recordIndex));

                    var record = Records[recordIndex];

                    var fieldIndex = -1;

                    for (var i = 0; i < Headers.Count; i++)
                    {
                        if (string.Compare(Headers[i], fieldName) != 0) continue;

                        fieldIndex = i;
                        break;
                    }

                    if (fieldIndex == -1)
                        throw new ArgumentException(
                            string.Format("There is no field header with the name '{0}'", fieldName));

                    if (fieldIndex > record.Fields.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no field at index {0} in record {1}.",
                                fieldIndex, recordIndex));

                    return record.Fields[fieldIndex];
                }
                set
                {
                    if (recordIndex > Records.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no record at index {0}.", recordIndex));

                    var record = Records[recordIndex];

                    var fieldIndex = -1;

                    for (var i = 0; i < Headers.Count; i++)
                    {
                        if (string.Compare(Headers[i], fieldName) != 0) continue;

                        fieldIndex = i;
                        break;
                    }

                    if (fieldIndex == -1)
                        throw new ArgumentException(
                            string.Format("There is no field header with the name '{0}'", fieldName));

                    if (fieldIndex > record.Fields.Count - 1)
                        throw new IndexOutOfRangeException(
                            string.Format("There is no field at index {0} in record {1}.",
                                fieldIndex, recordIndex));

                    record.Fields[fieldIndex] = value;
                }
            }

            #endregion Indexers

            #region Methods

            /// <summary>
            ///     Populates the current instance from the specified file
            /// </summary>
            /// <param name="filePath">File path</param>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            public void Populate(string filePath, bool hasHeaderRow)
            {
                Populate(filePath, null, hasHeaderRow, false);
            }

            /// <summary>
            ///     Populates the current instance from the specified file
            /// </summary>
            /// <param name="filePath">File path</param>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            /// <param name="trimColumns">True if column values should be trimmed, otherwise false</param>
            public void Populate(string filePath, bool hasHeaderRow, bool trimColumns)
            {
                Populate(filePath, null, hasHeaderRow, trimColumns);
            }

            /// <summary>
            ///     Populates the current instance from the specified file
            /// </summary>
            /// <param name="filePath">File path</param>
            /// <param name="encoding">Encoding</param>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            /// <param name="trimColumns">True if column values should be trimmed, otherwise false</param>
            public void Populate(
                string filePath, Encoding encoding, bool hasHeaderRow, bool trimColumns)
            {
                using (
                    var reader = new CsvReader(filePath, encoding)
                    { HasHeaderRow = hasHeaderRow, TrimColumns = trimColumns })
                {
                    PopulateCsvFile(reader);
                }
            }

            /// <summary>
            ///     Populates the current instance from a stream
            /// </summary>
            /// <param name="stream">Stream</param>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            public void Populate(Stream stream, bool hasHeaderRow)
            {
                Populate(stream, null, hasHeaderRow, false);
            }

            /// <summary>
            ///     Populates the current instance from a stream
            /// </summary>
            /// <param name="stream">Stream</param>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            /// <param name="trimColumns">True if column values should be trimmed, otherwise false</param>
            public void Populate(Stream stream, bool hasHeaderRow, bool trimColumns)
            {
                Populate(stream, null, hasHeaderRow, trimColumns);
            }

            /// <summary>
            ///     Populates the current instance from a stream
            /// </summary>
            /// <param name="stream">Stream</param>
            /// <param name="encoding">Encoding</param>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            /// <param name="trimColumns">True if column values should be trimmed, otherwise false</param>
            public void Populate(
                Stream stream, Encoding encoding, bool hasHeaderRow, bool trimColumns)
            {
                using (
                    var reader = new CsvReader(stream, encoding)
                    { HasHeaderRow = hasHeaderRow, TrimColumns = trimColumns })
                {
                    PopulateCsvFile(reader);
                }
            }

            /// <summary>
            ///     Populates the current instance from a string
            /// </summary>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            /// <param name="csvContent">Csv text</param>
            public void Populate(bool hasHeaderRow, string csvContent)
            {
                Populate(hasHeaderRow, csvContent, null, false);
            }

            /// <summary>
            ///     Populates the current instance from a string
            /// </summary>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            /// <param name="csvContent">Csv text</param>
            /// <param name="trimColumns">True if column values should be trimmed, otherwise false</param>
            public void Populate(bool hasHeaderRow, string csvContent, bool trimColumns)
            {
                Populate(hasHeaderRow, csvContent, null, trimColumns);
            }

            /// <summary>
            ///     Populates the current instance from a string
            /// </summary>
            /// <param name="hasHeaderRow">True if the file has a header row, otherwise false</param>
            /// <param name="csvContent">Csv text</param>
            /// <param name="encoding">Encoding</param>
            /// <param name="trimColumns">True if column values should be trimmed, otherwise false</param>
            public void Populate(
                bool hasHeaderRow, string csvContent, Encoding encoding, bool trimColumns)
            {
                using (
                    var reader = new CsvReader(encoding, csvContent)
                    { HasHeaderRow = hasHeaderRow, TrimColumns = trimColumns })
                {
                    PopulateCsvFile(reader);
                }
            }

            /// <summary>
            ///     Populates the current instance using the CsvReader object
            /// </summary>
            /// <param name="reader">CsvReader</param>
            private void PopulateCsvFile(CsvReader reader)
            {
                Headers.Clear();
                Records.Clear();

                var addedHeader = false;

                while (reader.ReadNextRecord())
                {
                    if (reader.HasHeaderRow && !addedHeader)
                    {
                        reader.Fields.ForEach(field => Headers.Add(field));
                        addedHeader = true;
                        continue;
                    }

                    var record = new CsvRecord();
                    reader.Fields.ForEach(field => record.Fields.Add(field));
                    Records.Add(record);
                }
            }

            #endregion Methods
        }

        /// <summary>
        ///     Class for a collection of CsvRecord objects
        /// </summary>
        [Serializable]
        public sealed class CsvRecords : List<CsvRecord>
        {
        }

        /// <summary>
        ///     Csv record class
        /// </summary>
        [Serializable]
        public sealed class CsvRecord
        {
            #region Properties

            /// <summary>
            ///     Gets the Fields in the record
            /// </summary>
            public readonly List<string> Fields = new List<string>();

            /// <summary>
            ///     Gets the number of fields in the record
            /// </summary>
            public int FieldCount
            {
                get { return Fields.Count; }
            }

            #endregion Properties
        }

        /// <summary>
        ///     Read text based configuration file based on key value pairs.
        ///     open source from https://github.com/virtualdreams/configfile  MIT license
        ///     need to do a enhancement to be a safereader
        /// </summary>
        public class ConfigReader
        {
            private readonly bool ignoreError;
            private readonly FileSystemWatcher watcher = new FileSystemWatcher();

            /// <summary>
            ///     Hold the key value pairs.
            /// </summary>
            protected Dictionary<string, string> _configValues =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            private readonly string _filename;

            /// <summary>
            ///     Hold lines for here mode.
            /// </summary>
            private readonly List<string> _here = new List<string>();

            /// <summary>
            ///     Initialize new instance and read the configuration from file.
            /// </summary>
            /// <param name="filename"></param>
            public ConfigReader(string filename) : this(filename, true)
            {
            }

            public ConfigReader(string filename, bool ignoreError)
            {
                this.ignoreError = ignoreError;
                _filename = EnvironmentHelper.ReplaceEnvironmentVariables(filename);
                watcher.Path = Path.GetDirectoryName(_filename);
                watcher.Filter = Path.GetFileName(_filename);
                watcher.Changed += OnProcess;
                watcher.Created += OnProcess;
                watcher.Renamed += OnRenamed;
                watcher.EnableRaisingEvents = true;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = false;


                if (string.IsNullOrEmpty(_filename))
                    throw new ArgumentNullException(_filename);
                Open(_filename);
            }

            private void OnRenamed(object sender, RenamedEventArgs e)
            {
                //e.o
            }

            private void OnProcess(object sender, FileSystemEventArgs e)
            {
                Open(_filename);
                ConfigChange?.Invoke(sender, this);
            }

            /// <summary>
            ///     Occurs before a new key is added.
            /// </summary>
            public event EventHandler<ConfigReaderEventArgs> OnKeyAdd;

            public event EventHandler<ConfigReader> ConfigChange;

            /// <summary>
            ///     Occurs before a existing key is overwritten.
            /// </summary>
            public event EventHandler<ConfigReaderEventArgs> OnKeyChange;

            /// <summary>
            ///     Open configuration file and clear existing values.
            /// </summary>
            /// <param name="filename"></param>
            public void Open(string filename)
            {
                _configValues.Clear();

                using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    Parse(() => stream);
                }
            }

            /// <summary>
            ///     Open configuration file and add values.
            /// </summary>
            /// <param name="filename"></param>
            public void Append(string filename)
            {
                if (!File.Exists(filename))
                    throw new FileNotFoundException("File not found.", filename);

                Parse(() => File.OpenRead(filename));
            }

            /// <summary>
            ///     Read stream and add values.
            /// </summary>
            /// <param name="stream"></param>
            public void Append(Stream stream)
            {
                if (stream == null)
                    throw new ArgumentException("stream");

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Position = 0;

                    Parse(() => ms);
                }
            }

            /// <summary>
            ///     Read all lines from stream.
            /// </summary>
            /// <param name="streamProvider"></param>
            /// <returns></returns>
            private IEnumerable<string> ReadAllLines(Func<Stream> streamProvider)
            {
                using (var stream = streamProvider())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                            yield return reader.ReadLine();
                    }
                }
            }

            /// <summary>
            ///     Read a stream and parse.
            /// </summary>
            /// <param name="stream">The stream.</param>
            private void Parse(Func<Stream> stream)
            {
                var linenumber = 0;
                var key = "";
                var value = "";
                var mode = ParserMode.Normal;
                var stop = "";
                var literal = false;

                foreach (var rawline in ReadAllLines(stream))
                {
                    linenumber++;

                    var line = rawline.Trim();

                    // Test for comments or empty lines and not here mode
                    if ((line.StartsWith("#") || line.StartsWith(";") || line.Length == 0) &&
                        mode != ParserMode.Here)
                        continue;

                    // normal parser mode
                    if (mode == ParserMode.Normal)
                    {
                        // Split to <key> = <value> pair
                        var pair = line.Split(new[] { '=' }, 2);

                        // Test for one part
                        if (pair.Length == 1)
                            if (!ignoreError)
                                throw new ConfigException(
                                    string.Format("Not a key value pair. Line {0}.", linenumber));
                            else
                                continue;

                        // Test for two parts
                        if (pair.Length == 2)
                        {
                            key = pair[0].Trim();
                            value = pair[1].Trim();

                            if (string.IsNullOrEmpty(key))
                                if (!ignoreError)
                                    throw new ConfigException(
                                        string.Format("The key is null. Line {0}.", linenumber));
                                else
                                    continue;

                            // test for valid key format
                            if (
                                !Regex.IsMatch(key, "^@?[a-zA-Z_][a-zA-Z0-9_.]*$",
                                    RegexOptions.Singleline))
                                if (!ignoreError)
                                    throw new ConfigException(
                                        string.Format(
                                            "The format for key '{0}' is invalid. Line {1}.", key,
                                            linenumber));
                                else
                                    continue;

                            // literal mode
                            if (key.StartsWith("@"))
                            {
                                key = key.Substring(1);
                                literal = true;
                            }
                        }

                        // Test for here document mode sequence
                        var inline = Regex.Match(value, "^<<([a-zA-Z][a-zA-Z0-9]*)$");
                        if (inline.Success)
                        {
                            // test for literal
                            if (literal)
                                throw new ConfigException(
                                    "Literal mode in here document not allowed.");

                            // stopword
                            stop = inline.Groups[1].Value;

                            _here.Clear();

                            mode = ParserMode.Here;
                            continue;
                        }
                    }

                    // Test, if in continuation mode
                    if (mode == ParserMode.Continuation)
                        value = string.Concat(value, line);

                    // Test, if in here document mode
                    if (mode == ParserMode.Here)
                    {
                        // Test for stopword.
                        if (rawline.Equals(stop))
                        {
#if NET35
						Add(key, String.Join("\n", _here.ToArray()));
#else
                            Add(key, string.Join("\n", _here));
#endif

                            // reset parser
                            literal = false;
                            mode = ParserMode.Normal;

                            _here.Clear();

                            continue;
                        }

                        _here.Add(rawline);
                        continue;
                    }

                    // Test for trailing backslash (continuation mode)
                    if (value.EndsWith("\\"))
                    {
                        value = value.Substring(0, value.Length - 1);

                        mode = ParserMode.Continuation;
                        continue;
                    }

                    // Test for surrounding double quotes
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                        value = value.Substring(1, value.Length - 2);

                    if (!literal)
                        value = Regex.Replace(value, @"(\\(.?))", m =>
                        {
                            switch (m.Groups[2].Value)
                            {
                                case "n":
                                    return "\n";

                                case "t":
                                    return "\t";

                                case "\\":
                                    return "\\";

                                case "":
                                    throw new ConfigException(
                                        string.Format("Empty escape sequence. Line {0}.", linenumber));

                                default:
                                    throw new ConfigException(
                                        string.Format("Unknown escape sequence: \\{0}. Line {1}.",
                                            m.Groups[2].Value, linenumber));
                            }
                        });

                    Add(key, value);

                    // reset parser
                    literal = false;
                    mode = ParserMode.Normal;
                }

                if (mode != ParserMode.Normal)
                    throw new ConfigException(string.Format("End of file encountered in {0} mode.",
                        mode.ToString().ToLower()));
            }

            /// <summary>
            ///     Add the key and value to storage or override existing.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            private void Add(string key, string value)
            {
                var e = new ConfigReaderEventArgs(key, value);
                if (!_configValues.ContainsKey(key))
                {
                    OnKeyAdd?.Invoke(this, e);

                    if (!e.Decline)
                        _configValues.Add(key, value);
                }
                else
                {
                    OnKeyChange?.Invoke(this, e);

                    if (!e.Decline)
                        _configValues[key] = value;
                }
            }

            /// <summary>
            ///     Gets the value associated with the specified key.
            ///     If the key does not exists, the default value is returned.
            /// </summary>
            /// <typeparam name="T">Destination type.</typeparam>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="defaultValue">The default value if key not found.</param>
            /// <param name="defaultIfEmpty">If value is empty, then return the default value.</param>
            /// <returns>The value from the key, otherwise the default value.</returns>
            public T GetValue<T>(string key, T defaultValue, bool defaultIfEmpty = false)
                where T : IConvertible
            {
                return GetValue(key, defaultValue, CultureInfo.InvariantCulture, defaultIfEmpty);
            }

            /// <summary>
            ///     Gets the value associated with the specified key.
            ///     If the key does not exists, the default value is returned.
            /// </summary>
            /// <typeparam name="T">Destination type.</typeparam>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="defaultValue">The default value if key not found.</param>
            /// <param name="provider">The format provider.</param>
            /// <param name="defaultIfEmpty">If value is empty, then return the default value.</param>
            /// <returns>The value from the key, otherwise the default value.</returns>
            public T GetValue<T>(
                string key, T defaultValue, IFormatProvider provider, bool defaultIfEmpty = false)
                where T : IConvertible
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                if (_configValues.ContainsKey(key))
                {
                    var _value = _configValues[key];
                    if (defaultIfEmpty && string.IsNullOrEmpty(_value))
                        return defaultValue;

                    try
                    {
                        return (T)Convert.ChangeType(_value, typeof(T), provider);
                    }
                    catch (FormatException e)
                    {
                        throw new ConfigException(
                            string.Format("Can't convert key '{0}' to destination type '{1}'.", key,
                                typeof(T)), e);
                    }
                }
                return defaultValue;
            }

            /// <summary>
            ///     Gets the value associated with the specified key.
            ///     If the key does not exists, an exception is thrown.
            /// </summary>
            /// <typeparam name="T">Destination type.</typeparam>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="valueMustNotBeEmpty">If value is empty, then throw an exception.</param>
            /// <returns>Throws an exception if the key is not found.</returns>
            public T TryGetValue<T>(string key, bool valueMustNotBeEmpty = false)
                where T : IConvertible
            {
                return TryGetValue<T>(key, CultureInfo.InvariantCulture, valueMustNotBeEmpty);
            }

            /// <summary>
            ///     Gets the value associated with the specified key.
            ///     If the key does not exists, an exception is thrown.
            /// </summary>
            /// <typeparam name="T">Destination type.</typeparam>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="provider">The format provider.</param>
            /// <param name="valueMustNotBeEmpty">If value is empty, then throw an exception.</param>
            /// <returns>Throws a exception if the key is not found.</returns>
            public T TryGetValue<T>(
                string key, IFormatProvider provider, bool valueMustNotBeEmpty = false)
                where T : IConvertible
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                if (_configValues.ContainsKey(key))
                {
                    var _value = _configValues[key];
                    if (valueMustNotBeEmpty && string.IsNullOrEmpty(_value))
                        throw new ConfigException(
                            string.Format("The value for key '{0}' must not be empty.", key));

                    try
                    {
                        return (T)Convert.ChangeType(_value, typeof(T), provider);
                    }
                    catch (FormatException e)
                    {
                        throw new ConfigException(
                            string.Format("Can't convert key '{0}' to destination type '{1}'.", key,
                                typeof(T)), e);
                    }
                }
                throw new ConfigException(
                    string.Format("The configuration key '{0}' does not exists.", key));
            }

            /// <summary>
            ///     Gets the value associated with the specified key.
            /// </summary>
            /// <typeparam name="T">The destination type.</typeparam>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="value">The value if the key is found.</param>
            /// <param name="valueMustNotBeEmpty">If value is empty, then false is returned.</param>
            /// <returns>Returns true if the key is found, otherwise false.</returns>
            public bool TryGetValue<T>(string key, out T value, bool valueMustNotBeEmpty = false)
                where T : IConvertible
            {
                return TryGetValue(key, out value, CultureInfo.InvariantCulture, valueMustNotBeEmpty);
            }

            /// <summary>
            ///     Gets the value associated with the specified key.
            /// </summary>
            /// <typeparam name="T">The destination type.</typeparam>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="value">The value if the key is found.</param>
            /// <param name="provider">The format provider.</param>
            /// <param name="valueMustNotBeEmpty">If value is empty, then false is returned.</param>
            /// <returns>Returns true if the key is found, otherwise false.</returns>
            public bool TryGetValue<T>(
                string key, out T value, IFormatProvider provider, bool valueMustNotBeEmpty = false)
                where T : IConvertible
            {
                value = default(T);

                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                if (_configValues.ContainsKey(key))
                {
                    var _value = _configValues[key];
                    if (valueMustNotBeEmpty && string.IsNullOrEmpty(_value))
                        return false;

                    try
                    {
                        value = (T)Convert.ChangeType(_value, typeof(T), provider);
                        return true;
                    }
                    catch (FormatException e)
                    {
                        throw new ConfigException(
                            string.Format("Can't convert key '{0}' to destination type '{1}'.", key,
                                typeof(T)), e);
                    }
                }

                return false;
            }

            /// <summary>
            ///     Test if the key exists.
            /// </summary>
            /// <param name="key">The configuration key.</param>
            /// <returns></returns>
            public bool KeyExists(string key)
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                return _configValues.ContainsKey(key);
            }

            /// <summary>
            ///     Get all available configuration keys.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<string> GetKeys()
            {
                return _configValues.Keys;
            }

            /// <summary>
            ///     Get all available configuration values.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<string> GetValues()
            {
                return _configValues.Values;
            }
        }

        /// <summary>
        ///     Write text based configuration file based on key value pairs.
        ///     open source from https://github.com/virtualdreams/configfile  MIT license
        /// </summary>
        public class ConfigWriter
        {
            /// <summary>
            ///     Hold the key value pairs.
            /// </summary>
            protected Dictionary<string, string> _configValues =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            ///     Initialize new configuration writer.
            /// </summary>
            public ConfigWriter()
            {
            }

            /// <summary>
            ///     Initialize new configuration writer and read in an existing reader.
            /// </summary>
            /// <param name="reader">The configuration reader.</param>
            public ConfigWriter(ConfigReader reader) : this(reader, CultureInfo.InvariantCulture)
            {
            }

            /// <summary>
            ///     Initialize new configuration writer and read in an existing reader.
            /// </summary>
            /// <param name="reader">The configuration reader.</param>
            /// <param name="provider">The format provider.</param>
            public ConfigWriter(ConfigReader reader, IFormatProvider provider)
            {
                if (reader == null)
                    throw new ArgumentNullException("reader");

                if (provider == null)
                    throw new ArgumentNullException("provider");

                foreach (var key in reader.GetKeys())
                    AddValue(key, reader.GetValue(key, ""), provider);
            }

            /// <summary>
            ///     Save configuration to file.
            /// </summary>
            /// <param name="filename">The filename.</param>
            public void Save(string filename)
            {
                using (
                    var stream = File.Open(filename, FileMode.Create, FileAccess.Write,
                        FileShare.None))
                {
                    Writer(stream);
                }
            }

            /// <summary>
            ///     Save configuration to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            public void Save(Stream stream)
            {
                Writer(stream);
            }

            /// <summary>
            ///     Write key value pair to stream.
            /// </summary>
            /// <param name="stream">The stream.</param>
            private void Writer(Stream stream)
            {
                var writer = new StreamWriter(stream);

                foreach (var key in _configValues.Keys)
                    writer.WriteLine("{0} = {1}", key, Prepare(_configValues[key]));

                writer.Flush();
            }

            /// <summary>
            ///     Add or set a key value pair.
            /// </summary>
            /// <typeparam name="T">Type of value.</typeparam>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            public void AddValue<T>(string key, T value) where T : IConvertible
            {
                AddValue(key, value, CultureInfo.InvariantCulture);
            }

            /// <summary>
            ///     Add or set a key value pair.
            /// </summary>
            /// <typeparam name="T">Type of value.</typeparam>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            /// <param name="provider">The format provider.</param>
            /// <exception cref="ConfigException"></exception>
            public void AddValue<T>(string key, T value, IFormatProvider provider)
                where T : IConvertible
            {
                if (string.IsNullOrEmpty(key))
                    throw new ConfigException("The key is null or empty.");

                if (!Regex.IsMatch(key, "^[a-zA-Z_][a-zA-Z0-9_.]*$", RegexOptions.Singleline))
                    throw new ConfigException(string.Format("The format for key '{0}' is invalid.",
                        key));

                if (!_configValues.ContainsKey(key))
                    _configValues.Add(key, string.Format(provider, "{0}", value));
                else
                    _configValues[key] = string.Format(provider, "{0}", value);
            }

            /// <summary>
            ///     Prepare the value for writing.
            /// </summary>
            /// <param name="input">The value.</param>
            /// <returns>The prepared value.</returns>
            private string Prepare(string input)
            {
                if (char.IsWhiteSpace(input.FirstOrDefault()) ||
                    char.IsWhiteSpace(input.LastOrDefault()))
                    input = string.Format("\"{0}\"", input);

                input = Regex.Replace(input, @"(\n|\t|\\)", m =>
                {
                    switch (m.Groups[1].Value)
                    {
                        case "\n":
                            return "\\n";

                        case "\t":
                            return "\\t";

                        case "\\":
                            return "\\\\";
                    }

                    throw new ConfigException(string.Format("Unknown escape sequence: \\{0}.",
                        m.Groups[1].Value));
                });

                return input;
            }
        }

        /// <summary>
        ///     ConfigFile event args.
        /// </summary>
        public class ConfigReaderEventArgs : EventArgs
        {
            /// <summary>
            ///     Initialize new event arguments.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            /// <param name="decline">Decline value.</param>
            public ConfigReaderEventArgs(string key, string value, bool decline = false)
            {
                Key = key;
                Value = value;
                Decline = decline;
            }

            /// <summary>
            ///     Get the key.
            /// </summary>
            public string Key { get; private set; }

            /// <summary>
            ///     Get the value.
            /// </summary>
            public string Value { get; private set; }

            /// <summary>
            ///     Set to true to decline.
            /// </summary>
            public bool Decline { get; set; }
        }

        /// <summary>
        ///     This exception is thrown when an error in the ConfigFile occurs.
        /// </summary>
        /// <remarks>
        ///     This is the base exception for all exceptions thrown in the ConfigFile
        /// </remarks>
        public class ConfigException : Exception
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="T:CS.Helper.ConfigFileException" /> class.
            /// </summary>
            public ConfigException() : base("ConfigFile caused an exception.")
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:CS.Helper.ConfigFileException" /> class.
            /// </summary>
            public ConfigException(Exception ex) : base("ConfigFile caused an exception.", ex)
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:CS.Helper.ConfigFileException" /> class.
            /// </summary>
            public ConfigException(string message) : base(message)
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:CS.Helper.ConfigFileException" /> class.
            /// </summary>
            public ConfigException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}