using System;
using System.Collections.Generic;
using System.Dynamic;
using xyLOGIX.Api.Data.Iterators.Events;

namespace xyLOGIX.Api.Data.Repositories.Interfaces
{
    /// <summary>
    /// Defines the public-exposed methods and properties of an object that
    /// implements an interface styled after a Repository pattern, but which is
    /// really a Facade for consuming paginated (but potentially infinite) REST
    /// API results.
    /// </summary>
    /// <typeparam name="T">
    /// Name -- should be that of a concrete type -- of the type of a POCO that
    /// specifies a single element of the collection exposed by this repository.
    /// <para />
    /// An interface name may be used; implementers must then adapt their
    /// methods' outputs to the interface specified, in this case.
    /// <para />
    /// Implementers and clients are cautioned that if this type parameter is
    /// supplied with an interface, further generic abstraction may be required
    /// in other areas of the software system.
    /// </typeparam>
    /// <remarks>
    /// Implementers are free to deny access to specific functionality by
    /// throwing <see cref="T:System.NotSupportedException" /> for any of the
    /// methods this interface exposes, given varying target REST API use-case
    /// and support scenarios.
    /// </remarks>
    public interface IApiRepository<T> where T : class
    {
        /// <summary>
        /// Gets or sets the maximum number of elements per page that the API
        /// will allow to be fetched.
        /// </summary>
        /// <remarks>
        /// This quantity is specified by nearly every REST API out there. This
        /// property is set by a required constructor parameter.
        /// </remarks>
        int MaxPageSize { get; }

        /// <summary>
        /// Gets or sets the page size, i.e., how many elements to request at a
        /// time from the target REST API.
        /// </summary>
        /// <remarks>
        /// The Find, Delete, DeleteAll, and Update methods, by default, iterate
        /// through the target REST API's data set a single element at a time.
        /// <para />
        /// Because we have to be careful about not hitting rate limits during
        /// these operations, this property allows clients of this class to
        /// customize the number of elements taken at a time to be different
        /// from 1 by setting this property.
        /// </remarks>
        int PageSize { get; set; }

        /// <summary>
        /// Occurs when an exception is thrown during the iteration process.
        /// </summary>
        event IterationErrorEventHandler IterationError;

        /// <summary>
        /// If offered by the endpoint, uses any DELETE request exposed to
        /// remove something from the target REST API's dataset.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object that represents a single element in the target
        /// REST API endpoint's dataset.
        /// </typeparam>
        /// <param name="recordToDelete">
        /// (Required.) Reference to an instance of the model type,
        /// <typeparamref name="T" /> , that specifies which object should be
        /// deleted from the API's dataset.
        /// </param>
        /// <remarks>
        /// Not all REST APIs expose a means of deleting items from their
        /// datasets. In this case, implementations of this method must throw
        /// <see cref="T:System.NotSupportedException" />
        /// <para />
        /// Implementers are free to deny access to this functionality (even if
        /// the target REST API supports it) by throwing
        /// <see cref="T:System.NotSupportedException" />.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is made on the library that
        /// accesses the target REST API in the event the operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does not support the concept of element deletion.
        /// </exception>
        void Delete(T recordToDelete);

        /// <summary>
        /// If supported by the target REST API, removes all elements from the
        /// dataset that satisfy the criteria expressed by the supplied
        /// <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// (Required.) Predicate expression that returns either <c>true</c> or
        /// <c>false</c> when supplied with an instance of the element model
        /// type, <typeparamref name="T" />, as a parameter.
        /// <para />
        /// By element model we mean an instance of whatever POCO is supplied by
        /// the library providing access to the target REST API that represents
        /// a single element of the dataset.
        /// <para />
        /// If the predicate returns <c>true</c> for a given instance of the
        /// element model object, then this object strives to remove that
        /// element from the dataset using the appropriate method call on the
        /// target REST API's client library.
        /// </param>
        /// <remarks>
        /// Not all REST APIs expose a means of deleting items from their
        /// datasets. In this case, implementations of this method must throw
        /// <see cref="T:System.NotSupportedException" />.
        /// <para />
        /// Implementers are free to deny access to this functionality (even if
        /// the target REST API supports it) by throwing
        /// <see cref="T:System.NotSupportedException" />.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is made on the library that
        /// accesses the target REST API in the event the operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does not support the concept of element deletion.
        /// </exception>
        void DeleteAll(Predicate<T> predicate);

