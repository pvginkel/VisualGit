using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace VisualGit
{
    public interface IVisualGitServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to get</typeparam>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        T GetService<T>()
            where T : class;


        /// <summary>
        /// Gets the service of the specified type safely casted to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        T GetService<T>(Type serviceType)
            where T : class;
    }

    /// <summary>
    /// Simple service container implementing <see cref="IVisualGitServiceProvider"/>
    /// </summary>
    public class VisualGitServiceContainer : ServiceContainer, IVisualGitServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualGitServiceContainer"/> class.
        /// </summary>
        /// <param name="parentProvider">A parent service provider.</param>
        public VisualGitServiceContainer(IServiceProvider parentProvider)
            : base(parentProvider)
        {
            AddService(typeof(IVisualGitServiceProvider), this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualGitServiceContainer"/> class.
        /// </summary>
        public VisualGitServiceContainer()
        {
            AddService(typeof(IVisualGitServiceProvider), this);
        }

        #region IVisualGitServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to get</typeparam>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        [DebuggerStepThrough]
        public T GetService<T>()
            where T : class
        {
            return (T)GetService(typeof(T));
        }

        #endregion

        #region IVisualGitServiceProvider Members

        /// <summary>
        /// Gets the service of the specified type safely casted to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public T GetService<T>(Type serviceType)
            where T : class
        {
            return GetService(serviceType) as T;
        }

        #endregion
    }


    /// <summary>
    /// Simple service container implementing <see cref="IVisualGitServiceProvider"/>
    /// </summary>
    public class VisualGitServiceProviderWrapper : IVisualGitServiceProvider
    {
        readonly IServiceProvider _parentProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualGitServiceContainer"/> class.
        /// </summary>
        /// <param name="parentProvider">A parent service provider.</param>
        public VisualGitServiceProviderWrapper(IServiceProvider parentProvider)
        {
            if (parentProvider == null)
                throw new ArgumentNullException("parentProvider");

            _parentProvider = parentProvider;
        }

        #region IVisualGitServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
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

        #endregion

        #region IVisualGitServiceProvider Members

        /// <summary>
        /// Gets the service of the specified type safely casted to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public T GetService<T>(Type serviceType)
            where T : class
        {
            return GetService(serviceType) as T;
        }

        #endregion

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
            object value = _parentProvider.GetService(serviceType);

            if (value == null && serviceType == typeof(IVisualGitServiceProvider))
                return this;

            return value;
        }
    }

    public interface IVisualGitQueryService : IVisualGitServiceProvider
    {
        /// <summary>
        /// Gets the Visual Studio service by its guid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceGuid">The service GUID.</param>
        /// <returns></returns>
        T QueryService<T>(Guid serviceGuid)
            where T : class;
    }
}
