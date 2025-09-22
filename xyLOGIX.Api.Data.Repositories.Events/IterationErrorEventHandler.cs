using PostSharp.Patterns.Diagnostics;
namespace xyLOGIX.Api.Data.Repositories.Events
{
    /// <summary>
    /// Defines the method signature for the handlers of a
    /// <c>IterationError</c> event.
    /// </summary>
    /// <param name="sender">
    /// Reference to the instance of the object that raised the
    /// event.
    /// </param>
    /// <param name="e">
    /// A
    /// <see cref="T:xyLOGIX.Api.Data.Iterators.Events.IterationErrorEventArgs" /> that
    /// contains the event data.
    /// </param>
    public delegate void IterationErrorEventHandler(
        [NotLogged] object sender,
        [NotLogged] IterationErrorEventArgs e
    );
}