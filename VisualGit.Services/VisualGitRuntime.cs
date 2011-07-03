// VisualGit.Services\VisualGitRuntime.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;
using VisualGit.Commands;
using System.Diagnostics;
using System.Reflection;
using VisualGit.UI;
using System.Threading;
using System.Runtime.InteropServices;

namespace VisualGit
{
    public class VisualGitRuntime : IVisualGitServiceProvider
    {
        readonly IServiceContainer _container;
        readonly CommandMapper _commandMapper;
        readonly VisualGitContext _context;
        readonly List<IVisualGitServiceImplementation> _services = new List<IVisualGitServiceImplementation>();
        readonly VisualGitServiceEvents _events;
        bool _ensureServices;

        public VisualGitRuntime(IServiceContainer parentContainer)
        {
            if (parentContainer == null)
                throw new ArgumentNullException("parentContainer");

            _container = parentContainer;

            _commandMapper = GetService<CommandMapper>();
            if (_commandMapper == null)
                _container.AddService(typeof(CommandMapper), _commandMapper = new CommandMapper(this));

            _context = GetService<VisualGitContext>();
            if (_context == null)
                _container.AddService(typeof(VisualGitContext), _context = VisualGitContext.Create(this));

            InitializeServices();

            _events = GetService<VisualGitServiceEvents>();
        }

        public VisualGitRuntime(IServiceProvider parentProvider)
            : this((parentProvider as IServiceContainer) ?? new VisualGitServiceContainer(parentProvider))
        {
            if (parentProvider == null)
                throw new ArgumentNullException("parentProvider");            
        }

        void InitializeServices()
        {
            if (GetService<VisualGitRuntime>() == null)
            {
                _container.AddService(typeof(VisualGitRuntime), this, true);

                if (_container.GetService(typeof(IVisualGitServiceProvider)) == null)
                    _container.AddService(typeof(IVisualGitServiceProvider), this);
            }

            if (GetService<VisualGitServiceEvents>() == null)
            {
                VisualGitServiceEvents se = new VisualGitServiceEvents(Context);
                _container.AddService(typeof(VisualGitServiceEvents), se);
                _container.AddService(typeof(IVisualGitServiceEvents), se);
            }

            if(GetService<SynchronizationContext>() == null)
                _container.AddService(typeof(SynchronizationContext), new System.Windows.Forms.WindowsFormsSynchronizationContext());

#if DEBUG
            PreloadServicesViaEnsure = true;
#endif
        }

        /// <summary>
        /// Gets or sets a value indicating whether all modules must preload their services
        /// </summary>
        /// <value><c>true</c> if all services should preload their required services, otherwise <c>false</c>.</value>
        public bool PreloadServicesViaEnsure
        {
            get { return _ensureServices; }
            set { _ensureServices = value; }
        }

        #region IVisualGitServiceProvider Members

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T">The type of service to get</typeparam>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        [DebuggerStepThrough]
        public T GetService<T>()
            where T : class
        {
            return GetService<T>(typeof(T));
        }

        /// <summary>
        /// Gets the service of the specified type safely casted to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public T GetService<T>(Type type)
            where T : class
        {
            return GetService(type) as T;
        }

        #endregion

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        [DebuggerStepThrough]
        public object GetService(Type serviceType)
        {
            return _container.GetService(serviceType);
        }

        #endregion

        /// <summary>
        /// Gets the command mapper.
        /// </summary>
        /// <value>The command mapper.</value>
        public CommandMapper CommandMapper
        {
            get { return _commandMapper; }
        }

        /// <summary>
        /// Gets the single context instance
        /// </summary>
        /// <value>The context.</value>
        public VisualGitContext Context
        {
            get { return _context; }
        }

        readonly List<Module> _modules = new List<Module>();

        public void AddModule(Module module)
        {
            _modules.Add(module);

            module.OnPreInitialize();
        }

        public void Start()
        {
            foreach (Module module in _modules)
            {
                module.OnInitialize();
            }

            foreach (IVisualGitServiceImplementation service in _services)
            {
                service.OnInitialize();
            }

            if (_events != null)
            {
                _events.OnRuntimeLoaded(EventArgs.Empty);
                _events.OnRuntimeStarted(EventArgs.Empty);
            }
        }

        public static VisualGitRuntime Get(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            return (VisualGitRuntime)serviceProvider.GetService(typeof(VisualGitRuntime));
        }

        readonly static Type[] _serviceConstructorParams = new Type[] { typeof(IVisualGitServiceProvider) };

        /// <summary>
        /// Loads the services from the specified assembly and adds them to the container
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assembly">The assembly.</param>
        public void LoadServices(IServiceContainer container, System.Reflection.Assembly assembly)
        {
            LoadServices(container, assembly, Context);
        }

        /// <summary>
        /// Loads the services from the specified assembly and adds them to the specified container; pass context to the services
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="context">The context.</param>
        public void LoadServices(IServiceContainer container, System.Reflection.Assembly assembly, IVisualGitServiceProvider context)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            else if (context == null)
                throw new ArgumentNullException("context");

            object[] constructorArgs = null;
            foreach (Type type in assembly.GetTypes())
            {
                if (!typeof(IVisualGitServiceImplementation).IsAssignableFrom(type))
                {
#if DEBUG
                    if (type.GetCustomAttributes(typeof(GlobalServiceAttribute), false).Length > 0)
                        Debug.WriteLine(string.Format("Ignoring VisualGitGlobalServiceAttribute on {0} as it does not implement IVisualGitServiceImplementation", type.AssemblyQualifiedName));
#endif
                    continue;
                }

                IVisualGitServiceImplementation instance = null;

                foreach (GlobalServiceAttribute attr in type.GetCustomAttributes(typeof(GlobalServiceAttribute), false))
                {
                    Type serviceType = attr.ServiceType;
#if DEBUG
                    if (!serviceType.IsAssignableFrom(type))
                        throw new InvalidOperationException(string.Format("{0} does not implement global service {1} but has an attribute that says it does", type.AssemblyQualifiedName, serviceType.FullName));
#endif
                    if (attr.AllowPreRegistered && null != (container.GetService(serviceType)))
                        continue;

                    if (instance == null)
                    {
                        try
                        {
                            ConstructorInfo ci = type.GetConstructor(
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.ExactBinding,
                                null, _serviceConstructorParams, null);

                            if (ci == null)
                            {
                                string msg = string.Format("Servicetype {0} has no valid contructor", type.AssemblyQualifiedName);
                                Trace.WriteLine(msg);

                                throw new InvalidOperationException(msg);
                            }

                            if (constructorArgs == null)
                                constructorArgs = new object[] { context };

                            instance = (IVisualGitServiceImplementation)ci.Invoke(constructorArgs);
                        }
                        catch (Exception e)
                        {
                            VisualGitMessageBox mb = new VisualGitMessageBox(Context);

                            mb.Show(e.ToString());
                            continue;
                        }
                    }
#if DEBUG
                    if (attr.PublicService && !Marshal.IsTypeVisibleFromCom(serviceType))
                        Trace.WriteLine(string.Format("ServiceType {0} is not visible from com, but promoted", serviceType.AssemblyQualifiedName));
#endif
                    container.AddService(serviceType, instance, attr.PublicService);
                }

                if (instance != null)
                {
                    _services.Add(instance);
                    instance.OnPreInitialize();
                }
            }
        }
    }
}
