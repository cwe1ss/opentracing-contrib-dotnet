namespace OpenTracing.Contrib.ZipkinTracer
{
    public class BinaryAnnotation
    {
        public Endpoint Host { get; set; }

        public string Key { get; set; }

        public object Value { get; set; }

        public AnnotationType AnnotationType => Value.GetType().AsAnnotationType();
    }
}