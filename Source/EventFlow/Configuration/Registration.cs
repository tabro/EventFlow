﻿// The MIT License (MIT)
//
// Copyright (c) 2015 Rasmus Mikkelsen
// https://github.com/rasmus/EventFlow
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using Autofac;
using EventFlow.Configuration.Resolvers;

namespace EventFlow.Configuration
{
    public enum Lifetime
    {
        AlwaysUnique,
        Singleton,
    }

    public class Registration
    {
        private readonly Type _implementationType;

        public Type ServiceType { get; protected set; }
        public Lifetime Lifetime { get; protected set; }

        public Registration(){ }

        public Registration(Type serviceType, Type implementationType, Lifetime lifetime = Lifetime.AlwaysUnique)
        {
            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException(string.Format(
                    "Type '{0}' is not assignable to '{1}'",
                    implementationType.Name,
                    serviceType.Name));
            }

            ServiceType = serviceType;
            Lifetime = lifetime;
            _implementationType = implementationType;
        }

        internal virtual void Configure(ContainerBuilder containerBuilder)
        {
            switch (Lifetime)
            {
                case Lifetime.AlwaysUnique:
                    containerBuilder.RegisterType(_implementationType).As(ServiceType);
                    break;
                case Lifetime.Singleton:
                    containerBuilder.RegisterType(_implementationType).As(ServiceType).SingleInstance();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Registration<TService> : Registration
        where TService : class
    {
        public Func<IResolver, object> Factory { get; protected set; }

        public Registration(Func<IResolver, TService> factory, Lifetime lifetime = Lifetime.AlwaysUnique)
        {
            ServiceType = typeof (TService);
            Factory = factory;
            Lifetime = lifetime;
        }

        internal override void Configure(ContainerBuilder containerBuilder)
        {
            switch (Lifetime)
            {
                case Lifetime.AlwaysUnique:
                    containerBuilder.Register(cc => Factory(new AutofacResolver(cc))).As(ServiceType);
                    break;
                case Lifetime.Singleton:
                    containerBuilder.Register(cc => Factory(new AutofacResolver(cc))).As(ServiceType).SingleInstance();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Registration<TService, TImplementation> : Registration
        where TImplementation : class, TService
    {
        public Registration(Lifetime lifetime = Lifetime.AlwaysUnique)
        {
            Lifetime = lifetime;
            ServiceType = typeof (TService);
        }

        public override string ToString()
        {
            return string.Format(
                "{{Service: {0}, Implementation: {1}, Lifetime: {2}}}",
                typeof(TService).Name,
                typeof(TImplementation).Name,
                Lifetime);
        }

        internal override void Configure(ContainerBuilder containerBuilder)
        {
            switch (Lifetime)
            {
                case Lifetime.AlwaysUnique:
                    containerBuilder.RegisterType<TImplementation>().As<TService>();
                    break;
                case Lifetime.Singleton:
                    containerBuilder.RegisterType<TImplementation>().As<TService>().SingleInstance();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
