using PostSharp.Patterns.Collections;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Threading;
using System;
using System.Collections.Generic;
using xyLOGIX.Api.Data.Iterables.Interfaces;
using xyLOGIX.Api.Data.Iterators.Exceptions;
using xyLOGIX.Api.Data.Repositories.Events;
using xyLOGIX.Api.Data.Repositories.Interfaces;

namespace xyLOGIX.Api.Data.Repositories
{
    /// <summary>
    /// Provides functionality that is common to all REST API repository
    /// objects.
    /// </summary>
    /// <typeparam name="T">
    /// Name -- should be that of a concrete type -- of the type
    /// of a POCO that specifies a single element of the collection exposed by this
    /// repository.
    /// <para />
    /// An interface name may be used; however, implementers must then adapt their
    /// methods' outputs to the interface specified, in this case.
    /// <para />
    /// Implementers and clients are cautioned that if this type parameter is supplied
    /// with an interface, further generic abstraction may be required in other areas
    /// of the software system.
    /// </typeparam>
    /// <remarks>
    /// This class provides access to a paginated/iterated REST API data set,
    /// of an a priori unknown length, as if it were, e.g., a SQL Server data source.
    /// Basically, we try to merge the concepts of data retrieval from a REST API data
    /// set and the concepts of the, e.g., Repository pattern in the N-Tier data access
    /// world.
    /// <para />
    /// Implementers are free to deny access to specific functionality by throwing
    /// <see cref="T:System.NotSupportedException" /> for any of the methods this
    /// interface exposes, given varying target REST API use-case and support
    /// scenarios.
    /// </remarks>
    public abstract class ApiRepositoryBase<T> : IApiRepository<T>
        where T : class
    {
        /// <summary>
        /// Reference to an instance of an object that implements the
        /// <see cref="T:xyLOGIX.Api.Data.Iterables.Interfaces.IIterable{T}" /> interface.
        /// This objects provides us with data. It's kind of like the DbContext field we
        /// utilize in a Repository class that is used in Entity Framework.
        /// </summary>
        [Reference] private IIterable<T> _iterable;

        /// <summary>
        /// Initializes static data or performs actions that need to be performed once only
        /// for the <see cref="T:xyLOGIX.Api.Data.Repositories.ApiRepositoryBase" /> class.
        /// </summary>
        /// <remarks>
        /// This constructor is called automatically prior to the first instance being
        /// created or before any static members are referenced.
        /// </remarks>
        [Log(AttributeExclude = true)]
        static ApiRepositoryBase() { }

        /// <summary>
        /// Initializes a new instance of
        /// <see cref="T:xyLOGIX.Api.Data.Repositories.ApiRepositoryBase" /> and returns a
        /// reference to it.
        /// </summary>
        /// <remarks>
        /// <strong>NOTE:</strong> This constructor is marked <see langword="protected" />
        /// due to the fact that this class is marked <see langword="abstract" />.
        /// </remarks>
        [Log(AttributeExclude = true)]
        protected ApiRepositoryBase() { }

        /// <summary>
        /// Gets or sets the maximum number of elements per page that the API
        /// will allow to be fetched.
        /// </summary>
        /// <remarks>
        /// This is an abstract property because this quantity is different for
        /// every target REST API.
        /// </remarks>
        public abstract int MaxPageSize { get; protected set; }

        /// <summary>
        /// Gets or sets the page size, i.e., how many elements to request at a
        /// time from the target REST API.
        /// </summary>
        /// <remarks>
        /// The Find, Delete, DeleteAll, and Update methods, by default, iterate
        /// through the target REST API data set a single element at a time.
        /// <para />
        /// Because we have to be careful about not hitting rate limits during these
        /// operations, this property allows clients of this class to customize the number
        /// of elements taken at a time to be different from 1 by setting this property.
        /// </remarks>
        public abstract int PageSize { get; set; }

        /// <summary> Occurs when an exception is thrown during the iteration process. </summary>
        public event IterationErrorEventHandler IterationError;

        /// <summary> Associates this repository with a data source. </summary>
        /// <param name="iterable">
        /// (Required.) Reference to an instance of a class that
        /// implements the <see cref="T:xyLOGIX.Api.Data.Iterables.Interfaces.IIterable" />
        /// interface to which to associate this repository.
        /// </param>
        /// <returns> </returns>
        public IApiRepository<T> AttachDataSource(IIterable<T> iterable)
        {
            _iterable = iterable ??
                        throw new ArgumentNullException(nameof(iterable));

            return this;
        }

        /// <summary>
        /// If offered by the endpoint, uses any DELETE request exposed to remove
        /// something from the target REST API dataset.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object that represents a single element in the
        /// target REST API endpoint's dataset.
        /// </typeparam>
        /// <param name="recordToDelete">
        /// (Required.) Reference to an instance of the model
        /// type, <typeparamref name="T" /> , that specifies which object should be deleted
        /// from the API dataset.
        /// </param>
        /// <remarks>
        /// Not all REST APIs expose a means of deleting items from their
        /// dataset. In this case, implementations of this method must throw
        /// <see cref="T:System.NotSupportedException" />
        /// <para />
        /// Implementers are free to deny access to this functionality (even if the target
        /// REST API supports it) by throwing <see cref="T:System.NotSupportedException" />
        /// .
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is
        /// made on the library that accesses the target REST API in the event the
        /// operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does
        /// not support the concept of element deletion.
        /// </exception>
        public abstract void Delete(T recordToDelete);

        /// <summary>
        /// If supported by the target REST API, removes all elements from the
        /// dataset that satisfy the criteria expressed by the supplied
        /// <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// (Required.) Predicate expression that returns either
        /// <c>true</c> or <c>false</c> when supplied with an instance of the element model
        /// type, <typeparamref name="T" />, as a parameter.
        /// <para />
        /// By element model we mean an instance of whatever POCO is supplied by the
        /// library providing access to the target REST API that represents a single
        /// element of the dataset.
        /// <para />
        /// If the predicate returns <c>true</c> for a given instance of the element model
        /// object, then this object strives to remove that element from the dataset using
        /// the appropriate method call on the target REST API client library.
        /// </param>
        /// <remarks>
        /// Not all REST APIs expose a means of deleting items from their
        /// dataset. In this case, implementations of this method must throw
        /// <see cref="T:System.NotSupportedException" />.
        /// <para />
        /// Implementers are free to deny access to this functionality (even if the target
        /// REST API supports it) by throwing <see cref="T:System.NotSupportedException" />
        /// .
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is
        /// made on the library that accesses the target REST API in the event the
        /// operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does
        /// not support the concept of element deletion.
        /// </exception>
        public abstract void DeleteAll(Predicate<T> predicate);

        /// <summary>
        /// Iterates through the dataset of the target REST API,
        /// <see cref="P:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.PageSize" />
        /// elements at a time (default is 1), and tries to find an element matching the
        /// criteria provided.
        /// </summary>
        /// <param name="predicate">
        /// (Required.) Lambda expression specifying how to tell
        /// if an element is to be retrieved.
        /// </param>
        /// <returns>
        /// This method iterates through the dataset of the target REST API,
        /// testing each element against the provided <paramref name="predicate" /> . The
        /// first element for which the <paramref name="predicate" /> evaluates to
        /// <c>true</c> is then returned, or <c>null</c> if an error occurred or the
        /// matching element was otherwise not found.
        /// </returns>
        /// Reference to an instance of an object of type
        /// <typeparamref name="T" />
        /// if an object matching the criteria is found;
        /// <c> null </c>
        /// otherwise.
        /// <remarks>
        /// Clients of this repository should use this method instead of invoking
        /// the GetAll operation and then filtering with the LINQ Where method, in order to
        /// retrieve just those API elements that need to be retrieved until the desired
        /// one is found.
        /// <para />
        /// GetAll will suck down the entire dataset, and this may not be desirable because
        /// of rate limits etc.
        /// <para />
        /// Implementations should throw <see cref="T:System.NotSupportedException" /> in
        /// the event that the API does not support pagination -- or delegate the call to
        /// this object's GetAll followed by <see cref="M:System.Linq.Enumerable.Where" />
        /// followed by <see cref="M:System.Linq.Enumerable.FirstOrDefault" />.
        /// <para />
        /// Alternatively, implementers may delegate this method to
        /// <see cref="M:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.Get" />.
        /// <para />
        /// This repository provides these two seemingly redundant ways of searching for
        /// objects since not all REST API controllers expose the same functionality set or
        /// have the same rate-limit concerns.
        /// <para />
        /// <strong>EXTREME CAUTION</strong> IF rate-limiting and data set size are
        /// concerns, then it is probably better to call the
        /// <see cref="M:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.Get" />
        /// method. This passes the search criteria directly to the target REST API server
        /// and harnesses the server's own power to perform the search for the desired
        /// item, instead of going through the entire data set, one by one, until a match
        /// is found.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is
        /// made on the library that accesses the target REST API in the event the
        /// operation was not successful.
        /// </exception>
        /// <exception cref="T:ArgumentNullException">
        /// Thrown if the required parameter,
        /// <paramref name="predicate" /> , is passed a <c>null</c> value.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Might be if the target API
        /// does not support the concept of pagination.
        /// </exception>
        public T Find(Predicate<T> predicate)
        {
            if (predicate == null) // required value
                throw new ArgumentNullException(nameof(predicate));

            T result = null;

            var iterator = _iterable.GetIterator();

            if (iterator == null)
                return result;

            // save a backup of the current page size configured for the
            // iterator. For this operation only, we will ramp the page size to
            // 1, so this way we can search for the desired item, one at a time
            // (with excess results being cached).
            var priorPageSize = iterator.PageSize;

            if (MaxPageSize >=
                1) // Make sure that 1 is a legal value for the page size.
                iterator.PageSize = 1;

            try
            {
                var current = default(T);

                /*
                 * NOTE: You may be wondering why we are using a do/while loop here rather than a
                 * while loop, such as is the case for most examples of using IEnumerator to iterate
                 * through a collection.  Datasets provided by REST APIs implement greedy cursors.
                 * This is to say, you call a method to fetch the first page and then get a pagination
                 * resource over and over again until finally the pagination resource has a null for the
                 * next item.  Meaning, REST API data sets are treated very much in the same way as
                 * C linked lists.  it's impossible to first test 'has next' and then 'move next' -- you
                 * have to do the opposite -- get the first page with the method call that does this,
                 * and then retrieve each subsequent pagination resource until the pagination resource
                 * reports that there is no next page.
                 */

                do
                {
                    current = iterator.Current;
                    if (current == null || !predicate(current))
                        continue;
                    result = current;
                    break;
                } while (current != null && iterator.MoveNext());
            }
            catch (Exception ex)
            {
                OnIterationError(
                    new IterationErrorEventArgs(
                        new IteratorException(
                            "A problem was occurred during the iteration operation.",
                            ex
                        )
                    )
                );

                // in the event an exception occurred, just return the empty list
                result = default;
            }

            // restore the prior value for the iterator object's page size.
            iterator.PageSize = priorPageSize;

            return result;
        }

        /// <summary>
        /// Strives to invoke the appropriate GET method exposed by the target
        /// REST API to simply retrieve the object matching the specified
        /// <paramref name="searchParams" /> without pagination or iteration.
        /// </summary>
        /// <param name="searchParams">
        /// (Required.) A
        /// <see cref="T:System.Dynamic.ExpandoObject" /> whose parameters contain search
        /// values (or <c>null</c> s, if allowed by various REST APIs) to be fed to the
        /// target REST API method that retrieves the desired element of the dataset
        /// exposed by the API.
        /// </param>
        /// <returns>
        /// Reference to an instance of an object of type
        /// <typeparam name="T" />
        /// that contains the data from the found element or <c>null</c> if not found.
        /// </returns>
        /// <remarks>
        /// If a target REST API supports it, clients of this repository may want
        /// to avail themselves of this method when they know that their request needs to
        /// result in a single API call and a single element to be returned from the
        /// dataset.
        /// <para />
        /// At first glance, it would appear that this method is a duplicate of
        /// <see cref="M:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.Find" />.
        /// <para />
        /// Implementers should make this method call the REST API method that directly
        /// retrieves the object satisfying the provided criteria; if such a method is not
        /// available, then implementers should delegate to the
        /// <see cref="M:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.Find" />
        /// method.
        /// <para />
        /// This repository provides these two seemingly redundant ways of searching for
        /// objects since not all REST API controllers expose the same functionality set or
        /// have the same rate-limit concerns.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is
        /// made on the library that accesses the target REST API in the event the
        /// operation was not successful.
        /// </exception>
        /// <exception cref="T:ArgumentNullException">
        /// Thrown if the required parameter,
        /// <paramref name="searchParams" /> , is set to a <c>null</c> reference.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Might be if the target API
        /// does not support the concept of pagination.
        /// </exception>
        public abstract T Get(dynamic searchParams);

        /// <summary>
        /// Obtains the gamut of elements in the target REST API dataset, using
        /// the largest page size as more of a 'chunk size', using as many calls to the
        /// target REST API as are necessary. Extreme caution should be used with both
        /// implementing and calling this method, both due to rate-limiting, communications
        /// bandwidth, and memory storage concerns.
        /// </summary>
        /// <returns>
        /// Collection of instances of the element model object,
        /// <typeparamref name="T" /> , that can be used to further narrow the results.
        /// Implementers should write the code for this method to make as aggressive an
        /// attempt as possible to access the gamut of the available objects exposed by the
        /// target REST API endpoint.
        /// </returns>
        /// <remarks>
        /// Implementers must throw <see cref="T:System.NotSupportedException" />
        /// in the event that the target REST API does not support retrieving its entire
        /// available value set of elements.
        /// <para />
        /// <strong>EXTREME CAUTION</strong> This method is all-or-nothing. If the dataset
        /// is large, or infinite, then this method will have severe performance issues.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is
        /// made on the library that accesses the target REST API in the event the
        /// operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does
        /// not support getting the entire available collection of data elements in the
        /// server's database, even with paging.
        /// </exception>
        public virtual IEnumerable<T> GetAll()
        {
            IList<T> result = new AdvisableCollection<T>();

            var iterator = _iterable.GetIterator();
            if (iterator == null)
                return result;

            // save a backup of the current page size configured for the
            // iterator. For this operation only, we will ramp the page size to
            // the maximum, in an effort to minimize the number of calls to the
            // API, in order to limit the amount of requests.
            var priorPageSize = iterator.PageSize;

            if (MaxPageSize >= 1)
                iterator.PageSize = MaxPageSize;

            try
            {
                var current = default(T);

                /*
                 * NOTE: You may be wondering why we are using a do/while loop here rather than a
                 * while loop, such as is the case for most examples of using IEnumerator to iterate
                 * through a collection.  Datasets provided by REST APIs implement greedy cursors.
                 * This is to say, you call a method to fetch the first page and then get a pagination
                 * resource over and over again until finally the pagination resource has a null for the
                 * next item.  Meaning, REST API data sets are treated very much in the same way as
                 * C linked lists.  it's impossible to first test 'has next' and then 'move next' -- you
                 * have to do the opposite -- get the first page with the method call that does this,
                 * and then retrieve each subsequent pagination resource until the pagination resource
                 * reports that there is no next page.
                 */

                do
                {
                    current = iterator.Current;
                    if (current == null) break;
                    result.Add(current);
                } while (current != null && iterator.MoveNext());
            }
            catch (Exception ex)
            {
                OnIterationError(
                    new IterationErrorEventArgs(
                        new IteratorException(
                            "A problem was occurred during the iteration operation.",
                            ex
                        )
                    )
                );

                // in the event an exception occurred, just return the empty list
                result = new AdvisableCollection<T>();
            }

            // restore the prior value for the iterator object's page size.
            iterator.PageSize = priorPageSize;

            return result;
        }

        /// <summary>
        /// Calls a PUT method on the target REST API (if supported) to change
        /// the data element specified by the criteria in
        /// <paramref name="recordToUpdate" />.
        /// </summary>
        /// <param name="recordToUpdate"> </param>
        /// <remarks>
        /// If the target REST API does not support the concept of updating
        /// specific data elements, then implementers must throw
        /// <see cref="T:System.NotSupportedException" />.
        /// <para />
        /// It should be noted that there is no Save method in this repository pattern.
        /// This is due to the fact that, when making this kind of call on a REST API,
        /// changes are (conventionally) applied immediately.
        /// <para />
        /// Implementers are free to deny access to this functionality (even if the target
        /// REST API supports it) by throwing <see cref="T:System.NotSupportedException" />
        /// .
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is
        /// made on the library that accesses the target REST API in the event the
        /// operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does
        /// not support a single element of its dataset, or if this repository chooses to
        /// not allow access to that functionality.
        /// </exception>
        public abstract void Update(T recordToUpdate);

        /// <summary>
        /// Raises the
        /// <see cref="E:xyLOGIX.Api.Data.Iterators.IteratorBase.IterationError" /> event.
        /// </summary>
        /// <param name="e">
        /// A
        /// <see cref="T:xyLOGIX.Api.Data.Iterators.Events.IterationErrorEventArgs" /> that
        /// contains the event data.
        /// </param>
        [Yielder]
        protected virtual void OnIterationError(IterationErrorEventArgs e)
            => IterationError?.Invoke(this, e);
    }
}