        /// <summary>
        /// Iterates through the dataset of the target REST API,
        /// <see
        ///     cref="P:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.PageSize" />
        /// elements at a time (default is 1), and tries to find an element
        /// matching the criteria provided.
        /// </summary>
        /// <param name="predicate">
        /// (Required.) Lambda expression specifying how to tell if an element
        /// is to be retrieved.
        /// </param>
        /// <returns>
        /// This method iterates through the dataset of the target REST API,
        /// testing each element against the provided
        /// <paramref
        ///     name="predicate" />
        /// . The first element for which the
        /// <paramref
        ///     name="predicate" />
        /// evaluates to <c>true</c> is then returned,
        /// or <c>null</c> if an error occurred or the matching element was
        /// otherwise not found.
        /// </returns>
        /// <remarks>
        /// Clients of this repository should use this method instead of
        /// invoking the GetAll operation and then filtering with the LINQ Where
        /// method, in order to retrieve just those API elements that need to be
        /// retrieved until the desired one is found.
        /// <para />
        /// GetAll will suck down the entire dataset, and this may not be
        /// desirable because of rate limits etc.
        /// <para />
        /// Implementations should throw
        /// <see
        ///     cref="T:System.NotSupportedException" />
        /// in the event that the API
        /// does not support pagination -- or delegate the call to this object's
        /// GetAll followed by <see cref="M:System.Linq.Enumerable.Where" />
        /// followed by <see cref="M:System.Linq.Enumerable.FirstOrDefault" />.
        /// <para />
        /// Alternatively, implementers may delegate this method to
        /// <see cref="M:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.Get" />.
        /// <para />
        /// This repository provides these two seemingly redundant ways of
        /// searching for objects since not all REST API controllers expose the
        /// same functionality set or have the same rate-limit concerns.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is made on the library that
        /// accesses the target REST API in the event the operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Might be if the target API does not support the concept of pagination.
        /// </exception>
        T Find(Predicate<T> predicate);

        /// <summary>
        /// Strives to invoke the appropriate GET method exposed by the target
        /// REST API to simply retrieve the object matching the specified
        /// <paramref name="searchParams" /> without pagination or iteration.
        /// </summary>
        /// <param name="searchParams">
        /// (Required.) A <see cref="T:System.Dynamic.ExpandoObject" /> whose
        /// parameters contain search values (or <c>null</c> s, if allowed by
        /// various REST APIs) to be fed to the target REST API method that
        /// retrieves the desired element of the dataset exposed by the API.
        /// </param>
        /// <returns>
        /// Reference to an instance of an object of type
        /// <typeparam name="T" />
        /// that contains the data from the found element or <c>null</c> if not found.
        /// </returns>
        /// <remarks>
        /// If a target REST API supports it, clients of this repository may
        /// want to avail themselves of this method when they know that their
        /// request needs to result in a single API call and a single element to
        /// be returned from the dataset.
        /// <para />
        /// At first glance, it would appear that this method is a duplicate of
        /// <see cref="M:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.Find" />.
        /// <para />
        /// Implementers should make this method call the REST API method that
        /// directly retrieves the object satisfying the provided criteria; if
        /// such a method is not available, then implementers should delegate to
        /// the
        /// <see
        ///     cref="M:xyLOGIX.Api.Data.Repositories.Interfaces.IApiRepository.Find" />
        /// method.
        /// <para />
        /// This repository provides these two seemingly redundant ways of
        /// searching for objects since not all REST API controllers expose the
        /// same functionality set or have the same rate-limit concerns.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is made on the library that
        /// accesses the target REST API in the event the operation was not successful.
        /// </exception>
        /// <exception cref="T:ArgumentNullException">
        /// Thrown if the required parameter, <paramref name="searchParams" />
        /// , is set to a <c>null</c> reference.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Might be if the target API does not support the concept of pagination.
        /// </exception>
        T Get(ExpandoObject searchParams);

        /// <summary>
        /// Obtains the gamut of elements in the target REST API dataset, using
        /// the largest page size as more of a 'chunk size', using as many calls
        /// to the target REST API as are necessary. Extreme caution should be
        /// used with both implementing and calling this method, both due to
        /// rate-limiting, communications bandwidth, and memory storage concerns.
        /// </summary>
        /// <returns>
        /// Collection of instances of the element model object,
        /// <typeparamref
        ///     name="T" />
        /// , that can be used to further narrow the results.
        /// Implementers should write the code for this method to make as
        /// aggressive an attempt as possible to access the gamut of the
        /// available objects exposed by the target REST API endpoint.
        /// </returns>
        /// <remarks>
        /// Implementers must throw <see cref="T:System.NotSupportedException" />
        /// in the event that the target REST API does not support retrieving
        /// its entire available value set of elements.
        /// <para />
        /// This method is all-or-nothing.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is made on the library that
        /// accesses the target REST API in the event the operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does not support getting the entire
        /// available collection of data elements in the server's database, even
        /// with paging.
        /// </exception>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Calls a PUT method on the target REST API (if supported) to change
        /// the data element specified by the criteria in
        /// <paramref name="recordToUpdate" />.
        /// </summary>
        /// <param name="recordToUpdate">
        /// </param>
        /// <remarks>
        /// If the target REST API does not support the concept of updating
        /// specific data elements, then implementers must throw
        /// <see cref="T:System.NotSupportedException" />.
        /// <para />
        /// It should be noted that there is no Save method in this repository
        /// pattern. This is due to the fact that, when making this kind of call
        /// on a REST API, changes are (conventionally) applied immediately.
        /// <para />
        /// Implementers are free to deny access to this functionality (even if
        /// the target REST API supports it) by throwing
        /// <see cref="T:System.NotSupportedException" />.
        /// </remarks>
        /// <exception cref="T:System.Exception">
        /// Bubbled up from whichever method call is made on the library that
        /// accesses the target REST API in the event the operation was not successful.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown if the target API does not support a single element of its
        /// dataset, or if this repository chooses to not allow access to that
        /// functionality.
        /// </exception>
        void Update(T recordToUpdate);
    }
}