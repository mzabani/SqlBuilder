SqlBuilder
==========

SqlBuilder helps build SQL queries programatically and list the results into objects just by matching the object's properties names to the selected names in the SQL query. The intent of this library is not to abstract away differences between DBs, nor to get in the way of the user. It aims to be let the user take control of the generated SQL, while providing "shortcuts", i.e. easier ways to achieve the desired result. Most of all, this is NOT AN ORM.

Usage
-----
Let's begin with an example. Suppose we have a class whose objects will hold our query results:
```
class Product {
  public int productid;
  public string name;
  public string description;
  public DateTime available_since;
}
```

Now let's fetch the results from the "products" table:

```
QueryBuilder qb = new QueryBuilder("products");

// The column with the "available_since" date is called "since" in the DB,
// so we will need to treat it a little differently
qb.AddColumnsOf<Product>(x => x.productid, x => x.name, x => x.description);
qb.Select("since AS available_since");

// Let's apply some filters
qb.Where(Cond.IsNotNull<Product>(x => x.name))

// Our Postgres text search too! Searching for descriptions with the words "porcelain" or "vase". This class's interface
// does not yet have a strongly typed constructor (check the string with the name of the field: "description").
// Calling "Where()" again ANDs this next condition with the entire previous one.
var tsvector = new TsVector("english", "description", true);
var tsquery = new TsQuery("english", "porcelain | vase", false);
qb.Where(SqlBuilder.Postgres.FullText.Match(tsvector, tsquery));

// Let's create one last condition just to illustrate a concept. If you want to compare two columns, you must call  
// ToSqlFragment() on one of them. This is an anti-programmer precautionary measure: if you want to compare 
// a column to a string, the current API will treat the string as a query parameter and not as a column to avoid 
// SQL Injection issues. If you want to compare two columns, you will have to be explicit. You still have to watch out to always
// use the first parameter as the column name and the second as the object, though.

// The condition is that we don't want products whose names are equal to their description
qb.Where(Cond.NotEqualTo("name", "description".ToSqlFragment()));

// Let's order by relevancy
qb.OrderBy(new OrderByFragment(new TsRank(tsvector, tsquery), OrderBy.Desc));

// And let's take only the results from 51 to 60
qb.Skip(50).Take(10);

// List the results! You have to have an open IDbConnection at this point.
List<Product> products = qb.List<Product>(openIDbConnection);
```

More
----
In this library, it is all about the **SqlFragment** class. This class represents a collection of text fragments and parameters. The **QueryBuilder** class, for example, makes use of many **SqlFragment**s to compose a query. All the conditions returned by the static methods in the **SqlBuilder.Cond** class return **WhereCondition**s, which are derived from **SqlFragment** themselves, but also contain methods to AND and OR other **WhereCondition**s together. If you want, you can create your own fragments to help you compose your queries easily: just inherit from **SqlFragment**.
