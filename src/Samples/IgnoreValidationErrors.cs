﻿namespace Jasper.Testing.Samples
{
    // SAMPLE: IgnoreValidationErrors
    public class IgnoreValidationErrors : JasperOptions
    {
        public IgnoreValidationErrors()
        {
            Advanced.ThrowOnValidationErrors = false;
        }
    }

    // ENDSAMPLE
}
