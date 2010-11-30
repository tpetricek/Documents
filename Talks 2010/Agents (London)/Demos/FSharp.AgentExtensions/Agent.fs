// ----------------------------------------------------------------------------
// Agent alias
// ----------------------------------------------------------------------------
namespace FSharp.Control

open System
open System.Collections.Generic

/// A convenience type alias for 'MailboxProcessor<T>' type
type Agent<'T> = MailboxProcessor<'T>


