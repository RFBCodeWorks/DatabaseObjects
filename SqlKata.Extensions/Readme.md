# RFBCodeworks.SqlKata.Extensions
## About

This library primarily contains extension methods for converting SqlKata.Query objects to System.Data.DbCommand objects.

This library also provides some helper objects that can be used to dynamically create SqlKata queries.

## SelectStatementBuilder
- This class is designed to help with dynamically building a SELECT query
- It accepts IWhereCondition objects that will provide the 'WHERE' clauses to determine what should be selected.

## IWhereCondition
Interface that allows an object to apply a 'WHERE' clause to a SqlKata.Query

## WhereOperators
These objects represent the various 'operators' in use by the library, and ensure that a proper operator (such as '=') is supplied to the query builder.

## Where* classes
These objects generate various types of 'Where' clauses. 
For example, the 'WhereNumericValue' class will accept a numeric value, a NumericOperator and a column name, then generate the following clause:
	"WHERE [Column] = Value"
The actual sql being generated here though will be determined by the SqlKata.Compiler object that is used.

## Required Libraries
- SqlKata