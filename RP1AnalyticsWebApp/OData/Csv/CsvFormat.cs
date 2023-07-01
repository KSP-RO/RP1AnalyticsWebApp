using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.OData
{
    public class CsvFormat : ODataFormat
    {
        public override ODataInputContext CreateInputContext(ODataMessageInfo messageInfo, ODataMessageReaderSettings messageReaderSettings)
        {
            throw new NotImplementedException();
        }

        public override Task<ODataInputContext> CreateInputContextAsync(ODataMessageInfo messageInfo, ODataMessageReaderSettings messageReaderSettings)
        {
            throw new NotImplementedException();
        }

        public override ODataOutputContext CreateOutputContext(ODataMessageInfo messageInfo, ODataMessageWriterSettings messageWriterSettings)
        {
            throw new NotImplementedException();
        }

        public override Task<ODataOutputContext> CreateOutputContextAsync(
            ODataMessageInfo messageInfo, ODataMessageWriterSettings messageWriterSettings)
        {
            return Task.FromResult<ODataOutputContext>(
                new CsvOutputContext(this, messageWriterSettings, messageInfo));
        }

        public override IEnumerable<ODataPayloadKind> DetectPayloadKind(ODataMessageInfo messageInfo, ODataMessageReaderSettings settings)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(ODataMessageInfo messageInfo, ODataMessageReaderSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}