# Lurgle.Logging

[![Version](https://img.shields.io/nuget/v/Lurgle.Logging?style=plastic)](https://www.nuget.org/packages/Lurgle.Logging)
[![Downloads](https://img.shields.io/nuget/dt/Lurgle.Logging?style=plastic)](https://www.nuget.org/packages/Lurgle.Logging)
[![License](https://img.shields.io/github/license/MattMofDoom/Lurgle.Logging?style=plastic)](https://github.com/MattMofDoom/Lurgle.Logging/blob/dev/LICENSE)

[Serilog](https://serilog.net/) is a fantastic open source logging solution, which can be configured in innumerable ways. Anyone needing a logging solution can add Serilog and get started.

**Lurgle.Logging** is an implementation of Serilog that can help to save time in getting up and running, and provides some useful functionality.

- Includes sinks for Console, Windows Event Log, File, Seq, and Splunk with configurable properties exposed via config
- Includes enrichers for log context, thread id, environment user name, machine name, process id, process name, memory usage
- Includes AppName and AppVersion properties which can use the executing assembly name/version, or set appname via config
- Test enabled sinks at initialisation and return a FailureReason if an error occurs
- Add common properties to every log event (roughly equivalent to Enrich.WithProperty) 
- Use fluent chaining to add properties to log events as needed (equivalent to ForContext)
- Automatically add calling method property to log events (or disable via config)
- Automatically add source file path property to log events (or disable via config)
- Automatically add source line number property to log events (or disable via config)
- Automatically mask properties based on the configured masking policy and properties
- Automatically generate a correlation id, or pass through an existing correlation id
- Expose configuration of log formatting to config for flexibility
- Configuration via app config or passing a config class to the library
  - Default settings supplied where it's sane to do so, allowing only essential properties to be configured

This implementation is at its most useful with a Seq, Splunk, or Json file implementation (or all of the above!), where a common log library is needed in multiple projects and a common set of standards is required (such as implementing common properties, data masking, and correlation ids).
