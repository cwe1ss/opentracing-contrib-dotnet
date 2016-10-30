using System;
using System.Collections.Generic;
using System.Text;

namespace OpenTracing.Contrib
{
    public static class SpanExtensions
    {
        private const string ErrorMessage = "error_message";
        private const string ErrorStacktrace = "error_stacktrace";
        private const string ErrorType = "error_type";
        private const string ErrorInner = "error_inner";
        private const string ErrorData = "error_data";
        private const string ErrorHResult = "error_hresult";
        private const string ErrorCustom = "error_custom";

        public static void SetException(this ISpan span, Exception ex, string customMessage = null)
        {
            if (span == null || ex == null)
                return;

            span.SetTag(Tags.Error, true);

            var fields = new Dictionary<string, object>();
            fields.Add(ErrorMessage, ex.Message);
            fields.Add(ErrorType, ex.GetType().FullName);
            fields.Add(ErrorStacktrace, ex.StackTrace);

            if (ex.InnerException != null)
            {
                fields.Add(ErrorInner, ex.InnerException.ToString());
            }

            if (ex.Data?.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (object key in ex.Data.Keys)
                {
                    sb.Append($"{key}:{ex.Data[key]};");
                }

                sb.Length--; // removes last ";"

                fields.Add(ErrorData, sb.ToString());
            }

            if (ex.HResult != 0)
            {
                fields.Add(ErrorHResult, ex.HResult);
            }

            if (customMessage != null)
            {
                fields.Add(ErrorCustom, customMessage);
            }

            span.Log(fields);
        }
    }
}