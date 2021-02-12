using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ZOOM_SDK_DOTNET_WRAP;

namespace ZoomJWAssistant.Core.Threading
{
    class BaseUIDispatchingService
    {
        protected Dispatcher uiDispatcher = Application.Current.Dispatcher;
        protected long nextInvocationTime = 0L;

        protected void InvokeAPI(Action codeToInvoke)
        {
            uiDispatcher.Invoke(DispatcherPriority.Normal,
                new Action(delegate
                {
                    codeToInvoke.Invoke();
                })
            );
        }

        protected T InvokeAPI<T>(Func<T> codeToInvoke)
        {
            if (nextInvocationTime > 0L)
            {
                var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var diff = DateTimeOffset.FromUnixTimeMilliseconds(nextInvocationTime).AddMilliseconds(-1 * now).ToUnixTimeMilliseconds();

                if (diff > 0L)
                {
                    Thread.Sleep((int) diff);
                    return InvokeAPI(codeToInvoke);
                }
            }

            var result = (T)uiDispatcher.Invoke(DispatcherPriority.Normal,
                new Func<T>(delegate
                {
                    return codeToInvoke.Invoke();
                })
            );

            if (result != null && result.GetType().IsEnum)
            {
                var enumType = result.GetType();
                var underlyingType = Enum.GetUnderlyingType(enumType);
                var numericValue = (int) System.Convert.ChangeType(result, underlyingType);
                if (numericValue == 18)
                {
                    nextInvocationTime = DateTimeOffset.Now.AddMilliseconds(150).ToUnixTimeMilliseconds();
                    result = InvokeAPI(codeToInvoke);
                }
            }

            return result;
        }
    }
}
