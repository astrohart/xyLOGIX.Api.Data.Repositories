# xyLOGIX.Api.Data.Repositories `class library`

This class library project serves the following purposes:

* Marry the concepts of accessing a REST API dataset of unknown length and iterating/paging through that data set;
* The Repository data-access pattern from the N-Tier application world.

**NOTE:** This library merely specifies the _framework_ for implementing this pattern; applications must refer to the libraries in this repo and the ones on which it depends, and then implement their own custom functionality to pull the data required and expose the actions listed below to clients.

That being said, the idea is, we are trying to build up a way to treat a REST API's paginated data as if it were a relational database table.

Each `ApiRepository` object, whose functionality is exposed by the `IApiRepository` interface, has the following actions (suppose `T` is the type of POCO that holds a single element of the dataset):

* `Attach(IIterable<T>)` - Specify an `IIterable<T>` that can be used to access the underlying REST API dataset;
* `Delete(T)` - If applicable (means, supported by the underlying REST API), will search for the item specified in the REST API's dataset and call on the API to delete its data from its own data sources;
* `DeleteAll(Predicate)` - If applicable (means, supported by the underlying REST API), will use a `Predicate` to determine which elements of the underlying collection exposed by the REST API are to be removed from it;
* `Find(Predicate<T>)` - If applicable (means, supported by the underlying REST API), iterates through the dataset provided by the API, element-by-element, testing each element against the supplied `Predicate`.  A reference to the first occurrence of an element satisfying the conditions in the `Predicate` is returned.
* `Get(dynamic)` - If supported by the underlying REST API, invokes the `HTTP GET` method necessary to simply pluck the desired element from the dataset directly, without doing any iteration.  The parameter is left as `dynamic` so that arbitrary search parameters can be supplied.
* `GetAll` basically implements a cursor that greedily consumes the entire REST API dataset -- however many elements may be -- across all available pages, and then returns an `IEnumerable<T>` on the entire collection.  This does not use `yield return` and `yield break` -- you get the entire collection back all at once.
* `Update(T)` - If applicable (means, supported by the underlying REST API), will use the values in the provided POCO instance to invoke `HTTP POST` on the REST API to update the element of the dataset that matches the values in `T`.

**NOTE**: Not all REST APIs are compatible with all of the actions listed above.  Any methods listed above that are for an unsupported action are free to be implemented such that they throw `NotSupportedException` if called.

Clients of this library will not need to know the implementation details of how exactly data must be obtained from a REST API data set; they just have to treat the data set just like any other database table, say, that you might encounter in SQL Server or somesuch.