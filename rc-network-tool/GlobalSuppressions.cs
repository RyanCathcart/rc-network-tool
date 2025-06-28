// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", 
    Justification = "Platform-specific implementations for services need to all be a part of the same namespace.", 
    Scope = "namespace", Target = "~N:rc_network_tool.Services")]
