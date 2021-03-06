﻿using Autofac;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SnackMachineApp.Domain.SeedWork;
using SnackMachineApp.Infrastructure.Data;
using SnackMachineApp.Infrastructure.Data.EntityFramework;
using SnackMachineApp.Infrastructure.Data.NHibernate;
using SnackMachineApp.Infrastructure.Repositories;
using SnackMachineApp.Interface.Data;
using SnackMachineApp.Interface.Data.EntityFramework;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SnackMachineApp.Infrastructure
{
    public sealed class ObjectFactory : IServiceProvider
    {
        #region Singleton
        private static readonly Lazy<ObjectFactory> _lazy = new Lazy<ObjectFactory>(() => new ObjectFactory());

        public static ObjectFactory Instance => _lazy.Value;

        private ObjectFactory()
        {
            Init();
        }

        #endregion
        public object GetService(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        #region Private
        private IServiceProvider _serviceProvider;

        private void Init()
        {
            var serviceCollection = new ServiceCollection();

            // The Microsoft.Extensions.Logging package provides this one-liner
            // to add logging services.
            serviceCollection.AddLogging();

            var builder = new ContainerBuilder();
            builder.Populate(serviceCollection);

            IConfigurationBuilder config = new ConfigurationBuilder();
            config.AddJsonFile("autofac.json");

            var configModule = new ConfigurationModule(config.Build());

            builder.RegisterModule(configModule);

           
            var container = builder.Build();

            _serviceProvider = new AutofacServiceProvider(container);
        }

        private class DbConnectionProviderRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                var cmdConnectionString = new CommandsConnectionProvider(ConfigurationManager.ConnectionStrings["AppCnn"].ConnectionString);

                builder.RegisterInstance(cmdConnectionString).As<CommandsConnectionProvider>().SingleInstance();
            }
        }

        private class DapperRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                var queryConnectionString = new QueriesConnectionProvider(ConfigurationManager.ConnectionStrings["QueryAppCnn"].ConnectionString);
                builder.RegisterInstance(queryConnectionString).As<QueriesConnectionProvider>().SingleInstance();

                builder.Register(c => new SqlConnection(c.Resolve<QueriesConnectionProvider>().Value))
                    .As<IDbConnection>();

                builder.RegisterType<DapperRepositor1y>();
            }
        }

        private class NHibernateRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                //RegisterInstance method allows you to register an instance not built by Autofac.
                //https://stackoverflow.com/questions/31582000/autofac-registerinstance-vs-singleinstance
                builder.RegisterType<SessionFactory>().SingleInstance();
                builder.RegisterType<NHibernateUnitOfWork>().As<ITransactionUnitOfWork>().InstancePerLifetimeScope();
                builder.RegisterGeneric(typeof(NHibernateDbPersister<>)).As(typeof(IDbPersister<>));
            }
        }

        private class EfRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                //RegisterInstance method allows you to register an instance not built by Autofac.
                //https://stackoverflow.com/questions/31582000/autofac-registerinstance-vs-singleinstance
                builder.RegisterType<EfUnitOfWork>().As<ITransactionUnitOfWork>().InstancePerLifetimeScope();
                builder.RegisterType<AppDbContext>().As<DbContext>();
                builder.RegisterGeneric(typeof(EFDbPersister<>)).As(typeof(IDbPersister<>));
            }
        }

        private class RepositoryRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterAssemblyTypes(typeof(Repository<>).Assembly)
                    .Where(t => t.Name.EndsWith("Repository"))
                    .As(t => t.GetInterfaces()?.FirstOrDefault(
                        i => i.Name == "I" + t.Name))
                    .PropertiesAutowired();

                //working: builder.RegisterType<HeadOffice>().InjectFields(true);
                //builder.RegisterType(typeof(Repository<>)).InjectFields();
                builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).PropertiesAutowired();
            }
        }

        private class DomainEventHandlersRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                //When hosting applications in IIS all assemblies are loaded into the AppDomain when the application first starts, 
                //but when the AppDomain is recycled by IIS the assemblies are then only loaded on demand.
                //System.Web.Compilation.BuildManager.GetReferencedAssemblies().Cast<Assembly>();
                //https://autofaccn.readthedocs.io/en/latest/register/scanning.html

                builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                        .Where(t => t.Name.EndsWith("EventHandler") &&
                            t.GetInterfaces().Any(y => y.IsGenericType && 
                            y.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                        .AsImplementedInterfaces();

                builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                    .Where(t => t.Name.EndsWith("EventHandler") &&
                            t.GetInterfaces().Any(y => y.GetType() == typeof(IDomainEventHandler)))
                    .AsClosedTypesOf(typeof(IDomainEventHandler));

            }
        }

        private class MediatorRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                //builder.RegisterType<Mediator>().As<IMediator>().SingleInstance();
                //builder.RegisterType<PaymentGateway>().As<IPaymentGateway>().SingleInstance();
                //builder.RegisterType<DomainEventDispatcher>().As<IDomainEventDispatcher>().SingleInstance();

                //builder.RegisterType<WithdrawCommandHandler>().As<IRequestHandler<WithdrawCommand, Atm>>().SingleInstance();
                //builder.RegisterType<BuySnackCommandHandler>().As<IRequestHandler<BuySnackCommand, SnackMachine>>().SingleInstance();
            }
        }

        private class DecoratorsRegistrationModule : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {

                //builder.RegisterDecorator<ThreadScopedSnackMachineServiceDecorator, ISnackMachineService>();

                //builder.RegisterDecorator<ISnackMachineService>(
                //    (c, inner) => c.ResolveNamed<ISnackMachineService>("decorator", TypedParameter.From(inner)), "original")
                //    .As<ISnackMachineService>();

                //builder.RegisterType<ThreadScopedSnackMachineServiceDecorator>()
                //       .InstancePerLifetimeScope();

            }
        }

        #endregion Private
    }

    public static class AutofacExtensions
    {
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> InjectFields
            (this IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> builder)
        {
            builder.OnActivated(args => InjectFields(args.Context, args.Instance));

            return builder;
        }

        public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> InjectFields<T>
            (this IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> builder)
        {
            builder.OnActivated(args => InjectFields(args.Context, args.Instance));

            return builder;
        }

        public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> InjectProperties<T>
            (this IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> builder, bool overrideSetValues = true)
        {
            builder.OnActivated(args => InjectProperties(args.Context, args.Instance, overrideSetValues));

            return builder;
        }

        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> InjectProperties
              (this IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> builder)
        {
            builder.OnActivated(args => InjectProperties(args.Context, args.Instance));

            return builder;
        }

        public static void InjectProperties(IComponentContext context,
               object instance, bool overrideSetValues = true)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (instance == null)
                throw new ArgumentNullException("instance");
            foreach (
               PropertyInfo propertyInfo in
                   //BindingFlags.NonPublic flag added for non public properties
                   instance.GetType().GetProperties(BindingFlags.Instance |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic))
            {
                Type propertyType = propertyInfo.PropertyType;
                if ((!propertyType.IsValueType || propertyType.IsEnum) &&
                    propertyInfo.GetIndexParameters().Length == 0 &&
                        context.IsRegistered(propertyType))
                {
                    //Changed to GetAccessors(true) to return non public accessors
                    MethodInfo[] accessors = propertyInfo.GetAccessors(true);
                    if ((accessors.Length != 1 ||
                        !(accessors[0].ReturnType != typeof(void))) &&
                         (overrideSetValues || accessors.Length != 2 ||
                         propertyInfo.GetValue(instance, null) == null))
                    {
                        object obj = context.Resolve(propertyType);
                        propertyInfo.SetValue(instance, obj, null);
                    }
                }
            }
        }

        public static void InjectFields(IComponentContext context, object instance)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (instance == null)
                throw new ArgumentNullException("instance");

            foreach (var fieldInfo in instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                Type fieldType = fieldInfo.FieldType;
                if (//(!fieldType.IsValueType || fieldType.IsEnum) && 
                    context.IsRegistered(fieldType))
                {
                    object obj = context.Resolve(fieldType);

                    fieldInfo.SetValue(instance, obj);
                }
            }
        }
    }

}
