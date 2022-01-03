using System;
using System.Collections.Generic;
using Skyla.Engine.Buffers;
using Skyla.Engine.Database;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
using Skyla.Engine.Plans;

namespace Skyla.Engine.Drivers;

public record ExecutionResult(int AffectedRows, string Message) : IExecutionResult;
