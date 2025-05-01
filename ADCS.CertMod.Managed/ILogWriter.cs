using System;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents a type used to perform logging.
/// </summary>
public interface ILogWriter {
    /// <summary>
    /// Gets or sets the current logging level.
    /// </summary>
    LogLevel LogLevel { get; set; }

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <code>User {User} logged in from {Address}"</code></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogTrace(String message, params Object[]? args);
    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Application-specific source that raised the exception.</param>
    void LogTrace(Exception exception, String source);
    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <code>User {User} logged in from {Address}"</code></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogDebug(String message, params Object[]? args);
    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Application-specific source that raised the exception.</param>
    void LogDebug(Exception exception, String source);
    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <code>User {User} logged in from {Address}"</code></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogInformation(String message, params Object[]? args);
    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Application-specific source that raised the exception.</param>
    void LogInformation(Exception exception, String source);
    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <code>User {User} logged in from {Address}"</code></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogWarning(String message, params Object[]? args);
    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Application-specific source that raised the exception.</param>
    void LogWarning(Exception exception, String source);
    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <code>User {User} logged in from {Address}"</code></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogError(String message, params Object[]? args);
    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Application-specific source that raised the exception.</param>
    void LogError(Exception exception, String source);
    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <code>User {User} logged in from {Address}"</code></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogCritical(String message, params Object[]? args);
    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Application-specific source that raised the exception.</param>
    void LogCritical(Exception exception, String source);

    /// <summary>
    /// Formats and writes a log message at the specified log level.
    /// </summary>
    /// <param name="logLevel">Log level to write log message into.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <code>User {User} logged in from {Address}"</code></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void Log(LogLevel logLevel, String message, params Object[]? args);
    /// <summary>
    /// Formats and writes a log message at the specified log level.
    /// </summary>
    /// <param name="logLevel">Log level to write log message into.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="source">Application-specific source that raised the exception.</param>
    void Log(LogLevel logLevel, Exception exception, String source);
}