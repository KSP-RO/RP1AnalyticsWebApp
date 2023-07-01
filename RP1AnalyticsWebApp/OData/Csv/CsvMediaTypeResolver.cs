using Microsoft.OData;
using System.Collections.Generic;
using System.Linq;

namespace RP1AnalyticsWebApp.OData
{
    public class CsvMediaTypeResolver : ODataMediaTypeResolver
    {
        private readonly ODataMediaTypeFormat[] _mediaTypeFormats =
        {
            new ODataMediaTypeFormat(new ODataMediaType("text", "csv"), new CsvFormat()),
        };

        public override IEnumerable<ODataMediaTypeFormat> GetMediaTypeFormats(ODataPayloadKind payloadKind)
        {
            if (payloadKind == ODataPayloadKind.Resource || payloadKind == ODataPayloadKind.ResourceSet)
            {
                return _mediaTypeFormats.Concat(base.GetMediaTypeFormats(payloadKind));
            }

            return base.GetMediaTypeFormats(payloadKind);
        }
    }
}
