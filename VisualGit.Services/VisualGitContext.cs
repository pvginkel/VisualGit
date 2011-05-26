using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using VisualGit.VS;
using System.Windows.Forms.Design;

namespace VisualGit
{
    /// <summary>
    /// Globally available context; the entry point for the service framework.
    /// </summary>
    /// <remarks>Members should only be added for the most common operations. Everything else should be handled via the <see cref="IVisualGitServiceProvider"/> methods</remarks>
    public class VisualGitContext : VisualGitService, IVisualGitServiceProvider
    {
        readonly IVisualGitServiceProvider _runtime;

        protected VisualGitContext(IVisualGitServiceProvider context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _runtime = GetService<VisualGitRuntime>();
        }

        /// <summary>
        /// Creates a new <see cref="VisualGitContext"/> instance over the specified context
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static VisualGitContext Create(IVisualGitServiceProvider context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return new VisualGitContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="VisualGitContext"/> instance over the specified context
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static VisualGitContext Create(IServiceProvider context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            IVisualGitServiceProvider sp = context as IVisualGitServiceProvider;

            if (sp != null)
                return new VisualGitContext(sp);
            else
                return new VisualGitContext(new VisualGitServiceProviderWrapper(context));
        }

        /// <summary>
        /// Gets a IWin32Window handle to a window which should be used as dialog owner
        /// </summary>
        /// <value>The dialog owner or null if no dialog owner is registered.</value>
        public IWin32Window DialogOwner
        {
            get
            {
                IUIService service = GetService<IUIService>();

                if (service != null)
                    return service.GetDialogOwnerWindow();
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to get</typeparam>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        [DebuggerStepThrough]
        public new T GetService<T>()
            where T : class
        {
            return base.GetService<T>();
        }

        /// <summary>
        /// Gets the service of the specified type safely casted to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new T GetService<T>(Type serviceType)
            where T : class
        {
            return base.GetService<T>(serviceType);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        [DebuggerStepThrough]
        public new object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }
    }
}
