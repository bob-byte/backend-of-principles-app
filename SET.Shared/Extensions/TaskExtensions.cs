using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Extensions;

public static class TaskExtensions
{
    public static ConfiguredTaskAwaitable DefaultConfigureAwait( this Task task )
    {
        return task.ConfigureAwait( continueOnCapturedContext: false );
    }

    public static ConfiguredTaskAwaitable<T> DefaultConfigureAwait<T>( this Task<T> task )
    {
        return task.ConfigureAwait( continueOnCapturedContext: false );
    }

    public static ConfiguredValueTaskAwaitable DefaultConfigureAwait( this ValueTask valueTask )
    {
        return valueTask.ConfigureAwait( continueOnCapturedContext: false );
    }

    public static ConfiguredValueTaskAwaitable<T> DefaultConfigureAwait<T>( this ValueTask<T> valueTask )
    {
        return valueTask.ConfigureAwait( continueOnCapturedContext: false );
    }
}
