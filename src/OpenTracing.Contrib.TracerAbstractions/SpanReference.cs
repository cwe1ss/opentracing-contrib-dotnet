using System;

namespace OpenTracing.Contrib.TracerAbstractions
{
    public class SpanReference
    {
        public string ReferenceType { get; }

        public ISpanContext ReferencedContext { get; }

        public SpanReference(string referenceType, ISpanContext referencedContext)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
                throw new ArgumentNullException(nameof(referenceType));

            if (referencedContext == null)
                throw new ArgumentNullException(nameof(referencedContext));

            ReferenceType = referenceType;
            ReferencedContext = referencedContext;
        }
    }
}