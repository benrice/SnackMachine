

# DDD implementation of Snack Machine

## Overview

DDD implementation of Snack Machine, coming from [design-in-practice](https://app.pluralsight.com/library/courses/domain-driven-design-in-practice/) course.


## Libraries used

* **Mediator pattern** - process(request) delegator inside [Application\Seedwork](https://github.com/bahmani00/SnackMachine/tree/master/SnackMachineApp.Application/Seedwork).
* **Dapper** - to query database
* **GuardClauses** - use [GuardClauses](https://github.com/ardalis/GuardClauses)
* **FluentValidation, xUnit** - for unit testing
* **AutoMapper** - define mappings between Domain Entities and DTOs.
* **NHibernate** - .Net ORM to access data.
* **Entity Framework Core** - MS .Net ORM to access data.
* **Autofac & .Net DependencyInjection** - Dependency Injection library to wire project's dependencies into .Net built-in DI(ServiceCollection).

## Getting started

1. Clone the repo.
2. Run 'database.sql' on sql server.
3. Set ConnectionString on 'App.config'
3. Run the WPF project
4. Switch to either NHibernate or Ef through `autofac.json`.

## Vocabularies

1. Seedwork
2. Mediator
3. UnitOfWork
4. Domain Event
5. Anemic class

## TODOs

* Checkout [Issues](https://github.com/bahmani00/SnackMachine/issues)
* TODOs comments in the codebase
