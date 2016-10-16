using System;
using System.Text;

namespace OpenTracing.Contrib
{
    public static class SpanExtensions
    {
        private const string ErrorMessage = "error_message";
        private const string ErrorStacktrace = "error_stacktrace";
        private const string ErrorInner = "error_inner";
        private const string ErrorData = "error_data";
        private const string ErrorHResult = "error_hresult";

        public static void SetException(this ISpan span, Exception ex)
        {
            if (span == null || ex == null)
                return;

            span.SetTag(Tags.Error, true);

            span.SetTag(ErrorMessage, ex.Message);
            span.SetTag(ErrorStacktrace, ex.StackTrace);

            if (ex.InnerException != null)
            {
                span.SetTag(ErrorInner, ex.InnerException.ToString());
            }

            if (ex.Data?.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (object key in ex.Data.Keys)
                {
                    sb.Append($"{key}:{ex.Data[key]};");
                }

                sb.Length--; // removes last ";"

                span.SetTag(ErrorData, sb.ToString());
            }

            if (ex.HResult != 0)
            {
                span.SetTag(ErrorHResult, ex.HResult);
            }
        }
    }
}