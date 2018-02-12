using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.AspNetCore.Mvc.Proxies;
using OpenTracing.Contrib.Internal;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.AspNetCore.Mvc
{
    public class MvcInterceptor : DiagnosticInterceptor
    {
        // Events
        private const string EventBeforeAction = "Microsoft.AspNetCore.Mvc.BeforeAction";
        private const string EventAfterAction = "Microsoft.AspNetCore.Mvc.AfterAction";
        private const string EventBeforeActionResult = "Microsoft.AspNetCore.Mvc.BeforeActionResult";
        private const string EventAfterActionResult = "Microsoft.AspNetCore.Mvc.AfterActionResult";

        private const string Component = "AspNetCoreMvc";

        private const string ActionOperationName = "mvc_action";
        private const string ActionItemKey = "ot-MvcAction";
        private const string ActionTagActionName = "action";
        private const string ActionTagControllerName = "controller";

        private const string ResultOperationName = "mvc_result";
        private const string ResultItemKey = "ot-MvcResult";
        private const string ResultTagType = "result.type";

        private readonly ProxyAdapter _proxyAdapter;

        public MvcInterceptor(ILoggerFactory loggerFactory, ITracer tracer)
            : base(loggerFactory, tracer)
        {
            _proxyAdapter = new ProxyAdapter();

            _proxyAdapter.Register("Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor");
            _proxyAdapter.Register("Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor");
        }

        protected override bool IsEnabled(string listenerName)
        {
            if (listenerName == EventBeforeAction) return true;
            if (listenerName == EventAfterAction) return true;
            if (listenerName == EventBeforeActionResult) return true;
            if (listenerName == EventAfterActionResult) return true;

            return false;
        }

        [DiagnosticName(EventBeforeAction)]
        public void OnBeforeAction(object actionDescriptor, HttpContext httpContext)
        {
            // NOTE: This event is the start of the action pipeline. The action has been selected, the route
            //       has been selected but no filters have run and model binding hasn't occured.
            Execute(() =>
            {
                var parent = Tracer.ActiveSpan;
                if (parent == null)
                {
                    Logger.LogWarning("ActiveSpan not found");
                    return;
                }

                var typedActionDescriptor = ConvertActionDescriptor(actionDescriptor);

                var span = Tracer.BuildSpan(ActionOperationName).Start();

                Tags.Component.Set(span, Component);

                span.SetTag(ActionTagControllerName, typedActionDescriptor.ControllerName);
                span.SetTag(ActionTagActionName, typedActionDescriptor.ActionName);

                // Tells OnAfterAction that a span was created and should be finished.
                httpContext.Items[ActionItemKey] = true;
            });
        }

        [DiagnosticName(EventAfterAction)]
        public void OnAfterAction(HttpContext httpContext)
        {
            Execute(() =>
            {
                if (!httpContext.Items.ContainsKey(ActionItemKey))
                    return;

                var span = Tracer.ActiveSpan;
                if (span == null)
                {
                    Logger.LogError("ActiveSpan not found");
                    return;
                }

                span.Finish();
            });
        }

        [DiagnosticName(EventBeforeActionResult)]
        public void OnBeforeActionResult(IActionContext actionContext, object result)
        {
            // NOTE: This event is the start of the result pipeline. The action has been executed, but
            //       we haven't yet determined which view (if any) will handle the request

            Execute(() =>
            {
                var parent = Tracer.ActiveSpan;
                if (parent == null)
                {
                    Logger.LogWarning("ActiveSpan not found");
                    return;
                }

                string resultType = result.GetType().Name;

                var span = Tracer.BuildSpan(ResultOperationName).Start();

                Tags.Component.Set(span, Component);
                span.SetTag(ResultTagType, resultType);

                // Tells OnAfterActionResult that a span was created and should be finished.
                actionContext.HttpContext.Items[ResultItemKey] = true;
            });
        }

        [DiagnosticName(EventAfterActionResult)]
        public void OnAfterActionResult(IActionContext actionContext)
        {
            Execute(() =>
            {
                if (!actionContext.HttpContext.Items.ContainsKey(ResultItemKey))
                    return;

                var span = Tracer.ActiveSpan;
                if (span == null)
                {
                    Logger.LogError("ActiveSpan not found");
                    return;
                }

                span.Finish();
            });
        }

        private IActionDescriptor ConvertActionDescriptor(object actionDescriptor)
        {
            var typedActionDescriptor = (IActionDescriptor)null;

            // NOTE: ActionDescriptor is usually ControllerActionDescriptor but the compile time type is
            //       ActionDescriptor. This is a problem because we are missing the ControllerName which
            //       we use a lot.
            switch (actionDescriptor.GetType().FullName)
            {
                case "Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor":
                    typedActionDescriptor = _proxyAdapter.Process<IActionDescriptor>("Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor", actionDescriptor);
                    break;
                case "Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor":
                    typedActionDescriptor = _proxyAdapter.Process<IActionDescriptor>("Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor", actionDescriptor);
                    break;
            }

            return typedActionDescriptor;
        }
    }
}
