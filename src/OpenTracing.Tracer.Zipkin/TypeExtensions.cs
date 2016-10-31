using System;
using System.Collections.Generic;

namespace OpenTracing.Tracer.Zipkin
{
    public static class TypeExtensions
    {
        private const int TicksPerMicrosecond = 10;

        public static readonly DateTime UnixEpochStart = new DateTime(1970, 1, 1);

        public static long ToUnixMicroseconds(this DateTime date)
        {
            return (date.Ticks - UnixEpochStart.Ticks) / TicksPerMicrosecond;
        }

        private static readonly Dictionary<Type, AnnotationType> annotationTypeMappings =
            new Dictionary<Type, AnnotationType>()
            {
                { typeof(bool), AnnotationType.Boolean },
                { typeof(byte[]), AnnotationType.ByteArray },
                { typeof(short), AnnotationType.Int16 },
                { typeof(int), AnnotationType.Int32 },
                { typeof(long), AnnotationType.Int64 },
                { typeof(double), AnnotationType.Double },
                { typeof(string), AnnotationType.String }
            };

        public static AnnotationType AsAnnotationType(this Type type)
        {
            return annotationTypeMappings.ContainsKey(type) ? annotationTypeMappings[type] : AnnotationType.String;
        }
    }

}