using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing.Contrib.TracerAbstractions;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class ZipkinSpan : SpanBase
    {
        private readonly Endpoint _endpoint;

        private List<Annotation> _annotations;
        private List<BinaryAnnotation> _binaryAnnotations;

        public ZipkinSpanContext TypedContext => (ZipkinSpanContext)Context;

        public IEnumerable<Annotation> Annotations => _annotations ?? Enumerable.Empty<Annotation>();
        public IEnumerable<BinaryAnnotation> BinaryAnnotations => _binaryAnnotations ?? Enumerable.Empty<BinaryAnnotation>();

        public ZipkinSpan(
            ZipkinTracer tracer,
            ZipkinSpanContext context,
            string operationName,
            DateTime? startTimestamp,
            IEnumerable<KeyValuePair<string, object>> tags)
            : base(tracer?.Reporter, context, operationName, startTimestamp)
        {
            _endpoint = tracer.Endpoint;

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    AddTag(tag.Key, tag.Value);
                }
            }
        }

        public override ISpan SetTag(string key, string value)
        {
            return AddTag(key, value);
        }

        public override ISpan SetTag(string key, double value)
        {
            return AddTag(key, value);
        }

        public override ISpan SetTag(string key, int value)
        {
            return AddTag(key, value);
        }

        public override ISpan SetTag(string key, bool value)
        {
            return AddTag(key, value);
        }

        protected override void LogInternal (DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            // TODO @cweiss How should we store fields?
            string value = string.Join(", ", fields.Select(x => $"{x.Key}:{x.Value}"));

            if (_annotations == null)
                _annotations = new List<Annotation>();

            _annotations.Add(new Annotation(_endpoint, value, timestamp));
        }

        private ISpan AddTag(string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            bool added = AddTagAsAnnotation(key, value);
            if (!added)
            {
                AddTagAsBinaryAnnotation(key, value);
            }

            return this;
        }

        private bool AddTagAsAnnotation(string key, object value)
        {
            string annotationValue = null;

            string stringValue = value?.ToString();

            if (key == Tags.SpanKind && stringValue == Tags.SpanKindServer)
            {
                annotationValue = AnnotationConstants.ServerReceive;
            }
            else if (key == Tags.SpanKind && stringValue == Tags.SpanKindClient)
            {
                annotationValue = AnnotationConstants.ClientSend;
            }

            if (annotationValue != null)
            {
                if (_annotations == null)
                    _annotations = new List<Annotation>();

                _annotations.Add(new Annotation(_endpoint, annotationValue, StartTimestamp));
                return true;
            }

            return false;
        }

        private void AddTagAsBinaryAnnotation(string key, object value)
        {
            if (_binaryAnnotations == null)
                _binaryAnnotations = new List<BinaryAnnotation>();

            if (key == Tags.Component)
                key = AnnotationConstants.LocalComponent;

            _binaryAnnotations.Add(new BinaryAnnotation
            {
                Host = _endpoint,
                Key = key,
                Value = value
            });
        }
    }
}